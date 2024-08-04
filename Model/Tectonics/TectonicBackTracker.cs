using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public class TectonicBackTracker : BackTracker<ITectonic, IPossibilitiesGiver>
{
    private readonly Dictionary<IZone, BitSet8> _zones = new();
    private readonly InfiniteBitmap[] _neighbors = new InfiniteBitmap[5];
    
    public TectonicBackTracker() : base(new BlankTectonic(), new EmptyPossibilitiesGiver()){}

    public TectonicBackTracker(ITectonic puzzle, IPossibilitiesGiver data) : base(puzzle, data) { }
    
    protected override bool Search(int position)
    {
        var full = Current.RowCount * Current.ColumnCount;
        for (; position < full; position++)
        {
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            
            if (Current[row, col] != 0) continue;

            var zone = Current.GetZone(row, col);
            if (!_zones.TryGetValue(zone, out var bs))
            {
                bs = new BitSet8();
                _zones[zone] = bs;
            }
            
            foreach (var possibility in _giver.EnumeratePossibilitiesAt(row, col))
            {
                var n = _neighbors[possibility - 1];
                if(bs.Contains(possibility) || n.HasNeighbor(row, col)) continue;

                Current[row, col] = possibility;
                bs.Add(possibility);
                n.Add(row, col);

                if (Search(position + 1)) return true;
                
                Current[row, col] = 0;
                bs.Remove(possibility);
                n.Remove(row, col);
            }

            return false;
        }

        return true;
    }
    
    protected override bool Search(IBackTrackingResult<ITectonic> result, int position)
    {
        var full = Current.RowCount * Current.ColumnCount;
        for (; position < full; position++)
        {
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            
            if (Current[row, col] != 0) continue;

            var zone = Current.GetZone(row, col);
            if (!_zones.TryGetValue(zone, out var bs))
            {
                bs = new BitSet8();
                _zones[zone] = bs;
            }
            
            foreach (var possibility in _giver.EnumeratePossibilitiesAt(row, col))
            {
                var n = _neighbors[possibility - 1];
                if(bs.Contains(possibility) || n.HasNeighbor(row, col)) continue;

                Current[row, col] = possibility;
                bs.Add(possibility);
                n.Add(row, col);

                var search = Search(result, position + 1);
                
                Current[row, col] = 0;
                bs.Remove(possibility);
                n.Remove(row, col);

                if (search) return true;
            }

            return false;
        }

        result.AddNewResult(Current);
        return result.Count >= StopAt;
    }
    
    protected override void Initialize(bool reset)
    {
        if(reset) _zones.Clear();
        if (Current.RowCount == 0 || Current.ColumnCount == 0) return;
        
        for (int i = 0; i < 5; i++)
        {
            _neighbors[i] = new InfiniteBitmap(Current.RowCount, Current.ColumnCount);
        }

        for (int row = 0; row < Current.RowCount; row++)
        {
            for (int col = 0; col < Current.ColumnCount; col++)
            {
                var number = Current[row, col];
                if(number == 0) continue;
                
                var zone = Current.GetZone(row, col);
                if (!_zones.TryGetValue(zone, out var bs))
                {
                    bs = new BitSet8();
                    _zones[zone] = bs;
                }
                
                _neighbors[number - 1].Add(row, col);
                bs.Add(number);
            }
        }
    }
}