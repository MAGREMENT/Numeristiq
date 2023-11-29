using System.Windows;
using System.Windows.Media;
using Presenter.Translator;
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

        TextBlock.Foreground = new SolidColorBrush(ColorManager.ToColor(strategy.Intensity));
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

            DefaultBorder();
            var relativePosition = args.GetPosition(this).Y > ActualHeight / 2 ? position + 1 : position;
            if (data.FromSearch) StrategyAdded?.Invoke(data.Name, relativePosition);
            else if (data.CurrentPosition != -1) StrategiesInterchanged?.Invoke(data.CurrentPosition, relativePosition);

            args.Handled = true;
        };
        DragOver += (_, args) =>
        {
            DragBorder(args.GetPosition(this).Y <= ActualHeight  / 2);
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
            Background = ColorManager.Background1;
        };
    }

    private void DefaultBorder()
    {
        UpperBorder.BorderBrush = Brushes.Transparent;
        LowerBorder.BorderBrush = Brushes.Transparent;
    }

    private void DragBorder(bool upper)
    {
        DefaultBorder();
        if (upper) UpperBorder.BorderBrush = Brushes.Aqua;
        else LowerBorder.BorderBrush = Brushes.Aqua;
    }
}

public delegate void OnShowAsked(int position);