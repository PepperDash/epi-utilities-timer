using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PepperDash.Essentials.Plugins.Tests;

/// <summary>
/// Config contracts. The compiled DLL is inspected for [JsonProperty] names; the same POCOs
/// (src/TimerPropertiesConfig.cs, Newtonsoft-only) are linked into this project so the README and
/// configurationFile.json shapes can be deserialized exactly as the factories do
/// (dc.Properties.ToObject&lt;T&gt;()).
/// </summary>
public class ConfigDeserializationTests
{
    private const string PluginNamespace = "PepperDash.Essentials.Plugins";

    private static readonly string[] FactoryTypeNames = { "countdownTimer", "countupTimer" };

    // ---- compiled-assembly contract -------------------------------------------------------

    [Theory]
    [InlineData("CountdownTimerPropertiesConfig")]
    [InlineData("CountupTimerPropertiesConfig")]
    public void Config_Class_Exists_With_Public_Parameterless_Constructor(string className)
    {
        var type = AssemblyFixture.PluginAssembly.GetType($"{PluginNamespace}.{className}");

        type.Should().NotBeNull();
        type!.IsPublic.Should().BeTrue();
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)
            .Should().NotBeNull("Newtonsoft.Json needs a parameterless constructor");
    }

    [Theory]
    [InlineData("CountdownTimerPropertiesConfig", "countdownTime", "Int32")]
    [InlineData("CountdownTimerPropertiesConfig", "warningTime", "Nullable`1")]
    [InlineData("CountdownTimerPropertiesConfig", "extendTime", "Nullable`1")]
    [InlineData("CountupTimerPropertiesConfig", "autoStopOnStartRelease", "Boolean")]
    public void Config_Property_Has_JsonPropertyAttribute(string className, string jsonName, string clrTypeName)
    {
        var type = AssemblyFixture.PluginAssembly.GetType($"{PluginNamespace}.{className}")!;

        var property = type.GetProperties().SingleOrDefault(p =>
            p.CustomAttributes.Any(a =>
                a.AttributeType.Name == "JsonPropertyAttribute"
                && a.ConstructorArguments.Any(arg => string.Equals(arg.Value?.ToString(), jsonName, StringComparison.Ordinal))));

        property.Should().NotBeNull($"{className} should map [JsonProperty(\"{jsonName}\")]");
        property!.PropertyType.Name.Should().Be(clrTypeName);
        property.CanWrite.Should().BeTrue();
    }

    // ---- sample JSON shapes ---------------------------------------------------------------

    private static JArray ConfigurationFileDevices()
    {
        var json = JObject.Parse(File.ReadAllText(Path.Combine(AssemblyFixture.RepoRoot, "configurationFile.json")));
        return (JArray)json["template"]!["devices"]!;
    }

    private static List<JObject> ReadmeDeviceExamples()
    {
        var readme = File.ReadAllText(Path.Combine(AssemblyFixture.RepoRoot, "README.md"));
        return Regex.Matches(readme, @"```json\s*(?<json>.*?)```", RegexOptions.Singleline)
            .Select(m => JToken.Parse(m.Groups["json"].Value))
            .OfType<JObject>()
            .Where(o => o["type"] != null && o["properties"] != null)
            .ToList();
    }

    private static JObject DeviceByType(IEnumerable<JToken> devices, string type) =>
        devices.OfType<JObject>().Single(d =>
            string.Equals((string?)d["type"], type, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void ConfigurationFile_Timer_Types_Resolve_To_Registered_Factory_TypeNames()
    {
        // Essentials matches device "type" to factory TypeNames case-insensitively.
        var timerTypes = ConfigurationFileDevices()
            .Select(d => (string?)d["type"])
            .Where(t => t != null && t.Contains("timer", StringComparison.OrdinalIgnoreCase))
            .ToList();

        timerTypes.Should().NotBeEmpty();
        timerTypes.Should().OnlyContain(t => FactoryTypeNames.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Readme_Examples_Use_Registered_Factory_TypeNames()
    {
        var examples = ReadmeDeviceExamples();

        examples.Should().HaveCount(2);
        examples.Select(e => (string)e["type"]!)
            .Should().OnlyContain(t => FactoryTypeNames.Contains(t, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Countdown_Readme_Example_Deserializes()
    {
        var props = DeviceByType(ReadmeDeviceExamples(), "countdowntimer")["properties"]!
            .ToObject<CountdownTimerPropertiesConfig>();

        props.Should().NotBeNull();
        props!.CountdownTime.Should().Be(30);
        props.WarningTime.Should().Be(10);
        props.ExtendTime.Should().Be(5);
    }

    [Fact]
    public void Countup_Readme_Example_Deserializes()
    {
        var props = DeviceByType(ReadmeDeviceExamples(), "countuptimer")["properties"]!
            .ToObject<CountupTimerPropertiesConfig>();

        props.Should().NotBeNull();
        props!.autoStopOnStartRelease.Should().BeTrue();
    }

    [Fact]
    public void Countdown_ConfigurationFile_Example_Deserializes()
    {
        // configurationFile.json uses PascalCase keys; Newtonsoft matches them case-insensitively.
        var props = DeviceByType(ConfigurationFileDevices(), "countdowntimer")["properties"]!
            .ToObject<CountdownTimerPropertiesConfig>();

        props.Should().NotBeNull();
        props!.CountdownTime.Should().Be(15);
        props.WarningTime.Should().Be(5);
        props.ExtendTime.Should().Be(5);
    }

    [Fact]
    public void Countup_ConfigurationFile_Example_Deserializes()
    {
        var props = DeviceByType(ConfigurationFileDevices(), "countuptimer")["properties"]!
            .ToObject<CountupTimerPropertiesConfig>();

        props.Should().NotBeNull();
        props!.autoStopOnStartRelease.Should().BeTrue();
    }

    [Theory]
    [InlineData("countdowntimer", typeof(CountdownTimerPropertiesConfig))]
    [InlineData("countuptimer", typeof(CountupTimerPropertiesConfig))]
    public void Sample_Property_Keys_All_Map_To_Config_Properties(string deviceType, Type configType)
    {
        // Fails on contract drift: a key in the docs/sample that no longer maps to a property would be silently ignored.
        var jsonNames = configType.GetProperties()
            .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? p.Name)
            .ToList();

        var samples = new[] { DeviceByType(ReadmeDeviceExamples(), deviceType), DeviceByType(ConfigurationFileDevices(), deviceType) };
        foreach (var sample in samples)
        {
            foreach (var key in ((JObject)sample["properties"]!).Properties().Select(p => p.Name))
                jsonNames.Should().Contain(n => string.Equals(n, key, StringComparison.OrdinalIgnoreCase),
                    $"'{key}' in the {deviceType} sample should map to a {configType.Name} property");
        }
    }

    [Fact]
    public void Countdown_Optional_Times_Default_To_Null_When_Omitted()
    {
        var props = JObject.Parse(@"{ ""countdownTime"": 60 }").ToObject<CountdownTimerPropertiesConfig>();

        props!.CountdownTime.Should().Be(60);
        props.WarningTime.Should().BeNull();
        props.ExtendTime.Should().BeNull();
    }

    [Fact]
    public void Countup_Defaults_AutoStop_To_False_When_Omitted()
    {
        JObject.Parse("{}").ToObject<CountupTimerPropertiesConfig>()!
            .autoStopOnStartRelease.Should().BeFalse();
    }
}
