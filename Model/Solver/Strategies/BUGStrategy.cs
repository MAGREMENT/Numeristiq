using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;

namespace Model.Solver.Strategies;

public class BUGStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public BUGStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        var triple = OnlyDoublesAndOneTriple(strategyManager);
        if (triple.Row == -1) return;
        
        foreach (var possibility in strategyManager.PossibilitiesAt(triple.Row, triple.Column))
        {
            if (strategyManager.ColumnPositionsAt(triple.Column, possibility).Count != 3 ||
                strategyManager.RowPositionsAt(triple.Row, possibility).Count != 3 ||
                strategyManager.MiniGridPositionsAt(triple.Row / 3, triple.Column / 3, possibility).Count != 3) 
                continue;
            
            strategyManager.ChangeBuffer.ProposeSolutionAddition(possibility, triple.Row, triple.Column);
            break;
        }

        strategyManager.ChangeBuffer.Commit(this, new BUGReportBuilder(triple));
    }

    private Cell OnlyDoublesAndOneTriple(IStrategyManager strategyManager)
    {
        var triple = new Cell(-1, -1);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.PossibilitiesAt(row, col).Count != 2)
                {
                    if (strategyManager.PossibilitiesAt(row, col).Count != 3 || triple.Row != -1)
                        return new Cell(-1, -1);

                    triple = new Cell(row, col);
                }
            }
        }

        return triple;
    }
}

public class BUGReportBuilder : IChangeReportBuilder
{
    private readonly Cell _triple;

    public BUGReportBuilder(Cell triple)
    {
        _triple = triple;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_triple.Row, _triple.Column, ChangeColoration.CauseOnOne);
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}