using Model;
using Model.Utility;

namespace Repository;

public class HardCodedThemeRepository : IRepository<ChosenTheme>
{
    public bool Initialize(bool createNewOnNoneExisting)
    {
        return true;
    }

    public ChosenTheme? Download()
    {
        return new ChosenTheme(new[]
        {
            new Theme(RGB.FromHex(0x0E0E15),RGB.FromHex(0x1C1C2C),RGB.FromHex(0x272742),
                RGB.FromHex(0xFC880B),RGB.FromHex(0xEE810C),RGB.FromHex(0xF52A0A),
                RGB.FromHex(0xE8280B),RGB.FromHex(0xF50EE2),RGB.FromHex(0xFFFFFF),
                RGB.FromHex(0xFFFFFF))
        }, 0);
    }

    public bool Upload(ChosenTheme DAO)
    {
        return false;
    }
}