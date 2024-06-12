using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public static class ForcingNetsUtility
{
    public static void HighlightAllPaths(ISudokuHighlighter lighter, List<LinkGraphChain<ISudokuElement>> paths, Coloring startColoring)
    {
        HashSet<ISudokuElement> alreadyHighlighted = new();

        foreach (var path in paths)
        {
            for (int i = path.Links.Length - 1; i >= 0; i--)
            {
                var from = path.Elements[i];
                var to = path.Elements[i + 1];
                var link = path.Links[i];

                if (alreadyHighlighted.Contains(to)) break;
                
                lighter.HighlightElement(to, link == LinkStrength.Strong ? ChangeColoration.CauseOnOne : ChangeColoration.CauseOffOne);
                lighter.CreateLink(from, to, link);
                alreadyHighlighted.Add(to);
            }
            
            var first = path.Elements[0];

            if (!alreadyHighlighted.Contains(first))
            {
                lighter.HighlightElement(first, startColoring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffOne);
                alreadyHighlighted.Add(first);
            }
        }
    }

    public static string AllPathsToString(List<LinkGraphChain<ISudokuElement>> paths)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < paths.Count; i++)
        {
            var letter = (char)('a' + i);
            builder.Append($"{letter}) {paths[i]}\n");
        }

        return builder.ToString();
    }

    public static List<LinkGraphChain<ISudokuElement>> FindEveryNeededPaths(LinkGraphChain<ISudokuElement> basePath,
        IColoringResult<ISudokuElement> result, ILinkGraph<ISudokuElement> graph, IUpdatableSudokuSolvingState snapshot)
    {
        var list = new List<LinkGraphChain<ISudokuElement>> {basePath};
        HashSet<ISudokuElement> allElements = new(basePath.Elements);
        Queue<LinkGraphChain<ISudokuElement>> queue = new();
        queue.Enqueue(basePath);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            for (int i = 0; i < current.Links.Length; i++)
            {
                if (current.Elements[i] is not CellPossibility from) continue;
                if (current.Elements[i + 1] is not CellPossibility to) continue;

                var currentLink = current.Links[i];
                if (currentLink != LinkStrength.Strong ||
                    graph.AreNeighbors(current.Elements[i], current.Elements[i + 1], LinkStrength.Strong)) continue;

                foreach (var offCell in FindOffCellsInJumpLinks(result, snapshot, from, to))
                {
                    if (allElements.Contains(offCell)) continue;

                    var path = result.History!.GetPathToRootWithGuessedLinks(offCell, Coloring.Off);
                    list.Add(path);
                    allElements.UnionWith(path.Elements);
                    queue.Enqueue(path);
                }
            }
        }

        return list;
    }

    private static List<CellPossibility> FindOffCellsInJumpLinks(IColoringResult<ISudokuElement> result,
        IUpdatableSudokuSolvingState snapshot, CellPossibility from, CellPossibility to)
    {
        List<CellPossibility>? best = null;
        
        if (from.Possibility == to.Possibility)
        {
            if (from.Row == to.Row)
            {
                var cols = snapshot.RowPositionsAt(from.Row, from.Possibility);
                bool ok = true;

                foreach (var col in cols)
                {
                    if (col == from.Column || col == to.Column) continue;
                    var current = new CellPossibility(from.Row, col, from.Possibility);

                    if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                        continue;

                    ok = false;
                    break;
                }

                if (ok)
                {
                    var buffer = new List<CellPossibility>();
                    foreach (var col in cols)
                    {
                        if (col == from.Column || col == to.Column) continue;

                        buffer.Add(new CellPossibility(from.Row, col, from.Possibility));
                    }

                    if (best is null || buffer.Count < best.Count) best = buffer;
                }
            }

            if (from.Column == to.Column)
            {
                var rows = snapshot.ColumnPositionsAt(from.Column, from.Possibility);
                bool ok = true;

                foreach (var row in rows)
                {
                    if (row == from.Row || row == to.Row) continue;
                    var current = new CellPossibility(row, from.Column, from.Possibility);

                    if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                        continue;

                    ok = false;
                    break;
                }

                if (ok)
                {
                    var buffer = new List<CellPossibility>();
                    foreach (var row in rows)
                    {
                        if (row == from.Row || row == to.Row) continue;

                        buffer.Add(new CellPossibility(row, from.Column, from.Possibility));
                    }
                    
                    if (best is null || buffer.Count < best.Count) best = buffer;
                }
            }

            if (from.Row / 3 == to.Row / 3 && from.Column / 3 == to.Column / 3)
            {
                var positions = snapshot.MiniGridPositionsAt(from.Row / 3, from.Column / 3, from.Possibility);
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
                    var buffer = new List<CellPossibility>();
                    foreach (var pos in positions)
                    {
                        var current = new CellPossibility(pos, from.Possibility);
                        if (current == from || current == to) continue;

                        buffer.Add(new CellPossibility(pos, from.Possibility));
                    }
                    
                    if (best is null || buffer.Count < best.Count) best = buffer;
                }
            }
        }
        else if (from.Row == to.Row && from.Column == to.Column)
        {
            var possibilities = snapshot.PossibilitiesAt(from.Row, from.Column);
            bool ok = true;

            foreach (var pos in possibilities.EnumeratePossibilities())
            {
                if (pos == from.Possibility || pos == to.Possibility) continue;

                var current = new CellPossibility(from.Row, from.Column, pos);
                if (result.TryGetColoredElement(current, out var coloring) && coloring == Coloring.Off)
                    continue;

                ok = false;
                break;
            }

            if (ok)
            {
                var buffer = new List<CellPossibility>();
                foreach (var pos in possibilities.EnumeratePossibilities())
                {
                    if (pos == from.Possibility || pos == to.Possibility) continue;

                    buffer.Add(new CellPossibility(from.Row, from.Column, pos));
                }
                
                if (best is null || buffer.Count < best.Count) best = buffer;
            }
        }

        return best ?? new List<CellPossibility>();
    }

}