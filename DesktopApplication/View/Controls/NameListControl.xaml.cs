using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Controls;

public partial class NameListControl
{
    public NameListControl(ISetting setting) : base(setting)
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

        ComboBox.SelectedIndex = setting.Get().ToInt();
    }

    public override void Set()
    {
        Setting.Set(new IntSettingValue(ComboBox.SelectedIndex));
    }
}