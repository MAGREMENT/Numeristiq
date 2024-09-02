using Model.Repositories;
using Model.Utility;

namespace Repository.HardCoded;

public class HardCodedThemeRepository : IThemeRepository
{
    private readonly Theme[] _themes =
    {
        new("Abyss",
                RGB.FromHex(0x0E0E15), RGB.FromHex(0x1C1C2C), RGB.FromHex(0x272742),
                RGB.FromHex(0x272742),
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
                RGB.FromHex(0x49A37C))
    };

    public IReadOnlyList<Theme> GetThemes() => _themes;

    public int Count() => _themes.Length;

    public void AddTheme(Theme theme) { }
    public void ChangeTheme(int index, Theme newTheme) { }
    public Theme? FindTheme(string name)
    {
        return _themes.FirstOrDefault(t => t is not null && t.Name.Equals(name), null);
    }

    public void ClearThemes()
    {
        
    }
}