using System.Windows;
using System.Windows.Media;
using Presenter.Translator;
using View.Utility;

namespace View.Pages.StrategyManager.UserControls;

public partial class StrategyUserControl
{
    private const double TotalBorderThickness = 4;
    
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
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;

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
        
        DefaultBorder();
    }

    private void DefaultBorder()
    {
        Border.BorderThickness = new Thickness(0, TotalBorderThickness / 2, 0, TotalBorderThickness / 2);
        Border.BorderBrush = Brushes.Transparent;
    }

    private void DragBorder(bool upper)
    {
        Border.BorderThickness = upper 
            ? new Thickness(0, TotalBorderThickness, 0, 0) 
            : new Thickness(0, 0, 0, TotalBorderThickness);
        Border.BorderBrush = Brushes.Aqua;
    }
}

public delegate void OnShowAsked(int position);