using DesktopApplication.Presenter;
using Model.Helpers.Settings;

namespace DesktopApplication.View.Settings;

public static class SettingTranslator
{
    public static SettingControl? Translate(ISettingCollection presenter, IReadOnlySetting setting, int index)
    {
        return setting.InteractionInterface switch
        {
            NameListInteractionInterface => new NameListControl(presenter, setting, index),
            SliderInteractionInterface => new SliderControl(presenter, setting, index),
            MinMaxSliderInteractionInterface => new MinMaxSliderControl(presenter, setting, index),
            CheckBoxInteractionInterface => new CheckBoxControl(presenter, setting, index),
            _ => null
        };
    }
}