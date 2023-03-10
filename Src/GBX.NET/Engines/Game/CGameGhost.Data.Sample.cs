﻿namespace GBX.NET.Engines.Game;

public partial class CGameGhost
{
    public partial class Data
    {
        public class Sample
        {
            public TimeInt32 Time { get; }
            public byte[] Data { get; }

            public Vec3 Position { get; set; }
            public Quat Rotation { get; set; }
            public float Speed => Velocity.GetMagnitude();
            public Vec3 Velocity { get; set; }

            internal Sample(TimeInt32 time, byte[] data)
            {
                Time = time;
                Data = data;
            }

            public override string ToString()
            {
                return $"{Time}, {Data.Length} bytes";
            }

            public virtual void Read(MemoryStream ms, GameBoxReader r, int version)
            {

            }
        }
    }
}
