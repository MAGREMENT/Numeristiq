using System;
using System.Windows.Controls;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Settings;

public partial class AutoFillingControl
{
    private readonly bool _raiseEvent;
    private readonly AutoFillingInteractionInterface _i;
    
    public AutoFillingControl(ISettingCollection presenter, IReadOnlySetting setting, int index)
        : base(presenter, setting, index)
    {
        InitializeComponent();

        if (setting.InteractionInterface is not AutoFillingInteractionInterface i) throw new Exception();
        _i = i;

        _raiseEvent = false;
        Search.Text = setting.Get().ToString();
        Actual.Text = setting.Get().ToString();
        _raiseEvent = true;
    }

    public override void Set()
    {
        Set(new StringSettingValue(Actual.Text));
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        Actual.Text = _i.Fill(Search.Text);
        if(AutoSet && _raiseEvent) Set();
    }
}