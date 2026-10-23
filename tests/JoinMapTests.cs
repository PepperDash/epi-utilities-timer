using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Plugins.Tests;

/// <summary>
/// Join maps are well-formed: public JoinDataComplete fields with matching [JoinName] attributes
/// (checked on the compiled DLL) and non-overlapping join numbers per signal type and direction
/// (checked from source, because JoinData values are set in field initializers that
/// MetadataLoadContext cannot execute).
///
/// Join capability (direction) is intentionally NOT asserted per join: TimerExtend is labelled
/// ToSIMPL in source and README while it behaves as an input (issue #18), and these tests must not
/// pin either the bug or the fix.
/// </summary>
public class JoinMapTests
{
    private const string PluginNamespace = "PepperDash.Essentials.Plugins";

    public static TheoryData<string> JoinMapNames => new() { "CountdownTimerJoinMap", "CountupTimerJoinMap" };

    private sealed record SourceJoin(string FieldName, string JoinName, uint Number, uint Span, string Capabilities, string Type);

    private static Type JoinMapType(string joinMapName)
    {
        var type = AssemblyFixture.PluginAssembly.GetType($"{PluginNamespace}.{joinMapName}");
        type.Should().NotBeNull($"{joinMapName} should be compiled into the plugin");
        return type!;
    }

    private static List<FieldInfo> JoinFields(Type joinMap) =>
        joinMap.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType.Name == "JoinDataComplete")
            .ToList();

    private static string? JoinNameArgument(MemberInfo member) =>
        member.CustomAttributes
            .FirstOrDefault(a => a.AttributeType.Name == "JoinNameAttribute")?
            .ConstructorArguments.FirstOrDefault().Value as string;

    private static List<SourceJoin> ParseSourceJoins(string joinMapName)
    {
        var body = AssemblyFixture.FindClassBody(joinMapName);
        body.Should().NotBeNull($"source for {joinMapName} should exist under src/");

        var pattern = new Regex(
            @"\[JoinName\(""(?<joinName>[^""]+)""\)\]\s*public\s+JoinDataComplete\s+(?<field>\w+)\s*=\s*new\s+JoinDataComplete\s*\(" +
            @"\s*new\s+JoinData\s*\{(?<data>[^}]*)\}\s*,\s*new\s+JoinMetadata\s*\{(?<meta>[^}]*)\}",
            RegexOptions.Singleline);

        static string Get(string block, string name, string valuePattern)
        {
            var m = Regex.Match(block, $@"\b{name}\s*=\s*(?<v>{valuePattern})");
            m.Success.Should().BeTrue($"'{name}' should be set in: {block.Trim()}");
            return m.Groups["v"].Value;
        }

        return pattern.Matches(body!).Select(m => new SourceJoin(
                m.Groups["field"].Value,
                m.Groups["joinName"].Value,
                uint.Parse(Get(m.Groups["data"].Value, "JoinNumber", @"\d+")),
                uint.Parse(Get(m.Groups["data"].Value, "JoinSpan", @"\d+")),
                Get(m.Groups["meta"].Value, "JoinCapabilities", @"eJoinCapabilities\.\w+").Split('.')[1],
                Get(m.Groups["meta"].Value, "JoinType", @"eJoinType\.\w+").Split('.')[1]))
            .ToList();
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_Derives_From_JoinMapBaseAdvanced_With_JoinStart_Constructor(string joinMapName)
    {
        var type = JoinMapType(joinMapName);

        type.IsPublic.Should().BeTrue();
        AssemblyFixture.InheritsFrom(type, "JoinMapBaseAdvanced").Should().BeTrue();

        var uintType = AssemblyFixture.Context.CoreAssembly!.GetType("System.UInt32")!;
        type.GetConstructor(new[] { uintType }).Should().NotBeNull("devices construct join maps with new XJoinMap(joinStart)");
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_Fields_Are_Public_Instance_With_Matching_JoinName(string joinMapName)
    {
        var fields = JoinFields(JoinMapType(joinMapName));
        fields.Should().NotBeEmpty();

        foreach (var field in fields)
        {
            field.IsPublic.Should().BeTrue($"{field.Name} must be public for JoinMapBaseAdvanced to discover it");
            field.IsStatic.Should().BeFalse($"{field.Name} must be an instance field");
            JoinNameArgument(field).Should().Be(field.Name, $"{field.Name} needs [JoinName(\"{field.Name}\")]");
        }
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_JoinNames_Are_Unique(string joinMapName)
    {
        JoinFields(JoinMapType(joinMapName)).Select(JoinNameArgument).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_Source_Matches_Compiled_Fields(string joinMapName)
    {
        // Guards the source parser below: every compiled join field must be parsed from source.
        var compiled = JoinFields(JoinMapType(joinMapName)).Select(f => f.Name);
        var parsed = ParseSourceJoins(joinMapName);

        parsed.Select(j => j.FieldName).Should().BeEquivalentTo(compiled);
        parsed.Should().OnlyContain(j => j.JoinName == j.FieldName);
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_Joins_Have_Valid_Number_Span_And_Type(string joinMapName)
    {
        foreach (var join in ParseSourceJoins(joinMapName))
        {
            join.Number.Should().BeGreaterThan(0u, $"{join.FieldName} join numbers are 1-based");
            join.Span.Should().BeGreaterThan(0u, $"{join.FieldName} span");
            join.Type.Should().BeOneOf(new[] { "Digital", "Analog", "Serial" }, $"{join.FieldName} type");
            join.Capabilities.Should().NotBe("None", $"{join.FieldName} capabilities");
        }
    }

    [Theory]
    [MemberData(nameof(JoinMapNames))]
    public void JoinMap_Join_Numbers_Do_Not_Overlap_Per_Type_And_Direction(string joinMapName)
    {
        // Input and output joins of the same type may share a number (e.g. digital 1 start / counting);
        // two joins in the same direction may not. ToFromSIMPL occupies both directions.
        var joins = ParseSourceJoins(joinMapName);

        foreach (var direction in new[] { "FromSIMPL", "ToSIMPL" })
        {
            foreach (var group in joins
                         .Where(j => j.Capabilities == direction || j.Capabilities == "ToFromSIMPL")
                         .GroupBy(j => j.Type))
            {
                var occupied = new Dictionary<uint, string>();
                foreach (var join in group)
                {
                    for (var n = join.Number; n < join.Number + join.Span; n++)
                    {
                        occupied.Should().NotContainKey(n,
                            $"{group.Key} {direction} join {n} is used by both {(occupied.TryGetValue(n, out var other) ? other : "?")} and {join.FieldName}");
                        occupied[n] = join.FieldName;
                    }
                }
            }
        }
    }

    // Documented bridge contract (README join tables): join number and signal type only.
    [Theory]
    [InlineData("CountdownTimerJoinMap", "TimerStart", 1u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerCounting", 1u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerCancel", 2u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerExpired", 2u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerFinish", 3u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerWarning", 3u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerExtend", 4u, "Digital")]
    [InlineData("CountdownTimerJoinMap", "TimerCountdownSet", 1u, "Analog")]
    [InlineData("CountdownTimerJoinMap", "TimerPercentage", 1u, "Analog")]
    [InlineData("CountdownTimerJoinMap", "TimerValue", 1u, "Serial")]
    [InlineData("CountupTimerJoinMap", "CountupTimerStart", 1u, "Digital")]
    [InlineData("CountupTimerJoinMap", "CountupTimerCounting", 1u, "Digital")]
    [InlineData("CountupTimerJoinMap", "CountupTimerStop", 2u, "Digital")]
    [InlineData("CountupTimerJoinMap", "CountupTimerValue", 1u, "Serial")]
    public void JoinMap_Matches_Documented_Join_Number_And_Type(string joinMapName, string joinName, uint number, string type)
    {
        var join = ParseSourceJoins(joinMapName).SingleOrDefault(j => j.JoinName == joinName);

        join.Should().NotBeNull($"{joinMapName} should define {joinName}");
        join!.Number.Should().Be(number);
        join.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("CountdownTimerJoinMap", 10)]
    [InlineData("CountupTimerJoinMap", 4)]
    public void JoinMap_Has_Expected_Join_Count(string joinMapName, int count)
    {
        JoinFields(JoinMapType(joinMapName)).Should().HaveCount(count);
    }
}
