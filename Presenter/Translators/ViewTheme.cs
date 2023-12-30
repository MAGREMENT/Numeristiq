using Global;
using Global.Enums;
using Model;

namespace Presenter.Translators;

public record ViewTheme(RGB Background1, RGB Background2, RGB Background3, RGB Primary1, RGB Primary2,
    RGB Secondary1, RGB Secondary2, RGB Accent, RGB Text, IconColor IconColor)
{
    public static ViewTheme From(ThemeDAO dao)
    {
        return new ViewTheme(dao.Background1, dao.Background2, dao.Background3, dao.Primary1, dao.Primary2,
            dao.Secondary1, dao.Secondary2, dao.Accent, dao.Text, dao.IconColor);
    }

    public static ViewTheme[] From(ThemeDAO[] daos)
    {
        var result = new ViewTheme[daos.Length];
        for (int i = 0; i < daos.Length; i++)
        {
            result[i] = From(daos[i]);
        }

        return result;
    }
}