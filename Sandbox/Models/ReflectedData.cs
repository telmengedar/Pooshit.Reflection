using Pooshit.Reflection;

namespace Sandbox.Models;

[ReflectTypes]
public class ReflectedData {
    public Tuple<SnakeData, DataWithIndexer> Tuple { get; set; }
    public SnakeData SnakeData { get; set; }
}