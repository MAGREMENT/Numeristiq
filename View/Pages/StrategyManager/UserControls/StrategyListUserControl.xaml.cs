using System.Collections.Generic;
using Presenter.Translator;

namespace View.Pages.StrategyManager.UserControls;

public partial class StrategyListUserControl
{
    public event OnStrategyAdditionAtEnd? StrategyAddedAtEnd;
    public event OnStrategyAddition? StrategyAdded;
    public event OnStrategyRemoval? StrategyRemoved;
    public event OnStrategiesInterchange? StrategiesInterchanged;
    public event OnShowAsked? ShowAsked;
    
    public StrategyListUserControl()
    {
        InitializeComponent();

        List.AllowDrop = true;
        List.Drop += (_, args) =>
        {
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;
            if (!data.FromSearch) return;
            
            StrategyAddedAtEnd?.Invoke(data.Name);
        };

        Trash.AllowDrop = true;
        Trash.Drop += (_, args) =>
        {
            if (args.Data.GetData(typeof(StrategyShuffleData)) is not StrategyShuffleData data) return;
            if (data.FromSearch || data.CurrentPosition == -1) return;
            
            StrategyRemoved?.Invoke(data.CurrentPosition);
        };
    }

    public void SetStrategies(IReadOnlyList<ViewStrategy>  strategies)
    {
        List.Children.Clear();

        for (int i = 0; i < strategies.Count; i++)
        {
            var s = new StrategyUserControl(strategies[i], i);
            
            s.StrategyAdded += (strategy, position) => StrategyAdded?.Invoke(strategy, position);
            s.StrategiesInterchanged += (pos1, pos2) => StrategiesInterchanged?.Invoke(pos1, pos2);
            s.ShowAsked += pos => ShowAsked?.Invoke(pos);
            
            List.Children.Add(s);
        }
    }
}

public delegate void OnStrategyAdditionAtEnd(string s);
public delegate void OnStrategyAddition(string s, int position);
public delegate void OnStrategyRemoval(int position);
public delegate void OnStrategiesInterchange(int pos1, int pos2);