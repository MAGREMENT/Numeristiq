using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver;

public interface IStrategyManager : IPossibilitiesHolder
{ 
    bool LogsManaged { get; }

    ChangeBuffer ChangeBuffer { get; }
    
    LinkGraphManager GraphManager { get; }
    
    PreComputer PreComputer { get; }
    
    AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    
    AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    bool UniquenessDependantStrategiesAllowed { get; }
}





