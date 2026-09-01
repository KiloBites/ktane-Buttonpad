using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using static ButtonPosition;
using static Symbol;
using static ButtonColor;
using static UnityEngine.Random;

public class ButtonpadGenerator
{
    private readonly KMBombInfo _bomb;

    private ButtonInfo[] _buttons, _buttonsSorted;
    private LEDInfo[] _leds;
    private int[] _rowDigitsForSubmission;
    private int _columnIndex;

    private static readonly ButtonColor[] _buttonColorByColumn = { Yellow, Blue, Red, White, Blue, White, Yellow, Red };

    private static readonly Symbol[] _symbolTable =
    {
        Copyright, WeirdBike, WeirdNose, SpeechBubble, Dragon, AT, Circle, Six,
        FilledStar, MeltedThree, AT, AE, RightC, Cursive, QuestionMark, BT,
        HollowStar, SquidKnife, Balloon, UpsideDownY, Tracks, Euro, WeirdNose, Crucible,
        SmileyFace, Paragraph, HookN, Euro, MeltedThree, Six, SpeechBubble, NWithHat,
        DoubleK, LeftC, SquidKnife, Clover, Pumpkin, Tripod, SmileyFace, Teepee,
        Omega, Dragon, Tracks, Crucible, Pitchfork, RightC, Six, UpsideDownY,
        SquidKnife, BT, Dragon, HollowStar, Clover, SmileyFace, Paragraph, Balloon,
        Pumpkin, Teepee, BT, Six, Copyright, SquigglyN, NWithHat, Cursive,
        HookN, Tripod, WeirdBike, LeftC, WeirdNose, Paragraph, Teepee, Circle,
        Teepee, AE, SmileyFace, Copyright, Balloon, Pumpkin, Crucible, WeirdNose,
        Six, WeirdNose, Circle, WeirdBike, FilledStar, SpeechBubble, DoubleK, Omega,
        SquigglyN, Crucible, DoubleK, Circle, HookN, NWithHat, Euro, Clover,
        AT, DoubleK, Omega, SquidKnife, Cursive, Teepee, LeftC, Tracks,
        AE, Six, SpeechBubble, RightC, SmileyFace, HookN, Dragon, Euro,
        MeltedThree, Pitchfork, UpsideDownY, SmileyFace, Paragraph, WeirdNose, WeirdBike, DoubleK,
        Euro, Pumpkin, LeftC, SquigglyN, BT, MeltedThree, Omega, Pitchfork,
        Circle, Omega, QuestionMark, Pumpkin, SpeechBubble, Pitchfork, BT, HookN,
        NWithHat, HollowStar, Six, Dragon, WeirdBike, AE, SquidKnife, Copyright,
        Dragon, Euro, HollowStar, Tracks, Tripod, DoubleK, Clover, MeltedThree,
        QuestionMark, SmileyFace, MeltedThree, Balloon, HollowStar, LeftC, RightC, SquigglyN,
        Paragraph, NWithHat, Pumpkin, Tripod, Circle, UpsideDownY, AT, AE,
        RightC, Cursive, FilledStar, HookN, UpsideDownY, WeirdBike, Pitchfork, QuestionMark,
        Pitchfork, AT, Copyright, DoubleK, Crucible, Tracks, HookN, LeftC,
        Tripod, SpeechBubble, RightC, Teepee, Omega, FilledStar, Balloon, SquidKnife,
        Cursive, RightC, AE, Pitchfork, NWithHat, Balloon, HollowStar, WeirdBike,
        Tracks, HookN, Tripod, Cursive, DoubleK, HollowStar, Pumpkin, Dragon,
        Balloon, Clover, Crucible, QuestionMark, LeftC, Copyright, Cursive, Paragraph,
        WeirdNose, Circle, SquigglyN, MeltedThree, AE, BT, FilledStar, AT,
        UpsideDownY, Tracks, Teepee, Omega, SquigglyN, Clover, MeltedThree, Tripod,
        LeftC, QuestionMark, Clover, AT, QuestionMark, Omega, AE, SmileyFace,
        WeirdBike, UpsideDownY, Euro, BT, SquidKnife, Crucible, Tracks, FilledStar,
        BT, SquigglyN, NWithHat, Paragraph, Teepee, Dragon, Copyright, SpeechBubble,
        Clover, Balloon, Pitchfork, FilledStar, Euro, Circle, Tripod, HollowStar,
        Crucible, FilledStar, Paragraph, WeirdNose, AT, SquidKnife, UpsideDownY, RightC,
        SpeechBubble, Copyright, Cursive, NWithHat, Six, QuestionMark, SquigglyN, Pumpkin
    };
    
    //ROYGBPW

    private static readonly LEDColor[] _ledColorTable = new[]
    {
        "ROYBWRPG",
        "BPWYGROO",
        "WROGBYRR",
        "PPBPOGYG",
        "YYGPYPGY",
        "OOYYOBBR",
        "WBRYWWWW",
        "BPOOBPPW",
        "RYGWPBYP",
        "PPPGGWBO",
        "YBRWWGWR",
        "RPGOGWBO",
        "YBYOYOGW",
        "WWOWBYOR",
        "RBGRORPY",
        "GRYBPWRB",
        "WWWOPOYB",
        "PGPGRPPW",
        "YWWWYPGP",
        "GYRPPBRB",
        "POBGBBBP",
        "OGOPROOY",
        "YGYBWBWP",
        "BRGRYGGY",
        "OOPBORYR",
        "YORRGYYB",
        "BYWGRPBG",
        "GGRBYWOO",
        "PWWRRRGP",
        "OGBYGYPG",
        "GBOOBGRP",
        "RWPPWORO",
        "GRBYOYWB",
        "ORYWPGOG",
        "WBPRROWY"
    }.SelectMany(x => x.Select(y => (LEDColor)"ROYGBPW".IndexOf(y))).ToArray();

    private static readonly int[] _rowNumbers =
    {
        1,
        9,
        3,
        4,
        8,
        2,
        6,
        7,
        0,
        5,
        8,
        1,
        7,
        0,
        5,
        4,
        8,
        3,
        0,
        8,
        2,
        6,
        7,
        4,
        3,
        1,
        8,
        6,
        8,
        5,
        0,
        1,
        4,
        7,
        0
    };

    private static readonly ButtonPosition[][] _possibleMappings =
    {
        new[] { TR, BL, BR, TL },
        new[] { TR, BR, TL, BL },
        new[] { BL, BR, TR, TL },
        new[] { BL, TL, BR, TR },
        new[] { BR, BL, TL, TR },
        new[] { BR, TL, TR, BL }
    };

    public ButtonpadGenerator(KMBombInfo bomb)
    {
        _bomb = bomb;
        GenerateSet();
    }

    public LEDInfo[] GetLEDS() => _leds;

    private void GenerateSet()
    {
        var selectedIndices = Enumerable.Range(0, 35).ToList().Shuffle().Take(4).OrderBy(x => x).ToArray();

        _columnIndex = Range(0, 8);
        
        var symbolColumn = _symbolTable.GetColumn(_columnIndex);
        var colorColumn = _ledColorTable.GetColumn(_columnIndex);

        _buttonsSorted = selectedIndices.Select(x => new ButtonInfo(symbolColumn[x], (ButtonColor)Range(0, 4))).ToArray();
        _buttons = _buttonsSorted.ToArray().Shuffle();

        var selectedButtonPositions = _possibleMappings.PickRandom();

        _leds = selectedIndices.Select((x, i) => new LEDInfo(selectedButtonPositions[i], _buttons[i], colorColumn[x])).ToArray();
        _rowDigitsForSubmission = selectedIndices.Select(x => _rowNumbers[x]).ToArray();
    }
    
    

    public string GetColoredSymbols()
    {
        var symbolColumn = _symbolTable.GetColumn(_columnIndex);
        var colorColumn = _ledColorTable.GetColumn(_columnIndex);

        var buttonSymbols = _buttonsSorted.Select(x => x.ButtonSymbol).ToArray();

        var buttonOrderIndices = symbolColumn.IndicesOf(buttonSymbols.Contains).ToArray();

        return $"Column {_columnIndex + 1} has been selected with the following order: {buttonOrderIndices.Select(x => $"{colorColumn[x]} {symbolColumn[x]}").Join(", ")}";
    }
    
    public ButtonPosition GetExpectedPosition(int index) => (ButtonPosition)Array.IndexOf(_buttons, _buttonsSorted[index]);
    
    public ButtonPosition GetExpectedPositionFromButton(ButtonInfo button) => (ButtonPosition)Array.IndexOf(_buttons, button);

    public int GetDigitForSubmission(ButtonInfo button) => _rowDigitsForSubmission[Array.IndexOf(_buttonsSorted, button)];
    
    public bool CheckButtons(IEnumerable<ButtonInfo> buttonsToCheck, int pressedSoFar) => (pressedSoFar == 4 ? _buttonsSorted : _buttonsSorted.Take(pressedSoFar)).SequenceEqual(buttonsToCheck);

    public static int CalculateDigitalRoot(LEDInfo led)
    {
        var buttonColorIndices = _buttonColorByColumn.IndicesOf(x => led.Button.ButtonColor == x).ToArray();
        
        var symbolColumns = buttonColorIndices.Select(_symbolTable.GetColumn).ToArray();

        var symbolIndices = symbolColumns.Select(x => _rowNumbers[x.IndexOf(y => led.Button.ButtonSymbol == y)]).ToArray();
        
        return ButtonpadTools.DigitalRoot(symbolIndices[0], symbolIndices[1]);
    }

    public override string ToString()
    {
        var appliedRule = Enumerable.Range(0, 10).First(CheckRule);

        string rule;

        var firstButtonPositions = GetFirstButtonToHold().Select(x => (ButtonPosition)Array.IndexOf(_buttons, x)).ToArray();
        
        switch (appliedRule)
        {
            case 0:
                rule = "There is a blue button with the right C symbol on it";
                break;
            case 1:
                rule = "Exactly three buttons are the same color";
                break;
            case 2:
                rule = "There are two or more batteries on the bomb, and one of the buttons has a crucible symbol on it";
                break;
            case 3:
                rule = "There is a serial port and a BOB indicator present on the bomb";
                break;
            case 4:
                rule = "There is a button with the Six or Copyright symbol on it";
                break;
            case 5:
                rule = "None of the buttons contained the Question Mark symbol, and an unlit NSA indicator is present on the bomb";
                break;
            case 6:
                rule = "Exactly one button has either the Weird Bike, Hook N, or Clover symbol on it";
                break;
            case 7:
                rule = "All but one button has either the Squidknife, Pumpkin, Smiley Face, or the Euro symbol on it";
                break;
            case 8:
                rule = "None of the symbols in the above rules appeared on any of the buttons";
                break;
            default:
                rule = "None of the rules apply";
                break;
        }

        return $"Rule {appliedRule + 1} applies: {rule}. The first correct button(s) to hold are: {firstButtonPositions.Join(", ")}";
    }

    public IEnumerable<ButtonInfo> GetFirstButtonToHold()
    {
        var appliedRule = Enumerable.Range(0, 10).First(CheckRule);
        
        var rule5Symbols = new[] { Six, Copyright };
        var rule7Symbols = new[] { WeirdBike, HookN, Clover };
        var rule8Symbols = new[] { SquidKnife, Pumpkin, SmileyFace, Euro };

        switch (appliedRule)
        {
            case 0:
                return _buttons.Where(x => x.ButtonColor == Blue && x.ButtonSymbol == RightC);
            case 1:
                return _buttons.GroupBy(x => x.ButtonColor).Where(x => x.Count() == 1).Select(x => x.First());
            case 2:
                return new[] { _buttons.Last() };
            case 3:
                return _buttons.All(x => x.ButtonColor != Blue) ? new[] { _buttons.First() } : _buttons.Where(x => x.ButtonColor == Blue);
            case 4:
                return _buttons.Where(x => !rule5Symbols.Contains(x.ButtonSymbol));
            case 5:
                return _buttons.GroupBy(x => x.ButtonColor).Count(x => x.Count() == 1) == 0 ? new[] { _buttons[1] } : _buttons.GroupBy(x => x.ButtonColor).Where(x => x.Count() == 1).Select(x => x.First());
            case 6:
                return Enumerable.Range(0, 4).Where(x => rule7Symbols.Contains(_buttons[x].ButtonSymbol)).Select(x => GetAdjacent((ButtonPosition)x));
            case 7:
                return Enumerable.Range(0, 4).Where(x => !rule8Symbols.Contains(_buttons[x].ButtonSymbol)).Select(x => GetAdjacent((ButtonPosition)x, true));
            case 8:
                return _buttons.GroupBy(x => x.ButtonColor).Count(x => x.Count() == 1) == 0 ? new[] { _buttons[2] } : _buttons.GroupBy(x => x.ButtonColor).Where(x => x.Count() == 1).Select(x => GetDiametricOpposite((ButtonPosition)Array.IndexOf(_buttons, x.First())));
            default:
                return new[] { _buttons.First() };
        }
    }

    private ButtonInfo GetAdjacent(ButtonPosition position, bool needVertical = false)
    {
        var horiz = new[] { TR, TL, BR, BL };
        var vert = new[] { BL, BR, TL, TR };
        
        var index = (int)(needVertical ? vert : horiz)[(int)position];
        
        return _buttons[index];
    }

    private ButtonInfo GetDiametricOpposite(ButtonPosition position)
    {
        var diametricOpposition = new[] { BR, BL, TR, TL };
        
        var index = (int)diametricOpposition[(int)position];
        
        return _buttons[index];
    }

    private bool CheckRule(int index)
    {
        var rule5Symbols = new[] { Six, Copyright };
        var rule7Symbols = new[] { WeirdBike, HookN, Clover };
        var rule8Symbols = new[] { SquidKnife, Pumpkin, SmileyFace, Euro };

        switch (index)
        {
            case 0:
                return _buttons.Any(x => x.ButtonSymbol == RightC && x.ButtonColor == Blue);
            case 1:
                return _buttons.Select(x => x.ButtonColor).GroupBy(x => x).Any(x => x.Count() == 3);
            case 2:
                return _bomb.GetBatteryCount() >= 2 && _buttons.Any(x => x.ButtonSymbol == Crucible);
            case 3:
                return _bomb.IsPortPresent(Port.Serial) && _bomb.IsIndicatorPresent(Indicator.BOB);
            case 4:
                return _buttons.Select(x => x.ButtonSymbol).Any(rule5Symbols.Contains);
            case 5:
                return _buttons.All(x => x.ButtonSymbol != QuestionMark) && _bomb.IsIndicatorOff(Indicator.NSA);
            case 6:
                return _buttons.Select(x => x.ButtonSymbol).Count(rule7Symbols.Contains) == 1;
            case 7:
                return _buttons.Select(x => x.ButtonSymbol).Count(rule8Symbols.Contains) == 3;
            case 8:
                var checkAllSymbols = new[] { RightC, Crucible }.Concat(rule5Symbols).Concat(rule7Symbols).Concat(rule8Symbols).ToList();
                return _buttons.Select(x => x.ButtonSymbol).All(x => !checkAllSymbols.Contains(x));
            default:
                return true;
        }
    }
}