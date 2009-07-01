using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;



namespace VVVV.Nodes.Http
{
    abstract class BaseGUIBuilder
    {


        #region field declaration

        protected SortedList<int, BaseDatenObjekt> mGuiDatenObjekte;
        protected Page mPage = new Page(true);


        public SortedList<string, string> mCssFile = new SortedList<string, string>();
        public SortedList<string, string> mJsFile = new SortedList<string, string>();
        public SortedList<string, string> mDocumentReady = new SortedList<string, string>();

        public Page Page
        {
            get
            {
                return mPage;
            }
        }

        public Body Body
        {
            get
            {
                return mPage.Body;
            }
        }

        public Head Head
        {
            get
            {
                return mPage.Head;
            }
        }

        public string CssMainFile
        {
            get
            {
                string tCssFile = "";
                foreach(KeyValuePair<string,string> pPair in mCssFile)
                {
                    tCssFile += pPair.Value + Environment.NewLine;
                }
                return tCssFile;
            }
        }

        public string JsFile
        {
            get
            {
                string tJsFile = "";
                foreach (KeyValuePair<string, string> pPair in mJsFile)
                {
                    tJsFile += pPair.Value + Environment.NewLine; 
                }
                return tJsFile;
            }
        }

        #endregion field declaration




        #region absract build Methods


        protected abstract Tag BuildTextBody(Tag pText, DatenGuiText pTextDaten);
        protected abstract Tag BuildTextfieldBody(Tag pTextfield, DatenGuiTextfield pTextfieldDaten);
        protected abstract Tag BuildButtonBody(Tag pButton,DatenGuiButton pButtonDaten);
        protected abstract Tag BuildImageBody(Tag pImage, DatenGuiImage pImageDaten);
        protected abstract Tag BuildSliderBody(Tag pSlider,DatenGuiSlider pSliderDaten);
        protected abstract string BuildBodySpecials();


        protected abstract string BuildButtonHead();
        protected abstract string BuildSliderHead();
        protected abstract string BuildImageHead();
        protected abstract string BuildTextHead();
        protected abstract string BuildTextfieldHead();
        protected abstract string BuildHeadSpecials();
        protected abstract void BuildHeadSpecialEnd();


        #endregion absract build Methods




        /// <summary>
        /// Creates the first Level Objekt of each GuiElement in the List mGuiDatenObjekte
        /// </summary>
        

        public void buildBody()
        {
            foreach (KeyValuePair<int, BaseDatenObjekt> pKey in mGuiDatenObjekte)
            {
                if (pKey.Value != null)
                {
                    BaseDatenObjekt tDatenObjekt = pKey.Value;
                    Tag tList = BuildGuiElement(Page.Body, tDatenObjekt);
                    mPage.Body.Insert(tList);
                }
            }

            
          
        }


        private Tag BuildGuiElement(Tag pTagToInsertIn, BaseDatenObjekt pDatenObjekt)
        {





            #region Text

            if (pDatenObjekt.Type == "Text")
            {
                DatenGuiText tTextDaten = (DatenGuiText)pDatenObjekt;
                Text tText = new Text(tTextDaten.ID,tTextDaten.Label,false);
                HTMLDiv tDiv = new HTMLDiv();
                tDiv.AddAttribute(new HTMLAttribute("class" , tTextDaten.Class));
                tDiv.AddAttribute(new HTMLAttribute("overflow", "scroll"));
                tDiv.Insert(tText);

                if (mCssFile.ContainsKey(tTextDaten.Class))
                {

                }
                else
                {
                    mCssFile.Add(tTextDaten.Class, BuildCssRule(tTextDaten.Class, tTextDaten.CssProperties));
                }
                

                if (tTextDaten.GuiDepth > 0)
                {
                    List<BaseDatenObjekt> tBaseGuiDatenListe = tTextDaten.GuiObjektListe;
                    foreach (BaseDatenObjekt pObjekt in tBaseGuiDatenListe)
                    {
                        tDiv.Insert(BuildGuiElement(tText, pObjekt));
                    }
                }
                return tDiv;
            }

            #endregion Text


                




            # region Textfield

            else if (pDatenObjekt.Type == "Textfield")
            {
                DatenGuiTextfield tTextFieldDaten = (DatenGuiTextfield) pDatenObjekt;
                //Text tDiv = new Text(false);
                //tDiv.Insert(tTextFieldDaten.Label);


                TextArea tTextfield = new TextArea(tTextFieldDaten.ID, tTextFieldDaten.Class, tTextFieldDaten.Value);
                tTextfield.AddAttribute(new HTMLAttribute("onkeyup", tTextFieldDaten.JsFunktion.Name + "('" + tTextFieldDaten.ID + "')"));

                JavaFunction tTextfieldFunction = new JavaFunction(tTextFieldDaten.JsFunktion.Name, "pId", tTextFieldDaten.JsFunktion.Content);

                if (mJsFile.ContainsKey(tTextFieldDaten.Class) == false)
                {
                    mJsFile.Add(tTextFieldDaten.Class, tTextfieldFunction.Text);
                }
                
                //tDiv.Insert(tTextfield);


                //Build CSS
                if (mCssFile.ContainsKey(tTextFieldDaten.Class))
                {

                }
                else
                {
                    mCssFile.Add(tTextFieldDaten.Class, BuildCssRule(tTextFieldDaten.Class, tTextFieldDaten.CssProperties));
                }


                return tTextfield;
            }

            #endregion Textfield






            #region Button


            else if (pDatenObjekt.Type == "Button")
            {
                DatenGuiButton tButtonDaten = (DatenGuiButton)pDatenObjekt;
                Tag tTag = new HTMLDiv();

                HTMLDiv tButtonInlay = new HTMLDiv("ButtonInlay", "ButtonInlay");
                //if(tButtonDaten.Mode == "1")
                //{
                //    tButtonInlay.AddAttribute(new HTMLAttribute("style", new Property("background-color", "#808080").Text));
                //}

                tTag.Insert(tButtonInlay);

                tTag.AddAttribute(new HTMLAttribute("id", tButtonDaten.ID).Text);
                tTag.AddAttribute(new HTMLAttribute("class", tButtonDaten.Class + " button").Text);
                tTag.AddAttribute(new HTMLAttribute("Value", tButtonDaten.State).Text);
                tTag.AddAttribute(new HTMLAttribute("text-align", "center"));
                

                HTMLDiv tLabel = new HTMLDiv();
                tLabel.AddAttribute(new HTMLAttribute("class", tButtonDaten.Class));
                tLabel.AddAttribute(new HTMLAttribute("style", new Property("background-color","transparent").Text + new Property("text-algin","center").Text));



                if (tButtonDaten.Label != "")
                {
                    tLabel.Insert(tButtonDaten.Label);
                    tTag.Insert(tLabel);
                }
                


                tTag.AddAttribute(new HTMLAttribute("onclick",  tButtonDaten.Class + "('" +  tButtonDaten.ID + "')"));

                // Create Css Rule
                Rule tStyleRule = new Rule("." + tButtonDaten.Class);
                
                
                foreach (KeyValuePair<string, string> pKey in tButtonDaten.CssProperties)
                {
                    tStyleRule.AddProperty( new Property(pKey.Key, pKey.Value));
                }



                if (mCssFile.ContainsKey(tButtonDaten.Class))
                {
                    mCssFile.Remove( tButtonDaten.Class);
                    mCssFile.Add( tButtonDaten.Class, tStyleRule.Text);
                }
                else
                {
                    mCssFile.Add(tButtonDaten.Class, tStyleRule.Text);
                }

                //Create JS Function

                if (tButtonDaten.CreateJsFuntkion)
                {
                    JavaFunction tJsFunction = new JavaFunction(tButtonDaten.Class, "pId", tButtonDaten.JsFunktion.Content);
                    //JavaFunction tJsFunction = new JavaFunction("testFunktion", "pClass", JSToolkit.TestFunktion());

                    if (mJsFile.ContainsKey(tButtonDaten.Class))
                    {
                        mJsFile.Remove(tButtonDaten.Class);
                        mJsFile.Add(tButtonDaten.Class, tJsFunction.Text);
                    }
                    else
                    {
                        mJsFile.Add(tButtonDaten.Class, tJsFunction.Text);
                    }
                }



                //Rekursion auf eigenen Funktion
                if (pDatenObjekt.GuiDepth > 0)
                {
                    List<BaseDatenObjekt> tBaseGuiDatenListe = pDatenObjekt.GuiObjektListe;
                    foreach(BaseDatenObjekt pObjekt in tBaseGuiDatenListe)
                    {
                        if (pObjekt != null)
                        {
                            tTag.Insert(BuildGuiElement(tTag, pObjekt));
                        }
                        
                    }
                }


                return tTag;
            }


            #endregion Button






            #region Image


            else if (pDatenObjekt.Type == "Image")
            {
                DatenGuiImage tImageDaten = (DatenGuiImage) pDatenObjekt;
                Img tImage = new Img(tImageDaten.Src, tImageDaten.Alt);
                tImage.AddAttribute(new HTMLAttribute("class",tImageDaten.Class));

                if(mCssFile.ContainsKey(tImageDaten.Class))
                {

                }else
                {
                    mCssFile.Add(tImageDaten.Class, BuildCssRule(tImageDaten.Class, tImageDaten.CssProperties)); 
                }
                
                return tImage;
            }

            #endregion Image






            #region Slider

            else if (pDatenObjekt.Type == "Slider")
            {
                DatenGuiSlider tSliderDaten = (DatenGuiSlider)pDatenObjekt;
                Slider tSlider = new Slider(tSliderDaten.ID, tSliderDaten.Class);


                string part1 = @"	
                        {";

                string part3 = @"range: ""min"",
			                    min: 0,
			                    max: 1000,";

                string part4 = "value:" + tSliderDaten.Position + ",";

                string part5 = @"stop: function(event, ui) {
				                    makeRequest('";

                string part6 = @"',ui.value);
                                }
                                });
                                ";


                 
                if(mDocumentReady.ContainsKey(tSliderDaten.Class) == false)
                {
                    mDocumentReady.Add(tSliderDaten.Class, "$(\"." + tSliderDaten.Class + "\").slider(" + part1  + tSliderDaten.Orientation + part3 + part4 + part5 + tSliderDaten.ID + part6);
                }
                


                //Build CSS
                if (mCssFile.ContainsKey(tSliderDaten.Class))
                {

                }
                else
                {
                    mCssFile.Add(tSliderDaten.Class, BuildCssRule(tSliderDaten.Class, tSliderDaten.CssProperties));
                }
                


                return tSlider;
            }

            #endregion Slider





            #region Container


            else if (pDatenObjekt.Type == "Container")
            {
                
                DatenGuiContainer tContainerDaten = (DatenGuiContainer) pDatenObjekt;
                HTMLDiv tContainer = new HTMLDiv(tContainerDaten.ID, tContainerDaten.Class);


                if (mCssFile.ContainsKey(tContainerDaten.Class) == false)
                {
                    mCssFile.Add(tContainerDaten.Class, BuildCssRule(tContainerDaten.Class, tContainerDaten.CssProperties));
                }
                
                if (pDatenObjekt.GuiDepth > 0)
                {
                    List<BaseDatenObjekt> tBaseGuiDatenListe = pDatenObjekt.GuiObjektListe;
                    foreach (BaseDatenObjekt pObjekt in tBaseGuiDatenListe)
                    {
                        if (pObjekt != null)
                        {
                            tContainer.Insert(BuildGuiElement(tContainer, pObjekt));
                        }
                    }
                }
                return tContainer;
            }


            #endregion Container






            #region TwoPane


            else if (pDatenObjekt.Type == "TwoPane")
            {
                DatenGuiTwoPane tTwoPaneDaten = (DatenGuiTwoPane)pDatenObjekt;
                HTMLDiv tFixedPane = new HTMLDiv("fixed_area");
                HTMLDiv tMainArea = new HTMLDiv("main_area");
                

                HTMLDiv tMainDiv = new HTMLDiv("main_container");

                if (tTwoPaneDaten.GuiDepth > 0)
                {
                    List<BaseDatenObjekt> tBaseGuiDatenListe = tTwoPaneDaten.GuiObjektListe;
                    foreach (BaseDatenObjekt pObjekt in tBaseGuiDatenListe)
                    {
                        tMainArea.Insert(BuildGuiElement(tMainArea, pObjekt));
                    }
                } 
                
                
                if (tTwoPaneDaten.FixedPainDepth > 0)
                {
                    List<BaseDatenObjekt> tFixedPainDatenListe = tTwoPaneDaten.FixedPainGuiList;
                    foreach (BaseDatenObjekt pObjekt in tFixedPainDatenListe)
                    {
                        tFixedPane.Insert(BuildGuiElement(tFixedPane, pObjekt));
                    }
                }

                tMainDiv.Insert(tFixedPane);
                tMainDiv.Insert(tMainArea);

                return tMainDiv;

            }

            #endregion TwoPane

            




            #region PopUp


            else if (pDatenObjekt.Type == "PopUp")
            {
                DatenGuiPopUp tPopupDaten = (DatenGuiPopUp)pDatenObjekt;
                Tag tTag = new HTMLDiv();

                tTag.AddAttribute(new HTMLAttribute("id", tPopupDaten.ID).Text);
                tTag.AddAttribute(new HTMLAttribute("class", tPopupDaten.Class).Text);

                // Create Css Rule
                Rule tStyleRule = new Rule("." + tPopupDaten.Class);

                foreach (KeyValuePair<string, string> pKey in tPopupDaten.CssProperties)
                {
                    tStyleRule.AddProperty(new Property(pKey.Key, pKey.Value));
                }

                if (mCssFile.ContainsKey(tPopupDaten.Class))
                {
                    mCssFile.Remove(tPopupDaten.Class);
                    mCssFile.Add(tPopupDaten.Class, tStyleRule.Text);
                }
                else
                {
                    mCssFile.Add(tPopupDaten.Class, tStyleRule.Text);
                }


                //Create JS Function
                if (tPopupDaten.JsFunktionOpen.Name != "")
                {
                    if (mJsFile.ContainsKey(tPopupDaten.JsFunktionOpen.Name))
                    {
                        string tFunktionContentOut;
                        mJsFile.TryGetValue(tPopupDaten.JsFunktionOpen.Name, out tFunktionContentOut);
                        tFunktionContentOut += Environment.NewLine + "$(\"." + tPopupDaten.Class + "\").animate({opacity: 'show'}, \"slow\");";
                        mJsFile.Remove(tPopupDaten.JsFunktionOpen.Name);
                        mJsFile.Add(tPopupDaten.JsFunktionOpen.Name, tFunktionContentOut);
                    }
                    else
                    {
                        string tFunktionContent = tPopupDaten.JsFunktionOpen.Content;
                        //tFunktionContent = Environment.NewLine + "$(\"." + tPopupDaten.Class + "\").show();" + Environment.NewLine + tFunktionContent;
                        tFunktionContent += Environment.NewLine + "$(\"." + tPopupDaten.Class + "\").animate({opacity: 'show'}, \"slow\");";
                        JavaFunction tJsFunctionOpen = new JavaFunction(tPopupDaten.JsFunktionOpen.Name, "pId", tFunktionContent);
                        mJsFile.Add(tPopupDaten.JsFunktionOpen.Name, tJsFunctionOpen.Text);
                    }
                    

                }


                if (tPopupDaten.JsFunktionClose.Name != "")
                {
                    if (mJsFile.ContainsKey(tPopupDaten.JsFunktionClose.Name))
                    {
                        string tFunktionContentOut;
                        mJsFile.TryGetValue(tPopupDaten.JsFunktionClose.Name, out tFunktionContentOut);
                        tFunktionContentOut = Environment.NewLine + "$(\"." + tPopupDaten.Class + "\").animate({opacity: 'show'}, \"slow\");";
                        mJsFile.Remove(tPopupDaten.JsFunktionClose.Name);
                        mJsFile.Add(tPopupDaten.JsFunktionClose.Name, tFunktionContentOut);
                    }
                    else
                    {
                        string tFunktionContent = tPopupDaten.JsFunktionClose.Content;
                        tFunktionContent += Environment.NewLine + "$(\"." + tPopupDaten.Class + "\").animate({opacity: 'hide'}, \"slow\");";
                        JavaFunction tJsFunctionClose = new JavaFunction(tPopupDaten.JsFunktionClose.Name, "pId", tFunktionContent);
                        mJsFile.Add(tPopupDaten.JsFunktionClose.Name, tJsFunctionClose.Text);
                    }
                }

       

                //Rekursion 
                if (pDatenObjekt.GuiDepth > 0)
                {
                    List<BaseDatenObjekt> tBaseGuiDatenListe = pDatenObjekt.GuiObjektListe;
                    foreach (BaseDatenObjekt pObjekt in tBaseGuiDatenListe)
                    {
                        if (pObjekt != null)
                        {
                            tTag.Insert(BuildGuiElement(tTag, pObjekt));
                        }

                    }
                }


                return tTag;
            }

            #endregion PopUp



            else
            {
                Text tText = new Text(true);
                tText.Insert("You have to implement your GUI Type in the GUIBuilder");
                return tText;
            }
        }




        public void buildHead(string pPageName)
        {


            mPage.Head.Insert(BuildHeadSpecials());
            //mPage.Head.Insert(BuildHeadSpecialEnd());
            //mPage.Head.Insert(new Link("reset.css", "stylesheet", "text/css", "screen").Text);
            mPage.Head.Insert(new Link(pPageName + ".css", "stylesheet", "text/css", "screen").Text);

            //mPage.Head.Insert(new JavaScript("jquery_rule.js").Text);

            //mPage.Head.Insert(new JavaScript("jquery-1.3.1.js").Text);
            mPage.Head.Insert(new JavaScript("jquery-1.3.2.min.js").Text);
            mPage.Head.Insert(new JavaScript("jquery-ui-1.7.custom.min.js").Text);
            mPage.Head.Insert(new JavaScript("jquery.timers-1.1.2.js").Text);
            mPage.Head.Insert(new Link("jquery-ui-1.7.custom.css", "stylesheet", "text/css", "screen").Text);


            //slider
            //mPage.Head.Insert(new Link("ui.base.css", "stylesheet", "text/css", "screen").Text);
            //mPage.Head.Insert(new Link("ui.theme.css", "stylesheet", "text/css", "screen").Text);
            //mPage.Head.Insert(new JavaScript("ui.core.js").Text);
            //mPage.Head.Insert(new JavaScript("ui.slider.js").Text);


            mPage.Head.Insert(new JavaScript(pPageName + ".js").Text);

            JavaScript tReloadFuntion = new JavaScript("Reload", "window.location.reload() ;");
            mPage.Head.Insert(tReloadFuntion.Text);
        }



        public string BuildHTMLAttribute()
        {
            return "";
        }


        public void AddStyleRule()
        {
            Rule tButton = new Rule(".button");
            tButton.AddProperty(new Property("cursor", "pointer"));
            mCssFile.Add("button", tButton.Text);

            Rule tButtonInlay = new Rule(".ButtonInlay");
            tButtonInlay.AddProperty(new Property("cursor", "pointer"));
            tButtonInlay.AddProperty(new Property("border-style", "solid"));
            tButtonInlay.AddProperty(new Property("border-width", "1px"));
            tButtonInlay.AddProperty(new Property("border-color", "#9a9a9a"));
            tButtonInlay.AddProperty(new Property("position", "absolute"));
            tButtonInlay.AddProperty(new Property("width", "50%"));
            tButtonInlay.AddProperty(new Property("height", "50%"));
            tButtonInlay.AddProperty(new Property("left", "25%"));
            tButtonInlay.AddProperty(new Property("top", "25%"));
            mCssFile.Add("buttonInlay", tButtonInlay.Text);
        }

        public void AddJsFunction()
        {
            mJsFile.Add("tMakeRequest", JSToolkit.MakeRequest());
            mJsFile.Add("tAlter", JSToolkit.Alert());

            JavaFunction tsetVVVVDaten = new JavaFunction("setNewDaten", "adresse,value", "var Element =  document.getElementById(adresse);" + Environment.NewLine + " Element.value = value; " + Environment.NewLine );
            mJsFile.Add("setNewDaten", tsetVVVVDaten.Text); 
        }



        public string BuildCssRule(string pSelector, SortedList<string,string> pProperties)
        {
            Rule tRule = new Rule("." + pSelector);


            if (pProperties != null)
            {
                foreach (KeyValuePair<string, string> pCssValuePair in pProperties)
                {
                    if (pCssValuePair.Key != "")
                    {
                        tRule.AddProperty(new Property(pCssValuePair.Key,pCssValuePair.Value));
                    }
                }
            }

            return tRule.Text;
        }


        public void BuildBrowserFunction(string pWidth, string pHeight, SortedList<string,string> pBodyCss)
        {           

            Rule tBody = new Rule("body");

            foreach (KeyValuePair<string, string> pKeyValue in pBodyCss)
            {
                tBody.AddProperty(new Property(pKeyValue.Key, pKeyValue.Value));
                 
            }

            mCssFile.Add("body",tBody.Text);
        }

    }
}
