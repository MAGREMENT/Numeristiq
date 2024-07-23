using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model.Core.Settings;

namespace DesktopApplication.View.Settings;

public partial class StringListControl
{
    private readonly bool _raiseEvent;
    private readonly int[]? _indexTranslator;
    
    public StringListControl(ISettingCollection presenter, IReadOnlySetting setting, int index) : base(presenter, index)
    {
        InitializeComponent();

        if (setting.InteractionInterface is not IStringListInteractionInterface i) throw new Exception();
        _indexTranslator = i.IndexTranslator;

        SettingName.Text = setting.Name;
        foreach (var s in i)
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
        ComboBox.SelectedIndex = GetValueIndex(setting.Get());
        _raiseEvent = true;
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

    private int GetValueIndex(SettingValue value)
    {
        if (_indexTranslator is null) return value.ToInt();

        for (int i = 0; i < _indexTranslator.Length; i++)
        {
            if (_indexTranslator[i] == value.ToInt()) return i;
        }

        return 0;
    }
}