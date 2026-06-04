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

### Unity Optional Integrations

Unity enables optional integrations automatically when the matching packages are installed:

1. [ZLinq for Unity](https://github.com/Cysharp/ZLinq?tab=readme-ov-file#unity)
2. [MemoryPack for Unity](https://github.com/Cysharp/MemoryPack?tab=readme-ov-file#unity)

No manual compilation symbols are required in Unity. The package assembly definition defines `ZLINQ` and `MEMORY_PACK` when Unity detects the corresponding packages.

## .NET Installation

Add the package as a submodule:

```bash
git submodule add git@github.com:SaltboxGames/SaltboxGames.Core.git SaltboxGames.Core
```

Reference it from your consuming project's `.csproj`:

```xml
<ProjectReference Include="SaltboxGames.Core/SaltboxGames.Core.csproj" />
```

### .NET Optional Integrations

[ZLinq](https://github.com/Cysharp/ZLinq) integration is enabled when the `ZLINQ` compilation symbol is defined.

Create `Directory.Build.props` at the root of your solution:

```xml
<Project>
  <PropertyGroup>
    <EnableZLinq>true</EnableZLinq>
  </PropertyGroup>
</Project>
```

This defines `ZLINQ` and adds the ZLinq package reference.

[MemoryPack](https://github.com/Cysharp/MemoryPack) integration is enabled when the `MEMORY_PACK` compilation symbol is defined.

Add `EnableMemoryPack` to the same `Directory.Build.props` file:

```xml
<Project>
  <PropertyGroup>
    <EnableMemoryPack>true</EnableMemoryPack>
  </PropertyGroup>
</Project>
```

This defines `MEMORY_PACK` and adds the MemoryPack package reference.

## License

This package is licensed under MPL 2.0. See [LICENSE](./LICENSE).

## Attribution

Attribution is not required, but is greatly appreciated. 
If you use this package in your project, please contact us to let us know; we would love to see what you are making with it!
