using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver;

public interface IStrategyManager : IPossibilitiesHolder
{ 
    bool LogsManaged { get; }
    
    bool AddSolution(int number, int row, int col, IStrategy strategy);

    bool RemovePossibility(int possibility, int row, int col, IStrategy strategy);

    ChangeBuffer ChangeBuffer { get; }
    
    LinkGraphManager GraphManager { get; }
    
    PreComputer PreComputer { get; }

    bool UniquenessDependantStrategiesAllowed { get; }
}





