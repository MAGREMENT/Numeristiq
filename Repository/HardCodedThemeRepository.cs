using Model;
using Model.Utility;

namespace Repository;

public class HardCodedThemeRepository : IRepository<Theme[]>
{
    public bool Initialize(bool createNewOnNoneExisting)
    {
        return true;
    }

    public Theme[] Download()
    {
        return new[]
        {
            new Theme("Default Dark", RGB.FromHex(0x0E0E15), RGB.FromHex(0x1C1C2C),
                RGB.FromHex(0x272742), RGB.FromHex(0xFC880B), RGB.FromHex(0xEE810C),
                RGB.FromHex(0xF52A0A), RGB.FromHex(0xE8280B), RGB.FromHex(0xF50EE2),
                RGB.FromHex(0xFFFFFF), RGB.FromHex(0xFFFFFF)),
            new Theme("Default Light", RGB.FromHex(0xC7DDE2), RGB.FromHex(0xD4E8EC),
                RGB.FromHex(0xEDFBFF), RGB.FromHex(0xFC880B), RGB.FromHex(0xEE810C),
                RGB.FromHex(0xF52A0A), RGB.FromHex(0xE8280B), RGB.FromHex(0xF50EE2),
                RGB.FromHex(0x000000), RGB.FromHex(0x000000))
        };
    }

    public bool Upload(Theme[] DAO)
    {
        return false;
    }
}