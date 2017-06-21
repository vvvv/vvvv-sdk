using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class BoolExtension
{
    public static bool ParseEnglish(this String str)
    {
        return str.ToLower() == "true" ? true : false;
    }

    public static String ToStringEnglish(this bool b)
    {
        if (b) { return "True"; } else { return "False"; }
    }
}

