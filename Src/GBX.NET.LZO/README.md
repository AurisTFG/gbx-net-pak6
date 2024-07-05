# GBX.NET.LZO

[![NuGet](https://img.shields.io/nuget/vpre/GBX.NET.LZO?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/GBX.NET.LZO/)
[![Discord](https://img.shields.io/discord/1012862402611642448?style=for-the-badge&logo=discord)](https://discord.gg/tECTQcAWC9)

An LZO compression plugin for GBX.NET to allow de/serialization of compressed Gbx bodies. This official implementation uses lzo 2.10 from NativeSharpLzo and minilzo 2.06 port by zzattack.

The compression logic is split up from the read/write logic to **allow GBX.NET 2 library to be distributed under the MIT license**, as Oberhumer distributes the open source version of LZO under the GNU GPL v3. Therefore, using GBX.NET.LZO 2 requires you to license your project under the GNU GPL v3, see [License](#license).

**Gbx header is not compressed** and can contain useful information (icon data, replay time, ...), and also many of the **internal Gbx files from Pak files are not compressed**, so you can avoid LZO for these purposes.

## Usage

At the beginning of your program execution, you add the `Gbx.LZO = new Lzo();` to prepare the LZO compression. It should be run **only once**.

The `Lzo` implementation uses `999` compression, which is slightly more efficient than the compression used by Nadeo games.

> This project example expects you to have `<ImplicitUsings>enable</ImplicitUsings>`. If this does not work for you, add `using System.Linq;`.

```cs
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO; // Add this

Gbx.LZO = new Lzo(); // Add this ONLY ONCE and before you start using Parse methods

var map = Gbx.ParseNode<CGameCtnChallenge>("Path/To/My.Map.Gbx");

Console.WriteLine($"Block count: {map.GetBlocks().Count()}");
```

You should not get the LZO exception anymore when you read a compressed Gbx file.

### MiniLZO

If you want to use the port of minilzo 2.06, just use the `MiniLZO` class. MiniLZO uses `1` compression which is less efficient than the one used by Nadeo games.

```cs
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO(); 
```


## License

GBX.NET.LZO library is GNU GPL v3 Licensed.

If you use the LZO compression library, you must license your project under the GNU GPL v3.