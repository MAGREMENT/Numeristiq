using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Settings;

public partial class StringListControl
{
    private readonly bool _raiseEvent;
    private readonly int[]? _indexTranslator;
    
    public StringListControl(ISettingCollection presenter, IReadOnlySetting setting, int index, int[]? indexTranslator) : base(presenter, setting, index)
    {
        InitializeComponent();

        SettingName.Text = setting.Name;
        foreach (var s in setting.InteractionInterface as IStringListInteractionInterface ?? Enumerable.Empty<string>())
        {
            ComboBox.Items.Add(new ComboBoxItem
            {
                Content = s,
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        _raiseEvent = false;
        ComboBox.SelectedIndex = setting.Get().ToInt();
        _raiseEvent = true;
        
        _indexTranslator = indexTranslator;
    }

    public override void Set()
    {
        var n = _indexTranslator is null ? ComboBox.SelectedIndex : _indexTranslator[ComboBox.SelectedIndex];
        Set(new IntSettingValue(n));
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if(AutoSet && _raiseEvent) Set();
    }
}