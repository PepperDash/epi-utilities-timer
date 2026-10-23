using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Plugins.Tests;

/// <summary>
/// The plugin DLL loads, references Essentials v3, and exposes the factories Essentials scans for.
/// </summary>
public class FactoryDiscoveryTests
{
    private const string PluginNamespace = "PepperDash.Essentials.Plugins";

    [Fact]
    public void Assembly_Loads_Successfully()
    {
        AssemblyFixture.PluginAssembly.Should().NotBeNull();
        // GetTypes() forces resolution of every type's base chain - fails if a dependency is missing.
        AssemblyFixture.PluginAssembly.GetTypes().Should().NotBeEmpty();
    }

    [Fact]
    public void Assembly_Name_Matches_Expected()
    {
        AssemblyFixture.PluginAssembly.GetName().Name.Should().Be(AssemblyFixture.PluginAssemblyName);
    }

    [Fact]
    public void Plugin_Csproj_Pins_Essentials_v3_Release_Candidate()
    {
        var csproj = File.ReadAllText(AssemblyFixture.PluginCsprojPath);
        var match = Regex.Match(csproj,
            $@"<PackageReference\s+Include=""{AssemblyFixture.EssentialsPackageId}""\s+Version=""([^""]+)""");

        match.Success.Should().BeTrue("the plugin must reference the PepperDashEssentials package");
        match.Groups[1].Value.Should().Be(AssemblyFixture.EssentialsPackageVersion);
    }

    [Fact]
    public void Essentials_Dependencies_Resolve_From_v3_Release_Candidate()
    {
        var essentialsDlls = AssemblyFixture.ResolvedPackageDlls
            .Where(p => Path.GetFileName(p).StartsWith("PepperDash", StringComparison.OrdinalIgnoreCase))
            .ToList();

        essentialsDlls.Should().NotBeEmpty("src/obj/project.assets.json should resolve the Essentials packages");
        var expectedSegment = $"{Path.DirectorySeparatorChar}{AssemblyFixture.EssentialsPackageVersion}{Path.DirectorySeparatorChar}";
        essentialsDlls.Should().OnlyContain(p => p.Contains(expectedSegment, StringComparison.OrdinalIgnoreCase),
            $"every PepperDash assembly should come from {AssemblyFixture.EssentialsPackageVersion}");
    }

    [Fact]
    public void Factory_Base_Type_Resolves_From_Essentials_v3()
    {
        // The assembly that actually defines EssentialsPluginDeviceFactory<T> (as resolved for the plugin).
        var factory = AssemblyFixture.FindFactoryTypes().First();
        Type? baseFactory = null;
        for (var b = factory.BaseType; b != null && baseFactory == null; b = b.BaseType)
            if (b.IsGenericType && b.GetGenericTypeDefinition().Name.StartsWith("EssentialsPluginDeviceFactory", StringComparison.Ordinal))
                baseFactory = b.GetGenericTypeDefinition();

        baseFactory.Should().NotBeNull();
        var essentialsAssembly = baseFactory!.Assembly;

        essentialsAssembly.GetName().Version!.Major.Should().Be(3);
        AssemblyFixture.PluginAssembly.GetReferencedAssemblies()
            .Should().Contain(r => r.Name == essentialsAssembly.GetName().Name && r.Version!.Major == 3,
                "the plugin must be compiled against the Essentials v3 assembly");
        essentialsAssembly.Location.Should().Contain(
            $"{Path.DirectorySeparatorChar}{AssemblyFixture.EssentialsPackageVersion}{Path.DirectorySeparatorChar}");
    }

    [Fact]
    public void Factory_Count_Matches_Expected()
    {
        AssemblyFixture.FindFactoryTypes().Should().HaveCount(2);
    }

    [Theory]
    [InlineData("CountdownTimerFactory")]
    [InlineData("CountupTimerFactory")]
    public void Factory_Exists_ByName(string factoryName)
    {
        var factory = AssemblyFixture.FindFactoryTypes().SingleOrDefault(t => t.Name == factoryName);

        factory.Should().NotBeNull();
        factory!.Namespace.Should().Be(PluginNamespace);
        factory.IsPublic.Should().BeTrue("Essentials only discovers public factory types");
    }

    [Fact]
    public void All_Factories_Have_Public_Parameterless_Constructor()
    {
        foreach (var factory in AssemblyFixture.FindFactoryTypes())
        {
            factory.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)
                .Should().NotBeNull($"{factory.Name} is created via Activator.CreateInstance during plugin load");
        }
    }

    [Theory]
    [InlineData("CountdownTimerFactory", "CountdownTimer")]
    [InlineData("CountupTimerFactory", "CountupTimer")]
    public void Factory_Builds_Expected_Device_Type(string factoryName, string deviceName)
    {
        var factory = AssemblyFixture.FindFactoryTypes().Single(t => t.Name == factoryName);

        Type? genericArg = null;
        for (var b = factory.BaseType; b != null; b = b.BaseType)
        {
            if (b.IsGenericType && b.GetGenericTypeDefinition().Name.StartsWith("EssentialsPluginDeviceFactory", StringComparison.Ordinal))
            {
                genericArg = b.GetGenericArguments()[0];
                break;
            }
        }

        genericArg.Should().NotBeNull();
        genericArg!.Name.Should().Be(deviceName);
        genericArg.Namespace.Should().Be(PluginNamespace);
    }

    [Theory]
    [InlineData("CountdownTimer")]
    [InlineData("CountupTimer")]
    public void Device_Type_Is_Public_EssentialsDevice(string deviceName)
    {
        var device = AssemblyFixture.PluginAssembly.GetType($"{PluginNamespace}.{deviceName}");

        device.Should().NotBeNull();
        device!.IsPublic.Should().BeTrue();
        device.IsAbstract.Should().BeFalse();
        AssemblyFixture.InheritsFrom(device, "EssentialsDevice").Should().BeTrue();
    }

    [Theory]
    [InlineData("CountdownTimer")]
    [InlineData("CountupTimer")]
    public void Device_Type_Is_Bridgeable(string deviceName)
    {
        var device = AssemblyFixture.PluginAssembly.GetType($"{PluginNamespace}.{deviceName}")!;

        var bridgeable = AssemblyFixture.InheritsFrom(device, "EssentialsBridgeableDevice")
            || device.GetInterfaces().Any(i => i.Name == "IBridgeAdvanced");

        bridgeable.Should().BeTrue($"{deviceName} is linked to an EISC bridge via its join map");
    }
}
