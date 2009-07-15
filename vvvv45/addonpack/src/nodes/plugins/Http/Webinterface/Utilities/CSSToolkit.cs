using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Webinterface.Utilities
{

    class CSSToolkit
    {

        #region field declaration

        #endregion field declarartion

        #region constructor/destructor

        public CSSToolkit()
        {
            
        }

        #endregion constructor/destructor

        #region CSS Tags

        // Tag Element <head>
        public static string TransformPosition(string type, string top, string left, string right, string bottom, string width, string height, string zIndex)
        {

            string whole = "";

            if (type == "percent")
            {
                whole = " position:absolute; top:" +  top + "%; left:" + left + "%; right:" + right + 
                        "%; bottom:" + bottom + "%; width:" + width + "%; height:" + height + "%; z-index:" + zIndex + ";";

            }
            else if (type == "pixel")
            {

            }
            else
            {
               whole = "debug";
            }

            if (whole != "")
            {
                return whole;
            }
            else
            {
                return "error";
            }

        }

        public static string Font(string type, string size, string font, string colorR, string colorG, string colorB)
        {

            string css = "";

            if (type == "percent")
            {
                css = " font-size:" + size + "%; font-family:" + font + "; color: rgb(" + colorR + "%," + colorG + "%," + colorB + "%);";
            }
            else if (type == "pixel")
            {

            }
            else
            {
                css = "debug";
            }

            if (css != "")
            {
                return css;
            }
            else
            {
                return "error";
                
            }

        }

        public static string Margin(string pType, string pTop, string pBottom, string pLeft, string pRight)
        {

            string css = " margin-left:" + pLeft + "; margin-right:" + pRight + "; margin-top:" + pTop + "; margin-bottom:" + pBottom + ";";


            if (css != "")
            {
                return css;
            }
            else
            {
                return "error";
            }

        }

        public static string Padding(string pType, string pTop, string pBottom, string pLeft, string pRight)
        {

            string css = " padding-left:" + pLeft + "; padding-right:" + pRight + "; padding-top:" + pTop + "; padding-bottom:" + pBottom + ";";


            if (css != "")
            {
                return css;
            }
            else
            {
                return "error";
            }

        }

        #endregion CSS Tags

        #region change Values



        #endregion change Values

    }
}
