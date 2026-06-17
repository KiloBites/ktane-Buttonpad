using System;


public struct ButtonInfo : IEquatable<ButtonInfo>
{
    public Symbol ButtonSymbol { get; private set; }
    public ButtonColor ButtonColor { get; private set; }

    public ButtonInfo(Symbol buttonSymbol, ButtonColor buttonColor)
    {
        ButtonSymbol = buttonSymbol;
        ButtonColor = buttonColor;
    }

    public override bool Equals(object obj) => obj is ButtonInfo && Equals((ButtonInfo)obj);

    public override int GetHashCode() => (int)ButtonSymbol ^ (int)ButtonColor;

    public bool Equals(ButtonInfo other) => ButtonSymbol == other.ButtonSymbol && ButtonColor == other.ButtonColor;
    
    public static bool operator == (ButtonInfo a, ButtonInfo b) => a.Equals(b);

    public static bool operator != (ButtonInfo a, ButtonInfo b) => !(a == b);
    
    public override string ToString() => $"Symbol: {ButtonSymbol}, Color: {ButtonColor}";
}