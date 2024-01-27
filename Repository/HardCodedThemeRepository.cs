using Global;
using Global.Enums;
using Model;
using Model.SudokuSolving;

namespace Repository;

public class HardCodedThemeRepository : IRepository<ThemeDAO[]>
{
    public bool UploadAllowed { get; set; }

    public void Initialize()
    {
        
    }

    public ThemeDAO[] Download()
    {
        return new ThemeDAO[]
        {
            new(RGB.FromHex(0xFFFFFF), RGB.FromHex(0xF8F8F8), RGB.FromHex(0xF0F0F0), 
                RGB.FromHex(0xFC880B), RGB.FromHex(0xEE810C), RGB.FromHex(0xF52A0A),
                RGB.FromHex(0xE8280B), RGB.FromHex(0xF50EE2), RGB.FromHex(0x858585),
                RGB.FromHex(0x000000), IconColor.Black),
            new (RGB.FromHex(0x282828), RGB.FromHex(0x2F2F2F), RGB.FromHex(0x373737), 
                RGB.FromHex(0xFC880B), RGB.FromHex(0xEE810C), RGB.FromHex(0xF52A0A),
                RGB.FromHex(0xE8280B), RGB.FromHex(0xF50EE2), RGB.FromHex(0x858585),
                RGB.FromHex(0xFFFFFF), IconColor.White)
        };
    }

    public void Upload(ThemeDAO[] DAO)
    {
        
    }

    public void New(ThemeDAO[] DAO)
    {
        
    }
}