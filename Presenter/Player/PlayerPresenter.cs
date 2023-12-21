using Global;
using Global.Enums;
using Model;

namespace Presenter.Player;

public class PlayerPresenter //TODO : Add clear
{
    private readonly IPlayerView _view;
    private readonly IPlayer _player;
    private readonly IPlayerSettings _settings;
    
    private readonly HashSet<Cell> _selected = new();

    private ChangeMode _changeMode = ChangeMode.Add;
    private LocationMode _locationMode = LocationMode.Middle;

    public PlayerPresenter(IPlayer player, IPlayerView view, IPlayerSettings settings)
    {
        _view = view;
        _settings = settings;
        _player = player;
    }

    public void Bind()
    {
        _view.SetChangeMode(_changeMode);
        _view.SetLocationMode(_locationMode);

        _player.Changed += Update;
        _player.MoveAvailabilityChanged += _view.SetMoveAvailability;
    }

    public void RestartSelection(int rowFrom, int colFrom)
    {
        var cell = new Cell(rowFrom, colFrom);
        var contained = _selected.Contains(cell);
        
        _selected.Clear();
        _view.ClearCursor();

        if (!contained)
        {
            _selected.Add(cell);
            _view.PutCursorOn(_selected);
        }
        
        _view.Refresh();
    }

    public void AddToSelection(int row, int col)
    {
        var count = _selected.Count;
        _selected.Add(new Cell(row, col));
        
        if (_selected.Count > count)
        {
            _view.PutCursorOn(_selected);
            _view.Refresh();
        }
    }

    public void SetChangeMode(ChangeMode mode)
    {
        _changeMode = mode;
    }

    public void SetLocationMode(LocationMode mode)
    {
        _locationMode = mode;
    }

    public void ApplyChange(int n)
    {
        if (_selected.Count == 0) return;
        
        switch (_changeMode)
        {
            case ChangeMode.Add :
                if (_locationMode == LocationMode.Solution) _player.SetNumber(n, _selected);
                else _player.AddPossibility(n, Translate(_locationMode), _selected);
                break;
            case ChangeMode.Remove :
                if (_locationMode == LocationMode.Solution) _player.RemoveNumber(n, _selected);
                else _player.RemovePossibility(n, Translate(_locationMode), _selected);
                break;
        }
    }

    public void Remove()
    {
        if (_selected.Count == 0) return;
        
        if (_locationMode == LocationMode.Solution) _player.RemoveNumber(_selected);
        else _player.RemovePossibility(Translate(_locationMode), _selected);
    }

    public void Highlight(HighlightColor color)
    {
        if (_selected.Count == 0) return;

        _player.Highlight(color, _selected);
    }

    public void MoveBack()
    {
        _player.MoveBack();
    }

    public void MoveForward()
    {
        _player.MoveForward();
    }

    public void SetMultiHighlighting(bool yes)
    {
        _player.MultiHighlighting = yes;
    }

    public void ClearNumbers()
    {
        if (_selected.Count > 0) _player.ClearNumbers(_selected);
        else _player.ClearNumbers();
    }

    public void ClearHighlightings()
    {
        if (_selected.Count > 0) _player.ClearHighlights(_selected);
        else _player.ClearHighlights();
    }

    public void ComputeDefaultPossibilities()
    {
        if (_selected.Count > 0) _player.ComputeDefaultPossibilities(_selected);
        else _player.ComputeDefaultPossibilities();
    }

    public void Paste(string s)
    {
        switch (SudokuTranslator.TryGetFormat(s))
        {
            case SudokuStringFormat.Line :
                _player.Paste(SudokuTranslator.TranslateToSudoku(s));
                break;
            case SudokuStringFormat.Grid :
                _player.Paste(SudokuTranslator.TranslateToState(s, _settings.TransformSoloPossibilityIntoGiven));
                break;
        }
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void Update()
    {
        _view.ClearNumbers();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = _player[row, col];
                if(current.IsNumber()) _view.ShowNumber(row, col, current.Number(), current.Editable 
                    ? _settings.SolvingColor 
                    : _settings.GivenColor);
                else
                {
                    _view.ShowPossibilities(row, col, current.Possibilities(PossibilitiesLocation.Top), PossibilitiesLocation.Top);
                    _view.ShowPossibilities(row, col, current.Possibilities(PossibilitiesLocation.Middle), PossibilitiesLocation.Middle);
                    _view.ShowPossibilities(row, col, current.Possibilities(PossibilitiesLocation.Bottom), PossibilitiesLocation.Bottom);
                }
            }
        }
        
        _view.ClearDrawings();
        foreach (var ch in _player.Highlighting)
        {
            if(ch.Highlighting.Count == 1) _view.HighlightCell(ch.Cell.Row, ch.Cell.Column, ch.Highlighting.GetFirst());
            else _view.HighlightCell(ch.Cell.Row, ch.Cell.Column, ch.Highlighting.GetAll());
        }
        
        _view.Refresh();
    }

    private PossibilitiesLocation Translate(LocationMode mode)
    {
        return mode switch
        {
            LocationMode.Top => PossibilitiesLocation.Top,
            LocationMode.Middle => PossibilitiesLocation.Middle,
            LocationMode.Bottom => PossibilitiesLocation.Bottom,
            _ => throw new ArgumentException()
        };
    }
}

public enum ChangeMode
{
    Add, Remove
}

public enum LocationMode
{
    Solution, Top, Middle, Bottom
}