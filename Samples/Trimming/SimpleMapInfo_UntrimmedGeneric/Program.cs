﻿using GBX.NET;
using GBX.NET.Engines.Game;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run <path to map>");
    return;
}

var map = Gbx.ParseHeaderNode<CGameCtnChallenge>(args[0]);

Console.WriteLine($"Map name: {map.MapName}");