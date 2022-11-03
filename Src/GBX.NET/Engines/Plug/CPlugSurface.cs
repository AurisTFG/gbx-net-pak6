﻿namespace GBX.NET.Engines.Plug;

/// <remarks>ID: 0x0900C000</remarks>
[Node(0x0900C000)]
[NodeExtension("Shape")]
public class CPlugSurface : CPlug
{
    private CPlugSurfaceGeom? geom;
    private SurfMaterial[]? materials;
    private ISurf? surf;
    private CPlugSkel? skel;

    [NodeMember]
    [AppliedWithChunk(typeof(Chunk0900C000))]
    public CPlugSurfaceGeom? Geom { get => geom; set => geom = value; }

    [NodeMember]
    [AppliedWithChunk(typeof(Chunk0900C000))]
    public SurfMaterial[]? Materials { get => materials; set => materials = value; }

    [NodeMember(ExactName = "m_GmSurf")]
    [AppliedWithChunk(typeof(Chunk0900C003))]
    public ISurf? Surf { get => surf; set => surf = value; }

    [NodeMember(ExactlyNamed = true)]
    [AppliedWithChunk(typeof(Chunk0900C003), sinceVersion: 1)]
    public CPlugSkel? Skel { get => skel; set => skel = value; }

    internal CPlugSurface()
    {

    }

    private static void ArchiveSurf(ref ISurf? surf, GameBoxReaderWriter rw)
    {
        // 0 - Sphere
        // 1 - Ellipsoid
        // 6 - Box (Primitive)
        // 7 - Mesh
        // 8 - VCylinder (Primitive)
        // 9 - MultiSphere (Primitive)
        // 10 - ConvexPolyhedron
        // 11 - Capsule (Primitive)
        // 12 - Circle (Non3d)
        // 13 - Compound
        // 14 - SphereLocated (Primitive)
        // 15 - CompoundInstance
        // 16 - Cylinder (Primitive)
        // 17 - SphericalShell
        var surfId = rw.Int32(surf?.Id, defaultValue: -1);

        surf = surfId switch // ArchiveGmSurf
        {
            7 => rw.Archive(surf as Mesh), // Mesh
            13 => rw.Archive(surf as Compound), // Compound
            -1 => null,
            _ => throw new NotSupportedException("Unknown surf type: " + surfId)
        };
    }

    /// <summary>
    /// CPlugSurface 0x000 chunk
    /// </summary>
    [Chunk(0x0900C000)]
    public class Chunk0900C000 : Chunk<CPlugSurface>
    {
        public string? U01;

        public override void ReadWrite(CPlugSurface n, GameBoxReaderWriter rw)
        {
            if (n is CPlugSurfaceGeom)
            {
                rw.Id(ref U01);
                return;
            }

            rw.NodeRef(ref n.geom);

            rw.ArrayArchive<SurfMaterial>(n.materials);
        }
    }

    /// <summary>
    /// CPlugSurface 0x001 chunk
    /// </summary>
    [Chunk(0x0900C001)]
    public class Chunk0900C001 : Chunk<CPlugSurface>
    {
        public bool U01;

        public override void ReadWrite(CPlugSurface n, GameBoxReaderWriter rw)
        {
            rw.Boolean(ref U01);
        }
    }

    /// <summary>
    /// CPlugSurface 0x003 chunk
    /// </summary>
    [Chunk(0x0900C003)]
    public class Chunk0900C003 : Chunk<CPlugSurface>, IVersionable
    {
        private int version;

        public int U01;
        public byte[]? U02;

        public int U06;

        public int Version { get => version; set => version = value; }

        public override void ReadWrite(CPlugSurface n, GameBoxReaderWriter rw)
        {
            rw.Int32(ref version);

            if (version >= 2)
            {
                rw.Int32(ref U01);
            }

            ArchiveSurf(ref n.surf, rw);

            rw.ArrayArchive<SurfMaterial>(ref n.materials); // ArchiveMaterials
            rw.Bytes(ref U02);

            if (version >= 1)
            {
                rw.NodeRef<CPlugSkel>(ref n.skel);
            }
        }
    }

    public class SurfMaterial : IReadableWritable
    {
        private CPlugMaterial? material;
        private ushort? surfaceId;

        public CPlugMaterial? Mat { get => material; set => material = value; }
        public ushort? SurfaceId { get => surfaceId; set => surfaceId = value; }

        public void ReadWrite(GameBoxReaderWriter rw, int version = 0)
        {
            if (rw.Boolean(material is not null))
            {
                rw.NodeRef(ref material);
            }
            else
            {
                rw.UInt16(ref surfaceId);
            }
        }
    }

    public interface ISurf : IReadableWritable
    {
        int Id { get; }
    }

    public class Mesh : ISurf
    {
        public int Id => 7;

        private int v;
        private Vec3[] vertices = Array.Empty<Vec3>();
        private (Int3, int)[] triangles = Array.Empty<(Int3, int)>();
        private (Vec4, Int3, ushort, byte, byte)[] cookedTriangles = Array.Empty<(Vec4, Int3, ushort, byte, byte)>();
        private AABBTreeCell[] aABBTree = Array.Empty<AABBTreeCell>();

        public int Version { get => v; set => v = value; }
        public Vec3[] Vertices { get => vertices; set => vertices = value; }
        public (Int3, int)[] Triangles { get => triangles; set => triangles = value; }
        public AABBTreeCell[] AABBTree { get => aABBTree; set => aABBTree = value; }

        public void ReadWrite(GameBoxReaderWriter rw, int version = 0)
        {
            rw.Int32(ref v);

            switch (v)
            {
                case 5:
                    rw.Array<Vec3>(ref vertices!);
                    rw.Array(ref cookedTriangles!, // GmSurfMeshCookedTri
                        r => (r.ReadVec4(),
                            r.ReadInt3(),
                            r.ReadUInt16(),
                            r.ReadByte(),
                            r.ReadByte()),
                        (x, w) =>
                        {
                            w.Write(x.Item1);
                            w.Write(x.Item2);
                            w.Write(x.Item3);
                            w.Write(x.Item4);
                            w.Write(x.Item5);
                        });
                    rw.Int32(1);
                    rw.ArrayArchive<AABBTreeCell>(ref aABBTree!);
                    break;
                case 6:
                    rw.Array<Vec3>(ref vertices!);
                    rw.Array<(Int3, int)>(ref triangles!, // GmSurfMeshTri
                        r => (r.ReadInt3(), r.ReadInt32()),
                        (x, w) => { w.Write(x.Item1); w.Write(x.Item2); });
                    break;
                default:
                    throw new VersionNotSupportedException(v);
            }
        }

        public class AABBTreeCell : IReadableWritable
        {
            public Vec3 U01;
            public Vec3 U02;
            public int U03;

            public void ReadWrite(GameBoxReaderWriter rw, int version = 0)
            {
                rw.Vec3(ref U01);
                rw.Vec3(ref U02);
                rw.Int32(ref U03);
            }
        }
    }

    public class Compound : ISurf
    {
        private ISurf[] surfaces = Array.Empty<ISurf>();
        private Iso4[] u01 = Array.Empty<Iso4>();
        private ushort[] u02 = Array.Empty<ushort>();

        public int Id => 13;

        public ISurf[] Surfaces { get => surfaces; set => surfaces = value; }
        public Iso4[] U01 { get => u01; set => u01 = value; }
        public ushort[] U02 { get => u02; set => u02 = value; }

        public void ReadWrite(GameBoxReaderWriter rw, int version = 0)
        {
            var length = rw.Reader?.ReadInt32() ?? surfaces?.Length ?? 0;
            rw.Writer?.Write(length);

            if (rw.Reader is not null)
            {
                surfaces = new ISurf[length];
            }

            if (surfaces is not null)
            {
                for (int i = 0; i < length; i++)
                {
                    ArchiveSurf(ref surfaces[i]!, rw);
                }
            }

            rw.Array<Iso4>(ref u01!, length);

            //if (version >= 1) // Pass version?
            //{
                rw.Array<ushort>(ref u02!);
            //}
        }
    }
}