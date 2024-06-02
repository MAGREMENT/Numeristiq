using Model.Core;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;

namespace Model.Tectonics.Strategies;

public class NakedSingleStrategy : TectonicStrategy
{
    public NakedSingleStrategy() : base("Naked Single", StepDifficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicStrategyUser strategyUser)
    {
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var p = strategyUser.PossibilitiesAt(row, col);
                if (p.Count != 1) continue;
            
                strategyUser.ChangeBuffer.ProposeSolutionAddition(
                    p.First(1, strategyUser.Tectonic.GetZone(row, col).Count), row, col);
                strategyUser.ChangeBuffer.Commit(DefaultChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>.Instance);
            }
        }
    }
}