using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.ForcingNets;

public static class ForcingNetsUtility
{
    public static Dictionary<CellPossibility, Coloring> FilterPossibilityCoordinates( //TODO delete this shit
        Dictionary<ILinkGraphElement, Coloring> coloring)
    {
        Dictionary<CellPossibility, Coloring> result = new();
        foreach (var element in coloring)
        {
            if (element.Key is not CellPossibility coord) continue;
            result.Add(coord, element.Value);
        }

        return result;
    }

    public static void HighlightColoring(IHighlightable lighter, Dictionary<CellPossibility, Coloring> coloring)
    {
        foreach (var element in coloring)
        {
            lighter.HighlightPossibility(element.Key, element.Value == Coloring.On ? ChangeColoration.CauseOnOne :
                ChangeColoration.CauseOffOne);
        }
    }

    public static void HighlightJumpLinks(IHighlightable lighter, LinkGraphChain<ILinkGraphElement> path, IColoringResult<ILinkGraphElement> result,
        LinkGraph<ILinkGraphElement> graph, IPossibilitiesHolder snapshot) //TODO => use everywhere
    {
        for (int i = 0; i < path.Links.Length; i++)
        {
            if (path.Elements[i] is not CellPossibility from) continue;
            if (path.Elements[i + 1] is not CellPossibility to) continue;
            
            var currentLink = path.Links[i];
            if (currentLink != LinkStrength.Strong ||
                graph.HasLinkTo(path.Elements[i], path.Elements[i + 1], LinkStrength.Strong)) continue;

            if (from.Possibility == to.Possibility)
            {
                if (from.Row == to.Row)
                {
                    var cols = snapshot.RowPositionsAt(from.Row, from.Possibility);
                    bool ok = true;

                    foreach (var col in cols)
                    {
                        if (col == from.Col || col == to.Col) continue;
                        var current = new CellPossibility(from.Row, col, from.Possibility);

                        if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                            continue;

                        ok = false;
                        break;
                    }

                    if (ok)
                    {
                        foreach (var col in cols)
                        {
                            if (col == from.Col || col == to.Col) continue;

                            lighter.HighlightPossibility(from.Possibility, from.Row, col, ChangeColoration.CauseOffOne);
                        }

                        continue;
                    }
                }
                
                if (from.Col == to.Col)
                {
                    var rows = snapshot.ColumnPositionsAt(from.Col, from.Possibility);
                    bool ok = true;

                    foreach (var row in rows)
                    {
                        if (row == from.Row || row == to.Row) continue;
                        var current = new CellPossibility(row, from.Col, from.Possibility);

                        if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                            continue;

                        ok = false;
                        break;
                    }

                    if (ok)
                    {
                        foreach (var row in rows)
                        {
                            if (row == from.Row || row == to.Row) continue;
                            
                            lighter.HighlightPossibility(from.Possibility, row, from.Col, ChangeColoration.CauseOffOne);
                        }

                        continue;
                    }
                }
                
                if (from.Row / 3 == to.Row / 3 && from.Col / 3 == to.Col / 3)
                {
                    var positions = snapshot.MiniGridPositionsAt(from.Row / 3, from.Col / 3, from.Possibility);
                    bool ok = true;

                    foreach (var pos in positions)
                    {
                        var current = new CellPossibility(pos, from.Possibility);
                        if (current == from || current == to) continue;
                        
                        if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                            continue;

                        ok = false;
                        break;
                    }

                    if (ok)
                    {
                        foreach (var pos in positions)
                        {
                            var current = new CellPossibility(pos, from.Possibility);
                            if (current == from || current == to) continue;
                        
                            lighter.HighlightPossibility(current, ChangeColoration.CauseOffOne);
                        }
                    }
                }
            }
            else if (from.Row == to.Row && from.Col == to.Col)
            {
                var possibilities = snapshot.PossibilitiesAt(from.Row, from.Col);
                bool ok = true;

                foreach (var pos in possibilities)
                {
                    if (pos == from.Possibility || pos == to.Possibility) continue;

                    var current = new CellPossibility(from.Row, from.Col, pos);
                    if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                        continue;

                    ok = false;
                    break;
                }

                if (ok)
                {
                    foreach (var pos in possibilities)
                    {
                        if (pos == from.Possibility || pos == to.Possibility) continue;

                        lighter.HighlightPossibility(pos, from.Row, from.Col, ChangeColoration.CauseOffOne);
                    }
                }
            }
        }
    }
}