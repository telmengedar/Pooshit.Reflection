using System;
using System.Collections.Generic;

namespace Pooshit.Reflection;

/// <summary>
/// utility class to reflect on type information
/// </summary>
public class Reflection {
    static readonly Dictionary<Type, Model> models = new();

    /// <summary>
    /// get a model of a type
    /// </summary>
    /// <param name="type">type to get model of</param>
    /// <returns>model of specified type</returns>
    public static Model GetModel(Type type) {
        return models[type];
    }

    /// <summary>
    /// get a model of a type
    /// </summary>
    /// <typeparam name="T">type to get model of</typeparam>
    /// <returns>model of specified type</returns>
    public static Model GetModel<T>() {
        return GetModel(typeof(T));
    }
    
    /// <summary>
    /// adds a model to the lookup
    /// </summary>
    /// <param name="model">model to add</param>
    public static void AddModel(Model model) {
        models[model.Type] = model;
    }
}