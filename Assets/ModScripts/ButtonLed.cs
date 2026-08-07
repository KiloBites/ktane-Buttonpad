public struct ButtonLed
{
    public ButtonPosition Position { get; private set; }
    public LEDColor Color { get; private set; }

    public ButtonLed(ButtonPosition position, LEDColor color)
    {
        Position = position;
        Color = color;
    }
}