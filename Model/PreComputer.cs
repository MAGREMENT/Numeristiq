using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model;

public class PreComputer
{
    private readonly IStrategyManager _view;
    
    private readonly LinePositions?[,] _rows = new LinePositions[9, 9];
    private readonly LinePositions?[,] _cols = new LinePositions[9, 9];
    private readonly MiniGridPositions?[,,] _miniGrids = new MiniGridPositions[3, 3, 9];
    private bool _wasPrePosUsed;
    
    private List<AlmostLockedSet>? _als;

    private LinkGraph<ILinkGraphElement>? _graph;

    private readonly Dictionary<ILinkGraphElement, Coloring>?[,,] _onColoring
        = new Dictionary<ILinkGraphElement, Coloring>[9, 9, 9];
    private bool _wasPreColorUsed;

    public PreComputer(IStrategyManager view)
    {
        _view = view;
    }

    public void Reset()
    {
        _als = null;
        _graph = null;

        if (_wasPrePosUsed)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        _miniGrids[i, j, k] = null;

                        int l = i * 3 + j;
                        _rows[l, k] = null;
                        _cols[l, k] = null;
                    }
                }
            }
            _wasPrePosUsed = false;
        }

        if (_wasPreColorUsed)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        _onColoring[i, j, k] = null;
                    }
                }
            }
            _wasPreColorUsed = false;
        }
    }

    public LinePositions PossibilityPositionsInRow(int row, int number)
    {
        _wasPrePosUsed = true;
        
        _rows[row, number - 1] ??= DoPossibilityPositionsInRow(row, number);
        return _rows[row, number - 1]!;
    }
    
    public LinePositions PossibilityPositionsInColumn(int col, int number)
    {
        _wasPrePosUsed = true;
        
        _cols[col, number - 1] ??= DoPossibilityPositionsInColumn(col, number);
        return _cols[col, number - 1]!; 
    }
    
    public MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        _wasPrePosUsed = true;
        
        _miniGrids[miniRow, miniCol, number - 1] ??= DoPossibilityPositionsInMiniGrid(miniRow, miniCol, number);
        return _miniGrids[miniRow, miniCol, number - 1]!;
    }

    public List<AlmostLockedSet> AllAls()
    {
        _als ??= DoAllAls();
        return _als;
    }

    public LinkGraph<ILinkGraphElement> LinkGraph()
    {
        _graph ??= DoLinkGraph();
        return _graph;
    }

    public Dictionary<ILinkGraphElement, Coloring> OnColoring(int row, int col, int possibility)
    {
        _wasPreColorUsed = true;

        _onColoring[row, col, possibility - 1] ??=
            DoColor(new PossibilityCoordinate(row, col, possibility), Coloring.On);
        return _onColoring[row, col, possibility - 1]!;
    }
    
    public Dictionary<ILinkGraphElement, Coloring> OffColoring(int row, int col, int possibility)
    {
        return DoColor(new PossibilityCoordinate(row, col, possibility), Coloring.Off);
    }

    private LinePositions DoPossibilityPositionsInRow(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (_view.Sudoku[row, col] == number) return new LinePositions();
            if (_view.Possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }
    
    private LinePositions DoPossibilityPositionsInColumn(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (_view.Sudoku[row, col] == number) return new LinePositions();
            if (_view.Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    private MiniGridPositions DoPossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;

                if (_view.Sudoku[realRow, realCol] == number) return new MiniGridPositions(miniRow, miniCol);
                if (_view.Possibilities[realRow, realCol].Peek(number)) result.Add(i, j);
            }
        }

        return result;
    }

    private List<AlmostLockedSet> DoAllAls()
    {
        var result = new List<AlmostLockedSet>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_view.Sudoku[row, col] != 0) continue;

                if (_view.Possibilities[row, col].Count == 2)
                    result.Add(new AlmostLockedSet(new Coordinate(row, col), _view.Possibilities[row, col]));
                SearchRow(_view, row, col + 1, _view.Possibilities[row, col], 
                    new List<Coordinate> {new (row, col)}, result);
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                if (_view.Sudoku[row, col] != 0) continue;
                
                SearchColumn(_view, col, row + 1, _view.Possibilities[row, col], 
                    new List<Coordinate> {new (row, col)}, result);
            }
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int n = 0; n < 9; n++)
                {
                    int row = miniRow * 3 + n / 3;
                    int col = miniCol * 3 + n % 3;
                    if (_view.Sudoku[row, col] != 0) continue;
                    
                    SearchMiniGrid(_view, miniRow, miniCol, n + 1, _view.Possibilities[row, col],
                        new List<Coordinate> {new (row, col)}, result);
                }
            }
        }

        return result;
    }

    private void SearchRow(IStrategyManager strategyManager, int row, int start, IPossibilities current,
        List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int col = start; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            var copy = new List<Coordinate>(visited) { new (row, col) };

            if (mashed.Count == copy.Count + 1)
            {
                result.Add(new AlmostLockedSet(copy.ToArray(), mashed));
            }

            SearchRow(strategyManager, row, col + 1, mashed, copy, result);
        }
    }

    private void SearchColumn(IStrategyManager strategyManager, int col, int start, IPossibilities current,
        List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int row = start; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            var copy = new List<Coordinate>(visited) { new (row, col) };

            if (mashed.Count == copy.Count + 1)
            {
                result.Add(new AlmostLockedSet(copy.ToArray(), mashed));
            }

            SearchColumn(strategyManager, col, row + 1, mashed, copy, result);
        }
    }

    private void SearchMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, int start,
        IPossibilities current, List<Coordinate> visited, List<AlmostLockedSet> result)
    {
        for (int n = start; n < 9; n++)
        {
            int row = miniRow * 3 + n / 3;
            int col = miniCol * 3 + n % 3;
                
            if (strategyManager.Sudoku[row, col] != 0) continue;

            IPossibilities mashed = current.Mash(strategyManager.Possibilities[row, col]);
            if (mashed.Count == current.Count + strategyManager.Possibilities[row, col].Count) continue;

            var copy = new List<Coordinate>(visited) { new (row, col) };

            if (mashed.Count == copy.Count + 1 && NotInSameRowOrColumn(copy))
            {
                result.Add(new AlmostLockedSet(copy.ToArray(), mashed));
            }

            SearchMiniGrid(strategyManager, miniRow, miniCol, n + 1, mashed,
                copy, result);
        }
    }

    private bool NotInSameRowOrColumn(List<Coordinate> coord)
    {
        int row = coord[0].Row;
        int col = coord[0].Col;

        bool rowOk = false;
        bool colOk = false;

        for (int i = 1; i < coord.Count; i++)
        {
            if (!rowOk && coord[i].Row != row) rowOk = true;
            if (!colOk && coord[i].Col != col) colOk = true;

            if (rowOk && colOk) return true;
        }

        return false;
    }

    private LinkGraph<ILinkGraphElement> DoLinkGraph()
    {
        LinkGraph<ILinkGraphElement> graph = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in _view.Possibilities[row, col])
                {
                    PossibilityCoordinate current = new PossibilityCoordinate(row, col, possibility);
                    
                    //Row
                    var ppir = DoPossibilityPositionsInRow(row, possibility);
                    var strength = ppir.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var c in ppir)
                    {
                        if (c != col)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(row, c, possibility), strength);
                        }
                    }


                    //Col
                    var ppic = DoPossibilityPositionsInColumn(col, possibility);
                    strength = ppic.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var r in ppic)
                    {
                        if (r != row)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(r, col, possibility), strength);
                        }
                    }


                    //MiniGrids
                    var ppimn = DoPossibilityPositionsInMiniGrid(row / 3, col / 3, possibility);
                    strength = ppimn.Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in ppimn)
                    {
                        if (!(pos[0] == row && pos[1] == col))
                        {
                            graph.AddLink(current, new PossibilityCoordinate(pos[0], pos[1], possibility), strength);
                        }
                    }

                    strength = _view.Possibilities[row, col].Count == 2 ? LinkStrength.Strong : LinkStrength.Weak;
                    foreach (var pos in _view.Possibilities[row, col])
                    {
                        if (pos != possibility)
                        {
                            graph.AddLink(current, new PossibilityCoordinate(row, col, pos), strength);
                        }
                    }
                }
            }
        }

        for (int n = 1; n <= 9; n++)
        {
            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = DoPossibilityPositionsInMiniGrid(miniRow, miniCol, n);
                    if (ppimn.Count < 3) continue;
                    SearchForPointingInMiniGrid(_view, graph, ppimn, miniRow, miniCol, n);
                }
            }
        }
        
        SearchForAlmostNakedPossibilities(graph);


        return graph;
    }

    private void SearchForAlmostNakedPossibilities(LinkGraph<ILinkGraphElement> graph)
    {
        foreach (var als in AllAls())
        {
            if (als.Coordinates.Length is < 2 or > 4) continue;

            PossibilityCoordinate buffer = default;
            bool found = false;
            foreach (var possibility in als.Possibilities)
            {
                found = false;
                foreach (var coord in als.Coordinates)
                {
                    if (!_view.Possibilities[coord.Row, coord.Col].Peek(possibility)) continue;

                    if (!found)
                    {
                        buffer = new PossibilityCoordinate(coord.Row, coord.Col, possibility); 
                        found = true;
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (!found) continue;

            //Almost naked possibility found
            CoordinatePossibilities[] buildUp = new CoordinatePossibilities[als.Coordinates.Length];
            for (int i = 0; i < als.Coordinates.Length; i++)
            {
                buildUp[i] = new CoordinatePossibilities(als.Coordinates[i],
                    _view.Possibilities[als.Coordinates[i].Row, als.Coordinates[i].Col]);
            }

            AlmostNakedPossibilities anp = new AlmostNakedPossibilities(buildUp, buffer);
            graph.AddLink(buffer, anp, LinkStrength.Strong, LinkType.MonoDirectional);

            bool sameRow = true;
            int sharedRow = anp.CoordinatePossibilities[0].Coordinate.Row;
            bool sameCol = true;
            int sharedCol = anp.CoordinatePossibilities[0].Coordinate.Col;
            bool sameMini = true;
            int sharedMiniRow = anp.CoordinatePossibilities[0].Coordinate.Row / 3;
            int sharedMiniCol = anp.CoordinatePossibilities[0].Coordinate.Col / 3;

            for (int i = 1; i < anp.CoordinatePossibilities.Length; i++)
            {
                if (anp.CoordinatePossibilities[i].Coordinate.Row != sharedRow) sameRow = false;
                if (anp.CoordinatePossibilities[i].Coordinate.Col != sharedCol) sameCol = false;
                if (anp.CoordinatePossibilities[i].Coordinate.Row / 3 != sharedMiniRow ||
                    anp.CoordinatePossibilities[i].Coordinate.Col / 3 != sharedMiniCol) sameMini = false;
            }

            foreach (var possibility in als.Possibilities)
            {
                if (possibility == buffer.Possibility) continue;
                if (sameRow)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (!_view.Possibilities[sharedRow, col].Peek(possibility)) continue;

                        Coordinate current = new Coordinate(sharedRow, col);
                        if (als.Contains(current)) continue;
                        
                        graph.AddLink(anp, new PossibilityCoordinate(current.Row, current.Col, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }

                if (sameCol)
                {
                    for (int row = 0; row < 9; row++)
                    {
                        if (!_view.Possibilities[row, sharedCol].Peek(possibility)) continue;
                        
                        Coordinate current = new Coordinate(row, sharedCol);
                        if (als.Contains(current)) continue;
                        
                        graph.AddLink(anp, new PossibilityCoordinate(current.Row, current.Col, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }

                if (sameMini)
                {
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        for(int gridCol = 0; gridCol < 3; gridCol++)
                        {
                            int row = sharedMiniRow * 3 + gridRow;
                            int col = sharedMiniCol * 3 + gridCol;
                            
                            if (!_view.Possibilities[row, col].Peek(possibility)) continue;
                        
                            Coordinate current = new Coordinate(row, col);
                            if (als.Contains(current)) continue;
                        
                            graph.AddLink(anp, new PossibilityCoordinate(current.Row, current.Col, possibility),
                                LinkStrength.Weak, LinkType.MonoDirectional);
                        }
                    }
                }
            }
        }
    }

    private void SearchForPointingInMiniGrid(IStrategyManager view, LinkGraph<ILinkGraphElement> graph, MiniGridPositions ppimn, int miniRow,
        int miniCol, int numba)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            var colPos = ppimn.OnGridRowAsLine(gridRow);
            if (colPos.Count > 1)
            {
                List<PossibilityCoordinate> singles = new();
                List<PointingColumn> pcs = new();
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridRow) continue;
                        if (ppimn.PeekFromGridPositions(a, gridCol))
                        {
                            singles.Add(new PossibilityCoordinate(miniRow * 3 + a, miniCol * 3 + gridCol, numba));
                            if (buffer == -1) buffer = a;
                            else pcs.Add(new PointingColumn(numba, miniCol * 3 + gridCol,
                                miniRow * 3 + a, miniRow * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingRow(numba, miniRow * 3 + gridRow, colPos);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    graph.AddLink(current, pc, pcsStrength);
                }
                
                singles.Clear();
                var prs = new List<PointingRow>();

                for (int miniCol2 = 0; miniCol2 < 3; miniCol2++)
                {
                    if (miniCol == miniCol2) continue;

                    List<PossibilityCoordinate> aligned = new();
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = miniRow * 3 + gridRow;
                        int col = miniCol2 * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new PossibilityCoordinate(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) prs.Add(new PointingRow(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pr in prs)
                {
                    graph.AddLink(current, pr, prsStrength);
                }
            }
        }
        
        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            var rowPos = ppimn.OnGridColumnAsLine(gridCol);
            if (rowPos.Count > 1)
            {
                List<PossibilityCoordinate> singles = new();
                List<PointingRow> prs = new();
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridCol) continue;
                        if (ppimn.PeekFromGridPositions(gridRow, a))
                        {
                            singles.Add(new PossibilityCoordinate(miniRow * 3 + gridRow, miniCol * 3 + a, numba));
                            if (buffer == -1) buffer = a;
                            else prs.Add(new PointingRow(numba, miniRow * 3 + gridRow,
                                miniCol * 3 + a, miniCol * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingColumn(numba, miniCol * 3 + gridCol, rowPos);

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in prs)
                {
                    graph.AddLink(current, pc, prsStrength);
                }
                
                singles.Clear();
                var pcs = new List<PointingColumn>();

                for (int miniRow2 = 0; miniRow2 < 3; miniRow2++)
                {
                    if (miniRow == miniRow2) continue;

                    List<PossibilityCoordinate> aligned = new();
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow2 * 3 + gridRow;
                        int col = miniCol * 3 + gridCol;

                        if (view.Possibilities[row, col].Peek(numba)) aligned.Add(new PossibilityCoordinate(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) pcs.Add(new PointingColumn(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    graph.AddLink(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    graph.AddLink(current, pc, pcsStrength);
                }
            }
        }
    }

    private Dictionary<ILinkGraphElement, Coloring> DoColor(ILinkGraphElement start, Coloring firstColor)
    {
        var graph = LinkGraph();

        Queue<ILinkGraphElement> queue = new();
        queue.Enqueue(start);

        Dictionary<ILinkGraphElement, Coloring> result = new();
        result.Add(start, firstColor);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            Coloring opposite = result[current] == Coloring.On ? Coloring.Off : Coloring.On;

            foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
            {
                if (!result.ContainsKey(friend))
                {
                    result[friend] = opposite;
                    queue.Enqueue(friend);
                }
            }

            if (opposite == Coloring.Off)
            {
                foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
                {
                    if (!result.ContainsKey(friend))
                    {
                        result[friend] = opposite;
                        queue.Enqueue(friend);
                    }
                }
            }
            else if (current is PossibilityCoordinate pos)
            {
                PossibilityCoordinate? row = null;
                bool rowB = true;
                PossibilityCoordinate? col = null;
                bool colB = true;
                PossibilityCoordinate? mini = null;
                bool miniB = true;
            
                foreach (var friend in graph.GetLinks(current, LinkStrength.Weak))
                {
                    if (friend is not PossibilityCoordinate friendPos) continue;
                    if (rowB && friendPos.Row == pos.Row)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) rowB = false;
                        }
                        else
                        {
                            if (row is null) row = friendPos;
                            else rowB = false;  
                        }
                    
                    }

                    if (colB && friendPos.Col == pos.Col)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) colB = false;
                        }
                        else
                        {
                            if (col is null) col = friendPos;
                            else colB = false;
                        }
                    }

                    if (miniB && friendPos.Row / 3 == pos.Row / 3 && friendPos.Col / 3 == pos.Col / 3)
                    {
                        if (result.TryGetValue(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) miniB = false;
                        }
                        else
                        {
                            if (mini is null) mini = friendPos;
                            else miniB = false;
                        }
                    }
                }

                if (row is not null && rowB)
                {
                    result[row] = Coloring.On;
                    queue.Enqueue(row);
                }

                if (col is not null && colB)
                {
                    result[col] = Coloring.On;
                    queue.Enqueue(col);
                }

                if (mini is not null && miniB)
                {
                    result[mini] = Coloring.On;
                    queue.Enqueue(mini);
                }
            }
        }

        return result;
    }
}