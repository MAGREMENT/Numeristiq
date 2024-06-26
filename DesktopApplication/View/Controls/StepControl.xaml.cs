﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Model.Core.Steps;

namespace DesktopApplication.View.Controls;

public partial class StepControl
{
    private readonly int _id;
    private bool _shouldCallStateShownEvent = true;

    public event OnOpenRequest? OpenRequested;
    public event OnStateShownChange? StateShownChanged;
    public event OnHighlightShift? HighlightShifted;
    public event OnExplanationAsked? ExplanationAsked;
    
    public StepControl(IStep step, StateShown stateShown)
    {
        InitializeComponent();

        _id = step.Id;

        Title.Text = step.Title;
        Title.SetResourceReference(ForegroundProperty, ThemeInformation.ResourceNameFor(step.Difficulty));
        Number.Text = $"#{step.Id}";
        HighlightCount.Text = step.GetCursorPosition();
        SetStateShown(stateShown);
        TextOutput.Text = step.Description;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.SetResourceReference(BorderBrushProperty, "Primary1");
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.SetResourceReference(BorderBrushProperty, "Background2");
    }

    public void Open()
    {
        BottomPart.Visibility = Visibility.Visible;
    }

    public void Close()
    {
        BottomPart.Visibility = Visibility.Collapsed;
    }

    public void SetStateShown(StateShown stateShown)
    {
        _shouldCallStateShownEvent = false;
        if (stateShown == StateShown.Before) BeforeButton.IsChecked = true;
        else AfterButton.IsChecked = true;
        _shouldCallStateShownEvent = true;
    }


    public void SetCursorPosition(string s)
    {
        HighlightCount.Text = s;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        OpenRequested?.Invoke(_id);
    }

    private void BeforeChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.Before);
    }
    
    private void AfterChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.After);
    }
    
    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(true);
    }
    
    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(false);
    }

    private void OnExplanationAsked(object sender, RoutedEventArgs e)
    {
        ExplanationAsked?.Invoke();
    }
}

public delegate void OnOpenRequest(int id);
public delegate void OnStateShownChange(StateShown stateShown);
public delegate void OnHighlightShift(bool isLeft);
public delegate void OnExplanationAsked();