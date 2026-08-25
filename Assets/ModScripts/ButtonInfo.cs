using System;
using UnityEngine;

public class ButtonInfo : IEquatable<ButtonInfo>
{
    public Symbol ButtonSymbol { get; private set; }
    public ButtonColor ButtonColor { get; private set; }

    public ButtonInfo(Symbol buttonSymbol, ButtonColor buttonColor)
    {
        ButtonSymbol = buttonSymbol;
        ButtonColor = buttonColor;
    }

    private static readonly Color32[] _buttonColors =
    {
        new Color32(238, 0, 0, 255),
        new Color32(255, 227, 0, 255),
        new Color32(0, 85, 255, 255),
        Color.white
    };
    
    

    public override bool Equals(object obj)
    {
        var info = obj as ButtonInfo;
        return info != null && Equals(info);
    }

    public override int GetHashCode() => (int)ButtonSymbol ^ (int)ButtonColor;

    public override string ToString() => $"Symbol: {ButtonSymbol} Color: {ButtonColor}";

    public bool Equals(ButtonInfo other) => ButtonSymbol == other.ButtonSymbol && ButtonColor == other.ButtonColor;

    public static bool operator ==(ButtonInfo a, ButtonInfo b) => a.Equals(b);

    public static bool operator !=(ButtonInfo a, ButtonInfo b) => !a.Equals(b);

    public Color GetButtonColor() => _buttonColors[(int)ButtonSymbol];
}