using System.Collections.Generic;
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

    private LinkGraph<ILinkGraphElement>? _graph;

    public PreComputer(IStrategyManager view)
    {
        _view = view;
    }

    public void Reset()
    {
        _graph = null;

        if (!_wasPrePosUsed) return;
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

    public LinkGraph<ILinkGraphElement> PrecomputedLinkGraph()
    {
        _graph ??= DoLinkGraph();
        return _graph;
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

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_view.Possibilities[row, col].Count != 2) continue;
                SearchForUsableAls(graph, new Coordinate(row, col), _view.Possibilities[row, col]);
            }
        }
        

        return graph;
    }

    private void SearchForUsableAls(LinkGraph<ILinkGraphElement> graph, Coordinate biValueCell, IPossibilities biValue)
    {
        for (int row = 0; row < 9; row++)
        {
            IPossibilities evaluated = _view.Possibilities[row, biValueCell.Col];
            if (evaluated.Count != 3) continue;
            
            int buffer = -1;
            bool yes = true;
            foreach (var possibility in evaluated)
            {
                if (!biValue.Peek(possibility))
                {
                    if (buffer == -1) buffer = possibility;
                    else
                    {
                        yes = false;
                        break;
                    }
                }
            }

            if (!yes && buffer != -1) continue;
            //Usable ALS found
            AlmostLockedSet als = new AlmostLockedSet(new[] { biValueCell, new Coordinate(row, biValueCell.Col) },
                evaluated.Mash(biValue));

            graph.AddLink(new PossibilityCoordinate(row, biValueCell.Col, buffer),
                als, LinkStrength.Strong, LinkType.MonoDirectional);

            for (int r = 0; r < 9; r++)
            {
                if (r == row || r == biValueCell.Row) continue;
                foreach (var possibility in biValue)
                {
                    if(_view.Possibilities[r, biValueCell.Col].Peek(possibility))
                        graph.AddLink(als, new PossibilityCoordinate(r, biValueCell.Col, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                }
            }

            //Same mini grid
            if (row / 3 == biValueCell.Row / 3)
            {
                int startRow = row / 3 * 3;
                int startCol = biValueCell.Col / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int r = startRow + gridRow;
                        int c = startCol + gridCol;

                        if (c == biValueCell.Col && (r == row || r == biValueCell.Row)) continue;
                        foreach (var possibility in biValue)
                        {
                            if(_view.Possibilities[r, c].Peek(possibility))
                                graph.AddLink(als, new PossibilityCoordinate(r, c, possibility),
                                    LinkStrength.Weak, LinkType.MonoDirectional);
                        }
                    }
                }
            }
        }
        
        for (int col = 0; col < 9; col++)
        {
            IPossibilities evaluated = _view.Possibilities[biValueCell.Row, col];
            if (evaluated.Count != 3) continue;
            
            int buffer = -1;
            bool yes = true;
            foreach (var possibility in evaluated)
            {
                if (!biValue.Peek(possibility))
                {
                    if (buffer == -1) buffer = possibility;
                    else
                    {
                        yes = false;
                        break;
                    }
                }
            }

            if (!yes && buffer != -1) continue;
            //Usable ALS found
            AlmostLockedSet als = new AlmostLockedSet(new[] { biValueCell, new Coordinate(biValueCell.Row, col) },
                evaluated.Mash(biValue));

            graph.AddLink(new PossibilityCoordinate(biValueCell.Row, col, buffer),
                als, LinkStrength.Strong, LinkType.MonoDirectional);

            for (int c = 0; c < 9; c++)
            {
                if (c == col || c == biValueCell.Col) continue;
                foreach (var possibility in biValue)
                {
                    if(_view.Possibilities[biValueCell.Row, c].Peek(possibility))
                        graph.AddLink(als, new PossibilityCoordinate(biValueCell.Row, c, possibility),
                            LinkStrength.Weak, LinkType.MonoDirectional);
                }
            }
            
            //Same mini grid
            if (col / 3 == biValueCell.Col / 3)
            {
                int startRow = biValueCell.Row / 3 * 3;
                int startCol = col / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int r = startRow + gridRow;
                        int c = startCol + gridCol;

                        if (r == biValueCell.Row && (c == col || c == biValueCell.Col)) continue;
                        foreach (var possibility in biValue)
                        {
                            if(_view.Possibilities[r, c].Peek(possibility))
                                graph.AddLink(als, new PossibilityCoordinate(r, c, possibility),
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
}