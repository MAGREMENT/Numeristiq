using Model.Core.Settings;

namespace DesktopApplication.View.Settings;

public static class SettingTranslator
{
    public static SettingControl? Translate(ISettingCollection presenter, IReadOnlySetting setting, int index)
    {
        return setting.InteractionInterface switch
        {
            IStringListInteractionInterface => new StringListControl(presenter, setting, index),
            SliderInteractionInterface => new SliderControl(presenter, setting, index),
            MinMaxSliderInteractionInterface => new MinMaxSliderControl(presenter, setting, index),
            CheckBoxInteractionInterface => new CheckBoxControl(presenter, setting, index),
            AutoFillingInteractionInterface => new AutoFillingControl(presenter, setting, index),
            _ => null
        };
    }
}