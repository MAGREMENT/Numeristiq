using System;
using System.Collections.Generic;
using Model.Logs;
using Model.Positions;
using Model.Possibilities;

namespace Model;

public class ChangeBuffer
{
    private List<int>? _possibilityRemoved;
    private List<int>? _definitiveAdded;

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
        if (!_m.Possibilities[row, col].Peek(possibility)) return;
        _possibilityRemoved ??= new List<int>();
        _possibilityRemoved.Add(possibility * 81 + row * 9 + col);
    }

    public void AddDefinitiveToAdd(int number, int row, int col)
    {
        _definitiveAdded ??= new List<int>();
        _definitiveAdded.Add(number * 81 + row * 9 + col);
    }

    public void Push()
    {
        List<LogChange> changes = new();
        if (_possibilityRemoved is not null)
        {
            foreach (var possibility in _possibilityRemoved)
            {
                int n = possibility % 81;
                if (_m.RemovePossibility(possibility / 81, n / 9, n % 9)) changes.Add(
                    new LogChange(SolverNumberType.Possibility, possibility / 81, n / 9, n % 9));
            
            } 
        }

        if (_definitiveAdded is not null)
        {
            foreach (var definitive in _definitiveAdded)
            {
                int n = definitive % 81;
                if(_m.AddDefinitive(definitive / 81, n / 9, n % 9)) changes.Add(
                    new LogChange(SolverNumberType.Definitive, definitive / 81, n / 9, n % 9));
            }  
        }

        if(_m.LogsManaged && changes.Count > 0) _m.PushLog(changes, _causeFactory.CreateCauses(_m), _strategy);
    }
}

public interface IChangeCauseFactory
{
    IEnumerable<LogCause> CreateCauses(IChangeManager manager);
}

public class RowLinePositionsCauseFactory : IChangeCauseFactory
{
    private readonly int _row;
    private readonly LinePositions _cols;
    private readonly IPossibilities _mashed;

    public RowLinePositionsCauseFactory(int row, LinePositions cols, IPossibilities mashed)
    {
        _row = row;
        _cols = cols;
        _mashed = mashed;
    }
    
    public RowLinePositionsCauseFactory(int row, LinePositions cols, int number)
    {
        _row = row;
        _cols = cols;
        _mashed = IPossibilities.New();
        _mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        
        foreach (var col in _cols)
        {
            foreach (var possibility in _mashed)
            {
                if (manager.Possibilities[_row, col].Peek(possibility))
                    result.Add(new LogCause(SolverNumberType.Possibility,
                        possibility, _row, col, CauseColoration.One));
            }
        }

        return result;
    }
}

public class ColumnLinePositionsCauseFactory : IChangeCauseFactory
{
    private readonly int _col;
    private readonly LinePositions _rows;
    private readonly IPossibilities _mashed;

    public ColumnLinePositionsCauseFactory(int col, LinePositions rows, IPossibilities mashed)
    {
        _col = col;
        _rows = rows;
        _mashed = mashed;
    }
    
    public ColumnLinePositionsCauseFactory(int col, LinePositions rows, int number)
    {
        _col = col;
        _rows = rows;
        _mashed = IPossibilities.New();
        _mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        foreach (var row in _rows)
        {
            foreach (var possibility in _mashed)
            {
                if (manager.Possibilities[row, _col].Peek(possibility))
                    result.Add(new LogCause(SolverNumberType.Possibility,
                        possibility, row, _col, CauseColoration.One));
            }
        }

        return result;
    }
}

public class MiniGridPositionsCauseFactory : IChangeCauseFactory
{
    private readonly MiniGridPositions _gridPos;
    private readonly IPossibilities _mashed;

    public MiniGridPositionsCauseFactory(MiniGridPositions gridPos, IPossibilities mashed)
    {
        _gridPos = gridPos;
        _mashed = mashed;
    }
    
    public MiniGridPositionsCauseFactory(MiniGridPositions gridPos, int number)
    {
        _gridPos = gridPos;
        _mashed = IPossibilities.New();
        _mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        foreach (var pos in _gridPos)
        {
            foreach (var possibility in _mashed)
            {
                if (manager.Possibilities[pos[0], pos[1]].Peek(possibility))
                    result.Add(new LogCause(SolverNumberType.Possibility,
                        possibility, pos[0], pos[1], CauseColoration.One));
            }
        }

        return result;
    }
}

public class PossibilitiesCauseFactory : IChangeCauseFactory
{
    private readonly IPossibilities _possibilities;
    private readonly int _row;
    private readonly int _col;

    public PossibilitiesCauseFactory(IPossibilities possibilities, int row, int col)
    {
        _possibilities = possibilities;
        _row = row;
        _col = col;
    }
    
    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        foreach (var possibility in _possibilities)
        {
            yield return new LogCause(SolverNumberType.Possibility,
                possibility, _row, _col, CauseColoration.One);
        }
    }
}