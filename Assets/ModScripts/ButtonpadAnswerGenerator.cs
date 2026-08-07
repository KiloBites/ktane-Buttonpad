using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using static Symbol;
using static ButtonColor;
using static ButtonPosition;
using static UnityEngine.Random;

public class ButtonpadAnswerGenerator
{
    
    private static readonly LEDColor[] _ledColumns = new[] { 1, 2, 0, 3, 2, 3, 1, 0 }.Select(x => (LEDColor)x).ToArray();
    
    private static readonly int[] _rowDigits =
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

    private static readonly Symbol[][] _symbolColumns =
    {
        new[]
        {
            Copyright,
            FilledStar,
            HollowStar,
            SmileyFace,
            DoubleK,
            Omega,
            SquidKnife,
            Pumpkin,
            HookN,
            Teepee,
            Six,
            SquigglyN,
            AT,
            AE,
            MeltedThree,
            Euro,
            Circle,
            NWithHat,
            Dragon,
            QuestionMark,
            Paragraph,
            RightC,
            Pitchfork,
            Tripod,
            Cursive,
            Tracks,
            Balloon,
            WeirdNose,
            UpsideDownY,
            LeftC,
            WeirdBike,
            BT,
            Clover,
            Crucible,
            SpeechBubble
        },
        new[]
        {
            WeirdBike,
            MeltedThree,
            SquidKnife,
            Paragraph,
            LeftC,
            Dragon,
            BT,
            Teepee,
            Tripod,
            AE,
            WeirdNose,
            Crucible,
            DoubleK,
            Six,
            Pitchfork,
            Pumpkin,
            Omega,
            HollowStar,
            Euro,
            SmileyFace,
            NWithHat,
            Cursive,
            AT,
            SpeechBubble,
            RightC,
            HookN,
            Clover,
            Circle,
            Tracks,
            QuestionMark,
            UpsideDownY,
            SquigglyN,
            Balloon,
            FilledStar,
            Copyright
        },
        new[]
        {
            WeirdNose,
            AT,
            Balloon,
            HookN,
            SquidKnife,
            Tracks,
            Dragon,
            BT,
            WeirdBike,
            SmileyFace,
            Circle,
            DoubleK,
            Omega,
            SpeechBubble,
            UpsideDownY,
            LeftC,
            QuestionMark,
            Six,
            HollowStar,
            MeltedThree,
            Pumpkin,
            FilledStar,
            Copyright,
            RightC,
            AE,
            Tripod,
            Crucible,
            SquigglyN,
            Teepee,
            Clover,
            Euro,
            NWithHat,
            Pitchfork,
            Paragraph,
            Cursive
        },
        new[]
        {
            SpeechBubble,
            AE,
            UpsideDownY,
            Euro,
            Clover,
            Crucible,
            HollowStar,
            Six,
            LeftC,
            Copyright,
            WeirdBike,
            Circle,
            SquidKnife,
            RightC,
            SmileyFace,
            SquigglyN,
            Pumpkin,
            Dragon,
            Tracks,
            Balloon,
            Tripod,
            HookN,
            DoubleK,
            Teepee,
            Pitchfork,
            Cursive,
            QuestionMark,
            MeltedThree,
            Omega,
            AT,
            BT,
            Paragraph,
            FilledStar,
            WeirdNose,
            NWithHat
        },
        new[]
        {
            Dragon,
            RightC,
            Tracks,
            MeltedThree,
            Pumpkin,
            Pitchfork,
            Clover,
            Copyright,
            WeirdNose,
            Balloon,
            FilledStar,
            HookN,
            Cursive,
            SmileyFace,
            Paragraph,
            BT,
            SpeechBubble,
            WeirdBike,
            Tripod,
            HollowStar,
            Circle,
            UpsideDownY,
            Crucible,
            Omega,
            NWithHat,
            DoubleK,
            LeftC,
            AE,
            SquigglyN,
            QuestionMark,
            SquidKnife,
            Teepee,
            Euro,
            AT,
            Six
        },
        new[]
        {
            AT,
            Cursive,
            Euro,
            Six,
            Tripod,
            RightC,
            SmileyFace,
            SquigglyN,
            Paragraph,
            Pumpkin,
            SpeechBubble,
            NWithHat,
            Teepee,
            HookN,
            WeirdNose,
            MeltedThree,
            Pitchfork,
            AE,
            DoubleK,
            LeftC,
            UpsideDownY,
            WeirdBike,
            Tracks,
            FilledStar,
            Balloon,
            HollowStar,
            Copyright,
            BT,
            Clover,
            Omega,
            Crucible,
            Dragon,
            Circle,
            SquidKnife,
            QuestionMark
        },
        new[]
        {
            Circle,
            QuestionMark,
            WeirdNose,
            SpeechBubble,
            SmileyFace,
            Six,
            Paragraph,
            NWithHat,
            Teepee,
            Crucible,
            DoubleK,
            Euro,
            LeftC,
            Dragon,
            WeirdBike,
            Omega,
            BT,
            SquidKnife,
            Clover,
            RightC,
            AT,
            Pitchfork,
            HookN,
            Balloon,
            HollowStar,
            Pumpkin,
            Cursive,
            FilledStar,
            MeltedThree, 
            AE,
            Tracks,
            Copyright,
            Tripod,
            UpsideDownY,
            SquigglyN
        },
        new[]
        {
            Six,
            BT,
            Crucible,
            NWithHat,
            Teepee,
            UpsideDownY,
            Balloon,
            Cursive,
            Circle,
            WeirdNose,
            Omega,
            Clover,
            Tracks,
            Euro,
            DoubleK,
            Pitchfork,
            HookN,
            Copyright,
            MeltedThree,
            SquigglyN,
            AE,
            QuestionMark,
            LeftC,
            SquidKnife,
            WeirdBike,
            Dragon,
            Paragraph,
            AT,
            Tripod,
            SmileyFace,
            FilledStar,
            SpeechBubble,
            HollowStar,
            RightC,
            Pumpkin
        }
    };
    
    private static readonly ButtonColor[][] _colorColumns = new[]
    {
        "RBWPYOWBRPYRBWRGWPYGPOYBOYBGPOGRGOW",
        "OPRPYOBPYPBPYWBRWGWYOGGROOYGWGBWRRB",
        "YWOBGYROGPRGBOGYWPWRBOYGPRWRWBOPBYP",
        "BYGPPYYOWGWOOWRBOGWPGPBRBRGBRYOPYWR",
        "WGBOYOWBPGWGYBOPPRYPBRWYOGRYRGBWOPR",
        "RRYGPBWPBWGWOYRWOPPBBOBGRYPWRYGOYGO",
        "PORYGBWPYBWBGOPRYPGRBOWGYYBOGPRRWOW",
        "GORGYRWWPOROWRYBBWPBPYPYRBGOPGPOBGY"
    }.Select(x => x.Select(y => (ButtonColor)"ROYGBPW".IndexOf(y)).ToArray()).ToArray();


    private readonly ButtonInfo[] _buttons, _buttonsSorted;
    private readonly KMBombInfo _bomb;

    private int _selectedSet;
    private int[] _selectedIndices, _digits;
    
    public ButtonpadAnswerGenerator(KMBombInfo bomb)
    {
        _selectedSet = Range(0, 8);
        _selectedIndices = Enumerable.Range(0, _symbolColumns[_selectedSet].Length).ToList().Shuffle().Take(4).OrderBy(x => x).ToArray();
        _digits = _selectedIndices.Select(x => _rowDigits[x]).ToArray();
        _buttonsSorted = _selectedIndices.Select(x => new ButtonInfo(_symbolColumns[_selectedSet][x], _colorColumns[_selectedSet][x])).ToArray();
        _buttons = _buttonsSorted.ToArray().Shuffle();
        
        _bomb = bomb;
    }

    public ButtonInfo[] GetButtons() => _buttons.ToArray();

    public IEnumerable<ButtonInfo> GetFirstButtonToPress()
    {
        var getApplicableRuleIndex = Enumerable.Range(0, 10).First(DoesRuleApply);

        switch (getApplicableRuleIndex)
        {
            case 0:
                return _buttons.Where(x => x.ButtonColor == Blue);
            case 1:
                return _buttons.GroupBy(x => x.ButtonColor).First(x => x.Count() == 1);
            case 2:
                return new[] { _buttons.Last() };
            case 3:
                return _buttons.All(x => x.ButtonColor != Blue) ? new[] { _buttons.First() } : _buttons.Where(x => x.ButtonColor == Blue);
            case 4:
                return _buttons.Where(x => x.ButtonSymbol != Six || x.ButtonSymbol != Copyright);
            case 5:
                return _buttons.GroupBy(x => x.ButtonColor).Count(x => x.Count() == 1) == 0 ? new[] { _buttons[1] } : _buttons.GroupBy(x => x.ButtonColor).Where(x => x.Count() == 1).Select(x => x.First());
            case 6:
                return new[] { GetAdjacent((ButtonPosition)Enumerable.Range(0, 4).First(x => new[] { WeirdBike, HookN, Clover }.Contains(_buttons[x].ButtonSymbol)), false) };
            case 7:
                return new[] { GetAdjacent((ButtonPosition)Enumerable.Range(0, 4).First(x => !new[] { SquidKnife, Pumpkin, SmileyFace, Euro }.Contains(_buttons[x].ButtonSymbol)), true) };
            case 8:
                return _buttons.GroupBy(x => x.ButtonColor).Count(x => x.Count() == 1) == 0 ? new[] { _buttons[2] } : _buttons.GroupBy(x => x.ButtonColor).Where(x => x.Count() == 1).Select(x => (ButtonPosition)Array.IndexOf(_buttons, x.First())).Select(GetDiametricOpposite);
            default:
                return new[] { _buttons.First() };
        }
    }

    private ButtonInfo GetAdjacent(ButtonPosition pos, bool isVert)
    {
        var horiz = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [TL] = TR,
            [TR] = TL,
            [BL] = BR,
            [BR] = BL
        };

        var vert = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [TL] = BL,
            [TR] = BR,
            [BL] = TL,
            [BR] = TR
        };
        
        return _buttons[isVert ? (int)vert[pos] : (int)horiz[pos]];
    }

    private ButtonInfo GetDiametricOpposite(ButtonPosition pos)
    {
        var diametric = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [TL] = BR,
            [TR] = BL,
            [BL] = TR,
            [BR] = TL
        };
        
        return _buttons[(int)diametric[pos]];
    }
    
    private bool DoesRuleApply(int index)
    {
        switch (index)
        {
            case 0:
                return _buttons.Any(x => x.ButtonSymbol == RightC);
            case 1:
                return _buttons.Select(x => x.ButtonColor).Distinct().Count() != 4;
            case 2:
                return _bomb.GetBatteryCount() >= 2 && _buttons.Any(x => x.ButtonSymbol == Crucible);
            case 3:
                return _bomb.IsPortPresent(Port.Serial) && _bomb.IsIndicatorPresent(Indicator.BOB);
            case 4:
                return _buttons.Any(x => x.ButtonSymbol == Six || x.ButtonSymbol == Copyright);
            case 5:
                return _buttons.All(x => x.ButtonSymbol != QuestionMark) && _bomb.IsIndicatorOff(Indicator.NSA);
            case 6:
                return _buttons.Count(x => x.ButtonSymbol == WeirdBike || x.ButtonSymbol == HookN || x.ButtonSymbol == Clover) == 1;
            case 7:
                return _buttons.Count(x => x.ButtonSymbol == SquidKnife || x.ButtonSymbol == Pumpkin || x.ButtonSymbol == SmileyFace || x.ButtonSymbol == Euro) == 3;
            case 8:
                var symbolsToCheck = new[] { RightC, Crucible, Six, Copyright, QuestionMark, WeirdBike, HookN, Clover, SquidKnife, Pumpkin, SmileyFace, Euro };
                return !_buttons.Select(x => x.ButtonSymbol).All(symbolsToCheck.Contains);
            case 9:
                return true;
        }
        
        return false;
    }
}