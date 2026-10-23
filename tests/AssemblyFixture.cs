using System.Reflection;
using System.Text.Json;

namespace PepperDash.Essentials.Plugins.Tests;

/// <summary>
/// Shared infrastructure for reflection-only inspection of the built plugin DLL.
/// Nothing here executes plugin or Essentials code, so no Crestron runtime is needed.
/// </summary>
public static class AssemblyFixture
{
    public const string PluginAssemblyName = "epi-utilities-timer.4Series";
    public const string EssentialsPackageId = "PepperDashEssentials";
    public const string EssentialsPackageVersion = "3.0.0-rc.19";

    private static readonly Lazy<MetadataLoadContext> LazyContext = new(CreateContext);
    private static readonly Lazy<Assembly> LazyAssembly = new(LoadPluginAssembly);
    private static readonly Lazy<List<string>> LazyResolvedPackageDlls = new(ResolvePackageDlls);

    /// <summary>Build configuration, derived from tests/bin/{Configuration}/net8.0/.</summary>
    public static string Configuration
    {
        get
        {
            var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parts = baseDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return parts[^2];
        }
    }

    public static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static string SourceDirectory => Path.Combine(RepoRoot, "src");

    public static string PluginCsprojPath => Path.Combine(SourceDirectory, PluginAssemblyName + ".csproj");

    // src/epi-utilities-timer.4Series.csproj: <OutputPath>4Series\bin\$(Configuration)\</OutputPath>, <TargetFramework>net8</TargetFramework>
    public static string PluginDllPath =>
        Path.Combine(SourceDirectory, "4Series", "bin", Configuration, "net8", PluginAssemblyName + ".dll");

    private static string PluginOutputDir => Path.GetDirectoryName(PluginDllPath)!;

    public static MetadataLoadContext Context => LazyContext.Value;
    public static Assembly PluginAssembly => LazyAssembly.Value;

    /// <summary>Package DLLs resolved from src/obj/project.assets.json (the plugin's real dependency graph).</summary>
    public static IReadOnlyList<string> ResolvedPackageDlls => LazyResolvedPackageDlls.Value;

    private static MetadataLoadContext CreateContext()
    {
        if (!File.Exists(PluginDllPath))
            throw new FileNotFoundException(
                $"Plugin DLL not found at '{PluginDllPath}'. Build the plugin first.", PluginDllPath);

        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var dllByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Priority 1: plugin output dir
        foreach (var dll in Directory.GetFiles(PluginOutputDir, "*.dll"))
            dllByName[Path.GetFileName(dll)] = dll;

        // Priority 2: .NET runtime
        foreach (var dll in Directory.GetFiles(runtimeDir, "*.dll"))
            dllByName.TryAdd(Path.GetFileName(dll), dll);

        // Priority 3: plugin's project.assets.json (NOT deps.json - PepperDashEssentials is
        // referenced with ExcludeAssets=runtime, so deps.json omits every dependency).
        foreach (var path in ResolvedPackageDlls)
            dllByName.TryAdd(Path.GetFileName(path), path);

        return new MetadataLoadContext(new PathAssemblyResolver(dllByName.Values));
    }

    private static List<string> ResolvePackageDlls()
    {
        var assetsJsonPath = Path.Combine(SourceDirectory, "obj", "project.assets.json");
        return File.Exists(assetsJsonPath)
            ? ResolveProjectAssetsAssemblies(assetsJsonPath).ToList()
            : new List<string>();
    }

    private static IEnumerable<string> ResolveProjectAssetsAssemblies(string assetsJsonPath)
    {
        var nugetDir = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrWhiteSpace(nugetDir))
            nugetDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages");

        using var stream = File.OpenRead(assetsJsonPath);
        using var doc = JsonDocument.Parse(stream);

        if (!doc.RootElement.TryGetProperty("targets", out var targets))
            yield break;

        // Prefer the RID-less target ("net8"), never hard-code the TFM.
        var allTargets = targets.EnumerateObject().ToList();
        var target = allTargets.FirstOrDefault(t => !t.Name.Contains('/'));
        if (target.Value.ValueKind != JsonValueKind.Object)
            target = allTargets.FirstOrDefault();
        if (target.Value.ValueKind != JsonValueKind.Object)
            yield break;

        foreach (var lib in target.Value.EnumerateObject())
        {
            var slash = lib.Name.LastIndexOf('/');
            if (slash < 0) continue;
            var packageId = lib.Name[..slash].ToLowerInvariant();
            var version = lib.Name[(slash + 1)..].ToLowerInvariant();

            if (!lib.Value.TryGetProperty("compile", out var assets) &&
                !lib.Value.TryGetProperty("runtime", out assets))
                continue;

            foreach (var asset in assets.EnumerateObject())
            {
                if (!asset.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dllPath = Path.Combine(nugetDir, packageId, version,
                    asset.Name.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(dllPath))
                    yield return dllPath;
            }
        }
    }

    private static Assembly LoadPluginAssembly()
    {
        if (!File.Exists(PluginDllPath))
            throw new FileNotFoundException(
                $"Plugin DLL not found at '{PluginDllPath}'. Build the plugin first.", PluginDllPath);
        return Context.LoadFromAssemblyPath(PluginDllPath);
    }

    /// <summary>Concrete factory types, walking the full base chain (TimerBaseDeviceFactory&lt;T&gt; sits in between).</summary>
    public static List<Type> FindFactoryTypes(string baseTypePrefix = "EssentialsPluginDeviceFactory")
    {
        return PluginAssembly.GetTypes()
            .Where(t => !t.IsAbstract && InheritsFromGeneric(t, baseTypePrefix))
            .ToList();
    }

    public static bool InheritsFromGeneric(Type type, string prefix)
    {
        for (var b = type.BaseType; b != null; b = b.BaseType)
            if (b.IsGenericType && b.GetGenericTypeDefinition().Name.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        return false;
    }

    public static bool InheritsFrom(Type type, string baseTypeName)
    {
        for (var b = type.BaseType; b != null; b = b.BaseType)
            if (b.Name == baseTypeName)
                return true;
        return false;
    }

    private static readonly Lazy<string[]> AllSourceContents = new(() =>
        Directory.GetFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText)
            .ToArray());

    public static string? FindSourceForClass(string className) =>
        AllSourceContents.Value.FirstOrDefault(content => DeclaresClass(content, className));

    private static bool DeclaresClass(string content, string className)
    {
        var needle = "class " + className;
        for (var i = content.IndexOf(needle, StringComparison.Ordinal); i >= 0;
             i = content.IndexOf(needle, i + 1, StringComparison.Ordinal))
        {
            var after = i + needle.Length;
            var next = after < content.Length ? content[after] : ' ';
            if (!char.IsLetterOrDigit(next) && next != '_')
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the body of a class declaration (from its opening brace to the matching close brace).
    /// Used for constructor-assigned values MetadataLoadContext cannot execute.
    /// </summary>
    public static string? FindClassBody(string className)
    {
        var source = FindSourceForClass(className);
        if (source == null) return null;

        var match = System.Text.RegularExpressions.Regex.Match(source, $@"class\s+{className}\b");
        if (!match.Success) return null;

        var open = source.IndexOf('{', match.Index);
        if (open < 0) return null;

        var depth = 0;
        for (var i = open; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}' && --depth == 0)
                return source.Substring(open, i - open + 1);
        }
        return null;
    }
}
