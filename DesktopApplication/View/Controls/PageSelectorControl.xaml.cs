using System;
using System.Windows;

namespace DesktopApplication.View.Controls;

public partial class PageSelectorControl //TODO improve
{
    private int _max = 1;
    private int _current = 1;

    public int Max
    {
        get => _max;
        set
        {
            if (value < 1) return;
            
            _max = value;
            if (_max < _current) _max = _current;
            Update();
        }
    }

    public int Current
    {
        get => _current;
        set
        {
            if (value < 1 || value > _max) return;
            
            _current = value;
            Update();
        }
    }

    public event OnPageChange? PageChanged;
    
    public PageSelectorControl()
    {
        InitializeComponent();
    }

    private void Update()
    {
        CurrentTb.Text = _current.ToString();
        switch (_max)
        {
            case 1:
                BeforeBefore.Visibility = Visibility.Collapsed;
                Before.Visibility = Visibility.Collapsed;
                AfterAfter.Visibility = Visibility.Collapsed;
                After.Visibility = Visibility.Collapsed;
                
                MinusButton.IsEnabled = false;
                PlusButton.IsEnabled = false;
                break;
            case 2:
                BeforeBefore.Visibility = Visibility.Collapsed;
                AfterAfter.Visibility = Visibility.Collapsed;

                if (_current == 1)
                {
                    Before.Visibility = Visibility.Collapsed;
                    After.Visibility = Visibility.Visible;
                    
                    MinusButton.IsEnabled = false;
                    PlusButton.IsEnabled = true;
                    
                    After.Text = (_current + 1).ToString();
                }
                else
                {
                    After.Visibility = Visibility.Collapsed;
                    Before.Visibility = Visibility.Visible;
                    
                    MinusButton.IsEnabled = true;
                    PlusButton.IsEnabled = false;
                    
                    Before.Text = (_current - 1).ToString();
                }
                break;
            default:
                if (_current == 1)
                {
                    BeforeBefore.Visibility = Visibility.Collapsed;
                    Before.Visibility = Visibility.Collapsed;
                    AfterAfter.Visibility = Visibility.Visible;
                    After.Visibility = Visibility.Visible;

                    MinusButton.IsEnabled = false;
                    PlusButton.IsEnabled = true;

                    After.Text = (_current + 1).ToString();
                    AfterAfter.Text = (_current + 2).ToString();
                }
                else if(_current == _max)
                {
                    BeforeBefore.Visibility = Visibility.Visible;
                    Before.Visibility = Visibility.Visible;
                    AfterAfter.Visibility = Visibility.Collapsed;
                    After.Visibility = Visibility.Collapsed;

                    MinusButton.IsEnabled = true;
                    PlusButton.IsEnabled = false;

                    Before.Text = (_current - 1).ToString();
                    BeforeBefore.Text = (_current - 2).ToString();
                }
                else
                {
                    BeforeBefore.Visibility = Visibility.Collapsed;
                    Before.Visibility = Visibility.Visible;
                    AfterAfter.Visibility = Visibility.Collapsed;
                    After.Visibility = Visibility.Visible;

                    MinusButton.IsEnabled = true;
                    PlusButton.IsEnabled = true;

                    After.Text = (_current + 1).ToString();
                    Before.Text = (_current - 1).ToString();
                }
                
                break;
        }
    }

    private void PageBefore(object sender, EventArgs e)
    {
        if (_current == 1) return;
        Current = _current - 1;
        PageChanged?.Invoke(Current);
    }

    private void PageAfter(object sender, EventArgs e)
    {
        if (_current == _max) return;
        Current = _current + 1;
        PageChanged?.Invoke(Current);
    }
}

public delegate void OnPageChange(int newPage);