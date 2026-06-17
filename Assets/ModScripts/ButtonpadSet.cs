using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;
using static Symbol;
using static ButtonColor;

public class ButtonpadSet
{
    private static readonly int[] numberSet =
    {
        1, 9, 3, 4, 8, 2, 6, 7, 0, 5, 8,
        1, 7, 0, 5, 4, 8, 3, 0, 8, 2, 6,
        7, 4, 3, 1, 8, 6, 8, 5, 0, 1, 4,
        7, 0
    };
    
    private static readonly LEDColor[] ledColors = { LEDColor.Yellow, LEDColor.Blue, LEDColor.Red, LEDColor.White,  LEDColor.Blue, LEDColor.White,  LEDColor.Yellow, LEDColor.Red };

    private static readonly Symbol[][] symbolColumns =
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
            WeirdNose,
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
        }
    };

    private static readonly ButtonColor[][] symbolColors =
    {
        new[]
        {
            Red,
            Blue,
            White,
            Purple,
            Yellow,
            Orange,
            White,
            Blue,
            Red,
            Purple,
            Yellow,
            Red,
            Blue,
            White,
            Red,
            Green,
            White,
            Purple,
            Yellow,
            Green,
            Purple,
            Orange,
            Yellow,
            Blue,
            Orange,
            Yellow,
            Blue,
            Green,
            Purple,
            Orange,
            Green,
            Red,
            Green,
            Orange,
            White
        }
    };

    private int selectedIndex;

    public ButtonInfo[] ButtonSet;
    public ButtonInfo[] ButtonSetOrdered;
    public LEDColor SelectedLEDColor;

    public void Generate()
    {
        selectedIndex = Range(0, ledColors.Length);
        SelectedLEDColor = ledColors[selectedIndex];
        
        ButtonSetOrdered = Enumerable.Range(0, symbolColumns[selectedIndex].Length).ToList().Shuffle().Take(4).OrderBy(x => x).Select(x => new ButtonInfo(symbolColumns[selectedIndex][x], symbolColors[selectedIndex][x])).ToArray();
        ButtonSet = ButtonSetOrdered.ToArray().Shuffle();
    }
}