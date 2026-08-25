using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using static ButtonPosition;
using static Symbol;
using static ButtonColor;

public class ButtonpadGenerator
{
    private readonly KMBombInfo _bomb;

    private ButtonInfo[] _buttons;
    private LEDInfo[] _leds;

    public ButtonpadGenerator(KMBombInfo bomb)
    {
        _bomb = bomb;
    }

    public IEnumerable<ButtonInfo> GetFirstButtonToPress()
    {
        var firstRuleIndex = Enumerable.Range(0, 10).First(CheckRule);
        
        var rule5Symbols = new[] { Six, Copyright };
        var rule7Symbols = new[] { WeirdBike, HookN, Clover };
        var rule8Symbols = new[] { SquidKnife, Pumpkin, SmileyFace, Euro };

        switch (firstRuleIndex)
        {
            case 0:
                return _buttons.Where(x => x.ButtonColor == Blue);
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
                return _buttons.Select(x => x.ButtonColor).GroupBy(x => x).Count(x => x.Count() > 1) == 3;
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