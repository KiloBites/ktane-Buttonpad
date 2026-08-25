using System;
using System.Collections.Generic;
using System.Linq;

public static class ButtonpadTools
{
    public static int DigitalRoot(int a, int b) => (((a * 10 + b) - 1) % 9) + 1;
}