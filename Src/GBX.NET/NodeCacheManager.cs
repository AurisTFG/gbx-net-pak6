﻿using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace GBX.NET;

public static class NodeCacheManager
{
    private static readonly Assembly assembly = typeof(NodeCacheManager).Assembly;

    private static readonly Dictionary<uint, Type> availableClasses;
    private static readonly Dictionary<Type, List<uint>> availableInheritanceClasses;
    private static readonly Dictionary<Type, Dictionary<uint, Type>> availableChunkClasses;
    private static readonly Dictionary<Type, Dictionary<uint, Type>> availableHeaderChunkClasses;
    private static readonly Dictionary<Type, IEnumerable<Attribute>> availableClassAttributes;
    private static readonly Dictionary<Type, Dictionary<uint, IEnumerable<Attribute>>> availableChunkAttributes;
    private static readonly Dictionary<Type, IEnumerable<Attribute>> availableChunkAttributesByType;
    private static readonly Dictionary<uint, Func<CMwNod>> availableClassConstructors;
    private static readonly Dictionary<Type, Dictionary<uint, Func<Chunk>>> availableChunkConstructors;
    private static readonly Dictionary<Type, Dictionary<uint, Func<Chunk>>> availableHeaderChunkConstructors;
    private static readonly HashSet<Type> skippableChunks;

    public static bool TypesDefined { get; internal set; }

    public static Dictionary<uint, string> Names { get; }
    public static Dictionary<uint, uint> Mappings { get; } // key: older, value: newer
    public static Dictionary<uint, string> Extensions { get; }

    public static Dictionary<uint, Type> AvailableClasses
    {
        get
        {
            DefineTypes();
            return availableClasses;
        }
    }

    public static Dictionary<Type, List<uint>> AvailableInheritanceClasses
    {
        get
        {
            DefineTypes();
            return availableInheritanceClasses;
        }
    }

    public static Dictionary<Type, Dictionary<uint, Type>> AvailableChunkClasses
    {
        get
        {
            DefineTypes();
            return availableChunkClasses;
        }
    }

    public static Dictionary<Type, Dictionary<uint, Type>> AvailableHeaderChunkClasses
    {
        get
        {
            DefineTypes();
            return availableHeaderChunkClasses;
        }
    }


    public static Dictionary<Type, IEnumerable<Attribute>> AvailableClassAttributes
    {
        get
        {
            DefineTypes();
            return availableClassAttributes;
        }
    }

    public static Dictionary<Type, Dictionary<uint, IEnumerable<Attribute>>> AvailableChunkAttributes
    {
        get
        {
            DefineTypes();
            return availableChunkAttributes;
        }
    }

    public static Dictionary<Type, IEnumerable<Attribute>> AvailableChunkAttributesByType
    {
        get
        {
            DefineTypes();
            return availableChunkAttributesByType;
        }
    }

    public static Dictionary<uint, Func<CMwNod>> AvailableClassConstructors
    {
        get
        {
            DefineTypes();
            return availableClassConstructors;
        }
    }

    public static Dictionary<Type, Dictionary<uint, Func<Chunk>>> AvailableChunkConstructors
    {
        get
        {
            DefineTypes();
            return availableChunkConstructors;
        }
    }

    public static Dictionary<Type, Dictionary<uint, Func<Chunk>>> AvailableHeaderChunkConstructors
    {
        get
        {
            DefineTypes();
            return availableHeaderChunkConstructors;
        }
    }

    public static HashSet<Type> SkippableChunks
    {
        get
        {
            DefineTypes();
            return skippableChunks;
        }
    }

    static NodeCacheManager()
    {
        Names = new Dictionary<uint, string>();
        Mappings = new Dictionary<uint, uint>();
        Extensions = new Dictionary<uint, string>();

        availableClasses = new Dictionary<uint, Type>();
        availableInheritanceClasses = new Dictionary<Type, List<uint>>();
        availableChunkClasses = new Dictionary<Type, Dictionary<uint, Type>>();
        availableHeaderChunkClasses = new Dictionary<Type, Dictionary<uint, Type>>();

        availableClassAttributes = new Dictionary<Type, IEnumerable<Attribute>>();
        availableChunkAttributes = new Dictionary<Type, Dictionary<uint, IEnumerable<Attribute>>>();
        availableChunkAttributesByType = new Dictionary<Type, IEnumerable<Attribute>>();

        availableClassConstructors = new Dictionary<uint, Func<CMwNod>>();
        availableChunkConstructors = new Dictionary<Type, Dictionary<uint, Func<Chunk>>>();
        availableHeaderChunkConstructors = new Dictionary<Type, Dictionary<uint, Func<Chunk>>>();

        skippableChunks = new HashSet<Type>();

        DefineNames2(Names, Extensions);
        DefineMappings2(Mappings);
    }

    /// <summary>
    /// Gets the cached private constructor of the node. The node can potentially have null values in non-nullable properties and fields.
    /// </summary>
    /// <typeparam name="T">Type of the node to instantiate.</typeparam>
    /// <param name="classId">Class ID.</param>
    /// <returns>The instantiated node.</returns>
    /// <exception cref="NodeNotInstantiableException">Node instance cannot be created from this class ID.</exception>
    internal static T GetNodeInstance<T>(uint classId) where T : CMwNod
    {
        if (AvailableClassConstructors.TryGetValue(classId, out Func<CMwNod>? constructor))
        {
            var node = (T)constructor();
            node.SetIDAndChunks();
            return node;
        }

        throw new NodeNotInstantiableException(classId);
    }

    internal static void DefineNames(IDictionary<uint, string> names, IDictionary<uint, string> extensions)
    {
        using var reader = new StringReader(Resources.ClassID);

        var en = "";
        var engineName = "";

        while (true)
        {
            var line = reader.ReadLine();

            if (line is null)
            {
                break;
            }

            var ch = "000";

            var className = "";

            if (!line.StartsWith("  "))
            {
                en = line.Substring(0, 2);
                if (line.Length - 3 > 0) engineName = line.Substring(3);
                continue;
            }

            var cl = line.Substring(2, 3);
            if (line.Length - 6 > 0) className = line.Substring(6);

            var classIDString = $"{en}{cl}{ch}";

            var extension = default(string);

            var classNameSplit = className.Split(' ');

            if (classNameSplit.Length > 1)
            {
                className = classNameSplit[0];
                extension = classNameSplit[1];
            }

            if (uint.TryParse(classIDString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint classID))
            {
                names[classID] = engineName + "::" + className;
                if (extension != null)
                    extensions[classID] = extension;

                continue;
            }

            //Debug.WriteLine($"Invalid class ID {classIDString}, skipping");
        }

        //Debug.WriteLine("Classes named in " + watch.Elapsed.TotalMilliseconds + "ms");
    }

    internal static void DefineNames2(IDictionary<uint, string> names, IDictionary<uint, string> extensions)
    {
        var watch = Stopwatch.StartNew();

        using var reader = new StringReader(Resources.ClassID);

        var classIdSpan = new Span<char>(new[] { '0', '0', '0', '0', '0', '0', '0', '0' });
        var engineNameSpan = new ReadOnlySpan<char>();

        while (true)
        {
            var stringLine = reader.ReadLine();

            if (stringLine is null)
            {
                break;
            }

            var line = stringLine.AsSpan();

#if NET462 || NETSTANDARD2_0
            if (!line.StartsWith(new ReadOnlySpan<char>(new char[] { ' ', ' ' })))
#else
            if (!line.StartsWith("  "))
#endif
            {
                var engine = line.Slice(0, 2);
                classIdSpan[0] = engine[0];
                classIdSpan[1] = engine[1];

                if (line.Length > 3)
                {
                    engineNameSpan = line.Slice(3, line.Length - 3);
                }

                continue;
            }

            var classIdPart = line.Slice(2, 3);
            classIdSpan[2] = classIdPart[0];
            classIdSpan[3] = classIdPart[1];
            classIdSpan[4] = classIdPart[2];

            var classNameWithExtensionSpan = new ReadOnlySpan<char>();

            if (line.Length <= 6)
            {
                continue;
            }

            classNameWithExtensionSpan = line.Slice(6, line.Length - 6);
            var classNameWithExtensionSpaceIndex = classNameWithExtensionSpan.IndexOf(' ');
            var noExtension = classNameWithExtensionSpaceIndex == -1;

            var classNameSpan = noExtension
                ? classNameWithExtensionSpan
                : classNameWithExtensionSpan.Slice(0, classNameWithExtensionSpaceIndex);

            var extensionSpan = noExtension
                ? new ReadOnlySpan<char>()
                : classNameWithExtensionSpan.Slice(classNameWithExtensionSpaceIndex + 1,
                    length: classNameWithExtensionSpan.Length - classNameWithExtensionSpaceIndex - 1);

#if NET462 || NETSTANDARD2_0
            if (!uint.TryParse(classIdSpan.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint classID))
#else
            if (!uint.TryParse(classIdSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint classID))
#endif
            {
                continue;
            }

#if NET6_0_OR_GREATER
            var fullName = string.Concat(engineNameSpan, "::", classNameSpan); // .NET Core 3+
#else
            var fullNameSpan = new Span<char>(new char[engineNameSpan.Length + 2 + classNameSpan.Length]);
                
            var i = 0;
            engineNameSpan.CopyTo(fullNameSpan.Slice(i, engineNameSpan.Length));
            i += engineNameSpan.Length;

            new ReadOnlySpan<char>(new char[] { ':', ':' }).CopyTo(fullNameSpan.Slice(i, 2));
            i += 2;

            classNameSpan.CopyTo(fullNameSpan.Slice(i, classNameSpan.Length));

            var fullName = fullNameSpan.ToString();
#endif

            names.Add(classID, fullName);

            if (!extensionSpan.IsEmpty)
                extensions.Add(classID, extensionSpan.ToString());

            //Debug.WriteLine($"Invalid class ID {classIdSpan}, skipping");
        }

        //Debug.WriteLine("Classes named in " + watch.Elapsed.TotalMilliseconds + "ms");
    }

    internal static void DefineMappings(Dictionary<uint, uint> mappings)
    {
        using var reader = new StringReader(Resources.ClassIDMappings);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var valueKey = line.Split(new string[] { " -> " }, StringSplitOptions.None);

            if (valueKey.Length != 2)
                continue;

            if (!uint.TryParse(valueKey[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint key)
            || !uint.TryParse(valueKey[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
                continue;

            if (mappings.ContainsValue(key)) // Virtual Skipper solution
                mappings[mappings.FirstOrDefault(x => x.Value == key).Key] = value;
            mappings[key] = value;
        }
    }

    internal static void DefineMappings2(Dictionary<uint, uint> mappings)
    {
        using var reader = new StringReader(Resources.ClassIDMappings);

        while (true)
        {
            var stringLine = reader.ReadLine();

            if (stringLine is null)
            {
                break;
            }

            if (stringLine == "")
            {
                continue;
            }

            var line = stringLine.AsSpan();

            var from = line.Slice(0, 8);
            var to = line.Slice(12, 8);

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            if (!uint.TryParse(from, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint key)
            || !uint.TryParse(to, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
            {
                continue;
            }
#else
            if (!uint.TryParse(from.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint key)
            || !uint.TryParse(to.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
            {
                continue;
            }
#endif

            mappings.Add(key, value);
        }
    }

    internal static void DefineTypes()
    {
        DefineTypes(availableClasses, availableInheritanceClasses, availableChunkClasses,
                    availableHeaderChunkClasses, availableClassAttributes, availableChunkAttributes,
                    availableChunkAttributesByType, availableClassConstructors, availableChunkConstructors,
                    availableHeaderChunkConstructors, skippableChunks);
    }

    internal static void DefineTypes(IDictionary<uint, Type> availableClasses,
        IDictionary<Type, List<uint>> availableInheritanceClasses,
        IDictionary<Type, Dictionary<uint, Type>> availableChunkClasses,
        IDictionary<Type, Dictionary<uint, Type>> availableHeaderChunkClasses,
        IDictionary<Type, IEnumerable<Attribute>> availableClassAttributes,
        IDictionary<Type, Dictionary<uint, IEnumerable<Attribute>>> availableChunkAttributes,
        IDictionary<Type, IEnumerable<Attribute>> availableChunkAttributesByType,
        IDictionary<uint, Func<CMwNod>> availableClassConstructors,
        IDictionary<Type, Dictionary<uint, Func<Chunk>>> availableChunkConstructors,
        IDictionary<Type, Dictionary<uint, Func<Chunk>>> availableHeaderChunkConstructors,
        HashSet<Type> skippableChunks)
    {
        if (TypesDefined)
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();

        IEnumerable<Type> types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(x => x is not null)!;
        }

        var engineRelatedTypes = types.Where(IsGameBoxNode);

        var availableClassesByType = new Dictionary<Type, uint>();

        foreach (var type in engineRelatedTypes)
        {
            if (!type.IsSubclassOf(typeof(CMwNod)) && type != typeof(CMwNod)) // Engine types
                continue;

            var id = type.GetCustomAttribute<NodeAttribute>()?.ID;

            if (!id.HasValue)
                throw new Exception($"{type.Name} misses NodeAttribute.");

            availableClasses.Add(id.Value, type);
            availableClassesByType.Add(type, id.Value);
        }

        foreach (var typePair in availableClasses)
        {
            var id = typePair.Key;
            var type = typePair.Value;

            var classes = new List<uint>();

            var currentType = type.BaseType!;

            while (currentType != typeof(Node))
            {
                classes.Add(availableClassesByType[currentType]);

                currentType = currentType.BaseType!;
            }

            availableInheritanceClasses.Add(type, classes);

            var chunks = type.GetNestedTypes().Where(x => x.IsSubclassOf(typeof(Chunk)));

            var baseType = type.BaseType!;

            while (baseType!.IsSubclassOf(typeof(CMwNod)))
            {
                chunks = chunks.Concat(baseType.GetNestedTypes().Where(x => x.IsSubclassOf(typeof(Chunk))));

                baseType = baseType.BaseType;
            }

            var availableChunkClassesPerClass = new Dictionary<uint, Type>();
            var availableHeaderChunkClassesPerClass = new Dictionary<uint, Type>();

            foreach (var chunk in chunks)
            {
                var chunkAttribute = chunk.GetCustomAttribute<ChunkAttribute>();

                if (chunkAttribute == null)
                    throw new Exception($"Chunk {chunk.FullName} doesn't have ChunkAttribute.");

                if (chunk.GetInterface(nameof(IHeaderChunk)) == null)
                {
                    availableChunkClassesPerClass.Add(chunkAttribute.ID, chunk);
                }
                else
                {
                    availableHeaderChunkClassesPerClass.Add(chunkAttribute.ID, chunk);
                }

                if (chunk.BaseType?.GetGenericTypeDefinition() == typeof(SkippableChunk<>))
                {
                    skippableChunks.Add(chunk);
                }
            }

            availableChunkClasses.Add(type, availableChunkClassesPerClass);
            availableHeaderChunkClasses.Add(type, availableHeaderChunkClassesPerClass);
        }

        foreach (var idClassPair in availableClasses)
        {
            var id = idClassPair.Key;
            var classType = idClassPair.Value;

            availableClassAttributes.Add(classType, classType.GetCustomAttributes());
        }

        foreach (var classChunksPair in availableHeaderChunkClasses.Concat(availableChunkClasses))
        {
            var attributeDictionary = new Dictionary<uint, IEnumerable<Attribute>>();

            foreach (var chunkClassIdTypePair in classChunksPair.Value)
            {
                var id = chunkClassIdTypePair.Key;
                var chunkClass = chunkClassIdTypePair.Value;

                var attributes = chunkClass.GetCustomAttributes();

                attributeDictionary.Add(id, attributes);

                availableChunkAttributesByType[chunkClass] = attributes; // some duplicates
            }

            availableChunkAttributes[classChunksPair.Key] = attributeDictionary; // some duplicates
        }

        foreach (var idClassPair in availableClasses)
        {
            var id = idClassPair.Key;
            var classType = idClassPair.Value;

            var privateConstructor = classType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);

            if (privateConstructor is null)
                throw new PrivateConstructorNotFoundException(classType);

            if (classType.IsAbstract)
                continue;

            var newExp = Expression.New(privateConstructor);
            var lambda = Expression.Lambda<Func<CMwNod>>(newExp);
            var compiled = lambda.Compile();

            availableClassConstructors.Add(id, compiled);
        }

        foreach (var classChunksPair in availableChunkClasses)
        {
            var constructors = GetChunkConstructors(classChunksPair);
            if (constructors is not null)
                availableChunkConstructors.Add(classChunksPair.Key, constructors);
        }

        foreach (var classChunksPair in availableHeaderChunkClasses)
        {
            var constructors = GetChunkConstructors(classChunksPair);
            if (constructors is not null)
                availableHeaderChunkConstructors.Add(classChunksPair.Key, constructors);
        }

        TypesDefined = true;
    }

    private static bool IsGameBoxNode(Type t)
    {
        return t?.IsClass == true && t.Namespace?.StartsWith("GBX.NET.Engines") == true;
    }

    private static Dictionary<uint, Func<Chunk>>? GetChunkConstructors(
        KeyValuePair<Type, Dictionary<uint, Type>> classChunksPair)
    {
        if (classChunksPair.Value.Count == 0) return null;

        var constructorDictionary = new Dictionary<uint, Func<Chunk>>();

        foreach (var chunkClassIdTypePair in classChunksPair.Value)
        {
            var id = chunkClassIdTypePair.Key;
            var chunkClass = chunkClassIdTypePair.Value;

            var newExp = Expression.New(chunkClass);
            var lambda = Expression.Lambda<Func<Chunk>>(newExp);
            var compiled = lambda.Compile();

            constructorDictionary.Add(id, compiled);
        }

        return constructorDictionary;
    }
}
