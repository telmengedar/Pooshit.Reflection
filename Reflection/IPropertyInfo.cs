using System;

namespace Pooshit.Reflection;

public interface IPropertyInfo
{
    
    /// <summary>
    /// name of property
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// attributes of property
    /// </summary>
    Attribute[] Attributes { get; }
    
    /// <summary>
    /// type of property
    /// </summary>
    Type PropertyType { get; }

    /// <summary>
    /// get the property value
    /// </summary>
    /// <param name="instance">property host</param>
    /// <returns>value of property</returns>
    object GetValue(object instance);

    /// <summary>
    /// set the property value
    /// </summary>
    /// <param name="instance">property host</param>
    /// <param name="value">value to set</param>
    void SetValue(object instance, object value);

    /// <summary>
    /// determines whether the property has a getter method defined
    /// </summary>
    public bool HasGetter { get; }

    /// <summary>
    /// determines whether the property has a setter method defined
    /// </summary>
    public bool HasSetter { get; }
}