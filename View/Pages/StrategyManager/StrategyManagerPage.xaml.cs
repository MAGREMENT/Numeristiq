using System.Collections.Generic;
using Presenter;

namespace View.Pages.StrategyManager;

public partial class StrategyManagerPage : IStrategyManagerView
{
    private readonly StrategyManagerPresenter _presenter;
    
    public StrategyManagerPage(IPageHandler handler, PresenterFactory factory)
    {
        InitializeComponent();

        _presenter = factory.Create(this);

        SearchBar.SearchTextChanged += _presenter.Search;
    }

    public void ShowSearchResult(List<string> result)
    {
        SearchBar.ShowSearchResult(result);
    }
}