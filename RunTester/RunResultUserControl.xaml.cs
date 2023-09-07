using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using RunTester.Utils;

namespace RunTester;

public partial class RunResultUserControl
{
    public RunResultUserControl()
    {
        InitializeComponent();
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
                    TextAlignment = TextAlignment.Center
                };

                Grid.SetRow(tb, i);
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

                if (j == 0)
                {
                    tb.FontWeight = FontWeights.Bold;
                    tb.Foreground = new SolidColorBrush(ColorUtil.ToColor(current.Difficulty));
                }

                Grid.Children.Add(tb);
            }
        }
    }

    private void Clear()
    {
        Grid.Children.Clear();
        Grid.RowDefinitions.Clear();
    }
}