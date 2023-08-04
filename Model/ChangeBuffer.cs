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

        if(_m.LogsManaged && changes.Count > 0) _m.PushLog(changes, _causeFactory.CreateCauses(_m),
            _strategy.GetExplanation(_causeFactory), _strategy);
    }
}

public interface IChangeCauseFactory
{
    IEnumerable<LogCause> CreateCauses(IChangeManager manager);
}

public class RowLinePositionsCauseFactory : IChangeCauseFactory
{
    private int Row { get; }
    private LinePositions Columns { get; }
    private IPossibilities Mashed { get; }

    public RowLinePositionsCauseFactory(int row, LinePositions columns, IPossibilities mashed)
    {
        Row = row;
        Columns = columns;
        Mashed = mashed;
    }
    
    public RowLinePositionsCauseFactory(int row, LinePositions columns, int number)
    {
        Row = row;
        Columns = columns;
        Mashed = IPossibilities.New();
        Mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        
        foreach (var col in Columns)
        {
            foreach (var possibility in Mashed)
            {
                if (manager.Possibilities[Row, col].Peek(possibility))
                    result.Add(new LogCause(SolverNumberType.Possibility,
                        possibility, Row, col, CauseColoration.One));
            }
        }

        return result;
    }
}

public class ColumnLinePositionsCauseFactory : IChangeCauseFactory
{
    private int Col { get; }
    private LinePositions Rows { get; }
    private IPossibilities Mashed { get; }

    public ColumnLinePositionsCauseFactory(int col, LinePositions rows, IPossibilities mashed)
    {
        Col = col;
        Rows = rows;
        Mashed = mashed;
    }
    
    public ColumnLinePositionsCauseFactory(int col, LinePositions rows, int number)
    {
        Col = col;
        Rows = rows;
        Mashed = IPossibilities.New();
        Mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        foreach (var row in Rows)
        {
            foreach (var possibility in Mashed)
            {
                if (manager.Possibilities[row, Col].Peek(possibility))
                    result.Add(new LogCause(SolverNumberType.Possibility,
                        possibility, row, Col, CauseColoration.One));
            }
        }

        return result;
    }
}

public class MiniGridPositionsCauseFactory : IChangeCauseFactory
{
    private MiniGridPositions GridPositions { get; }
    private IPossibilities Mashed { get; }

    public MiniGridPositionsCauseFactory(MiniGridPositions gridPositions, IPossibilities mashed)
    {
        GridPositions = gridPositions;
        Mashed = mashed;
    }
    
    public MiniGridPositionsCauseFactory(MiniGridPositions gridPositions, int number)
    {
        GridPositions = gridPositions;
        Mashed = IPossibilities.New();
        Mashed.RemoveAll(number);
    }

    public IEnumerable<LogCause> CreateCauses(IChangeManager manager)
    {
        List<LogCause> result = new();
        foreach (var pos in GridPositions)
        {
            foreach (var possibility in Mashed)
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