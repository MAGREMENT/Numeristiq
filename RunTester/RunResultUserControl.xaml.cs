using System;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace RunTester;

public partial class RunResultUserControl
{
    public RunResultUserControl()
    {
        InitializeComponent();
        
        Clear();
    }

    public void Update(RunResult rr)
    {
        Grid.Dispatcher.Invoke(() => DispatchedUpdate(rr));
    }

    private void DispatchedUpdate(RunResult rr)
    {
        Clear();
        
        for (int i = 0; i < rr.Reports.Count; i++)
        {
            Grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(50)
            });
            
            var current = rr.Reports[i];
            
            for (int j = 0; j < 6; j++)
            {
                TextBlock tb = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                Grid.SetRow(tb, i + 1);
                Grid.SetColumn(tb, j);

                tb.Text = j switch
                {
                    0 => current.StrategyName,
                    1 => current.Tracker.Usage.ToString(),
                    2 => current.Tracker.Score.ToString(),
                    3 => Math.Round(current.Tracker.ScorePercentage(), 2) + " %",
                    4 => (double)current.Tracker.TimeUsed / 1000 + " s",
                    5 => Math.Round(current.Tracker.AverageTimeUsage(), 4) + " ms/usage",
                    _ => ""
                };

                Grid.Children.Add(tb);
            }
        }
    }

    private void Clear()
    {
        Grid.Children.Clear();
        Grid.RowDefinitions.Clear();
        Grid.RowDefinitions.Add(new RowDefinition()
        {
            Height = new GridLength(50)
        });

        for (int i = 0; i < 6; i++)
        {
            TextBlock tb = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, i);

            tb.Text = i switch
            {
                0 => "Strategy name",
                1 => "Usage",
                2 => "Score",
                3 => "Score percentage",
                4 => "Time used",
                5 => "Average time used",
                _ => ""
            };

            Grid.Children.Add(tb);
        }
    }
}