using Model.Repositories;
using Model.Utility;

namespace Repository;

public class HardCodedThemeRepository : IThemeRepository
{
    private readonly Theme[] _themes =
    {
        new("Abyss",
                RGB.FromHex(0x0E0E15), RGB.FromHex(0x1C1C2C), RGB.FromHex(0x272742),
                RGB.FromHex(0x920BF2), RGB.FromHex(0x7E0BD0),
                RGB.FromHex(0xF10FD3), RGB.FromHex(0xBE0DA6),
                RGB.FromHex(0x0BA1F1),
                RGB.FromHex(0xFFFFFF),
                RGB.FromHex(0x09AD09), RGB.FromHex(0xDE0E0E),
                RGB.FromHex(0x7D7D7D), 
                RGB.FromHex(0x3FBADF), RGB.FromHex(0x09AD09), 
                RGB.FromHex(0xFFA500), RGB.FromHex(0xDE0E0E), RGB.FromHex(0x6A178E), 
                RGB.FromHex(0x503D98),RGB.FromHex(0xFFFFFF),
                RGB.FromHex(0x2E3562), RGB.FromHex(0x237111), 
                RGB.FromHex(0x6BE74F), RGB.FromHex(0xFF0000), 
                RGB.FromHex(0xDF4525), RGB.FromHex(0xE4771D),
                RGB.FromHex(0xDEB522), RGB.FromHex(0x9D9D28),
                RGB.FromHex(0x835706), RGB.FromHex(0x793005),
                RGB.FromHex(0x751E0D), RGB.FromHex(0x551616),
                RGB.FromHex(0x4F2828), RGB.FromHex(0x0DA7F4),
                RGB.FromHex(0xFF0000), RGB.FromHex(0xEE6D16),
                RGB.FromHex(0x69EE16), RGB.FromHex(0x16EED2),
                RGB.FromHex(0x637ED7), RGB.FromHex(0xC2CB43),
                RGB.FromHex(0x49A37C)),
        new("Light",
            RGB.FromHex(0xE8E8E8), RGB.FromHex(0xFFFFFF), RGB.FromHex(0xC8C8C8),
            RGB.FromHex(0x34B314), RGB.FromHex(0x298712),
            RGB.FromHex(0xA2E70E), RGB.FromHex(0x7EB30C),
            RGB.FromHex(0xC80FE4),
            RGB.FromHex(0x000000),
            RGB.FromHex(0x09AD09), RGB.FromHex(0xDE0E0E),
            RGB.FromHex(0x7D7D7D),
            RGB.FromHex(0x3FBADF), RGB.FromHex(0x09AD09), 
            RGB.FromHex(0xFFA500), RGB.FromHex(0xDE0E0E), RGB.FromHex(0x6A178E), 
            RGB.FromHex(0x503D98), RGB.FromHex(0x000000),
            RGB.FromHex(0xA6AAB5), RGB.FromHex(0x1853E8), 
            RGB.FromHex(0x1AADDA), RGB.FromHex(0xF011C3), 
            RGB.FromHex(0xDF4525), RGB.FromHex(0xE4771D),
            RGB.FromHex(0xDEB522), RGB.FromHex(0x9D9D28),
            RGB.FromHex(0x835706), RGB.FromHex(0x793005),
            RGB.FromHex(0x751E0D), RGB.FromHex(0x551616),
            RGB.FromHex(0x4F2828), RGB.FromHex(0x0DA7F4),
            RGB.FromHex(0xFF0000), RGB.FromHex(0xEE6D16),
            RGB.FromHex(0x69EE16), RGB.FromHex(0x16EED2),
            RGB.FromHex(0x637ED7), RGB.FromHex(0xC2CB43),
            RGB.FromHex(0x49A37C)),
        new("Dark",
            RGB.FromHex(0x1B1B1B), RGB.FromHex(0x2D2D2D), RGB.FromHex(0x373737),
            RGB.FromHex(0x34B314), RGB.FromHex(0x298712),
            RGB.FromHex(0xA2E70E), RGB.FromHex(0x7EB30C),
            RGB.FromHex(0xC80FE4),
            RGB.FromHex(0xFFFFFF),
            RGB.FromHex(0x09AD09), RGB.FromHex(0xDE0E0E),
            RGB.FromHex(0x7D7D7D),
            RGB.FromHex(0x3FBADF), RGB.FromHex(0x09AD09), 
            RGB.FromHex(0xFFA500), RGB.FromHex(0xDE0E0E), RGB.FromHex(0x6A178E), 
            RGB.FromHex(0x503D98),RGB.FromHex(0xFFFFFF),
            RGB.FromHex(0xA6AAB5), RGB.FromHex(0x1853E8), 
            RGB.FromHex(0x1AADDA), RGB.FromHex(0xF011C3), 
            RGB.FromHex(0xDF4525), RGB.FromHex(0xE4771D),
            RGB.FromHex(0xDEB522), RGB.FromHex(0x9D9D28),
            RGB.FromHex(0x835706), RGB.FromHex(0x793005),
            RGB.FromHex(0x751E0D), RGB.FromHex(0x551616),
            RGB.FromHex(0x4F2828), RGB.FromHex(0x0DA7F4),
            RGB.FromHex(0xFF0000), RGB.FromHex(0xEE6D16),
            RGB.FromHex(0x69EE16), RGB.FromHex(0x16EED2),
            RGB.FromHex(0x637ED7), RGB.FromHex(0xC2CB43),
            RGB.FromHex(0x49A37C))
    
    };

    public IReadOnlyList<Theme> GetThemes() => _themes;

    public int Count() => _themes.Length;

    public void AddTheme(Theme theme) { }
    public void ChangeTheme(int index, Theme newTheme) { }
}