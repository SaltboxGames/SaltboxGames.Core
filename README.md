# SaltboxGames.Core

A lightweight, source-distributed utilities library for both **Unity** and **.NET Core** projects.
Includes optional support for [MemoryPack](https://github.com/Cysharp/MemoryPack) serialization via conditional compilation.

---

## 📦 Installation

### 🎮 Unity (via UPM Git URL)

```jsonc
"com.saltboxgames.core": "git@github.com:SaltboxGames/SaltboxGames.Core.git"
```

Add this to your project's `Packages/manifest.json`.

---

### 🎮 Unity (via Git Submodule)

```bash
git submodule add git@github.com:SaltboxGames/SaltboxGames.Core.git ./Packages/SaltboxGames.Core
```

* Unity will automatically recognize this as an embedded package.
* No `.asmdef` changes needed unless you want to add dependencies.

---

### ⚙️ .NET Core

1. Add the library as a submodule:

   ```bash
   git submodule add git@github.com:SaltboxGames/SaltboxGames.Core.git SaltboxGames.Core
   ```

2. Reference it in your consuming project's `.csproj`:

   ```xml
   <ProjectReference Include="SaltboxGames.Core/SaltboxGames.Core.csproj" />
   ```

---

## 🔧 Enabling MemoryPack Support (Optional)

SaltboxGames.Core can integrate with [MemoryPack](https://github.com/Cysharp/MemoryPack) for fast, zero-allocation serialization.
This support is **opt-in** via a build flag.

---

### ✅ For .NET Core Projects

To enable MemoryPack, create a file named `Directory.Build.props` at the **root of your solution** (alongside your `.sln` file):

```xml
<Project>
  <PropertyGroup>
    <EnableMemoryPack>true</EnableMemoryPack>
  </PropertyGroup>
</Project>
```

This will:

* Define `MEMORY_PACK`
* Add the `MemoryPack` NuGet package automatically

---

### ✅ For Unity Projects

1. Install [MemoryPack-Unity](https://github.com/Cysharp/MemoryPack?tab=readme-ov-file#unity)
2. This will automatically define `MEMORY_PACK`

---

## 🧪 MemoryPack Example

```csharp
#if MEMORY_PACK
[MemoryPackable]
public partial struct MyData
{
    public int Id;
    public string Name;
}
#else
public struct MyData
{
    public int Id;
    public string Name;
}
#endif
```

## 🙌 Credits

Built with ❤️ by SaltboxGames.
Pull requests, issues, and feedback welcome!
