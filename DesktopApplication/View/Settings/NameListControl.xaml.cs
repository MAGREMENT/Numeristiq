using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Settings;

public partial class NameListControl
{
    private readonly bool _raiseEvent;
    
    public NameListControl(ISettingCollection presenter, IReadOnlySetting setting, int index) : base(presenter, setting, index)
    {
        InitializeComponent();

        SettingName.Text = setting.Name;
        foreach (var s in setting.InteractionInterface as NameListInteractionInterface ?? Enumerable.Empty<string>())
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
    }

    public override void Set()
    {
        Set(new IntSettingValue(ComboBox.SelectedIndex));
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if(AutoSet && _raiseEvent) Set();
    }
}