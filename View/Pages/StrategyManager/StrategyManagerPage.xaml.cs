﻿using System.Collections.Generic;
using System.Windows;
using Presenter;
using Presenter.Translator;

namespace View.Pages.StrategyManager;

public partial class StrategyManagerPage : HandledPage, IStrategyManagerView
{
    private readonly StrategyManagerPresenter _presenter;
    private readonly IPageHandler _handler;
    
    public StrategyManagerPage(IPageHandler handler, PresenterFactory factory)
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
        
    }

    public override void OnQuit()
    {
        
    }
}