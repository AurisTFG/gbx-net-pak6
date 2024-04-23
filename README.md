![GBX.NET](logo_outline.png)

[![NuGet](https://img.shields.io/nuget/vpre/GBX.NET?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/GBX.NET/)
[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112/gbx-net?include_prereleases&style=for-the-badge&logo=github)](https://github.com/BigBang1112/gbx-net/releases)
[![GitHub last commit (branch)](https://img.shields.io/github/last-commit/bigbang1112/gbx-net/v2?style=for-the-badge&logo=github)](#)
[![Discord](https://img.shields.io/discord/1012862402611642448?style=for-the-badge&logo=discord)](https://discord.gg/tECTQcAWC9)

Welcome to GBX.NET 2!

A general purpose library for Gbx files - data from Nadeo games like Trackmania or Shootmania, written in C#/.NET. It supports high performance serialization and deserialization of 200+ Gbx classes.

GBX.NET is not just a library, but essentially **a modding platform** that connects all Nadeo games together. It is also exceptionally useful for bulk Gbx data processing, for example, when you want to fix broken sign locator URLs on hundreds of maps.

### Navigation

- [Supported games](#supported-games)
- [File types](#file-types)
- [Gbx Explorer 2](#gbx-explorer-2)
- [Lua support](#lua-support)
- [Framework support](#framework-support)
- **[Preparation](#preparation)**
  - [Create a new GBX.NET project (lightweight)](#create-a-new-gbxnet-project-lightweight-soon)
  - [Create a new GBX.NET project (Visual Studio Code)](#create-a-new-gbxnet-project-visual-studio-code)
  - [Create a new GBX.NET project (Visual Studio)](#create-a-new-gbxnet-project-visual-studio)
- **[Usage (simple examples)](#usage-simple-examples)**
  - [Load a map and display block count per block name](#load-a-map-and-display-block-count-per-block-name)
  - [Modify and save a map](#modify-and-save-a-map)
  - [Processing multiple Gbx types](#processing-multiple-gbx-types)
  - [Read a large amount of replay metadata quickly](#read-a-large-amount-of-replay-metadata-quickly)
- [Clarity](#clarity)
  - [Differences between `Gbx.Parse/Header/Node`](#differences-between-gbxparseheadernode)
  - [Do not repeat `gbx.Node.[any]` too often!](#do-not-repeat-gbxnodeany-too-often)
  - [Game Version Interfaces!](#game-version-interfaces)
- [Optimization](#optimization)
  - [Trimming (tree shaking)](#trimming-tree-shaking)
  - [Explicit vs. Implicit parse](#explicit-vs-implicit-parse)
  - [Only header parse](#only-header-parse)
  - [NativeAOT](#nativeaot)
- [Benchmarks](#benchmarks)
- [Build](#build)
- [License](#license)
- [Special thanks](#special-thanks)
- [Alternative Gbx parsers](#alternative-gbx-parsers)

## Supported games

Many *essential* Gbx files from many games are supported:

- **Trackmania (2020)**, April 2024 update
- **ManiaPlanet 4**(.1), TM2/SM
- **Trackmania Turbo**
- ManiaPlanet 3, TM2/SM
- ManiaPlanet 2, TM2/SM
- ManiaPlanet 1, TM2
- **TrackMania Forever**, Nations/United
- Virtual Skipper 5
- TrackMania United
- **TrackMania Nations ESWC**
- **TrackMania Sunrise eXtreme**
- TrackMania Original
- TrackMania Sunrise
- TrackMania Power Up
- TrackMania (1.0)

## File types

Here are some of the known file types to start with:

| Latest extension | Class | Can read | Can write | Other extension/s
| --- | --- | --- | --- | ---
| Map.Gbx | [CGameCtnChallenge](Src/GBX.NET/Engines/Game/CGameCtnChallenge.chunkl) | Yes | Yes | Challenge.Gbx
| Replay.Gbx | [CGameCtnReplayRecord](Src/GBX.NET/Engines/Game/CGameCtnReplayRecord.chunkl) | Yes | No
| Ghost.Gbx | [CGameCtnGhost](Src/GBX.NET/Engines/Game/CGameCtnGhost.chunkl) | Yes | Yes
| Clip.Gbx | [CGameCtnMediaClip](Src/GBX.NET/Engines/Game/CGameCtnMediaClip.chunkl) | Yes | Yes
| Item.Gbx | [CGameItemModel](Src/GBX.NET/Engines/GameData/CGameItemModel.chunkl) | Yes | Yes | Block.Gbx
| Mat.Gbx | [CPlugMaterialUserInst](Src/GBX.NET/Engines/Plug/CPlugMaterialUserInst.chunkl) | Yes | Yes
| Mesh.Gbx | [CPlugSolid2Model](Src/GBX.NET/Engines/Plug/CPlugSolid2Model.chunkl) | Yes | Yes
| Shape.Gbx | [CPlugSurface](Src/GBX.NET/Engines/Plug/CPlugSurface.chunkl) | Yes | Yes
| Macroblock.Gbx | [CGameCtnMacroBlockInfo](Src/GBX.NET/Engines/Game/CGameCtnMacroBlockInfo.chunkl) | Yes | Yes
| LightMapCache.Gbx | [CHmsLightMapCache](Src/GBX.NET/Engines/Hms/CHmsLightMapCache.chunkl) | No | No
| SystemConfig.Gbx | [CSystemConfig](Src/GBX.NET/Engines/System/CSystemConfig.chunkl) | Yes | Yes
| FidCache.Gbx | [CMwRefBuffer](Src/GBX.NET/Engines/MwFoundations/CMwRefBuffer.chunkl) | Yes | Yes
| Scores.Gbx | [CGamePlayerScore](Src/GBX.NET/Engines/Game/CGamePlayerScore.chunkl) | Yes | No

**Full list of supported file types is available in the [SUPPORTED GBX FILE TYPES](SUPPORTED_GBX_FILE_TYPES.md)**.

## Gbx Explorer 2

TODO

## Lua support

GBX.NET 2 *will* support dynamic Lua script execution around the .NET code, to simplify the library usage even further.

It should be supported to run offline locally through a **Gbx Lua Interpreter tool** and in **Gbx Explorer 2 in web browser, PWA, and Photino build.**

The goal is to also make it viable for NativeAOT.

## Framework support

Due to the recently paced evolution of .NET, framework support has been limited only to a few ones compared to GBX.NET 1:

- .NET 8
- .NET 6
- .NET Standard 2.0

You can still use GBX.NET 2 on the old .NET Framework, but the performance of the library could be degraded.

## Preparation

Using the NuGet packages is recommended.

### Create a new GBX.NET project (lightweight)

1. Install .NET SDK 8.
    - Windows: `winget install Microsoft.DotNet.SDK.8` (make sure you have WinGet installed)
    - [Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux) (just SDK)
2. Create directory for your project (anywhere), **go inside it**.
3. Create new console project: `dotnet new console`
4. Add the pre-release GBX.NET 2 NuGet package: `dotnet add package GBX.NET --prerelease`
5. *(optional)* Add the pre-release GBX.NET.LZO 2 NuGet package: `dotnet add package GBX.NET.LZO --prerelease`
6. Open `Program.cs` with your favorite text editor: `code . -g Program.cs` (for example)
7. Write code - see [Examples (simple)](#examples-simple).
8. Use `dotnet run` to run the app.

Steps 2-8:
```
mkdir MyGbxProject
cd MyGbxProject
dotnet new console
dotnet add package GBX.NET --prerelease
dotnet add package GBX.NET.LZO --prerelease
code . -g Program.cs
dotnet run
```

### Create a new GBX.NET project (Visual Studio Code)

1. Install C# Dev Kit extension.
2. Click on `Create .NET Project` button, or press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd>, type `.NET: New Project`.
3. Select `Console App` and create your project.
4. Open a new terminal and type `dotnet add package GBX.NET --prerelease` to add GBX.NET 2.
5. *(optional)* Add the GBX.NET.LZO 2 NuGet package: `dotnet add package GBX.NET.LZO --prerelease`
5. Write code - see [Examples (simple)](#examples-simple).
6. Run and debug as usual, select C# if prompted.

### Create a new GBX.NET project (Visual Studio)

1. Create a new Console project
2. Under your project in Solution Explorer, right-click on Dependencies and select `Manage NuGet packages...`
3. Search `GBX.NET` and click install
4. Write code - see [Examples (simple)](#examples-simple).

## IMPORTANT INFO about the LZO and Gbx compression

Reading or writing compressed Gbx files **require** to include the GBX.NET.LZO 2 library (or any other implementation that uses the `ILzo` interface). This is how you can include it:

Command line:

```
dotnet add package GBX.NET.LZO --prerelease
```

C# code:

```cs
using GBX.NET;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();
```

You should run this line of code **only once** for the whole program lifetime.

The compression logic is split up from the read/write logic to **allow GBX.NET 2 library to be distributed under the MIT license**, as Oberhumer distributes the open source version of LZO under the GNU GPL v3. Therefore, using GBX.NET.LZO 2 requires you to license your project under the GNU GPL v3, see [License](#license).

**Gbx header is not compressed** and can contain useful information (icon data, replay time, ...), and also many of the **internal Gbx files from Pak files are not compressed**, so you can avoid LZO for these purposes.

## Usage (simple examples)

### Load a map and display block count per block name

Required packages: `GBX.NET`, `GBX.NET.LZO`

> This project example expects you to have `<ImplicitUsings>enable</ImplicitUsings>`. If this does not work for you, add `using System.Linq;`.

```cs
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();

var map = Gbx.ParseNode<CGameCtnChallenge>("Path/To/My.Map.Gbx");

foreach (var block in map.GetBlocks().GroupBy(x => x.Name))
{
    Console.WriteLine($"{block.Key}: {block.Count()}");
}
```

This will print out all blocks on the map and their count. This code can potentially crash for at least 3 reasons:

1. The Gbx file is **not a map**. See [Explicit vs. Implicit parse](#explicit-vs-implicit-parse) in the [Optimization](#optimization) part.
2. There's **a Gbx exception**. See *Exceptions in GBX.NET 2* (TBD).
3. There's a file system problem.

### Modify and save a map

Required packages: `GBX.NET`, `GBX.NET.LZO`

GBX.NET's strength is in its ability to modify Gbx files and save them back. This example shows how you can change the map name of a map:

```cs
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();

var gbx = Gbx.Parse<CGameCtnChallenge>("Path/To/My.Map.Gbx");
var map = gbx.Node; // See Clarity section for more info

map.MapName = "My new map name";

gbx.Save("Path/To/MyNew.Map.Gbx");
```

The trick here is that the Gbx properties are saved in the `gbx` object variable (`Gbx` class). These properties ensure that the Gbx file is saved correctly across all Trackmania versions.

If you were to go with `ParseNode` in this case, this would **not work for TMF and older games**, but it is still possible if you specify the Gbx parameters in the `Save` method:

```cs
map.Save("Path/To/MyNew.Map.Gbx", new()
{
    PackDescVersion = 2 // Latest known PackDesc version in TMF
});
```

For TMS or TMN ESWC, you would have to specify `ClassIdRemapMode` for example:

```cs
map.Save("Path/To/MyNew.Map.Gbx", new()
{
    ClassIdRemapMode = ClassIdRemapMode.Id2006
    PackDescVersion = 1
});
```

These save parameters depend on the game of choice, but **since Trackmania 2, this does not matter** and you can safely use `Save` method on `CMwNod` without additional settings.

For more details, see [Differences between `Gbx.Parse/Header/Node`](#differences-between-gbxparseheadernode) in the [Clarity](#clarity) section.

### Processing multiple Gbx types

Required packages: `GBX.NET`, `GBX.NET.LZO`

> This project example expects you to have `<ImplicitUsings>enable</ImplicitUsings>`. If this does not work for you, add `using System.Linq;`.

This example shows how you can retrieve ghost objects from multiple different types of Gbx:

```cs
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();

var node = Gbx.ParseNode("Path/To/My.Gbx");

var ghost = node switch
{
    CGameCtnReplayRecord replay => replay.GetGhosts().FirstOrDefault(),
    CGameCtnMediaClip clip => clip.GetGhosts().FirstOrDefault(),
    CGameCtnGhost g => g,
    _ => null
};

if (ghost is null)
{
    Console.WriteLine("This Gbx file does not have any ghost.");
}
else
{
    Console.WriteLine("Time: {0}", ghost.RaceTime);
}
```

Using pattern matching with non-generic Parse methods is a safer approach (no exceptions on different Gbx types), but less trim-friendly, see [Explicit vs. Implicit parse](#explicit-vs-implicit-parse) in the [Optimization](#optimization) section.

### Read a large amount of replay metadata quickly

Required packages: `GBX.NET`

In case you only need the most basic information about many of the most common Gbx files (maps, replays, items, ...), do not read the full Gbx file, but only the header part. It is a great performance benefit for disk scans.

> [!NOTE]
> Reading only the header also does not infect you with GNU GPL v3 and you can use licenses compatible with MIT. Header is not compressed with LZO.

> This project example expects you to have `<ImplicitUsings>enable</ImplicitUsings>`. If this does not work for you, add `using System.IO;`.

```cs
using GBX.NET;
using GBX.NET.Engines.Game;

foreach (var filePath in Directory.EnumerateFiles("Path/To/My/Directory", "*.Replay.Gbx", SearchOption.AllDirectories))
{
    try
    {
        DisplayBasicReplayInfo(filePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Gbx exception occurred {Path.GetFileName(filePath)}: {ex}");
    }
}

void DisplayBasicReplayInfo(string filePath)
{
    var nodeHeader = Gbx.ParseHeaderNode(filePath);

    if (nodeHeader is CGameCtnReplayRecord replay)
    {
        Console.WriteLine($"{replay.MapInfo}: {replay.Time}");
    }
}
```

This code should only crash in case of a file system problem. Other problems will be printed out in the console.

> [!NOTE]
> It is still valuable to parse the full Gbx even when you just want a piece of information available in header, because **body info overwrites header info**. So you can use the benefit of full parse to fool people tackling with the Gbx header.

## Clarity

This section describes best practices to keep your projects clean when using GBX.NET 2.

### Differences between `Gbx.Parse/Header/Node`

Gbx files contain many different parameters that are not exactly part of the game objects. We commonly use `ParseNode` or `ParseHeaderNode` to simplify the access level, as Gbx parameters are usually unnecessary to know about, but they have to be present to ensure consistent serialization.

You can still save nodes into Gbx files by using the `Save` method - be careful specifying the Gbx parameters correctly, like the class ID mappings (wrap/unwrap).

- `Gbx.Parse`
  - TODO
- `Gbx.ParseNode`
  - TODO
- `Gbx.ParseHeader`
  - TODO
- `Gbx.ParseHeaderNode`
  - TODO

### Do not repeat `gbx.Node.[any]` too often!

This was more common back in the 0.X version days, but it is still possible to do today.

TODO

### Game Version Interfaces!

Interfaces (short name of Game Version Interfaces) is a new feature of GBX.NET 2 where you can scope the Gbx classes for specific Trackmania/Shootmania games to hide unrelated properties and avoid large amount of null checks. These null checks will be done for you behind the scenes and will throw exceptions if they are "exceptional" for the game version you pick.

TODO

## Optimization

GBX.NET 2 introduced rich optimization techniques to improve both performance and compiled size of your applications.

The goal of these optimizations is to prove that GBX.NET is not "too big" for anything small.

### Trimming (tree shaking)

On *publish* (the final build), you can trim out unused code by using this property in `.csproj`:

```xml
<PropertyGroup>
    <PublishTrimmed>true</PublishTrimmed>
</PropertyGroup>
```

The library does not load anything dynamically and does not use reflection, so this is **fully supported**.

GBX.NET is a huge library when everything is included (over 1.5MB), so please use this whenever it's possible. Code was written to be very trimmable, so the difference is huge (much bigger than in GBX.NET v1).

> [!NOTE]
> Expect this to work only with `dotnet publish`.

### Explicit vs. Implicit parse

TODO

### Only header parse

As mentioned earlier, you can largely speed up Gbx reading in case your information is available in the header part of Gbx.

However, if the information is something serious, you should still *validate it against the body*, in other words: read the full Gbx, which this process will use the information from the body instead, and overwrite what was read in the header.

### NativeAOT

GBX.NET **fully supports** NativeAOT, and it is highly recommended to use its potential on smaller-sized applications:

```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

It also automatically trims the application (no need for `<PublishTrimmed>true</PublishTrimmed>`).

On basic GBX.NET applications, native compilation has a couple of improvements:
- Reduces trimmed standalone binary size from ~7MB to 2.8MB.
- Startup time is reduced from 50ms to 0.5ms (JIT is removed, so you should be only bottlenecked by disk speed).
- If you're using only the MIT License libraries, you can make your project harder to reverse engineer.
- The app feels generally lighter, but can be slightly slower for long-running process than a runtime app with JIT (very small difference).

> [!NOTE]
> Expect this to work only with `dotnet publish`.

## Benchmarks

TODO

## Build

> [!NOTE]
> You don't need to build the solution/repository to use GBX.NET, **NuGet packages have been made for you**. This is only for internal development purposes.

> The solution has more than 30 projects for different purposes, make sure you have enough disk space, the full build could take up a couple hundreds of megabytes.

Make sure you have these framework SDKs available:

- .NET 8
- .NET 6
- .NET Standard 2.0

**Visual Studio 2022** should be able to install those with default installation settings. Using Visual Studio 2019 will likely not work.

You should also have **.NET WebAssembly Build Tools** installed additionally to build the full solution. It may not be required, but it will definitely help figuring out some problems.

In Visual Studio, you can just use Build Solution and everything should build. JetBrains Rider has been tested and also works.

In .NET CLI, run `dotnet build` on the solution (`.sln`) level.

## License

GBX.NET 2 is licensed under multiple licenses, depending on the part of the project. Here are the licenses and their directories:

- MIT License
  - **Src/GBX.NET**
  - Src/GBX.NET.Crypto
  - Src/GBX.NET.Hashing
  - Src/GBX.NET.Json
  - Src/GBX.NET.Lua
  - Generators
- GNU GPL v3 License
  - **Src/GBX.NET.LZO**
  - Samples
  - Tools
- The Unlicense
  - Resources

The Unlicense also applies on information gathered from the project (chunk structure, parse examples, data structure, wiki information, markdown).

If you use the LZO compression library, you must license your project under the GNU GPL v3.

## Special thanks

Without these people, this project wouldn't be what it is today (ordered by impact):

- Stefan Baumann (Solux)
- Melissa (Miss)
- florenzius
- Kim
- tilman
- schadocalex
- James Romeril
- frolad (Juice)
- Mika Kuijpers (TheMrMiku)
- donadigo

And many thanks to every bug reporter!

## Alternative Gbx parsers

- [gbx-py](https://github.com/schadocalex/gbx-py) by schadocalex (advanced read+write Gbx parser specialized on TM2020 and custom items)
- [gbx-ts](https://github.com/thaumictom/gbx-ts) by thaumictom (read-only Gbx parser for TypeScript)
- [ManiaPlanetSharp](https://github.com/stefan-baumann/ManiaPlanetSharp) by Solux (C# toolkit for accessing ManiaPlanet data, including read-only Gbx parser used by ManiaExchange)
- [pygbx](https://github.com/donadigo/pygbx) by Donadigo (read-only Gbx parser for Python)
