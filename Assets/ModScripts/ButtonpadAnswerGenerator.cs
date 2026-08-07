using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using static Symbol;
using static ButtonColor;
using static ButtonPosition;

public class ButtonpadAnswerGenerator
{
    private readonly ButtonInfo[] _buttons;
    private readonly KMBombInfo _bomb;
    
    
    public ButtonpadAnswerGenerator(KMBombInfo bomb)
    {
        
        _bomb = bomb;
    }

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