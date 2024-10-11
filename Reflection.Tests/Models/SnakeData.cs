using System.Runtime.Serialization;
using Pooshit.Reflection;

namespace Reflection.Tests.Models;

[ReflectType]
public class SnakeData {
        
    [DataMember(Name="over_the_top")]
    public int OverTheTop { get; set; }
}