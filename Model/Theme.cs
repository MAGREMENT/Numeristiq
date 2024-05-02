using Model.Utility;
using Model.Utility.Collections;

namespace Model;

public record Theme(string Name,
    RGB Background1, RGB Background2, RGB Background3,
    RGB Primary1, RGB Primary2,
    RGB Secondary1, RGB Secondary2,
    RGB Accent,
    RGB Text,
    RGB On, RGB Off,
    RGB Disabled,
    RGB DifficultyNone, RGB DifficultyBasic, RGB DifficultyEasy, RGB DifficultyMedium, RGB DifficultyHard,
        RGB DifficultyExtreme, RGB DifficultyInhuman, RGB DifficultyByTrial,
    RGB ChangeColorationNeutral, RGB ChangeColorationChangeOne, RGB ChangeColorationChangeTwo,
        RGB ChangeColorationCauseOffOne, RGB ChangeColorationCauseOffTwo, RGB ChangeColorationCauseOffThree,
        RGB ChangeColorationCauseOffFour, RGB ChangeColorationCauseOffFive, RGB ChangeColorationCauseOffSix,
        RGB ChangeColorationCauseOffSeven, RGB ChangeColorationCauseOffEight, RGB ChangeColorationCauseOffNine,
        RGB ChangeColorationCauseOffTen, RGB ChangeColorationCauseOnOne,
    RGB HighlightColorFirst, RGB HighlightColorSecond, RGB HighlightColorThird, RGB HighlightColorFourth,
        RGB HighlightColorFifth, RGB HighlightColorSixth, RGB HighlightColorSeventh) : INamed;