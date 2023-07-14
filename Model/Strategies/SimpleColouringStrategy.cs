using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Strategies;

public class SimpleColouringStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int number = 1; number <= 9; number++)
        {
            List<Chain> chains = new();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solver.Possibilities[row, col].Peek(number))
                    {
                        ColouringCoordinate current = new(row, col);
                        if (!DoesAnyChainContains(chains, current))
                        {
                            Chain chain = new();
                            InitChain(solver, chain, current, number);
                            if(chain.Count > 0) chains.Add(chain);
                        }
                    }
                }
            }

            foreach (var chain in chains)
            {
                ColouringCoordinate first = chain.First();
                first.Colouring = Colouring.On;
                RecursiveColouring(first, chain);
                
                SearchForTwiceInTheSameUnit(solver, number, chain);
                SearchForTwoColoursElsewhere(solver, number, chain);
            }
        }
    }

    private void SearchForTwiceInTheSameUnit(ISolver solver, int number, Chain chain)
    {
        var divided = chain.DivideVerticesBasedOnColoringInQueues();

        while (divided[0].Count > 0)
        {
            ColouringCoordinate current = divided[0].Dequeue();
            Queue<ColouringCoordinate> copy = new Queue<ColouringCoordinate>(divided[0]);
            while (copy.Count > 0)
            {
                ColouringCoordinate next = copy.Dequeue();
                if (ShareAUnit(current, next))
                {
                    foreach (var toRemove in chain.EachOfColouring(Colouring.On))
                    {
                        solver.RemovePossibility(number, toRemove.Row, toRemove.Col,
                            new SimpleColouringLog(toRemove.Row, toRemove.Col, number, true));
                    }
                }
            }
        }
        
        while (divided[1].Count > 0)
        {
            ColouringCoordinate current = divided[1].Dequeue();
            Queue<ColouringCoordinate> copy = new Queue<ColouringCoordinate>(divided[1]);
            while (copy.Count > 0)
            {
                ColouringCoordinate next = copy.Dequeue();
                if (ShareAUnit(current, next))
                {
                    foreach (var toRemove in chain.EachOfColouring(Colouring.Off))
                    {
                        solver.RemovePossibility(number, toRemove.Row, toRemove.Col,
                            new SimpleColouringLog(toRemove.Row, toRemove.Col, number, true));
                    }
                }
            }
        }
    }

    private void SearchForTwoColoursElsewhere(ISolver solver, int number, Chain chain)
    {
        var divided = chain.DivideVerticesBasedOnColoringInHashSets();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Peek(number))
                {
                    ColouringCoordinate current = new(row, col);
                    if (!chain.Vertices().Contains(current))
                    {
                        bool onSameUnit = false;

                        foreach (var coord in divided[0])
                        {
                            if (ShareAUnit(current, coord))
                            {
                                onSameUnit = true;
                                break;
                            }
                        }

                        if (onSameUnit)
                        {
                            foreach (var coord in divided[1])
                            {
                                if (ShareAUnit(current, coord))
                                {
                                    solver.RemovePossibility(number, row, col,
                                        new SimpleColouringLog(row, col, number, false));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool ShareAUnit(ColouringCoordinate one, ColouringCoordinate two)
    {
        return one.Row == two.Row || one.Col == two.Col ||
               (one.Row / 3 == two.Row / 3
                && one.Col / 3 == two.Col / 3);
    }

    private void RecursiveColouring(ColouringCoordinate current, Chain chain)
    {
        Colouring opposite = current.Colouring == Colouring.On ? Colouring.Off : Colouring.On;
        foreach (var coord in chain.VerticesLinked(current))
        {
            if (coord.Colouring == Colouring.None)
            {
                coord.Colouring = opposite;
                RecursiveColouring(coord, chain);
            }
        }
    }

    private void InitChain(ISolver solver, Chain chain, ColouringCoordinate current, int number)
    {
        var ppir = solver.PossibilityPositionsInRow(current.Row, number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir.All())
            {
                if (col != current.Col)
                {
                    ColouringCoordinate next = new ColouringCoordinate(current.Row, col);
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
                    ColouringCoordinate next = new ColouringCoordinate(row, current.Col);
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
                    ColouringCoordinate next = new ColouringCoordinate(pos[0], pos[1]);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next, number);
                    break;
                }
            }
        }
    }

    private static bool DoesAnyChainContains(IEnumerable<Chain> chains, ColouringCoordinate coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Has(coord)) return true;
        }

        return false;
    }
}

public class Chain
{
    private readonly HashSet<ColouringCoordinate> _vertices = new();
    private readonly List<Link> _links = new();

    public int Count => _links.Count;

    public bool AddLink(ColouringCoordinate one, ColouringCoordinate two)
    {
        if (_vertices.Contains(one) && _vertices.Contains(two)) return false;

        _vertices.Add(one);
        _vertices.Add(two);

        _links.Add(new Link(one, two));

        return true;
    }

    public bool Has(ColouringCoordinate coordinate)
    {
        return _vertices.Contains(coordinate);
    }

    public IEnumerable<ColouringCoordinate> VerticesLinked(ColouringCoordinate coord)
    {
        foreach (var link in _links)
        {
            var other = link.OtherEnd(coord);
            if (other is not null) yield return other;
        }
    }

    public HashSet<ColouringCoordinate> Vertices()
    {
        return _vertices;
    }

    public HashSet<ColouringCoordinate>[] DivideVerticesBasedOnColoringInHashSets()
    {
        HashSet<ColouringCoordinate>[] result = { new(), new() };
        foreach (var vertex in _vertices)
        {
            if (vertex.Colouring == Colouring.On) result[0].Add(vertex);
            else if (vertex.Colouring == Colouring.Off) result[1].Add(vertex);
        }
        return result;
    }
    
    public Queue<ColouringCoordinate>[] DivideVerticesBasedOnColoringInQueues()
    {
        Queue<ColouringCoordinate>[] result = { new(), new() };
        foreach (var vertex in _vertices)
        {
            if (vertex.Colouring == Colouring.On) result[0].Enqueue(vertex);
            else if (vertex.Colouring == Colouring.Off) result[1].Enqueue(vertex);
        }
        return result;
    }

    public IEnumerable<ColouringCoordinate> EachOfColouring(Colouring colouring)
    {
        foreach (var vertex in _vertices)
        {
            if (vertex.Colouring == colouring) yield return vertex;
        }
    }

    public ColouringCoordinate First()
    {
        return _vertices.First();
    }
}

public class Link
{
    private readonly ColouringCoordinate _one;
    private readonly ColouringCoordinate _two;

    public Link(ColouringCoordinate one, ColouringCoordinate two)
    {
        if (one.Equals(two)) throw new ArgumentException("Vertices have to be different");
        _one = one;
        _two = two;
    }

    public bool Has(ColouringCoordinate coord)
    {
        return _one.Equals(coord) || _two.Equals(coord);
    }

    public ColouringCoordinate? OtherEnd(ColouringCoordinate coord)
    {
        if (_one.Equals(coord)) return _two;
        if (_two.Equals(coord)) return _one;
        return null;
    }
}


public class ColouringCoordinate : Coordinate
{
    public Colouring Colouring { get; set; } = Colouring.None;

    public ColouringCoordinate(int row, int col) : base(row, col)
    {
    }
}

public enum Colouring
{
    None, On, Off
}

public class SimpleColouringLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public SimpleColouringLog(int row, int col, int number, bool twiceInUnitRule)
    {
        string rule = twiceInUnitRule ? "Twice in unit rule" : "Two elsewhere rule";
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of simple coloring " +
                   $": {rule}";
    }
}