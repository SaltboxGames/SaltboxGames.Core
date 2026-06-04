# SaltboxGames.Core

SaltboxGames.Core is a source-distributed C# utility package for Unity and standalone .NET projects.
It contains small collection types, pooling helpers, span/list extensions, deterministic random utilities, reflection helpers, and shared runtime shims.

## Documentation

Start with the package documentation index: [Docs/README.md](./Docs/README.md).

## Unity Installation

### Git Dependency

Add the package to your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.saltboxgames.core": "git@github.com:SaltboxGames/SaltboxGames.Core.git"
  }
}
```

### Embedded Package

Use a git submodule if you want the package checked into `Packages/`:

```bash
git submodule add git@github.com:SaltboxGames/SaltboxGames.Core.git ./Packages/com.saltboxgames.core
```

Unity will detect it as an embedded package.

### Unity Dependencies

Install these before using the package in Unity:

1. [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
2. [CommandLineParser](https://github.com/commandlineparser/commandline)
3. [ZLinq](https://github.com/Cysharp/ZLinq?tab=readme-ov-file#unity), if using the optional ZLinq integration

## .NET Installation

Add the package as a submodule:

```bash
git submodule add git@github.com:SaltboxGames/SaltboxGames.Core.git SaltboxGames.Core
```

Reference it from your consuming project's `.csproj`:

```xml
<ProjectReference Include="SaltboxGames.Core/SaltboxGames.Core.csproj" />
```

## Optional MemoryPack Support

[`SafeGuid`](./Docs/shims.md#safeguidcs) can integrate with [MemoryPack](https://github.com/Cysharp/MemoryPack) when the `MEMORY_PACK` compilation symbol is defined.

### .NET

Create `Directory.Build.props` at the root of your solution:

```xml
<Project>
  <PropertyGroup>
    <EnableMemoryPack>true</EnableMemoryPack>
  </PropertyGroup>
</Project>
```

This defines `MEMORY_PACK` and adds the MemoryPack package reference.

### Unity

Install [MemoryPack for Unity](https://github.com/Cysharp/MemoryPack?tab=readme-ov-file#unity). The Unity package defines `MEMORY_PACK` for compatible assemblies.

## License

This package is licensed under MPL 2.0. See [LICENSE](./LICENSE).
