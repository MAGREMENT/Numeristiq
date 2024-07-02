using Model.Core;
using Model.Core.Changes;
using Model.Sudokus;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Strategies;
using Model.Sudokus.Solver.Strategies.BlossomLoops;
using Model.Sudokus.Solver.Strategies.BlossomLoops.BranchFinder;
using Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;
using Model.Sudokus.Solver.Strategies.BlossomLoops.Types;
using Model.Sudokus.Solver.Strategies.UniquenessClueCover;
using Model.Sudokus.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;

namespace Tests.Sudokus;

public class SudokuStrategiesTests
{
    private readonly SudokuSolver _solver = new();

    [TearDown]
    public void TearDown()
    {
        _solver.StrategyManager.ClearStrategies();
    }
    
    #region SimpleColoring
    
    [Test]
    public void SimpleColoringTwiceInSameUnitTest()
    {
        TestSudokuStrategyInstance(new SimpleColoringStrategy(),
            "05k088880h03k011210hk81121c005s00348c0032148g111c40c0h0911c00305g1210hc0030h05c0214811g188c021g111880h440c0311810h0503210941g1g105480h4881032111214803g111480h8105",
            new NumericChange(ChangeType.SolutionAddition, 7, 1, 4),
            new NumericChange(ChangeType.SolutionAddition, 7, 4, 3),
            new NumericChange(ChangeType.SolutionAddition, 7, 3, 8),
            new NumericChange(ChangeType.SolutionAddition, 7, 5, 0),
            new NumericChange(ChangeType.SolutionAddition, 7, 7, 2),
            new NumericChange(ChangeType.SolutionAddition, 7, 8, 5));
    }

    [Test]
    public void SimpleColoringTwoColoursElsewhereTest()
    {
        TestSudokuStrategyInstance(new SimpleColoringStrategy(),
            "05l8d8c80h03s018210hl8d821d005s00348c00321c8g158c41c0h0950d00305g1210hc0030h05c8214811g1c8c021g111c80h440c0311810h0503210941g1g105480h4881032111214803g158580h8105",
            new NumericChange(ChangeType.PossibilityRemoval, 8, 1, 2),
            new NumericChange(ChangeType.PossibilityRemoval, 8, 2, 3));
    }
    
    #endregion

    #region XYWing

    [Test]
    public void XYWingTest1()
    {
        TestSudokuStrategyInstance(new XYWingStrategy(),
            "g188aa050hca5090b0cg118g21g1c0050903ea05aa8211ca4gg1agaig1bi41228g09059g8i8g05g10911218i41aq41bq8g2205g19i9g8g21g11i050i9i41091103880o41g18g2105050o411q81211i1gg1",
            new NumericChange(ChangeType.PossibilityRemoval, 4, 6, 3),
            new NumericChange(ChangeType.PossibilityRemoval, 4, 6, 5));
    }

    [Test]
    public void XYWingTest2()
    {
        TestSudokuStrategyInstance(new XYWingStrategy(),
            "2111k8co4oso03480505g2812248i20h48110a0h4a11054a81g121gi09210m810ig41141gg81h04s215og4030o4105120qg11q21810o11410hg10305092181g8i005a811a8410h0381220a6o4o6o1105g1",
            new NumericChange(ChangeType.PossibilityRemoval, 6, 7, 3));
    }

    #endregion

    #region BUG-Lite
    
    [Test]
    public void BugLiteTest1()
    {
        TestSudokuStrategyInstance(new BUGLiteStrategy(16),
            "41p0h021h494090h03820h2118588254g11405hahal00h12218150g2i2410hh409143481o8o80hh42114035c54112805810341gg28ggi205h252812ggg5009i841h81c1c2g8103gg0h1a811a58g1541421",
            new NumericChange(ChangeType.PossibilityRemoval, 9, 6, 0),
            new NumericChange(ChangeType.PossibilityRemoval, 9, 7, 0));
    }

    [Test]
    public void BugLiteTest2()
    {
        TestSudokuStrategyInstance(new BUGLiteStrategy(16),
            "41gg211103098gog050igq058121gg4111go11go8141gg050o0321092141g105118i8g0ig105110h81032868488g8g03210941g105112141g80311gg050o8105110h09418122i0g28282g805gg21114o4o",
            new NumericChange(ChangeType.PossibilityRemoval, 1, 1, 1),
            new NumericChange(ChangeType.PossibilityRemoval, 4, 1, 1));
    }

    [Test]
    public void BugLiteTest3()
    {
        TestSudokuStrategyInstance(new BUGLiteStrategy(16),
            "g121810k410k09110350090h810330g105605003053009g10h60810a84g14k8k4m2188110h413009o0308403g40a843030o40641o80h05h003k02188900h4881h009030k4k1460m4210h41g4118803o0gc",
            new NumericChange(ChangeType.PossibilityRemoval, 6, 7, 8),
            new NumericChange(ChangeType.PossibilityRemoval, 7, 7, 8));
    }

    [Test]
    public void BugLiteTest4()
    {
        TestSudokuStrategyInstance(new BUGLiteStrategy(16),
            "0309054g4g811121g1810h112109g1054242g16060051103810h0941222218810hg11c1409810hl0k4540350210511g14803210oc8cg2gg14a816k542o1e1m2g0582hiig0941929i1142ca4i6k442og18m",
            new NumericChange(ChangeType.PossibilityRemoval, 1, 8, 2),
            new NumericChange(ChangeType.PossibilityRemoval, 7, 8, 2));
    }

    [Test]
    public void BugLiteTest5()
    {
        TestSudokuStrategyInstance(new BUGLiteStrategy(18),
            "900h21g10984034114g105410h11038128289003092184410h14g109h00350218g05o04g05218103k0go11g84g41h00h1884oc28qaaa2181144803g8k00h140i48g1054g1168aaaa0i481481kg21k8140a",
            new NumericChange(ChangeType.PossibilityRemoval, 2, 5, 5),
            new NumericChange(ChangeType.PossibilityRemoval, 8, 5, 5));
    }

    #endregion

    #region JuniorExocet

    [Test]
    public void JuniorExocetTest1()
    {
        TestSudokuStrategyInstance(new JuniorExocetStrategy(),
            "929241b205j29aoa0hg1099m9icg5i21c654219i9m09sglid6s6l4cqsioi8keo4k4611i005lghg3g037o48i081dad221g1c8540h464c9ibi09412g06g18k34cg05og2g11g8c8eo035inihi06g881544k7c",
            "- 424 237 737 111 112 124 137 514 516 517 523 529 532 533 536" +
            "814 817 818 823 828 832 833 835 552 556 591 592 599 841 842 845 881 888");
    }

    [Test]
    public void JuniorExocetTest2()
    {
        TestSudokuStrategyInstance(new JuniorExocetStrategy(),
            "haga05gi81goh821412141goh6gchc810kgapaoago2141gch80kgagagq21h4gshc4181g8c005g8c00321g8110hs8oo11s0gos8210305gc2103s4g4s40h4811gcg881kk11kk0348210h114109210305g181",
            "- 224 524 924 339");
    }

    [Test]
    public void JuniorExocetTest3()
    {
        TestSudokuStrategyInstance(new JuniorExocetStrategy(),
            "a4a011030h09a4g141qk41qgb094b403099g0903agg1d4f4b43g9g2g3o05411a123o81g1q0r8410hpcp438300303poog9021p01o4105qgqg0309p0rg41059g118g0905c0cgg103214105qgb0p2ri901g09",
            "- 524 537 637");
    }

    [Test]
    public void JuniorExocetTest4()
    {
        TestSudokuStrategyInstance(new JuniorExocetStrategy(),
            "0m0641guie11imii81g1969m8ma6ag097i7g21099m41o6oggmhihg56n636oc0hu8q2raj881n03gg8m803igjo050mi60911q4q041qiig4ec6g121caco11co4o580hb0o8t805q0u8035af2b2oqtasoqg05mo",
            "- 211 212 224 237 824 937 114 115 117 118 123122 128 133 135 414 417 418" +
            "423 428 429 433 436 141142148 191 192 195 458 496 499");
    }

    [Test]
    public void JuniorExocetTest5()
    {
        TestSudokuStrategyInstance(new JuniorExocetStrategy(),
            "0s4s81ku11kiksikm803215sksscsglspkt81sg15s21cccg039kd82s0ug1116a6i812662419m3kgiq2qih409j2b89a38kaua05l0j20hrk9k3kg6i609hg4182hs5s0381k4l021hgh8r8d878k20hn2h88205",
            "- 924 927 527 212");
    }

    #endregion

    [Test]
    public void XWingTest()
    {
        TestSudokuStrategyInstance(new XWingStrategy(),
            "03c848csc4cs1121g10hg105481121034881c8112103c0g1050h485848g1210h4481140350210hs403c4k81448050381k0091121k00hc80h4811s4cck80321g1c811c821030hc805210503cos0cok8s811",
            "- 714 754 784 794 788 798");
    }

    [Test]
    public void BUGTest()
    {
        TestSudokuStrategyInstance(new BUGStrategy(5),
            "030h0581g11141210950o021054g09gg039050o00921034ggg90058121410305g1091g1g0911030h60608105g1g1050h110981214103214190091g0305g18g050990g13g2g038g410h03g1418105110921",
            "+ 485");
    }

    [Test]
    public void AlignedPairExclusionTest()
    {
        TestSudokuStrategyInstance(new AlignedPairExclusionStrategy(),
            "b0g141039409a40h94b00996940hh441i6p69g0m962141h4o4g609054109g1030h118121g181211c1441030c0h03110h0c2181gcgc41410k949kg103as2c848g0mg14109218k11860921969k9414ok41o6",
            "- 873");
    }
    
    #region BlossomLoop

    [Test]
    public void BlossomLoopTest()
    {
        TestSudokuStrategyInstance(new BlossomLoopStrategy(new BLLoopFinderV1(12), new BLBranchFinderV1(), new CellType()),
            "2105090hh0o2124192og03og94418409akb4cg4g11aea88e0i0mg10i2105g11o0o811241s009s2d694c6i2j60h114gsic68k21g2090609114ie0qgsg05qga24kg14gec03cs7gagb84m81214cgs11kigi0a",
            "- 228 431 463 689 889");
    }

    [Test]
    public void BlossomLoopTest2()
    {
        TestSudokuStrategyInstance(new BlossomLoopStrategy(new BLLoopFinderV1(12), new BLBranchFinderV1(), new UnitType()),
            "p2b2090hj2i241j494l205k0h2ja81hoj81o0hb0i005j841h80398210o0i12410m811cg1g441g2p2p009210h16o68q1121gggk0a4c46521i81ga05gihql821093i0541iiiihi811i42g16i88ai1105484o",
            "- 328 269 171 172 378 182 793");
    }
    
    #endregion

    #region XYZRing

    [Test]
    public void XYZRingTest1()
    {
        TestSudokuStrategyInstance(new XYZRingStrategy(),
            "2a052gga11i841gq81110o41gegc810q21gg2a81g10h6868110a050hg109810311054121052111g8koko81go0381410321gsgs0o11gg411i81h4ikik2i0m09g10q2g41812s2i0m11281o05182o03g18141",
            "- 472 482 395");
    }

    [Test]
    public void XYZRingTest2()
    {
        TestSudokuStrategyInstance(new XYZRingStrategy(),
            "2a052gga11i841gq81110o41gegc810i21gg2a81g10h6868110a050hg109810311054121052111g8koko81gg0381410321gkgk0911gg411281h4ikik2i0m09g10a2g41812s2i0m11281o05182g03g18141",
            "- 686 177");
    }

    [Test]
    public void XYZRingTest3()
    {
        TestSudokuStrategyInstance(new XYZRingStrategy(),
            "g105dg215i4i920982f003f0180548b00hg1093g3g811ig14105220ha0a20cg11122410c7409720m6g6gg1812m6460g10u81682i112u03cgcgg1098g052111b0g1091g3g058g0341b0bg05413iai09g18g",
            "- 661 667 669");
    }

    #endregion
    
    #region UniquenessClueCover

    [Test]
    public void UCCTest1()
    {
        TestSudokuStrategyInstance(new UniquenessClueCoverStrategy(BandCollection.FullCollection()),
            "duumuojstgtceivajadquiuojotgt8eiva05dsukuo03tgtcegv8j8c4u403h009p0640hj08kok1141oi2109g6g24g09mghg05h262n281cqci05g821ka118a0a21114881424eg10e0hg182881c120ha6ae41",
            "- 314 414 514 614 914 319 619 519 919");
    }

    [Test]
    public void UCCTest2()
    {
        TestSudokuStrategyInstance(new UniquenessClueCoverStrategy(BandCollection.FullCollection()),
            "rqmauidgmq05r8toj0rsm8ugdgmo3orcts03rumauidgmq3qrctsj4i2m20509c2920hp0j0gog811218k8go403412i816i5g4mg1341409i20hi20528411a1881a0110903agag41g4g4410582g111880a210h",
            "- 336 436 536 636 539 639 939");
    }
    
    #endregion

    private void TestSudokuStrategyInstance(SudokuStrategy strategy, string stateBefore32, string expectedAsString)
    {
        List<NumericChange> progresses = new();

        var type = ChangeType.PossibilityRemoval;
        int[] d = { -1, -1, -1 };
        int cursor = 0;
        foreach (var c in expectedAsString)
        {
            switch (c)
            {
                case '-' : type = ChangeType.PossibilityRemoval;
                    break;
                case '+' : type = ChangeType.SolutionAddition;
                    break;
                case ' ' : break;
                default:
                    var n = c - '0';
                    d[cursor++] = n;

                    if (cursor == 3)
                    {
                        progresses.Add(new NumericChange(type, d[0], d[1] - 1, d[2] - 1));
                        cursor = 0;
                    }

                    break;
            }
        }
        
        TestSudokuStrategyInstance(strategy, stateBefore32, progresses.ToArray());
    }

    private void TestSudokuStrategyInstance(SudokuStrategy strategy, string stateBefore32, params NumericChange[] expected)
    {
        strategy.InstanceHandling = InstanceHandling.UnorderedAll;
        _solver.StrategyManager.AddStrategy(strategy);
        _solver.SetState(SudokuTranslator.TranslateBase32Format(stateBefore32, new AlphabeticalBase32Translator()));
        _solver.Solve(true);

        List<NumericChange> progresses = new();
        foreach (var log in _solver.Steps)
        {
            progresses.AddRange(log.Changes);
        }
        
        Assert.Multiple(() =>
        {
            foreach (var progress in expected)
            {
                Assert.That(progresses, Does.Contain(progress));
            }
        });
    }
}