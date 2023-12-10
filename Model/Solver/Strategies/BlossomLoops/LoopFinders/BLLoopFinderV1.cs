using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops.LoopFinders;

public class BLLoopFinderV1 : IBlossomLoopLoopFinder //TODO
{
    public List<LinkGraphLoop<ILinkGraphElement>> Find(CellPossibility[] cps, LinkGraph<ILinkGraphElement> graph)
    {
        var result = new List<LinkGraphLoop<ILinkGraphElement>>();
        
        var onColorings = new ColoringDictionary<ILinkGraphElement>[cps.Length];
        var offColorings = new ColoringDictionary<ILinkGraphElement>[cps.Length];

        for (int i = 0; i < cps.Length; i++)
        {
            onColorings[i] = ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
                ColorHelper.Algorithm.ColorWithRules, graph, cps[i], Coloring.On, true);
            offColorings[i] = ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
                ColorHelper.Algorithm.ColorWithRules, graph, cps[i], Coloring.Off, true);
        }

        for (int i = 0; i < cps.Length; i++)
        {
            for (int j = 0; j < cps.Length; j++)
            {
                if (i == j) continue;

                foreach (var entry in onColorings[i])
                {
                    if (!onColorings[j].TryGetColoredElement(entry.Key, out var c) || c == entry.Value) continue;
                    
                    var path1 = onColorings[i].History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, entry.Value, graph);
                    var path2 = offColorings[j].History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, c, graph)
                        .AddInFront(LinkStrength.Weak, cps[i]);
                    if(path2.Count < 2 || path1.Count < 2 || path1.Count + path2.Count < 6) continue;
                    
                    var loop = path1.TryMakeLoop(path2);
                    if (loop is not null) result.Add(loop);
                }
            }
        }

        return result;
    }
}