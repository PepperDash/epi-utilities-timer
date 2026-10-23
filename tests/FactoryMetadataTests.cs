using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Plugins.Tests;

/// <summary>
/// TypeNames and MinimumEssentialsFrameworkVersion are assigned in factory constructors, which
/// MetadataLoadContext cannot execute, so they are validated from source. The ProjectReference in
/// the test csproj keeps the compiled DLL in step with the source being scanned.
/// </summary>
public class FactoryMetadataTests
{
    private static readonly string[] FactoryClassNames = { "CountdownTimerFactory", "CountupTimerFactory" };

    public static TheoryData<string> FactoryNames => new(FactoryClassNames);

    private static string FactoryBody(string factoryName)
    {
        var body = AssemblyFixture.FindClassBody(factoryName);
        body.Should().NotBeNull($"source for {factoryName} should exist under src/");
        return body!;
    }

    private static List<string> ExtractTypeNames(string factoryBody)
    {
        var block = Regex.Match(factoryBody, @"TypeNames\s*=\s*new\s+List<string>\s*\{([^}]*)\}");
        block.Success.Should().BeTrue("TypeNames should be assigned a List<string> initializer");
        return Regex.Matches(block.Groups[1].Value, "\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToList();
    }

    [Theory]
    [MemberData(nameof(FactoryNames))]
    public void Factory_Source_Sets_MinimumEssentialsFrameworkVersion_To_Numeric_Core_Of_Essentials_Package(string factoryName)
    {
        var match = Regex.Match(FactoryBody(factoryName), @"MinimumEssentialsFrameworkVersion\s*=\s*""([^""]+)""");
        match.Success.Should().BeTrue($"{factoryName} must set MinimumEssentialsFrameworkVersion");

        var value = match.Groups[1].Value;

        // Essentials parses this with System.Version; a prerelease suffix (-rc.N) throws and the plugin is skipped.
        Version.TryParse(value, out var parsed).Should().BeTrue($"'{value}' must be a plain numeric version");
        value.Should().MatchRegex(@"^\d+\.\d+\.\d+$");

        var numericCore = AssemblyFixture.EssentialsPackageVersion.Split('-')[0];
        value.Should().Be(numericCore);
        parsed!.Major.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(FactoryNames))]
    public void Factory_Source_Sets_TypeNames(string factoryName)
    {
        ExtractTypeNames(FactoryBody(factoryName)).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("CountdownTimerFactory", "countdownTimer")]
    [InlineData("CountupTimerFactory", "countupTimer")]
    public void Factory_Source_Registers_TypeName(string factoryName, string typeName)
    {
        ExtractTypeNames(FactoryBody(factoryName)).Should().Contain(typeName);
    }

    [Fact]
    public void No_Duplicate_TypeNames_Across_Factories()
    {
        // Essentials keys factories by type name case-insensitively, so compare that way.
        var all = FactoryClassNames
            .SelectMany(f => ExtractTypeNames(FactoryBody(f)))
            .ToList();

        all.Should().OnlyHaveUniqueItems(t => t.ToLowerInvariant());
    }

    [Fact]
    public void Every_Discovered_Factory_Is_Covered_By_These_Tests()
    {
        var discovered = AssemblyFixture.FindFactoryTypes().Select(t => t.Name);
        discovered.Should().BeEquivalentTo(FactoryClassNames);
    }
}
