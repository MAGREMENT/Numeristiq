using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class BoxLineReductionStrategy : IStrategy
{
    public string Name => "Box line reduction";
    public StrategyLevel Difficulty => StrategyLevel.Easy;
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
                            new BoxLineReductionReportWaiter(row, ppir, number, Unit.Row));
                    
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
                        new BoxLineReductionReportWaiter(col, ppic, number, Unit.Column));
                    
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

public class BoxLineReductionReportWaiter : IChangeReportWaiter
{
    private readonly int _unitNumber;
    private readonly LinePositions _linePos;
    private readonly int _number;
    private readonly Unit _unit;

    public BoxLineReductionReportWaiter(int unitNumber, LinePositions linePos, int number, Unit unit)
    {
        _unitNumber = unitNumber;
        _linePos = linePos;
        _number = number; 
        _unit = unit;
    }
    
    public ChangeReport Process(List<SolverChange> changes, IChangeManager manager)
    {
        List<Coordinate> causes = new();
        switch (_unit)
        {
            case Unit.Row :
                foreach (var col in _linePos)
                {
                    causes.Add(new Coordinate(_unitNumber, col));
                }
                break;
            case Unit.Column :
                foreach (var row in _linePos)
                {
                    causes.Add(new Coordinate(row, _unitNumber));
                }
                break;
        }

        return new ChangeReport(IChangeReportWaiter.ChangesToString(changes), lighter =>
        {
            foreach (var coord in causes)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportWaiter.HighlightChanges(lighter, changes);
        }, "");
    }
}