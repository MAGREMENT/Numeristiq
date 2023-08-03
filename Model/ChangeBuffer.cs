using System.Collections.Generic;
using Model.Logs;
using Model.Positions;

namespace Model;

public class ChangeBuffer
{
    private readonly List<int> _possibilityRemoved = new();
    private readonly List<int> _definitiveAdded = new();

    private readonly IChangeManager _m;
    private readonly IChangeCauseFactory _causeFactory;
    private readonly IStrategy _strategy;

    public ChangeBuffer(IChangeManager changeManager, IStrategy strategy, IChangeCauseFactory causesFactory)
    {
        _m = changeManager;
        _causeFactory = causesFactory;
        _strategy = strategy;
    }

    public void AddPossibilityToRemove(int possibility, int row, int col)
    {
        _possibilityRemoved.Add(possibility * 81 + row * 9 + col);
    }

    public void AddDefinitiveToAdd(int number, int row, int col)
    {
        _definitiveAdded.Add(number * 81 + row * 9 + col);
    }

    public void Push()
    {
        List<LogChange> changes = new();
        foreach (var possibility in _possibilityRemoved)
        {
            int n = possibility % 81;
            if (_m.RemovePossibility(possibility / 81, n / 9, n % 9)) changes.Add(
                new LogChange(SolverNumberType.Possibility, possibility / 81, n / 9, n % 9));
            
        }

        foreach (var definitive in _definitiveAdded)
        {
            int n = definitive % 81;
            if(_m.AddDefinitive(definitive / 81, n / 9, n % 9)) changes.Add(
                new LogChange(SolverNumberType.Definitive, definitive / 81, n / 9, n % 9));
        }

        if(changes.Count > 0) _m.PushLog(changes, _causeFactory.CreateCauses(), _strategy);
    }
}

public interface IChangeCauseFactory
{
    IEnumerable<LogCause> CreateCauses();
}

public class RowLinePositionsCause : IChangeCauseFactory
{
    private readonly int _row;
    private readonly LinePositions _cols;

    public RowLinePositionsCause(int row, LinePositions cols)
    {
        _row = row;
        _cols = cols;
    }

    public IEnumerable<LogCause> CreateCauses()
    {
        yield break;
        //TODO
    }
}