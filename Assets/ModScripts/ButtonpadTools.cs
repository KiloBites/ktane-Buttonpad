using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;

public static class ButtonpadTools
{
    private static bool FindRule(KMBombInfo bomb, ButtonInfo[] assignedButtons, int index)
    {
        switch (index)
        {
            case 0:
                return assignedButtons.Any(x => x.ButtonColor == ButtonColor.Blue && x.ButtonSymbol == Symbol.RightC);
            case 1:
                return assignedButtons.Select(x => x.ButtonColor).Distinct().Count() == 1;
            case 2:
                return bomb.GetBatteryCount() >= 2 && assignedButtons.Any(x => x.ButtonSymbol == Symbol.Crucible);
            case 3:
                return bomb.IsPortPresent(Port.Serial) && bomb.IsIndicatorPresent(Indicator.BOB);
            case 4:
                return assignedButtons.Any(x => x.ButtonSymbol == Symbol.Six || x.ButtonSymbol == Symbol.Copyright);
            case 5:
                return assignedButtons.All(x => x.ButtonSymbol != Symbol.QuestionMark) && bomb.IsIndicatorOff(Indicator.NSA);
            case 6:
                return assignedButtons.Count(x => x.ButtonSymbol == Symbol.WeirdBike || x.ButtonSymbol == Symbol.HookN || x.ButtonSymbol == Symbol.Clover) == 1;
            case 7:
                return assignedButtons.Count(x => x.ButtonSymbol == Symbol.SquidKnife || x.ButtonSymbol == Symbol.Pumpkin || x.ButtonSymbol == Symbol.SmileyFace || x.ButtonSymbol == Symbol.Euro) == 3;
            case 8:
                return NoSymbolRulesApply(bomb, assignedButtons);
        }

        return false;
    }

    private static bool NoSymbolRulesApply(KMBombInfo bomb, ButtonInfo[] assignedButtons)
    {
        var vetoNumbers = new[] { 1, 3 };
        
        return Enumerable.Range(0, 8).Where(x => !vetoNumbers.Contains(x)).All(x => !FindRule(bomb, assignedButtons, x));
    }

    public static string LogRule(KMBombInfo bomb, ButtonInfo[] assignedButtons)
    {
        if (Enumerable.Range(0, 9).All(x => !FindRule(bomb, assignedButtons, x)))
            return "None of the rules apply. Hold the bottom left button.";

        var firstApplicableRule = Enumerable.Range(0, 9).IndexOf(x => FindRule(bomb, assignedButtons, x));

        var buttons = DetermineCorrectButton(bomb, assignedButtons).Join(", ");
        
        switch (firstApplicableRule)
        {
            case 0:
                return $"Any button is blue and has the Forward C on it. Hold that button. ({buttons})";
            case 1:
                return $"Three of the buttons are the same color. Hold the button with the unique color ({buttons})";
            case 2:
                return $"Two or more batteries are present and one of the buttons does have the crucible on it. Hold the bottom-right button.";
            case 3:
                return "A serial port and a BOB indicator are present." + (assignedButtons.All(x => x.ButtonColor != ButtonColor.Blue) ? "Hold the top-left button" : $"Hold any of the buttons that are blue ({buttons})");
            case 4:
                return $"A button has either the six, or the copyright symbol. Hold any button that doesn't have those ({buttons}).";
        }

        return null;
    }

    public static IEnumerable<ButtonPosition> DetermineCorrectButton(KMBombInfo bomb, ButtonInfo[] assignedButtons)
    {
        if (Enumerable.Range(0, 9).All(x => !FindRule(bomb, assignedButtons, x)))
            return new[] { ButtonPosition.BL };
        
        var firstApplicableRule = Enumerable.Range(0, 9).IndexOf(x => FindRule(bomb, assignedButtons, x));
        
        var positions = (ButtonPosition[])Enum.GetValues(typeof(ButtonPosition));

        switch (firstApplicableRule)
        {
            case 0:
                return positions.Where(x => assignedButtons[(int)x].ButtonColor == ButtonColor.Blue);
            case 1:
                return positions.GroupBy(x => assignedButtons[(int)x].ButtonColor).Where(x => x.Count() == 1).SelectMany(x => x);
            case 2:
                return new[] { ButtonPosition.BR };
            case 3:
                return assignedButtons.All(x => x.ButtonColor != ButtonColor.Blue) ? new[] { ButtonPosition.TL } : positions.Where(x => assignedButtons[(int)x].ButtonColor == ButtonColor.Blue);
            case 4:
                return positions.Where(x =>  assignedButtons[(int)x].ButtonSymbol != Symbol.Six || assignedButtons[(int)x].ButtonSymbol == Symbol.Copyright);
            case 5:
                return assignedButtons.Select(x => x.ButtonColor).Distinct().Count() == 1 ? new[] { ButtonPosition.TR } : positions.GroupBy(x => assignedButtons[(int)x].ButtonColor).Where(x => x.Count() == 1).SelectMany(x => x);
            case 6:
                var horizPos = assignedButtons.IndexOf(x => x.ButtonSymbol == Symbol.WeirdBike || x.ButtonSymbol == Symbol.HookN || x.ButtonSymbol == Symbol.Clover);
                return new[] { (ButtonPosition)Array.IndexOf(assignedButtons, GetAdjacentButton(assignedButtons, assignedButtons[horizPos], false)) };
            case 7:
                var vertPos = (int)positions.Single(x => assignedButtons[(int)x].ButtonSymbol != Symbol.SquidKnife && assignedButtons[(int)x].ButtonSymbol != Symbol.Pumpkin && assignedButtons[(int)x].ButtonSymbol != Symbol.SmileyFace && assignedButtons[(int)x].ButtonSymbol != Symbol.Euro);
                return new[] { (ButtonPosition)Array.IndexOf(assignedButtons, GetAdjacentButton(assignedButtons, assignedButtons[vertPos], true)) };
            case 8:
                return assignedButtons.Select(x => x.ButtonColor).Distinct().Count() == 1 ? new[] { ButtonPosition.BR } : positions.GroupBy(x => assignedButtons[(int)x].ButtonColor).Where(x => x.Count() == 1).SelectMany(x => x).Select(x => (ButtonPosition)Array.IndexOf(assignedButtons, GetDiametricallyOppositeButton(assignedButtons, assignedButtons[(int)x])));
        }

        return null;
    }

    private static ButtonInfo GetAdjacentButton(ButtonInfo[] assignedButtons, ButtonInfo selectedButton, bool needsVertical)
    {
        var horiz = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [ButtonPosition.TL] = ButtonPosition.TR,
            [ButtonPosition.TR] = ButtonPosition.TL,
            [ButtonPosition.BL] = ButtonPosition.BR,
            [ButtonPosition.BR] = ButtonPosition.BL
        };

        var vert = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [ButtonPosition.TL] = ButtonPosition.BL,
            [ButtonPosition.TR] = ButtonPosition.BR,
            [ButtonPosition.BL] = ButtonPosition.TL,
            [ButtonPosition.BR] = ButtonPosition.TR
        };
        
        var buttonPosition = (ButtonPosition)Array.IndexOf(assignedButtons, selectedButton);
        
        return assignedButtons[needsVertical ? (int)vert[buttonPosition] : (int)horiz[buttonPosition]];
    }

    private static ButtonInfo GetDiametricallyOppositeButton(ButtonInfo[] assignedButtons, ButtonInfo selectedButton)
    {
        var diametricOpps = new Dictionary<ButtonPosition, ButtonPosition>
        {
            [ButtonPosition.TL] = ButtonPosition.BR,
            [ButtonPosition.TR] = ButtonPosition.BL,
            [ButtonPosition.BL] = ButtonPosition.TR,
            [ButtonPosition.BR] = ButtonPosition.TL
        };

        var buttonPosition = (ButtonPosition)Array.IndexOf(assignedButtons, selectedButton);
        
        return assignedButtons[(int)diametricOpps[buttonPosition]];
    }

    public static int DigitalRoot(int a, int b) => (((a * 10 + b) - 1) % 9) + 1;
}