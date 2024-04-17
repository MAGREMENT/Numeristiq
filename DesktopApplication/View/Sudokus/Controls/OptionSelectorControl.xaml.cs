using System;
using System.Windows;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class OptionSelectorControl
{
    private int _currentlySelected;
    private string[] _options = Array.Empty<string>();

    public event OnOptionChange? OptionChanged;
    
    public OptionSelectorControl()
    {
        InitializeComponent();
    }

    public void SetOptions(string[] options, int value)
    {
        if (options.Length == 0 || value < 0 || value >= options.Length) return;

        _options = options;
        _currentlySelected = value;
        CurrentOption.Text = _options[_currentlySelected];
    }


    private void GoUp(object sender, RoutedEventArgs e)
    {
        if (_options.Length == 0) return;

        _currentlySelected = --_currentlySelected < 0 ? _options.Length - 1 : _currentlySelected;
        CurrentOption.Text = _options[_currentlySelected];
        OptionChanged?.Invoke(_currentlySelected);
    }

    private void GoDown(object sender, RoutedEventArgs e)
    {
        if (_options.Length == 0) return;

        _currentlySelected = ++_currentlySelected % _options.Length;
        CurrentOption.Text = _options[_currentlySelected];
        OptionChanged?.Invoke(_currentlySelected);
    }
}

public delegate void OnOptionChange(int index);