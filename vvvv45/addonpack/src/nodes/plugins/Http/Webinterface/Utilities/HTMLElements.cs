

using System;
using System.Collections.Generic;
using System.Text;



namespace VVVV.Webinterface.Utilities
{


	#region Attributes

	class CSSAttribute : Attribute
	{
		public CSSAttribute(string pName, string pValue)
			: base(pName, pValue)
		{
			Structure = " name:value;";
		}
	}

	class HTMLAttribute : Attribute
	{
		public HTMLAttribute(string pName, string pValue)
			: base(pName, pValue)
		{
			Structure = " name=\"value\"";
		}
	}

	#endregion





	#region Tags

    class EmptyTag : Tag
    {
        public EmptyTag(string pName)
        {
            Name = pName;
        }
    }

	public class Page : Tag
    {
        public Head Head;
        public Body Body;

        public Page()
        {
            Name = "html";
        }

        public Page(bool bodyOrNot)
        {
            if (bodyOrNot == true)
            {
                HTMLHeader = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
                Name = "html";
                Head = new Head();
                Body = new Body();
                Insert(Head);
                Insert(Body);
            }
            else
            {
                Name = "html";
                Head = new Head();
                Insert(Head);
            }
        }

        public void Save(string pURL)
        {
            System.IO.FileStream tFile = new System.IO.FileStream(pURL, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            System.IO.StreamWriter tWriter = new System.IO.StreamWriter(tFile, System.Text.Encoding.UTF8);

            tWriter.Write(this.Text);
            tWriter.Flush();
            tWriter.Close();
        }
    }

    public class Head : Tag
    {
        public Head()
        {
            Name = "head";
        }
    }

    public class Body : Tag
    {
        public Body()
        {
            Name = "body";
        }
    }

    class Title : Tag
    {
        public Title(string pText)
        {
            Name = "title";
            Insert(pText);
        }
    }

    class Meta : Tag
    {
        public Meta()
        {
            Name = "meta";
        }
        
        public Meta(string pContent)
        {
            Name = "meta";
            AddAttribute(new HTMLAttribute("content", pContent));
        }
        
        public Meta(string pName, string pContent)
        {
            Name = "meta";
            AddAttribute(new HTMLAttribute("name", pName));
            AddAttribute(new HTMLAttribute("content", pContent));
           
            
        }
    }

    class Link : Tag
    {
        
        
        public Link(string pHRef, string pType)
        {
            Name = "link";
            AddAttribute(new HTMLAttribute("href", pHRef));
			AddAttribute(new HTMLAttribute("type", pType));
           
        }

        public Link(string pHref, string pRel, string pType)
        {
            Name = "link";
            AddAttribute(new HTMLAttribute("type", pType).Text);
            AddAttribute(new HTMLAttribute("rel", pRel).Text);
            AddAttribute(new HTMLAttribute("href", pHref).Text);
            
            
        }

        public Link(string pHref, string pRel, string pType, string pMedia)
        {
            Name = "link";
            AddAttribute(new HTMLAttribute("media", pMedia).Text);
            AddAttribute(new HTMLAttribute("type", pType).Text);
            AddAttribute(new HTMLAttribute("rel", pRel).Text);
            AddAttribute(new HTMLAttribute("href", pHref).Text);
            
            
           
        }

    }

    class IPodIconLink : Tag
    {
        public IPodIconLink(string pRel, string pHref)
        {
            Name = "link";
            AddAttribute(new HTMLAttribute("rel", pRel).Text);
            AddAttribute(new HTMLAttribute("href", pHref).Text);
        }
    }

    class Button : Tag
    {
        public Button()
        {
            Name = "input";
        }
        
        public Button(bool pIsInput)
        {
            if (pIsInput == true)
            {
                Name = "input";
            }
            else
            {
                Name = "button";
            }
            
        }
        
        public Button(string pButtonText)
        {
            Name = "button";
            AddAttribute(new HTMLAttribute("type", "button"));
            AddAttribute(new HTMLAttribute("value", pButtonText));
            Insert(new HTMLText (pButtonText, false));
        }

        public Button(string pButtonText, string pId)
        {
            Name = "button";
            AddAttribute(new HTMLAttribute("type", "button"));
            AddAttribute(new HTMLAttribute("id", pId));
            Insert(new HTMLText(pButtonText, false));
        }
        
        
        
        public Button(string pID, string pName, string pButtonText, string pButtonAction)
        {
            Name = "button";
            AddAttribute(new HTMLAttribute("id", pID));
			AddAttribute(new HTMLAttribute("type", "button"));
            AddAttribute(new HTMLAttribute("value", pButtonText));
            AddAttribute(new HTMLAttribute("onclick", pButtonAction));
			Insert(new HTMLText(pButtonText, false));
        }
    }

    class HTMLText : Tag
    {
        public HTMLText()
        {
            Name = "p";
        }
        
        
        public HTMLText(bool pEmbeddingInSpan)
        {
            if (pEmbeddingInSpan == true)
            {
                Name = "span";
            }
            else
                Name = "p";
        }
        
        
        public HTMLText(string pText, bool pEmbeddingInSpan)
        {
            if (pEmbeddingInSpan == true)
            {
                Name = "span";
                Insert(pText);
            }
            else
            {
                Name = "p";
                Insert(pText);
            }
        
        }

        public HTMLText(string pID ,string pText, bool pEmbeddingInSpan)
        {
            if (pEmbeddingInSpan == true)
            {
                Name = "span";
                Insert(pText);
            }
            else
            {
                Name = "p";
                Insert(pText);
                AddAttribute(new HTMLAttribute("id", pID).Text);
            }
        
        }

        public HTMLText(string pID,string pClass, string pText, bool pEmbeddingInSpan)
        {
            if (pEmbeddingInSpan == true)
            {
                Name = "span";
                Insert(pText);
                AddAttribute(new HTMLAttribute("id", pID).Text);
                AddAttribute(new HTMLAttribute("class", pClass).Text);
            }
            else
            {
                Name = "p";
                Insert(pText);
                AddAttribute(new HTMLAttribute("id", pID).Text);
                AddAttribute(new HTMLAttribute("class", pClass).Text);
            }

        }
    }
    
    class TextField : Tag
    {
        public TextField()
        {
            Name = "input";
        }

        public TextField(string pId)
        {
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "text"));
            AddAttribute(new HTMLAttribute("id", pId));
        }

        public TextField(string pID, string pValue )
        {
            
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "text"));
            AddAttribute(new HTMLAttribute("id", pID));
            AddAttribute(new HTMLAttribute("Value", pValue));
         }

        public TextField(string pID,string pClass, string pValue)
        {

            Name = "input";
            AddAttribute(new HTMLAttribute("type", "text"));
            AddAttribute(new HTMLAttribute("id", pID));
            AddAttribute(new HTMLAttribute("Value", pValue));
            AddAttribute(new HTMLAttribute("Class", pClass));
        }
        
    }

    class TextArea : Tag
    {
        public TextArea()
        {
            Name = "textarea";
            AddAttribute(new HTMLAttribute("wrap", "soft"));
        }

        public TextArea(string pId)
        {
            Name = "textarea";
            AddAttribute(new HTMLAttribute("id", pId));
            AddAttribute(new HTMLAttribute("wrap", "soft"));
        }

        public TextArea(string pId, string pClass)
        {
            Name = "textarea";
            AddAttribute(new HTMLAttribute("id", pId));
            AddAttribute(new HTMLAttribute("class", pClass));
            AddAttribute(new HTMLAttribute("wrap", "soft"));
        }

        public TextArea(string pId, string pClass, string pValue)
        {
            Name = "textarea";
            AddAttribute(new HTMLAttribute("id", pId));
            AddAttribute(new HTMLAttribute("class", pClass));
            AddAttribute(new HTMLAttribute("wrap", "soft"));
            Insert(pValue);
        }
    }

    class CheckBox : Tag
    {

        public CheckBox()
        {
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "checkbox"));
        }


        public CheckBox(string pId)
        {
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "checkbox"));
            AddAttribute(new HTMLAttribute("id", pId));
        }

        public CheckBox(string pId, string pValue, string pName)
        {
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "checkbox"));
            AddAttribute(new HTMLAttribute("id", pId));
            AddAttribute(new HTMLAttribute("value", pValue));
            Insert(new HTMLText(pName, false));
        }
    }


    class RadioButton : Tag
    {

        public RadioButton()
        {
            Name = "input";
            AddAttribute(new HTMLAttribute("type", "radio"));
        }
    }


    class Form : Tag
    {
        public Form()
        {
            Name = "form";
        }
        
        public Form( string pName)
        {
            Name = "form";
            AddAttribute(new HTMLAttribute("name", pName));
        }
        
        
        public Form(string pID, string pName, string pAction)
        {
            Name = "form";
            AddAttribute(new HTMLAttribute("id", pID));
            AddAttribute(new HTMLAttribute("name", pName));
            AddAttribute(new HTMLAttribute("action", pAction));
        }
    }

    class Img : Tag
    {

        public Img()
        {
            Name = "img";
        }

        public Img(string pFilePath)
        {
            Name = "img";
            AddAttribute(new HTMLAttribute("src", pFilePath));
        }
        
        public Img( string pFilePath, string pAlt)
        {
            Name = "img";
            AddAttribute(new HTMLAttribute("src", pFilePath));
            AddAttribute(new HTMLAttribute("alt", pAlt));
        }
    }

    class TableRow : Tag
    {
        public TableRow()
        {
            Name = "tr";
        }
    }

    class TableData : Tag
    {
        public TableData()
        {
            Name = "td";
        }
    }

    class Table : Tag
    {
        public Table(System.Data.DataTable pDataTable, int pBorderSize, int pCellPadding)
        {
            Name = "table";
            AddAttribute(new HTMLAttribute("border", pBorderSize.ToString()));
            AddAttribute(new HTMLAttribute("cellpadding", pCellPadding.ToString()));

            foreach (System.Data.DataRow tDataRow in pDataTable.Rows)
            {
                TableRow tRow = new TableRow();

                for (int i = 0; i < tDataRow.ItemArray.Length; i++)
                {
                    TableData tElement = new TableData();
					tElement.Insert(new HTMLText(tDataRow[i].ToString(), false));
					tRow.Insert(tElement);
                }

				Insert(tRow);
            }
        }
    }

    class CSSStyle : Tag
    {
        public CSSStyle()
        {
            Name = "style";
			AddAttribute(new HTMLAttribute("type", "text/css"));
        }
    }

    public class JavaScript : Tag
    {

        public JavaScript()
        {
            Name = "script";
            AddAttribute(new HTMLAttribute("type", "text/javascript"));
        }
        
        public JavaScript(string pContent, bool IsFileName)
        {

            Name = "script";
            if (IsFileName)
            {
                AddAttribute(new HTMLAttribute("type", "text/javascript"));
                AddAttribute(new HTMLAttribute("src", pContent));
            }
            else
            {
                Insert(pContent);
            }
        }

        public JavaScript(string pFunctionName, string pFunction)
        {
            Name = "script";
            AddAttribute(new HTMLAttribute("language", "javascript"));
			AddAttribute(new HTMLAttribute("type", "text/javascript"));
			Insert(new JavaFunction(pFunctionName, pFunction));
        }

        public JavaScript(JqueryFunction tJquery)
        {
            Name = "script";
            Insert(tJquery.Text);

        }

        public void AddVar(string pName, string pValue)
        {
            Insert("var " + pName + " = " + pValue + ";");

        }
    }

    public class JavaFunction : Tag
    {
        public JavaFunction(string pFunctionName, string pFunction)
        {
            Text = "function " + pFunctionName + "() {" + Environment.NewLine + pFunction + Environment.NewLine +"}";
        }

        public JavaFunction(string pFunctionName,string pParameter, string pFunction)
        {
            Text = "function " + pFunctionName + "(" + pParameter + ") {" + Environment.NewLine + pFunction + Environment.NewLine + "}";
        }
    }


    public class JqueryFunction : Tag
    {
        public JqueryFunction(bool pOnDocumentReady, string pSelector,string pEventType, string pFunctionContent)
        {
            if(pOnDocumentReady)
            {

                string tText = @" $(document).ready(function(){{
                   $('{0}').{1}(function(event){{
                     {2}
                   }});
                 }});";

                Text = String.Format(tText, pSelector, pEventType, pFunctionContent);
            }
            else
            {
                string tText =@"$('{0}').{1}(function(event){{
                                {{2}}
                              }});";

                Text = String.Format(tText, pSelector, pEventType, pFunctionContent);
            }
            
            
        }

        public JqueryFunction(bool pOnDocumentReady, string pSelector, string pCommand)
        {
            if (pOnDocumentReady)
            {

                string tText = @" $(document).ready(function(){{
                   $('{0}').{1};
                 }});";

                Text = String.Format(tText, pSelector, pCommand);
            }
            else
            {
                string tText = @"$('{0}').{1}";

                Text = String.Format(tText, pSelector, pCommand);
            }


        }
    }



    class Paragraph : Tag
    {
        public Paragraph(string pID)
        {
            Name = "p";
            AddAttribute(new HTMLAttribute("id", pID));
        }
    }

    class NoScript : Tag
    {
        public NoScript()
        {
            Name = "noscript";
        }
    }

    class IFrame : Tag
    {
        public IFrame()
        {
            Name = "iframe";
            AddAttribute(new HTMLAttribute("name","RSIFrame"));
            AddAttribute(new HTMLAttribute("style", new CSSAttribute("left","-100px").Text + new CSSAttribute("width","1px").Text + new CSSAttribute("height","1px").Text + new CSSAttribute("visibility","hidden").Text + new CSSAttribute("display","none").Text + new CSSAttribute("border","0px").Text));
            AddAttribute(new HTMLAttribute("src","dummy.html"));
        }
    }

    class HtmlDiv : Tag
    {
        public HtmlDiv()
        {
            Name = "div";
        }

        public HtmlDiv(string pId)
        {
            Name = "div";
            AddAttribute(new HTMLAttribute("id", pId).Text);
        }

        public HtmlDiv(string pId, string pClass)
        {
            Name = "div";
            AddAttribute(new HTMLAttribute("id", pId).Text);
            AddAttribute(new HTMLAttribute("class", pClass).Text);
        }
    }

    class Slider : Tag
    {
        public Slider()
        {
            Name = "div";
        }

        public Slider(string pId, string pClass)
        {
            Name = "div";
            AddAttribute( new HTMLAttribute("id", pId));
            AddAttribute(new HTMLAttribute("class", pClass));
        }
    }
	#endregion


}
