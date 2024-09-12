using Model.Repositories;
using Model.Utility;

namespace Repository.HardCoded;

public class HardCodedThemeRepository : IThemeRepository
{
    private readonly Theme[] _themes =
    {
        new("Abyss",                                                                          
            RGB.FromHex(921109), RGB.FromHex(1842220), RGB.FromHex(2236983), RGB.FromHex(2565954),     
            RGB.FromHex(9571314), RGB.FromHex(10703844), RGB.FromHex(15798227), RGB.FromHex(15031506), 
            RGB.FromHex(6863844), RGB.FromHex(16777215), RGB.FromHex(711178), RGB.FromHex(14552590),   
            RGB.FromHex(11842740), RGB.FromHex(4176607), RGB.FromHex(634121), RGB.FromHex(16753920),   
            RGB.FromHex(14552590), RGB.FromHex(6952846), RGB.FromHex(5258648), RGB.FromHex(16777215),  
            RGB.FromHex(3028322), RGB.FromHex(2322705), RGB.FromHex(7071567), RGB.FromHex(16711680),   
            RGB.FromHex(14632229), RGB.FromHex(14972701), RGB.FromHex(14595362), RGB.FromHex(10329384),
            RGB.FromHex(8607494), RGB.FromHex(7942149), RGB.FromHex(7675405), RGB.FromHex(5576214),    
            RGB.FromHex(5187624), RGB.FromHex(894964), RGB.FromHex(16711680), RGB.FromHex(15625494),   
            RGB.FromHex(6942230), RGB.FromHex(1502930), RGB.FromHex(6520535), RGB.FromHex(12766019),   
            RGB.FromHex(4825980))   
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