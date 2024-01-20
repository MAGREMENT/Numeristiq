using System.Collections.Generic;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopLoopFinder
{
    List<LinkGraphLoop<IChainingElement>> Find(CellPossibility[] cps, ILinkGraph<IChainingElement> graph);
}