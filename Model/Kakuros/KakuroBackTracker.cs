using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core.BackTracking;
using Model.Utility;

namespace Model.Kakuros;

public class KakuroBackTracker : BackTracker<IKakuro, IPossibilitiesGiver>
{
    private Cell[] _cells = Array.Empty<Cell>();
    private readonly Dictionary<IKakuroSum, List<int>> _sumInfos = new();
    
    private readonly IKakuroCombinationCalculator _calculator;
    
    public KakuroBackTracker(IKakuro puzzle, IPossibilitiesGiver data, IKakuroCombinationCalculator calculator) : base(puzzle, data)
    {
        _calculator = calculator;
    }

    protected override bool Search(int position)
    {
        for (; position < _cells.Length; position++)
        {
            var cell = _cells[position];
            if(Current[cell.Row, cell.Column] != 0) continue;

            var sum = Current.HorizontalSumFor(cell);
            var infoH = _sumInfos[sum];
            var combi = _calculator.CalculatePossibilities(sum.Amount,
                sum.Length, infoH);
            
            sum = Current.VerticalSumFor(cell);
            var infoV = _sumInfos[sum];
            combi &= _calculator.CalculatePossibilities(sum.Amount, 
                sum.Length, infoV);
            
            foreach (var p in _giver.EnumeratePossibilitiesAt(cell.Row, cell.Column))
            {
                if(!combi.Contains(p)) continue;

                Current[cell.Row, cell.Column] = p;
                infoH.Add(p);
                infoV.Add(p);

                if (Search(position + 1)) return true;

                Current[cell.Row, cell.Column] = 0;
                infoH.RemoveAt(infoH.Count - 1);
                infoV.RemoveAt(infoV.Count - 1);
            }

            return false;
        }

        return true;
    }

    protected override bool Search(IBackTrackingResult<IKakuro> result, int position)
    {
        for (; position < _cells.Length; position++)
        {
            var cell = _cells[position];
            if(Current[cell.Row, cell.Column] != 0) continue;

            var sum = Current.HorizontalSumFor(cell);
            var infoH = _sumInfos[sum];
            var combi = _calculator.CalculatePossibilities(sum.Amount,
                sum.Length, infoH);
            
            sum = Current.VerticalSumFor(cell);
            var infoV = _sumInfos[sum];
            combi &= _calculator.CalculatePossibilities(sum.Amount, 
                sum.Length, infoV);
            
            foreach (var p in _giver.EnumeratePossibilitiesAt(cell.Row, cell.Column))
            {
                if(!combi.Contains(p)) continue;

                Current[cell.Row, cell.Column] = p;
                infoH.Add(p);
                infoV.Add(p);

                var search = Search(result, position + 1);

                Current[cell.Row, cell.Column] = 0;
                infoH.RemoveAt(infoH.Count - 1);
                infoV.RemoveAt(infoV.Count - 1);

                if (search) return true;
            }

            return false;
        }

        result.AddNewResult(Current);
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        _cells = Current.EnumerateCells().ToArray();

        _sumInfos.Clear();
        foreach (var sum in Current.Sums)
        {
            var list = new List<int>();
            
            foreach (var cell in sum)
            {
                var n = Current[cell.Row, cell.Column];
                if (n == 0) continue;

                list.Add(n);
            }

            _sumInfos.Add(sum, list);
        }
    }
}