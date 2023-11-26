using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace View.Pages.StrategyManager.UserControls;

public partial class SearchUserControl
{
    private const int ListLimit = 30;
    
    private bool _callSearchBarEvent = true;
    private bool _searchTextShown;

    public delegate void OnSearchTextChange(string text);
    public event OnSearchTextChange? SearchTextChanged;
    
    public SearchUserControl()
    {
        InitializeComponent();
        
        SearchBarUnfocus(null, new RoutedEventArgs());
        SearchBar.AllowDrop = false;
    }

    private void SearchBarUnfocus(object? sender, RoutedEventArgs args)
    {
        _callSearchBarEvent = false;

        if (SearchBar.Text.Length == 0)
        {
            SearchBar.Foreground = Brushes.Gray;
            SearchBar.Text = "Search";
            _searchTextShown = true;
        }
        
        _callSearchBarEvent = true;
    }

    private void SearchBarFocus(object? sender, RoutedEventArgs args)
    {
        if (_searchTextShown)
        {
            SearchBar.Foreground = Brushes.Black;
            SearchBar.Text = "";
            _searchTextShown = false;
        }
    }

    private void SearchBarTextChange(object sender, TextChangedEventArgs e)
    {
        if (_callSearchBarEvent) SearchTextChanged?.Invoke(SearchBar.Text);
    }

    public void ShowSearchResult(List<string> search)
    {
        List.Children.Clear();

        var upper = Math.Min(ListLimit, search.Count);
        for (int i = 0; i < upper; i++)
        {
            var item = new SearchItemUserControl(search[i]);
            List.Children.Add(item);
        }
    }
}