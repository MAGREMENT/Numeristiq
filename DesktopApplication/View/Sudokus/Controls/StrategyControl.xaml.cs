using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Model.Core;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class StrategyControl
{
    private static readonly TimeSpan AnimationLength = new(0, 0, 0, 0, 150);
    private int _state = -2;
    private bool _fireClickEvent = true;

    public event OnStrategyEnabled? StrategyEnabled;
    
    public StrategyControl(Strategy strategy)
    {
        InitializeComponent();
        
        StrategyName.Text = strategy.Name;
        if (strategy.Locked) LockStrategy();
        else EnableStrategy(strategy.Enabled);

        MouseEnter += (_, _) => SetResourceReference(BackgroundProperty, "Background3");
        MouseLeave += (_, _) => SetResourceReference(BackgroundProperty, "Background2");
        SizeChanged += (_, _) => SetNameToPosition(_state);
    }

    public void SetHighlight(bool highlighted)
    {
        StrategyName.SetResourceReference(ForegroundProperty, highlighted ? "Primary1" : "Text");
    }

    public async void EnableStrategy(bool enabled)
    {
        _fireClickEvent = false;
        if (enabled)
        {
            CrossMark.Visibility = Visibility.Hidden;
            if (SetNameToPosition(1))
            {
                await Task.Delay(AnimationLength);
            }
            
            CheckMark.Visibility = Visibility.Visible;
            CheckMark.SetResourceReference(Shape.StrokeProperty, "On");
            
            _state = 1;
        }
        else
        {
            CheckMark.Visibility = Visibility.Hidden;
            if (SetNameToPosition(0))
            {
                await Task.Delay(AnimationLength);
            }
            
            CrossMark.Visibility = Visibility.Visible;
            CrossMark.SetResourceReference(Shape.StrokeProperty, "Off");

            _state = 0;
        }

        _fireClickEvent = true;
    }

    public void LockStrategy()
    {
        CheckMark.SetResourceReference(Shape.StrokeProperty, "Disabled");
        CrossMark.SetResourceReference(Shape.StrokeProperty, "Disabled");

        _state = -1;
    }

    private bool SetNameToPosition(int newState)
    {
        if (newState < 0) return false;
        var w = ActualWidth - 50 - StrategyName.ActualWidth;

        if (newState == _state || _state < 0)
        {
            StrategyName.Margin = new Thickness(newState == 0 ? w : 0, 0, 0, 0);
            return false;
        }

        var animation = new ThicknessAnimationUsingKeyFrames
        {
            BeginTime = TimeSpan.Zero
        };

        animation.KeyFrames.Add(new SplineThicknessKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
            Value = new Thickness(_state == 0 ? w : 0, 0, 0, 0)
        });
        animation.KeyFrames.Add(new SplineThicknessKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(AnimationLength),
            Value = new Thickness(newState == 0 ? w : 0, 0, 0, 0)
        });

        StrategyName.BeginAnimation(MarginProperty, animation);

        return true;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        if (!_fireClickEvent || _state < 0) return;

        bool nextEnabled = _state == 0;
        EnableStrategy(nextEnabled);
        StrategyEnabled?.Invoke(nextEnabled);
    }
}

public delegate void OnStrategyEnabled(bool enabled);