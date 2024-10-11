// See https://aka.ms/new-console-template for more information

using Pooshit.Reflection;
using Sandbox.Models;

Model GetModel(Type dataType) {
    return Reflection.GetModel(dataType);
}

Tuple<SnakeData, DataWithIndexer> data = new(new SnakeData() {
                                                                 OverTheTop = 3
                                                             }, new DataWithIndexer());
Type type = data.GetType();
Model model = GetModel(type);
Console.WriteLine(((SnakeData)model.GetProperty("item1", true).GetValue(data)).OverTheTop);