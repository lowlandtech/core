﻿namespace LowlandTech.Core.Attributes;

/// <summary>
/// Sets the identifier for an entity.
/// </summary>
/// <param name="id"></param>
[AttributeUsage(AttributeTargets.Property)]
public class DbSetIdAttribute(string id) : Attribute
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public string Id => id;
}
