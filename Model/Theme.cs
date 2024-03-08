using Model.Utility;

namespace Model;

public record Theme(string Name, RGB Background1, RGB Background2, RGB Background3,
    RGB Primary1, RGB Primary2, RGB Secondary1, RGB Secondary2,
    RGB Accent, RGB Text, RGB ThumbColor) : INamed;