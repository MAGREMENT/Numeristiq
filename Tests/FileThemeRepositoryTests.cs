using Repository;
using Repository.Files;
using Repository.Files.Types;
using Repository.HardCoded;
using Tests.Utility;

namespace Tests;

public class FileThemeRepositoryTests
{
    private readonly List<(FileThemeRepository, string)> _repositories = new();

    [OneTimeSetUp]
    public void SetUp()
    {
        _repositories.Add((new FileThemeRepository("theme-tests", false, true,
            new JsonType<List<ThemeDAO>>()), "JSON"));
        _repositories.Add((new FileThemeRepository("theme-tests", false, true,
            new ThemeNativeType()), "Native"));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        foreach (var repo in _repositories)
        {
            repo.Item1.TearDown();
        }
        
        _repositories.Clear();
    }

    [Test]
    public void SpeedTest()
    {
        var themes = new HardCodedThemeRepository().GetThemes();
        ImplementationSpeedComparator.Compare<FileThemeRepository>(_repositories, 200, repo =>
        {
            foreach (var theme in themes) repo.AddTheme(theme);

            var download = repo.GetThemes();
            Assert.That(download, Has.Count.EqualTo(themes.Count));
            foreach (var theme in themes)
            {
                Assert.That(download.Contains(theme), Is.True);
            }
            
            repo.ClearThemes();
            Assert.That(repo.GetThemes(), Has.Count.EqualTo(0));
        });
    }
}