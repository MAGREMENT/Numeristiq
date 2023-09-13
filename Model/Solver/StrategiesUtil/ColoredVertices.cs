using System;
using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Solver.StrategiesUtil;

public class ColoredVertices<T>
{
    public List<T> On { get; }
    public List<T> Off { get; }

    public int Count => On.Count + Off.Count;

    public ColoredVertices()
    {
        On = new List<T>();
        Off = new List<T>();
    }

    public ColoredVertices(int total)
    {
        var oneThird = total / 3;
        On = new List<T>(oneThird);
        Off = new List<T>(oneThird * 2);
    }

    public void Add(T vertex, Coloring coloring)
    {
        switch (coloring)
        {
            case Coloring.On : On.Add(vertex);
                break;
            case Coloring.Off : Off.Add(vertex);
                break;
            default: throw new ArgumentException("Invalid coloring");
        }
    }
}