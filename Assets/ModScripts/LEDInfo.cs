using UnityEngine;

public struct LEDInfo
{
    public ButtonPosition Position { get; private set; }
    public ButtonInfo Button { get; private set; }
    public LEDColor LEDColor { get; private set; }

    public LEDInfo(ButtonPosition position, ButtonInfo button, LEDColor ledColor)
    {
        Position = position;
        Button = button;
        LEDColor = ledColor;
    }

    public override string ToString() => $"the LED is {LEDColor} on the {Position} button";

    private static readonly Color32[] _ledColors =
    {
        Color.red,
        new Color32(255, 153, 0, 255),
        new Color(1, 1, 0),
        Color.green,
        new Color32(0, 136, 255, 255),
        new Color32(153, 0, 255, 255),
        Color.white
    };
    
    public Color GetLEDColor() => _ledColors[(int)LEDColor];
}