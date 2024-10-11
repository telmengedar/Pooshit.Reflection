using System.Diagnostics;
using System.Runtime.Serialization;
using Pooshit.Reflection;
using Reflection.Tests.Models;

namespace Reflection.Tests;

[TestFixture, Parallelizable]
public class AccessPropertiesTests {

    Model Method(Type type) {
        return Pooshit.Reflection.Reflection.GetModel(type);
    }
    
    [Test, Parallelizable]
    public void GetAndSet() {
        SnakeData data = new();
        var model = Pooshit.Reflection.Reflection.GetModel(typeof(SnakeData));
        model.GetProperty("overthetop", true).SetValue(data, 3);
        Assert.AreEqual(3, data.OverTheTop);
        Assert.AreEqual(3, model.GetProperty("OverTheTop").GetValue(data));
    }

    [Test, Parallelizable]
    public void GetAndSetWithGetType() {
        SnakeData data = new();
        var model = Pooshit.Reflection.Reflection.GetModel(data.GetType());
        model.GetProperty("overthetop", true).SetValue(data, 3);
        Assert.AreEqual(3, data.OverTheTop);
        Assert.AreEqual(3, model.GetProperty("OverTheTop").GetValue(data));
    }

    [Test, Parallelizable]
    public void GetAndSetWithTypeVariable() {
        SnakeData data = new();
        Type type = data.GetType();
        var model = Pooshit.Reflection.Reflection.GetModel(type);
        model.GetProperty("overthetop", true).SetValue(data, 3);
        Assert.AreEqual(3, data.OverTheTop);
        Assert.AreEqual(3, model.GetProperty("OverTheTop").GetValue(data));
    }

    [Test, Parallelizable]
    public void GetAndSetWithModelFromMethod() {
        SnakeData data = new();
        Type type = data.GetType();
        Model model = Method(type);
        model.GetProperty("overthetop", true).SetValue(data, 3);
        Assert.AreEqual(3, data.OverTheTop);
        Assert.AreEqual(3, model.GetProperty("OverTheTop").GetValue(data));
    }

    [Test, Parallelizable]
    public void ReflectedAttributesWithProperties() {
        SnakeData data = new();
        Type type = data.GetType();
        Model model = Method(type);
        IPropertyInfo property=model.GetProperty("overthetop", true);
        Assert.That(property.Attributes.Length, Is.EqualTo(1));
        DataMemberAttribute datamember = property.Attributes.First() as DataMemberAttribute;
        Assert.NotNull(datamember);
        Assert.That(datamember.Name, Is.EqualTo("over_the_top"));
    }

    [Test, Parallelizable]
    public void Performance() {
        SnakeData data = new();

        var property = data.GetType().GetProperty("OverTheTop");
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; ++i)
            property.SetValue(data, 11);
        TimeSpan reflectionTime = sw.Elapsed;

        var nproperty = Pooshit.Reflection.Reflection.GetModel(typeof(SnakeData)).GetProperty("OverTheTop");
        sw.Restart();
        for (int i = 0; i < 1000000; ++i)
            nproperty.SetValue(data, 11);
        TimeSpan nreflectionTime = sw.Elapsed;

        Console.WriteLine("Reflection: " + reflectionTime);
        Console.WriteLine("SourceGen: " + nreflectionTime);
    }
}