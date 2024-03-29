﻿namespace GBX.NET.Serialization;

public interface IReadable<T> where T : IClass
{
    void Read(GbxReader r, T n, int version = 0);
}
