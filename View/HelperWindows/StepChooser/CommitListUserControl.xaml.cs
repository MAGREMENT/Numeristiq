﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Presenter.Translators;
using View.Utility;

namespace View.HelperWindows.StepChooser;

public partial class CommitListUserControl
{
    public event OnCommitSelection? CommitSelected;
    
    public CommitListUserControl()
    {
        InitializeComponent();
    }

    public void Show(ViewCommit[] commits)
    {
        Panel.Children.Clear();

        for (int i = 0; i < commits.Length; i++)
        {
            var commit = commits[i];
            var iForEvent = i;
            
            var tb = new TextBlock
            {
                FontSize = 13,
                Foreground = new SolidColorBrush(ColorManager.ToColor(commit.StrategyIntensity)),
                Height = 20,
                Text = commit.StrategyName,
                TextWrapping = TextWrapping.Wrap
            };

            tb.MouseEnter += (_, _) => tb.Background = Brushes.DarkGray;
            tb.MouseLeave += (_, _) => tb.Background = Brushes.White;
            tb.MouseDown += (_, _) => CommitSelected?.Invoke(iForEvent);

            Panel.Children.Add(tb);
        }
    }
}

public delegate void OnCommitSelection(int index);