using System.Linq;
using Model.Positions;

namespace Model.Strategies;

public class BoxLineReductionStrategy : IStrategy
{
    public string Name => "Box line reduction";
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, number);
                if (ppir.AreAllInSameMiniGrid())
                {
                    var changeBuffer = strategyManager.CreateChangeBuffer(this,
                            new BoxLineReductionReport(row, ppir, number, Unit.Row));
                    
                    int miniRow = row / 3;
                    int miniCol = ppir.First() / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int realRow = miniRow * 3 + r;
                            int realCol = miniCol * 3 + c;

                            if (realRow != row) changeBuffer.AddPossibilityToRemove(number, realRow, realCol);
                        }
                    }
                    
                    changeBuffer.Push();
                }
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, number);
                if (ppic.AreAllInSameMiniGrid())
                {
                    var changeBuffer = strategyManager.CreateChangeBuffer(this,
                        new BoxLineReductionReport(col, ppic, number, Unit.Column));
                    
                    int miniRow = ppic.First() / 3;
                    int miniCol = col / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int realRow = miniRow * 3 + r;
                            int realCol = miniCol * 3 + c;

                            if (realCol != col) changeBuffer.AddPossibilityToRemove(number, realRow, realCol);
                        }
                    }
                    
                    changeBuffer.Push();
                }
            }
        }
    }
}

public class BoxLineReductionReport : IChangeReport //TODO
{
    private readonly int _unitNumber;
    private readonly LinePositions _linePos;
    private readonly int _number;
    private readonly Unit _unit;

    public BoxLineReductionReport(int unitNumber, LinePositions linePos, int number, Unit unit)
    {
        _unitNumber = unitNumber;
        _linePos = linePos;
        _number = number; 
        _unit = unit;
        
        CauseHighLighter = IChangeReport.DefaultCauseHighLighter;
        Explanation = "";
    }

    public string Explanation { get; }
    public HighLightCause CauseHighLighter { get; }
    public void Process()
    {
        throw new System.NotImplementedException();
    }
}