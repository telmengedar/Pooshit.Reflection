using System;
using System.Linq;

namespace Pooshit.Reflection;

/// <summary>
/// typed property info
/// </summary>
/// <typeparam name="TInstance">type of instance</typeparam>
/// <typeparam name="TPropertyType">type of property</typeparam>
public class PropertyInfo<TInstance, TPropertyType> : IPropertyInfo, IEquatable<PropertyInfo<TInstance, TPropertyType>>
{
    readonly Func<TInstance, TPropertyType> getValue;
    readonly Action<TInstance, TPropertyType> setValue;
    
    /// <summary>
    /// creates a new <see cref="PropertyInfo{TInstance,TPropertyType}"/>
    /// </summary>
    /// <param name="name">name of property</param>
    /// <param name="attributes">attributes of property</param>
    /// <param name="getValue">method used to get value of property</param>
    /// <param name="setValue">method used to set value of property</param>
    public PropertyInfo(
        string name,
        Attribute[] attributes,
        Func<TInstance, TPropertyType> getValue = null,
        Action<TInstance, TPropertyType> setValue = null)
    {
        Name = name;
        Attributes = attributes;
        PropertyType = typeof(TPropertyType);
        this.getValue = getValue;
        this.setValue = setValue;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public Type PropertyType { get; }

    /// <inheritdoc />
    public object GetValue(object instance) => getValue.Invoke((TInstance)instance);

    /// <inheritdoc />
    public void SetValue(object instance, object value) {
        setValue((TInstance)instance, (TPropertyType)value);
    }

    /// <inheritdoc />
    public bool HasGetter => getValue != null;

    /// <inheritdoc />
    public bool HasSetter => setValue != null;

    /// <inheritdoc />
    public bool Equals(PropertyInfo<TInstance, TPropertyType> other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Equals(getValue, other.getValue) && 
               Equals(setValue, other.setValue) && Name == other.Name && 
               Attributes.SequenceEqual(other.Attributes);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        PropertyInfo<TInstance, TPropertyType> other = (PropertyInfo<TInstance, TPropertyType>)obj;
        return Name == other.Name && Attributes.SequenceEqual(other.Attributes);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (getValue != null ? getValue.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (setValue != null ? setValue.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(PropertyInfo<TInstance, TPropertyType> left, PropertyInfo<TInstance, TPropertyType> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PropertyInfo<TInstance, TPropertyType> left, PropertyInfo<TInstance, TPropertyType> right)
    {
        return !Equals(left, right);
    }
}