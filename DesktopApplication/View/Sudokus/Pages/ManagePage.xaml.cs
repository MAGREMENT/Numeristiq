﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.View.Settings;
using DesktopApplication.View.Utility;
using Microsoft.Win32;
using Model.Core.Descriptions;
using Model.Sudokus.Solver;

namespace DesktopApplication.View.Sudokus.Pages;

public partial class ManagePage : ISudokuManageView
{
    private const int ToleranceForDragScroll = 80;
    private const int DragScrollOffset = 60;
    
    private readonly SudokuManagePresenter _presenter;
    
    public ManagePage()
    {
        InitializeComponent();
        _presenter = PresenterFactory.Instance.Initialize(this);
        _presenter.Initialize();
    }

    public void ClearSearchResults()
    {
        Search.ClearResult();
    }

    public void AddSearchResult(string s)
    {
        var tb = new TextBlock
        {
            Text = s,
            Style = (Style)FindResource("SearchResult")
        };
        tb.MouseLeftButtonDown += (_, _) => _presenter.OnSearchResultSelection(s);
        tb.MouseMove += (_, args) =>
        {
            if(args.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(tb, new StrategyDragDropData(s, -1), DragDropEffects.Move);
        };
        
        Search.AddResult(tb);
    }

    public void SetStrategyList(IReadOnlyList<SudokuStrategy> list)
    {
        StrategyPanel.Children.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var strategy = list[i];
            
            var tb = new TextBlock
            {
                Text = strategy.Name,
                AllowDrop = true,
                Style = (Style)FindResource("SimpleHoverBlock")
            };
            tb.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(strategy.Difficulty));
            
            var iForEvent = i;
            tb.MouseLeftButtonDown += (_, _) =>
            {
                _presenter.OnActiveStrategySelection(iForEvent);
            };
            tb.MouseMove += (_, args) =>
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                {
                    StrategyPanel.Children.RemoveAt(iForEvent);
                    DragDrop.DoDragDrop(tb, new StrategyDragDropData(strategy.Name, iForEvent), DragDropEffects.Move);
                }
            };
            tb.Drop += (_, args) => DropOn(tb, iForEvent, args);
            tb.DragOver += (_, args) =>
            {
                if (args.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData) return;

                tb.Padding = tb.IsUnderHalfHeight(args) 
                    ? new Thickness(5, 5, 0, 20) 
                    : new Thickness(5, 20, 0, 5);
            };
            tb.DragLeave += (_, _) =>
            {
                tb.Padding = new Thickness(5, 5, 0, 5);
            };
            
            StrategyPanel.Children.Add(tb);
        }
    }

    public void SetSelectedStrategyName(string name)
    {
        StrategyName.Text = name;
    }

    public void SetManageableSettings(StrategySettingsPresenter presenter)
    {
        InfoPanel.Children.Clear();
        foreach (var s in presenter)
        {
            var control = SettingTranslator.Translate(presenter, s.Item1, s.Item2);
            if (control is not null)
            {
                control.AutoSet = true;
                control.Margin = new Thickness(10, 10, 0, 0);
                InfoPanel.Children.Add(control);
            }
        }
    }

    public void SetStrategyDescription(IDescription description)
    {
        InfoPanel.Children.Clear();

        foreach (var line in description.EnumerateLines())
        {
            var element = TranslateDescriptionLine(line);
            if (element is not null)
            {
                element.Margin = new Thickness(10, 10, 10, 0);
                InfoPanel.Children.Add(element);
            }
        }
    }

    public void ClearSelectedStrategy()
    {
        StrategyName.Text = "No Strategy Selected";
        InfoPanel.Children.Clear();
    }

    public Stream? GetUploadPresetStream()
    {
        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "json",
            RestoreDirectory = true,
            Filter = "JSON File (*.json)|*.json"
        };
        var result = dialog.ShowDialog();

        if (result != true) return null;
        
        return dialog.OpenFile();
    }

    public Stream? GetDownloadPresetStream()
    {
        var dialog = new OpenFileDialog
        {
            AddExtension = true,
            Filter = "JSON File (*.json)|*.json",
            RestoreDirectory = true,
            DefaultExt = "json"
        };
        var result = dialog.ShowDialog();

        if (result != true) return null;
        
        return dialog.OpenFile();
    }

    public override string Header => "Manage";

    public override void OnShow()
    {
        _presenter.OnShow();
    }

    public override void OnClose()
    {
       
    }

    public override object? TitleBarContent()
    {
        return null;
    }

    private void OnSearch(string s)
    {
        _presenter.OnSearch(s);
    }

    private void DropInBin(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data) return;

        if (data.Index != -1) _presenter.RemoveStrategy(data.Index);
        if(sender is FrameworkElement element) element.SetResourceReference(BackgroundProperty, "Background2");
    }
    
    private void ScrollOnDrag(object sender, DragEventArgs e)
    {
        var pos = e.GetPosition(StrategyScrollViewer).Y;

        if (pos < ToleranceForDragScroll) StrategyScrollViewer.ScrollToVerticalOffset(
            StrategyScrollViewer.VerticalOffset - DragScrollOffset * (ToleranceForDragScroll - pos) / ToleranceForDragScroll);
        else if (pos > StrategyScrollViewer.ActualHeight - ToleranceForDragScroll) StrategyScrollViewer
            .ScrollToVerticalOffset(StrategyScrollViewer.VerticalOffset + DragScrollOffset * (pos - 
                StrategyScrollViewer.ActualHeight + ToleranceForDragScroll) / ToleranceForDragScroll);     
        
    }

    private void DropOn(TextBlock tb, int index, DragEventArgs args)
    {
        if (args.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data) return;
        
        if (tb.IsUnderHalfHeight(args)) index++;
        if (data.Index == -1) _presenter.AddStrategy(data.Name, index);
        else _presenter.InterchangeStrategies(data.Index, index);

        args.Handled = true;
    }

    private static FrameworkElement? TranslateDescriptionLine(IDescriptionLine line)
    {
        TextBlock tb;
        switch (line)
        {
            case TextDescriptionLine tdl:
                tb = new TextBlock
                {
                    FontSize = 14,
                    Text = tdl.Text,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };
                
                tb.SetResourceReference(ForegroundProperty, "Text");
                return tb;
            
            case TextImageDescriptionLine tidl :
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                
                tb = new TextBlock
                {
                    FontSize = 14,
                    Text = tidl.Text,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                tb.SetResourceReference(ForegroundProperty, "Text");
                Grid.SetColumn(tb, tidl.Disposition == TextDisposition.Left ? 0 : 1);
                grid.Children.Add(tb);
                
                var border = new Border
                {
                    BorderThickness = new Thickness(3),
                    Child = new Image
                    {
                        Source = new BitmapImage(new Uri($"pack://application:,,,/View/Images/Descriptions/{tidl.ImagePath}")),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    },
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.SetResourceReference(Border.BorderBrushProperty, "Background3");
                Grid.SetColumn(border, tidl.Disposition == TextDisposition.Left ? 1 : 0);
                grid.Children.Add(border);
                
                return grid;
            
            default: return null;
        }
    }

    private void Upload(object sender, RoutedEventArgs e)
    {
        _presenter.UploadPreset();
    }
    
    private void Download(object sender, RoutedEventArgs e)
    {
        _presenter.DownloadPreset();
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element 
            || e.Data.GetData(typeof(StrategyDragDropData)) is not StrategyDragDropData data
            || data.Index == -1) return;
        element.SetResourceReference(BackgroundProperty, "Background3");
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        element.SetResourceReference(BackgroundProperty, "Background2");
    }
}


public record StrategyDragDropData(string Name, int Index);