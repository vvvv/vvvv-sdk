using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using System.Diagnostics;

namespace VVVV.Webinterface.Utilities
{
    /// <summary>
    /// abstract Attribute class definition. 
    /// Holds the base functions for the HTML and CSS attributs 
    /// coded by chrismo
    /// </summary>
    public abstract class Attribute : ICloneable
    {
		private string m_Structure = "namevalue";
		private string m_Name;
        private List<string> m_Values = new List<string>();

        public string Structure { get { return m_Structure; } set { m_Structure = value; } }
		public string Name { get { return m_Name; } }
		public string[] Values { get { return m_Values.ToArray(); } }
		

        /// <summary>
        /// returns the attributes as String
        /// </summary>
		public string Text
        {
            get
            {
				StringBuilder tText = new StringBuilder();
				
				tText.Append(m_Structure.Replace("name", Name));

				StringBuilder tValueText = new StringBuilder();

                for (int i = 0; i < Values.Length; i++)
                {
                    tValueText.Append(Values[i]);
                }

				return tText.Replace("value", tValueText.ToString()).ToString();
            }
        }

        /// <summary>
        /// Attribute construcor.
        /// the attribute is build like name=value;
        /// </summary>
        /// <param name="pName">attribute name</param>
        /// <param name="pValue">attribute value</param>
        public Attribute(string pName, string pValue)
        {
            m_Name = pName;
            m_Values.Add(pValue);
        }

		/// <summary>
		/// Attribute construcor.
		/// builds an empty attribute for use in the clone method
		/// </summary>
		protected Attribute()
		{

		}

		/// <summary>
        /// inserts a value to the m_value List
        /// </summary>
        /// <param name="pValue"></param>
        public void InsertValue(string pValue)
        {
            m_Values.Add(pValue);
        }

        /// <summary>
        /// inserts an attribute to the m_Value List
        /// </summary>
        /// <param name="pAttribute"></param>
        public void InsertAttribute(Attribute pAttribute)
        {
            m_Values.Add(pAttribute.Text);
        }

		#region ICloneable Members

		public object Clone()
		{
			Attribute clonedObject = this;

			try
			{
				ConstructorInfo constructorInfo = this.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, Type.EmptyTypes, null);
				if (constructorInfo == null) throw (new Exception("Attribute Derived Class has no zero-argument constructor"));
				clonedObject = (Attribute)constructorInfo.Invoke(null);

				clonedObject.m_Structure = System.String.Copy(m_Structure);
				clonedObject.m_Name = System.String.Copy(m_Name);

				clonedObject.m_Values.Clear();
				
				foreach (string str in m_Values)
				{
                    if (str != null)
                        clonedObject.m_Values.Add(System.String.Copy(str));
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("Attribute cloning failed, could not obtain and instantiate derived class");
				Debug.WriteLine(e.Message);
			}

			return clonedObject;

		}
		
		#endregion
	}


    /// <summary>
    /// abstract Tag class defintion
    /// builds HTML or XML Tags
    /// </summary>
    public abstract class Tag : ICloneable
    {

        private int m_Level = 0;
        private string m_Text = "";
        private bool OverrideText = false;

        private string m_Name = "";
        private string m_OpenBegin = "";
        private string m_CloseBegin = "";
        private string m_End = "";
        private string mHtmlHeader = "";

        private List<Attribute> m_Attributes = new List<Attribute>();
        private List<string> m_AttributesAsStrings = new List<string>();

        private List<Tag> m_Tags = new List<Tag>();
        private List<string> m_Strings = new List<string>();

        public int Level { get { return m_Level; } set { m_Level = value; } }





        /// <summary>
        /// name of the Tag
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;

                m_OpenBegin = "<" + m_Name;
                m_CloseBegin = ">";
                m_End = "</" + m_Name + ">";
            }
        }

        public string HTMLHeader
        {
            set
            {
                mHtmlHeader =  value;

            }
        }

        /// <summary>
        /// generates a whole Tag out of the added attributes and the tag name
        /// </summary>
        public string Text
        { 
            get 
            {
                if (mHtmlHeader != "")
                {
                    return mHtmlHeader + Environment.NewLine + CreateText();
                }
                else
                {
                    return CreateText();
                }
                
            }
            set
            {
                m_Text = "\t" + value + Environment.NewLine;

                OverrideText = true;
            }
        }

        /// <summary>
        /// The whole list of alle added Attributes
        /// </summary>
        public Attribute[] Attributes
        {
            get
            {
                return m_Attributes.ToArray();
            }
        }


        public List<Tag> TagsInside
        {
            get
            {
                return m_Tags;
            }
        }



		public Tag()
		{
			
		}

        /// <summary>
        /// overloaded constructor 
        /// </summary>
        /// <param name="pName">sets the tag name attribute</param>
        public Tag(string pName)
        {
			m_Name = pName;
        }


        /// <summary>
        /// Adds an attribute Object to the Attribute List
        /// </summary>
        /// <param name="pAttribute">attribute instace</param>
        public void AddAttribute(Attribute pAttribute)
        {
            m_Attributes.Add(pAttribute);
        }

        /// <summary>
        /// adds an Attribute string tor the Attribute List
        /// </summary>
        /// <param name="pAttributeAsString"></param>
        public void AddAttribute(string pAttributeAsString)
        {
            m_AttributesAsStrings.Add(pAttributeAsString);
        }

        /// <summary>
        /// inserts an tag in the tag
        /// </summary>
        /// <param name="pTag"></param>
        public void Insert(Tag pTag)
        {
			pTag.Level = m_Level + 1;
			m_Tags.Add(pTag);
        }

        /// <summary>
        /// instert an string to the tag
        /// </summary>
        /// <param name="pString"></param>
        public void Insert(string pString)
        {
            m_Strings.Add(pString);
        }

        /// <summary>
        /// inserts an new line to the tag
        /// </summary>
        /// <param name="pNumber"></param>
        public void InsertNewLines(int pNumber)
        {
            string tBreaks = "";

            for (int i = 0; i < pNumber; i++)
                tBreaks += "<br>";

            m_Tags.Add(new HTMLText(tBreaks, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <returns></returns>
        public Tag GetTag(string pID)
        {
            foreach (Tag tTag in m_Tags)
            {
                foreach (Attribute tAttribute in tTag.Attributes)
                {
                    if (tAttribute.Name == "id")
                        return tTag;
                }
            }

            return null;
        }

        /// <summary>
        /// creats an string of alle elements (attributs, tags, text) in the tag class
        /// </summary>
        /// <returns></returns>
        private string CreateText()
        {
            if (OverrideText == true)
                return m_Text;

            string tTabs = "";

            for (int i = 0; i < m_Level-1; i++)
                tTabs += "\t";

            StringBuilder tText = new StringBuilder();

            tText.Append(tTabs + m_OpenBegin);


            for (int i = 0; i < m_AttributesAsStrings.Count; i++)
            {
                tText.Append(" " + m_AttributesAsStrings[i]);
            }

            for (int i = 0; i < m_Attributes.Count; i++)
            {
                tText.Append(m_Attributes[i].Text);
            }

            tText.Append(m_CloseBegin + Environment.NewLine);

            for (int i = 0; i <  m_Tags.Count; i++)
            {
                tText.Append(tTabs + m_Tags[i].Text);
            }

            for (int i = 0; i < m_Strings.Count; i++)
            {
                tText.Append(tTabs + m_Strings[i] + Environment.NewLine);
            }

            tText.Append(tTabs + m_End + Environment.NewLine);

            return tText.ToString();
        }

        public void ClearTagsInside()
        {
             m_Tags.Clear();
             m_Strings.Clear();
        }


		#region ICloneable Members

		public virtual object Clone()
		{
			Tag clonedObject = this;

			try
			{
				ConstructorInfo constructorInfo = this.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, Type.EmptyTypes, null);
				if (constructorInfo == null) throw (new Exception("Tag Derived Class has no zero-argument constructor"));
				clonedObject = (Tag)constructorInfo.Invoke(null);

				clonedObject.m_Level = m_Level;
				clonedObject.m_Text = System.String.Copy(m_Text);
				clonedObject.OverrideText = OverrideText;

				clonedObject.m_Name = System.String.Copy(m_Name);
				clonedObject.m_OpenBegin = System.String.Copy(m_OpenBegin);
				clonedObject.m_CloseBegin = System.String.Copy(m_CloseBegin);
				clonedObject.m_End = System.String.Copy(m_End);
				clonedObject.mHtmlHeader = System.String.Copy(mHtmlHeader);

				clonedObject.m_Attributes.Clear();
				foreach (Attribute attribute in m_Attributes)
				{
					clonedObject.m_Attributes.Add((Attribute)(attribute.Clone()));
				}

				clonedObject.m_AttributesAsStrings.Clear();
				foreach (string str in m_AttributesAsStrings)
				{
					clonedObject.m_AttributesAsStrings.Add(System.String.Copy(str));
				}

				clonedObject.m_Tags.Clear();
				foreach (Tag tag in m_Tags)
				{
					clonedObject.m_Tags.Add((Tag)(tag.Clone()));
				}

				clonedObject.m_Strings.Clear();
				foreach (String str in m_Strings)
				{
                    if(str != null)
                        clonedObject.m_Strings.Add(System.String.Copy(str));
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("Tag cloning failed, could not obtain and instantiate derived class");
				Debug.WriteLine(e.Message);
			}

			return clonedObject;
		}

		#endregion
	}



	#region CSS File

    /// <summary>
    /// creates an css Property
    /// inherit form the attribute, but with an other structure
    /// </summary>
	class Property : Attribute
	{
		protected Property()
		{

		}
		
		public Property(string pName, string pValue)
			: base(pName, pValue)
		{
			Structure = "name:value;";
		}
	}

    /// <summary>
    /// creates an css rule
    /// </summary>
	class Rule
	{
        public enum SelectorType
        {
            None = 0,
            Id = 1,
            Class = 2,
        }

		private string m_Selector = "";
		private string m_ClassName = "";
		private List<Property> m_Properties = new List<Property>();

        /// <summary>
        /// returns the whole rule as string
        /// </summary>
		public string Text
		{
			get
			{
				string tText = Environment.NewLine;

				tText += m_Selector;
				tText += m_ClassName;

				tText += "{" + Environment.NewLine;

				foreach (Property tProperty in m_Properties)
					tText += "\t" + tProperty.Text + Environment.NewLine;

				//tText += "}" + Environment.NewLine;
                tText += "}";

				return tText;
			}
		}

        /// <summary>
        /// crretas an Rule with an selecotor
        /// </summary>
        /// <param name="pSelector"></param>
		public Rule(string pSelector)
		{
			m_Selector = pSelector;
		}		
		
        /// <summary>
        /// overriden constructor
        /// creates an rule with a selector anf a classname
        /// </summary>
        /// <param name="pSelector"></param>
        /// <param name="pClassName"></param>
		public Rule(string pSelector, string pClassName)
		{
			m_Selector = pSelector;
			m_ClassName = "." + pClassName;
		}

        public Rule(string pSelectorName, SelectorType pSelectorType)
        {
            if (SelectorType.None == pSelectorType)
            {
                m_Selector = pSelectorName;
            }
            else if (SelectorType.Id == pSelectorType)
            {
                m_Selector = "#" + pSelectorName;
            }
            else
            {
                m_Selector = "." + pSelectorName;
            }
        }
 

        /// <summary>
        /// adds an property.
        /// </summary>
        /// <param name="pProperty"></param>
		public void AddProperty(Property pProperty)
		{
			m_Properties.Add(pProperty);
		}
	}


    /// <summary>
    /// can include all Rules, if there are more then one.
    /// </summary>
	class CSS
	{
		private List<Rule> m_Rules = new List<Rule>();

		public string Text
		{
			get
			{
				string tText = "";

				foreach (Rule tRule in m_Rules)
					tText += tRule.Text;

				return tText;
			}
		}

		public CSS()
		{

		}

        /// <summary>
        /// adds an rule 
        /// </summary>
        /// <param name="pRule">rule instance</param>
		public void AddRule(Rule pRule)
		{
			m_Rules.Add(pRule);
		}

        /// <summary>
        /// adds an rule with an selector
        /// </summary>
        /// <param name="pSelector">string</param>
        /// <param name="pProperties">Property Instance</param>
		public void AddRule(string pSelector, Property[] pProperties)
		{
			Rule tRule = new Rule(pSelector);

			foreach (Property tProperty in pProperties)
				tRule.AddProperty(tProperty);

			m_Rules.Add(tRule);
		}

        /// <summary>
        /// adds an rule with an selector and an classname
        /// </summary>
        /// <param name="pSelector">string</param>
        /// <param name="pClassName">string</param>
        /// <param name="pProperties">Property instance</param>
		public void AddRule(string pSelector, string pClassName, Property[] pProperties)
		{
			Rule tRule = new Rule(pSelector, pClassName);

			foreach (Property tProperty in pProperties)
				tRule.AddProperty(tProperty);

			m_Rules.Add(tRule);
		}


	}

	#endregion

}
