using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;

namespace VVVV.Nodes.Http
{
    class JQueryBuilder:BaseGUIBuilder
    {


        public JQueryBuilder(SortedList<int, BaseDatenObjekt> pDatenObjekt, string pBrowserWidth, string pBrowserHeigth, SortedList<string,string> pBodyProperties, string pPageName)
        {
            
            this.mGuiDatenObjekte = pDatenObjekt;
            buildBody();
            BuildBrowserFunction(pBrowserWidth, pBrowserHeigth, pBodyProperties);
            BuildHeadSpecialEnd();
            buildHead(pPageName);
            AddStyleRule();
            AddJsFunction();
            //mPage.Body.Insert(BuildBodySpecials());
        }


        #region Specials


        protected override string BuildBodySpecials()
        {
            return "";
        }



        protected override string BuildHeadSpecials()
        {

            return "";
        }


        protected override void BuildHeadSpecialEnd()
        {


            string tScript = "";
            tScript += "$(document).ready(function(){";

            if (mDocumentReady != null)
            {


                foreach (KeyValuePair<string, string> pValuePair in mDocumentReady)
                {
                    tScript += pValuePair.Value;
                }

            }

            tScript += "});";
            mJsFile.Add("DocumentReady", tScript);
            
        }

        #endregion Specials






        #region Button

        protected override Tag BuildButtonBody(Tag pButton, DatenGuiButton pButtonDaten)
        {

            //<input type="button" value="I do nothing" stylizeaction="clean" id="do_something" style="display: none;"/>
            //
            //Form tForm = new Form();
            //tForm.AddAttribute(new HTMLAttribute("action", "it_still_submits"));
            //tForm.AddAttribute(new HTMLAttribute("method", "post"));
            //Button tButton = new Button(true);

            ////foreach (KeyValuePair<string, string> pHtmlAttr in pValue)
            ////{
            ////    if (pHtmlAttr.Key != "")
            ////    {
            ////            tButton.AddAttribute(new HTMLAttribute(pHtmlAttr.Key, pHtmlAttr.Value));
            ////    }
            ////}

            ////tForm.AddAttribute(new HTMLAttribute("style",BuildCSSAttribute(pStyles)));
            //tButton.AddAttribute(new HTMLAttribute("type", "button"));
            ////tButton.AddAttribute( new HTMLAttribute("Value", pHtmlText));
            //tButton.AddAttribute(new HTMLAttribute("stylizeAction", "clean"));
            //tButton.AddAttribute(new HTMLAttribute("id","do_something"));

            
            //tForm.Insert(tButton.Text);

            //return pButton;
            return pButton;
        }


        protected override string BuildButtonHead()
        {
            
            
            //Link tCSSLink1 = new Link("stylesheet","text/css", "reset.css");
            //Link tCSSLink2 = new Link("stylesheet", "text/css", "main.css");
            //Link tCssLink3 = new Link("stylesheet", "text/css", "form.css");

            //JavaScript tJavaLink = new JavaScript("stylize_buttons.js");

            //JavaScript tScript = new JavaScript();
            //tScript.Insert("$(document).ready(function(){");
            //tScript.Insert("$(\"input\").stylizeButton();");
            //tScript.Insert("});");

            //return  tJavaLink.Text + Environment.NewLine + tCSSLink1.Text + Environment.NewLine + tCSSLink2.Text + Environment.NewLine + tCssLink3.Text + Environment.NewLine + tScript.Text ;
            return "";
        }

        #endregion Button






        #region Slider



        protected override Tag BuildSliderBody(Tag pSlider, DatenGuiSlider pSliderDaten)
        {
            //HTMLDiv tDiv = new HTMLDiv();
            //tDiv.AddAttribute(new HTMLAttribute("id", "slider" + Index.ToString()));
            //tDiv.AddAttribute(new HTMLAttribute("style", BuildCSSAttribute(pStyles)));
            //mDocumentReady.Add("$('#slider" + Index.ToString()  + "').slider();");
            return pSlider;


        }

        /// <summary>
        /// Slider is cross Linkes
        /// </summary>
        /// <returns></returns>
        protected override string BuildSliderHead()
        {

            /////Cross Link Fnd the right css classess
            //Link tLink = new Link("stylesheet", "text/css", "http://ui.jquery.com/testing/themes/base/ui.all.css");
            //JavaScript tJavaLink = new JavaScript("ui.core.js");
            //JavaScript tJavaLink2 = new JavaScript("ui.slider.js");


            //CSSStyle tStyle = new CSSStyle();
            //Rule tRule = new Rule("#slider");
            //tRule.AddProperty(new Property("margin","10px"));
            //tStyle.Insert(tRule.Text);         

            //return tLink.Text + Environment.NewLine + tJavaLink.Text + Environment.NewLine + tJavaLink2.Text  + Environment.NewLine ;
            return "";
        }

        #endregion  Slider





        #region Text


        protected override Tag BuildTextBody(Tag pText , DatenGuiText pTextDaten)
        {
            //Text tText = new Text(pHtmlText, true);
            //tText.AddAttribute(new HTMLAttribute("style",BuildCSSAttribute(pStyles)));
            return pText;
        }



        protected override string BuildTextHead()
        {
            
            
            return "";
        }


        #endregion Text



        #region Textfield

        protected override Tag BuildTextfieldBody(Tag pTextfield, DatenGuiTextfield pTextfieldDaten)
        {
            //Form tForm = new Form();
            //tForm.Insert(pHtmlText);



            //TextField tTextfield = new TextField("");
            //tForm.AddAttribute(new HTMLAttribute("style", BuildCSSAttribute(pStyles)));

            //foreach (KeyValuePair<string, string> pHtmlAttr in pValue)
            //{
            //    if ((pHtmlAttr.Key != "") && (pHtmlAttr.Key.Contains("Value")))
            //    {
            //        tTextfield.AddAttribute(new HTMLAttribute(pHtmlAttr.Key, pHtmlAttr.Value));
            //    }
            //}

            //tForm.Insert(tTextfield.Text);

            return pTextfield;
        }


        protected override string BuildTextfieldHead()
        {
            return "";
        }

        #endregion Textfield


        #region Image

        protected override Tag BuildImageBody(Tag pImage, DatenGuiImage pImageDaten)
        {

            //Img tImg = new Img();
            //foreach (KeyValuePair<string, string> pHtmlAttr in pValue)
            //{
            //    if (pHtmlAttr.Key != "")
            //    {
            //        tImg.AddAttribute(new HTMLAttribute(pHtmlAttr.Key, pHtmlAttr.Value));
            //    }
            //}

            //tImg.AddAttribute(new HTMLAttribute("style", BuildCSSAttribute(pStyles)));

            return pImage;
        }


       protected override string BuildImageHead()
        {
            return "";
        }




        #endregion Image

    }
}