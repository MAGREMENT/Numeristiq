using System;
using System.Collections.Generic;
using System.Linq;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model.Strategies;

public class SimpleColoringStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<SimpleColoringChain> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solver.Possibilities[row, col].Peek(number))
                    {
                        ColoringCoordinate current = new(row, col);
                        if (DoesAnyChainContains(chains, current)) continue;
                        
                        SimpleColoringChain chain = new();
                        InitChain(solver, chain, current, number);
                        if(chain.Count > 0) chains.Add(chain);
                    }
                }
            }

            foreach (var chain in chains)
            {
                ColoringCoordinate first = chain.First();
                first.Coloring = Coloring.On;
                RecursiveColoring(first, chain);
                
                SearchForTwiceInTheSameUnit(solver, number, chain);
                SearchForTwoColorsElsewhere(solver, number, chain);
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(ISolver solver, int number, SimpleColoringChain chain)
    {
        var divided = chain.DivideVerticesBasedOnColoringInQueues();

        while (divided[0].Count > 0)
        {
            ColoringCoordinate current = divided[0].Dequeue();
            Queue<ColoringCoordinate> copy = new Queue<ColoringCoordinate>(divided[0]);
            while (copy.Count > 0)
            {
                ColoringCoordinate next = copy.Dequeue();
                if (current.ShareAUnit(next))
                {
                    foreach (var toRemove in chain.EachOfColoring(Coloring.On))
                    {
                        solver.RemovePossibility(number, toRemove.Row, toRemove.Col,
                            new SimpleColoringLog(toRemove.Row, toRemove.Col, number, true));
                    }
                }
            }
        }
        
        while (divided[1].Count > 0)
        {
            ColoringCoordinate current = divided[1].Dequeue();
            Queue<ColoringCoordinate> copy = new Queue<ColoringCoordinate>(divided[1]);
            while (copy.Count > 0)
            {
                ColoringCoordinate next = copy.Dequeue();
                if (current.ShareAUnit(next))
                {
                    foreach (var toRemove in chain.EachOfColoring(Coloring.Off))
                    {
                        solver.RemovePossibility(number, toRemove.Row, toRemove.Col,
                            new SimpleColoringLog(toRemove.Row, toRemove.Col, number, true));
                    }
                }
            }
        }
    }

    private void SearchForTwoColorsElsewhere(ISolver solver, int number, SimpleColoringChain chain)
    {
        var divided = chain.DivideVerticesBasedOnColoringInHashSets();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Peek(number))
                {
                    ColoringCoordinate current = new(row, col);
                    if (!chain.Vertices().Contains(current))
                    {
                        bool onSameUnit = false;

                        foreach (var coord in divided[0])
                        {
                            if (current.ShareAUnit(coord))
                            {
                                onSameUnit = true;
                                break;
                            }
                        }

                        if (onSameUnit)
                        {
                            foreach (var coord in divided[1])
                            {
                                if (current.ShareAUnit(coord))
                                {
                                    solver.RemovePossibility(number, row, col,
                                        new SimpleColoringLog(row, col, number, false));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void RecursiveColoring(ColoringCoordinate current, SimpleColoringChain chain)
    {
        Coloring opposite = current.Coloring == Coloring.On ? Coloring.Off : Coloring.On;
        foreach (var coord in chain.VerticesLinked(current))
        {
            if (coord.Coloring == Coloring.None)
            {
                coord.Coloring = opposite;
                RecursiveColoring(coord, chain);
            }
        }
    }

    private void InitChain(ISolver solver, SimpleColoringChain chain, ColoringCoordinate current, int number)
    {
        var ppir = solver.PossibilityPositionsInRow(current.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir.All())
            {
                if (col != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(current.Row, col);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next, number);
                    break;
                }
            }
        }
        
        var ppic = solver.PossibilityPositionsInColumn(current.Col, number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic.All())
            {
                if (row != current.Row)
                {
                    ColoringCoordinate next = new ColoringCoordinate(row, current.Col);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next, number);
                    break;
                }
            }
        }
        
        var ppimn = solver.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    ColoringCoordinate next = new ColoringCoordinate(pos[0], pos[1]);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next, number);
                    break;
                }
            }
        }
    }

    private static bool DoesAnyChainContains(IEnumerable<SimpleColoringChain> chains, ColoringCoordinate coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Has(coord)) return true;
        }

        return false;
    }
}

public class SimpleColoringChain
{
    private readonly HashSet<ColoringCoordinate> _vertices = new();
    private readonly List<SimpleColoringLink> _links = new();

    public int Count => _links.Count;

    public bool AddLink(ColoringCoordinate one, ColoringCoordinate two)
    {
        if (_vertices.Contains(one) && _vertices.Contains(two)) return false;

        _vertices.Add(one);
        _vertices.Add(two);

        _links.Add(new SimpleColoringLink(one, two));

        return true;
    }

    public bool Has(ColoringCoordinate coordinate)
    {
        return _vertices.Contains(coordinate);
    }

    public IEnumerable<ColoringCoordinate> VerticesLinked(ColoringCoordinate coord)
    {
        foreach (var link in _links)
        {
            var other = link.OtherEnd(coord);
            if (other is not null) yield return other;
        }
    }

    public HashSet<ColoringCoordinate> Vertices()
    {
        return _vertices;
    }

    public HashSet<ColoringCoordinate>[] DivideVerticesBasedOnColoringInHashSets()
    {
        HashSet<ColoringCoordinate>[] result = { new(), new() };
        foreach (var vertex in _vertices)
        {
            if (vertex.Coloring == Coloring.On) result[0].Add(vertex);
            else if (vertex.Coloring == Coloring.Off) result[1].Add(vertex);
        }
        return result;
    }
    
    public Queue<ColoringCoordinate>[] DivideVerticesBasedOnColoringInQueues()
    {
        Queue<ColoringCoordinate>[] result = { new(), new() };
        foreach (var vertex in _vertices)
        {
            if (vertex.Coloring == Coloring.On) result[0].Enqueue(vertex);
            else if (vertex.Coloring == Coloring.Off) result[1].Enqueue(vertex);
        }
        return result;
    }

    public IEnumerable<ColoringCoordinate> EachOfColoring(Coloring Coloring)
    {
        foreach (var vertex in _vertices)
        {
            if (vertex.Coloring == Coloring) yield return vertex;
        }
    }

    public ColoringCoordinate First()
    {
        return _vertices.First();
    }
}

public class SimpleColoringLink
{
    private readonly ColoringCoordinate _one;
    private readonly ColoringCoordinate _two;

    public SimpleColoringLink(ColoringCoordinate one, ColoringCoordinate two)
    {
        if (one.Equals(two)) throw new ArgumentException("Vertices have to be different");
        _one = one;
        _two = two;
    }

    public ColoringCoordinate? OtherEnd(ColoringCoordinate coord)
    {
        if (_one.Equals(coord)) return _two;
        if (_two.Equals(coord)) return _one;
        return null;
    }
}

public class SimpleColoringLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public SimpleColoringLog(int row, int col, int number, bool twiceInUnitRule)
    {
        string rule = twiceInUnitRule ? "Twice in unit rule" : "Two elsewhere rule";
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of simple coloring " +
                   $": {rule}";
    }
}