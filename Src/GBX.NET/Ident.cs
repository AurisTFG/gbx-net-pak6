﻿namespace GBX.NET;

/// <summary>
/// [SGameCtnIdentifier] Identifier defined by ID, collection and author. Also known as "meta".
/// </summary>
public sealed record Ident(string Id, Id Collection, string Author)
{
    /// <summary>
    /// An empty <see cref="Ident"/> with <see cref="string.Empty"/> values.
    /// </summary>
    public static readonly Ident Empty = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Ident"/> with the specified identifier and default values for collection and author names.
    /// </summary>
    /// <param name="id">The identifier string.</param>
    public Ident(string id) : this(id, new Id(), string.Empty) { }

    public Ident(string id, int collection, string author) : this(id, new Id(collection), author) { }

    public Ident(string id, string collection, string author) : this(id, new Id(collection), author) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ident"/> with default values.
    /// </summary>
    public Ident() : this(string.Empty) { }

    /// <summary>
    /// Returns a string representation of the <see cref="Ident"/>.
    /// </summary>
    /// <returns>A string representation of the form '("{Id}", {Collection}, "{Author}")'.</returns>
    public override string ToString()
    {
        return $"""("{Id}", "{Collection}", "{Author}")""";
    }

    /// <summary>
    /// Implicitly converts a tuple of string, <see cref="NET.Id"/>, and string to an <see cref="Ident"/>.
    /// </summary>
    /// <param name="v">The tuple containing Id, Collection, and Author components.</param>
    public static implicit operator Ident((string Id, Id Collection, string Author) v)
    {
        return new(v.Id, v.Collection, v.Author);
    }
}
