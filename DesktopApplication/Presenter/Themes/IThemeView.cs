using System.Collections.Generic;
using Model.Repositories;
using Model.Utility;

namespace DesktopApplication.Presenter.Themes;

public interface IThemeView
{
    void SetCurrentTheme(string name);
    void SetOtherThemes(IEnumerable<(Theme, bool)> themes);
    void SetColors(IEnumerable<(string, RGB)> colors);
    void SelectColor(string name);
    void UnselectColor();
}