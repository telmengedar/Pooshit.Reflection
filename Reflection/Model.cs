using System;
using System.Collections.Generic;
using System.Linq;

namespace Pooshit.Reflection;

/// <summary>
/// reflection model for a type
/// </summary>
public class Model {
    readonly Dictionary<string, IPropertyInfo> propertyLookup = new();
    readonly object accessLock = new();
    
    /// <summary>
    /// type for which model information was collected
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// properties of model
    /// </summary>
    public IPropertyInfo[] Properties { get; set; }

    /// <summary>
    /// get a property from the model
    /// </summary>
    /// <param name="propertyName">name of property to get</param>
    /// <param name="ignoreCase">determines whether to ignore casing when comparing property names</param>
    /// <returns>property info for property with the specified name</returns>
    /// <exception cref="KeyNotFoundException">a property with the specified name was not found in the model</exception>
    public IPropertyInfo GetProperty(string propertyName, bool ignoreCase = false) {
        if (propertyLookup.TryGetValue(propertyName, out IPropertyInfo property))
            return property;

        property = Properties.FirstOrDefault(p => string.Compare(p.Name, propertyName, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) == 0);

        if (property == null)
            throw new KeyNotFoundException($"Property '{propertyName}' was not found in type '{Type}'");

        lock (accessLock)
            propertyLookup[propertyName] = property;
        return property;
    }
}