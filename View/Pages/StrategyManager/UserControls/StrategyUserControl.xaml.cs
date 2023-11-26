using System.Windows;
using System.Windows.Media;
using Presenter.Translator;
using View.Utility;

namespace View.Pages.StrategyManager.UserControls;

public partial class StrategyUserControl
{
    public event OnStrategyAddition? StrategyAdded;
    public event OnStrategiesInterchange? StrategiesInterchanged;
    
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
        };

        Drop += (_, args) =>
        {
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;
            if (data.FromSearch)
            {
                var p = args.GetPosition(this);
                StrategyAdded?.Invoke(data.Name, p.Y > ActualHeight / 2 ? position + 1 : position);
            }
            else if (data.CurrentPosition != -1)
            {
                StrategiesInterchanged?.Invoke(position, data.CurrentPosition);
            }

            args.Handled = true;
        };
    }
}