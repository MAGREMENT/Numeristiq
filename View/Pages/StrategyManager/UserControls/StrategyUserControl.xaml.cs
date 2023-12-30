using System.Windows;
using System.Windows.Media;
using Presenter.Translators;
using View.Utility;

namespace View.Pages.StrategyManager.UserControls;

public partial class StrategyUserControl
{
    public event OnStrategyAddition? StrategyAdded;
    public event OnStrategiesInterchange? StrategiesInterchanged;
    public event OnShowAsked? ShowAsked;
    
    public StrategyUserControl(ViewStrategy strategy, int position)
    {
        InitializeComponent();

        TextBlock.Foreground = new SolidColorBrush(ColorUtility.ToColor(strategy.Intensity));
        TextBlock.Text = strategy.Name;

        AllowDrop = true;
        MouseDown += (_, _) =>
        {
            DragDrop.DoDragDrop(this, new StrategyShuffleData(TextBlock.Text, false, position),
                DragDropEffects.All);
            ShowAsked?.Invoke(position);
        };
        Drop += (_, args) =>
        {
            DefaultBorder();
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;
            
            var relativePosition = args.GetPosition(this).Y > ActualHeight / 2 ? position + 1 : position;
            if (data.FromSearch) StrategyAdded?.Invoke(data.Name, relativePosition);
            else if (data.CurrentPosition != -1) StrategiesInterchanged?.Invoke(data.CurrentPosition, relativePosition);

            args.Handled = true;
        };
        DragOver += (_, args) =>
        {
            HighlightedBorder(args.GetPosition(this).Y <= ActualHeight  / 2);
        };
        DragLeave += (_, _) =>
        {
            DefaultBorder();
        };
        
        MouseEnter += (_, _) =>
        {
            Background = Brushes.LightGray;
        };
        MouseLeave += (_, _) =>
        {
            Background = Brushes.White;
        };
    }

    public void DefaultBorder()
    {
        UpperBorder.BorderBrush = Brushes.Transparent;
        LowerBorder.BorderBrush = Brushes.Transparent;
    }

    public void HighlightedBorder(bool upper)
    {
        if (upper)
        {
            UpperBorder.BorderBrush = Brushes.Aqua;
            LowerBorder.BorderBrush = Brushes.Transparent;
        }
        else
        {
            LowerBorder.BorderBrush = Brushes.Aqua;
            UpperBorder.BorderBrush = Brushes.Transparent;
        }
    }
}

public delegate void OnShowAsked(int position);