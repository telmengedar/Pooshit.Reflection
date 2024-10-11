using System.Runtime.Serialization;

namespace Sandbox.Models;

public class SnakeData {
        
    [DataMember(Name="over_the_top")]
    public int OverTheTop { get; set; }

    public int Bum => 7;

    public static bool Predicate => false;
}