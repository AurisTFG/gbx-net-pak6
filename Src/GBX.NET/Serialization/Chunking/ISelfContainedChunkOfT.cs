﻿namespace GBX.NET.Serialization.Chunking;

/// <summary>
/// Chunk that has a completely different node data stored inside.
/// </summary>
/// <typeparam name="T">A Gbx class.</typeparam>
public interface ISelfContainedChunk<T> : ISelfContainedChunk, IChunk<T> where T : IClass
{
    new T Node { get; set; }
}
