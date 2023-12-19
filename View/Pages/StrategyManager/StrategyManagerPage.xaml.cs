using System.Collections.Generic;
using System.Windows;
using Presenter;
using Presenter.StrategyManager;
using Presenter.Translators;

namespace View.Pages.StrategyManager;

public partial class StrategyManagerPage : IStrategyManagerView
{
    private readonly StrategyManagerPresenter _presenter;
    private readonly IPageHandler _handler;
    
    public StrategyManagerPage(IPageHandler handler, ApplicationPresenter factory)
    {
        InitializeComponent();

        _handler = handler;
        _presenter = factory.Create(this);

        SearchBar.SearchTextChanged += _presenter.Search;
        StrategyList.StrategyAdded += _presenter.AddStrategy;
        StrategyList.StrategyAddedAtEnd += _presenter.AddStrategy;
        StrategyList.StrategyRemoved += _presenter.RemoveStrategy;
        StrategyList.StrategiesInterchanged += _presenter.InterchangeStrategies;
        StrategyList.ShowAsked += _presenter.ShowStrategy;
        OptionModifier.BehaviorChanged += _presenter.ChangeStrategyBehavior;
        OptionModifier.UsageChanged += _presenter.ChangeStrategyUsage;
        OptionModifier.ArgumentChanged += _presenter.ChangeArgument;
        
        _presenter.Start();
    }

    public void ShowSearchResult(List<string> result)
    {
        SearchBar.ShowSearchResult(result);
    }

    public void SetStrategiesUsed(IReadOnlyList<ViewStrategy> strategies)
    {
        StrategyList.SetStrategies(strategies);
    }

    public void ShowStrategy(ViewStrategy strategy)
    {
        OptionModifier.Show(strategy);
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _handler.ShowPage(PagesName.First);
    }

    public override void OnShow()
    {
        _presenter.Start();
        OptionModifier.Hide();
    }

    public override void OnQuit()
    {
        
    }
}