using Model.Repositories;
using Model.Sudokus.Solver;

namespace DesktopApplication.Presenter.Sudokus.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;
    private readonly StrategyManager<SudokuStrategy> _manager;
    private readonly IStrategyRepository<SudokuStrategy> _repo;

    public SudokuManagePresenter(ISudokuManageView view, StrategyManager<SudokuStrategy> manager, 
        IStrategyRepository<SudokuStrategy> repo)
    {
        _view = view;
        _manager = manager;
        _repo = repo;
    }
    
    public void Initialize()
    {
        _view.SetStrategyList(_manager.Strategies);

        foreach (var result in StrategyPool.EnumerateStrategies())
        {
            _view.AddSearchResult(result);
        }
    }

    public void OnSearch(string s)
    {
        _view.ClearSearchResults();

        foreach (var result in StrategyPool.EnumerateStrategies(s))
        {
            _view.AddSearchResult(result);
        }
    }

    public void OnSearchResultSelection(string s)
    {
        _view.SetSelectedStrategyName(s);
        _view.SetStrategyDescription(StrategyPool.GetDescription(s));
    }
    
    public void OnActiveStrategySelection(int index)
    {
        if (index < 0 || index >= _manager.Strategies.Count) return;

        var s = _manager.Strategies[index];
        _view.SetSelectedStrategyName(s.Name);
        _view.SetManageableSettings(new StrategySettingsPresenter(s, _repo));
    }

    public void AddStrategy(string s, int position)
    {
        var strategy = StrategyPool.CreateFrom(s);
        if (strategy is null) return;
        
        _manager.AddStrategy(strategy, position);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void InterchangeStrategies(int posFrom, int posTo)
    {
        _manager.InterchangeStrategies(posFrom, posTo);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void RemoveStrategy(int index)
    {
        _manager.RemoveStrategy(index);
        _repo.SetStrategies(_manager.Strategies);
        _view.SetStrategyList(_manager.Strategies);
        _view.ClearSelectedStrategy();
    }

    public void OnShow()
    {
        _view.ClearSelectedStrategy();
    }
    
    
    public void UploadPreset()
    {
        using var stream = _view.GetUploadPresetStream();
        if (stream is null) return;

        _repo.AddPreset(_manager.Strategies, stream);
    }

    public void DownloadPreset()
    {
        using var stream = _view.GetDownloadPresetStream();
        if (stream is null) return;

        _manager.ClearStrategies();
        _manager.AddStrategies(_repo.GetPreset(stream));
        _view.SetStrategyList(_manager.Strategies);
    }
}