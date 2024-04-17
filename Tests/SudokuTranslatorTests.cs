using Model.Sudokus;

namespace Tests;

public class SudokuTranslatorTests
{
    private readonly IBase32Translator[] _translators = { new RFC4648Base32Translator(), new AlphabeticalBase32Translator() };
    
    [Test]
    public void Base32Test()
    {
        const string start =
            "+--------------------+--------------------+--------------------+\n" +
            "|8542   <1>    87    |<9>    542    <3>   |<6>    84     842   |\n" +
            "|65432  75432  763   |7521   <8>    7654  |931    941    94321 |\n" +
            "|<9>    8432   863   |21     642    64    |<5>    841    <7>   |\n" +
            "+--------------------+--------------------+--------------------+\n" +
            "|85     9875   <2>   |85     <1>    9865  |<4>    <3>    986   |\n" +
            "|8531   98753  9873  |<4>    965    <2>   |9871   987651 9861  |\n" +
            "|8531   <6>    <4>   |853    <7>    985   |<2>    9851   981   |\n" +
            "+--------------------+--------------------+--------------------+\n" +
            "|<7>    98432  <1>   |82     942    984   |983    9864   <5>   |\n" +
            "|8642   9842   986   |8752   <3>    98754 |9871   987641 98641 |\n" +
            "|843    9843   <5>   |<6>    94     <1>   |9873   <2>    9843  |\n" +
            "+--------------------+--------------------+--------------------+\n";

        foreach (var translator in _translators)
        {
            Console.WriteLine(start);
            var stateBefore = SudokuTranslator.TranslateGridFormat(start, false);
            var asString = SudokuTranslator.TranslateBase32Format(stateBefore, translator);

            Console.WriteLine(asString);
            Assert.That(asString, Has.Length.EqualTo(162));

            var stateAfter = SudokuTranslator.TranslateBase32Format(asString, translator);
            asString = SudokuTranslator.TranslateGridFormat(stateAfter);

            Console.WriteLine(asString);
            Assert.That(stateBefore, Is.EqualTo(stateAfter));
        }
    }
}