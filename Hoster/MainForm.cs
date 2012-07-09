using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{
	public partial class MainForm
	{
		public List<PlugInfo> FPlugins;
		private XmlDocument FDocument;
		private int FPluginCount = 0;
		private TabControl PluginTabs;
		
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
		public MainForm()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			FPlugins = new List<PlugInfo>();
			ScanForPlugins();

			if (! System.IO.File.Exists("./savegame.xml"))
			{
				XmlTextWriter xml = new XmlTextWriter("savegame.xml", null);
				xml.WriteStartElement("HOST");
				xml.WriteEndElement();
				xml.Close();
			}
			
			FDocument = new XmlDocument();
			LoadFile();
		}
		
		public PluginPage AddPlugin(string Path, string ClassName)
		{
			//if this is the first plugin, simply put it on the panel
			PluginPage pp = new PluginPage();
			pp.Dock = DockStyle.Fill;
			
			if (FPluginCount == 0)
				pp.Parent = MainPanel;
			else
			{
				TabPage tab;
				//if not yet there, create the TabControl
				if (PluginTabs == null)
				{
					PluginTabs = new TabControl();
					PluginTabs.MouseClick += PluginTabsMouseClick;
					PluginTabs.SelectedIndexChanged += PluginTabsSelectedIndexChanged;
					PluginTabs.Parent = MainPanel;
					PluginTabs.Dock = DockStyle.Fill;
					
					//move first plugin to TabPage
					tab = new TabPage();
					MainPanel.Controls[0].Parent = tab;
					tab.Text = (tab.Controls[0] as PluginPage).NodeInfoName;
					
					PluginTabs.TabPages.Add(tab);
				}
				
				tab = new TabPage();
				PluginTabs.TabPages.Add(tab);
				pp.Parent = tab;
				
				//tab.Height = PluginPages.Height; //initializing
				PluginTabs.SelectedIndex = PluginTabs.TabPages.Count-1;
			}
			
			//if this is an additional plugin (to the first)
			//create a TabPage and put allplugins there
			//AddPluginPage(FPlugins[PlugIndex].Path, FPlugins[PlugIndex].ClassName);
			
			pp.LoadPlugin(Path, ClassName);
			
			if (PluginTabs != null)
				PluginTabs.TabPages[PluginTabs.SelectedIndex].Text = pp.NodeInfoName;
			
			this.Text = pp.NodeInfoName + "SA";
			FPluginCount++;
			return pp;
		}
		
		private PluginPage AddPluginPage(string Path, string ClassName)
		{
			TabPage tab = new TabPage();
			tab.Text = ClassName;
			PluginTabs.TabPages.Add(tab);
			tab.Height = PluginTabs.Height; //initializing
			PluginTabs.SelectedIndex = PluginTabs.TabPages.Count-1;
			
			PluginPage pp = new PluginPage();
			pp.Parent = tab;
			pp.Dock = DockStyle.Fill;
			
			pp.LoadPlugin(Path, ClassName);
			
			this.Text = pp.NodeInfoName + "SA";
			
			return pp;
		}
		
		void ScanForPlugins()
		{
			string[] dlls;
			Type objInterface;
			string path = Application.StartupPath;
			dlls = System.IO.Directory.GetFileSystemEntries(path, "*.dll");
			
			System.Reflection.Assembly plugindll;
			for (int i=0; i<dlls.Length; i++)
			{
				try
				{
					plugindll = System.Reflection.Assembly.LoadFrom(dlls[i]);
					//Loop through each type in the DLL
					
					foreach (System.Type objType in plugindll.GetTypes())
					{
						//Only look at public, non abstract types
						if (objType.IsPublic && !objType.IsAbstract)
						{
							//See if this type implements our interface
							objInterface = objType.GetInterface("VVVV.PluginInterfaces.V1.IPlugin");
							if (objInterface != null)
							{
								PlugInfo p = new PlugInfo();
								p.ClassName = objType.FullName;
								p.Path = plugindll.Location;
								FPlugins.Add(p);
							}
						}
					}
				}
				catch
				{
					//some .dlls except on being asked for types
					//Console.Beep(400, 10);
				}
			}
			
			//add plugins to menu
			foreach (PlugInfo p in FPlugins)
			{
				newToolStripMenuItem.DropDownItems.Add(p.ClassName);
			}
		}
		
		void NewToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			int plugIndex = newToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem);
			AddPlugin(FPlugins[plugIndex].Path, FPlugins[plugIndex].ClassName);
		}
		
		void PluginTabsMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
			{
				for (int i=0; i<PluginTabs.TabPages.Count; i++)
				{
					if (PluginTabs.GetTabRect(i).Contains(e.Location))
					{
						RemovePage(PluginTabs.TabPages[i]);
						break;
					}
				}
			}
			
			//if there is only one plugin left, remove the TabControl
			if (PluginTabs.TabCount == 1)
			{
				TabPage page = PluginTabs.TabPages[0];
				page.Controls[0].Parent = MainPanel;
				RemovePage(page);
				PluginTabs.Dispose();
				PluginTabs = null;
			}
		}
		
		public void RemovePage(TabPage page)
		{
			PluginTabs.TabPages.Remove(page);
			page.Dispose();
		}

		void SaveButtonClick(object sender, System.EventArgs e)
		{
			Save();
		}

		void Save()
		{
			string path = Path.Combine(Application.StartupPath, "savegame.xml");
			FDocument.Load(path);
			XmlNode host = FDocument.SelectSingleNode(@"//HOST");
			if (host != null)
			{
				host.RemoveAll();
				
				XmlAttribute st;
				
				if (PluginTabs != null)
					for (int i=0; i<PluginTabs.TabPages.Count; i++)
				{
					if (PluginTabs.TabPages[i].Controls[0] is PluginPage)
						(PluginTabs.TabPages[i].Controls[0] as PluginPage).SaveToXML(FDocument);
					
					//save host attributes
					st = FDocument.CreateAttribute("selectedtab");
					st.Value = PluginTabs.SelectedIndex.ToString();
					host.Attributes.Append(st);
				}
				else
					(MainPanel.Controls[0] as PluginPage).SaveToXML(FDocument);
				
				st = FDocument.CreateAttribute("windowleft");
				st.Value = this.Left.ToString();
				host.Attributes.Append(st);
				st = FDocument.CreateAttribute("windowtop");
				st.Value = this.Top.ToString();
				host.Attributes.Append(st);
				st = FDocument.CreateAttribute("windowwidth");
				st.Value = this.Width.ToString();
				host.Attributes.Append(st);
				st = FDocument.CreateAttribute("windowheight");
				st.Value = this.Height.ToString();
				host.Attributes.Append(st);
			}
			
			FDocument.Save(path);
		}
		
		void LoadFile()
		{
			string path = Path.Combine(Application.StartupPath, "savegame.xml");
			FDocument.Load(path);
			
			XmlNode host;
			XmlAttribute attr;
			string[] nodename;
			char[] separator = {'|'};
			host = FDocument.SelectSingleNode(@"//HOST");
			
			if (host != null)
			{
				//restore window
				attr = host.Attributes.GetNamedItem("windowleft") as XmlAttribute;
				if (attr != null)
					this.Left = Convert.ToInt32(attr.Value);
				attr = host.Attributes.GetNamedItem("windowtop") as XmlAttribute;
				if (attr != null)
					this.Top = Convert.ToInt32(attr.Value);
				attr = host.Attributes.GetNamedItem("windowwidth") as XmlAttribute;
				if (attr != null)
					this.Width = Convert.ToInt32(attr.Value);
				attr = host.Attributes.GetNamedItem("windowheight") as XmlAttribute;
				if (attr != null)
					this.Height = Convert.ToInt32(attr.Value);
				
				foreach(XmlNode plugin in host.ChildNodes)
				{
					attr = plugin.LastChild.Attributes.GetNamedItem("nodename") as XmlAttribute;
					nodename = attr.Value.Split(separator);
					
					if (System.IO.File.Exists(nodename[0]))
					{
						PluginPage pp = AddPlugin(nodename[0], nodename[1]);
						System.Diagnostics.Debug.WriteLine("Loading From XML");
						pp.LoadFromXML(plugin);
					}
				}
				
				//select saved tabindex
				attr = host.Attributes.GetNamedItem("selectedtab") as XmlAttribute;
				if ((attr != null) && (PluginTabs != null))
					PluginTabs.SelectedIndex = Convert.ToInt32(attr.Value);
			}
		}
		
		void PluginTabsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (PluginTabs.TabPages.Count > 0)
				if (PluginTabs.SelectedTab.Controls.Count > 0)
					if (PluginTabs.SelectedTab.Controls[0] is PluginPage)
			{
				this.Text = (PluginTabs.SelectedTab.Controls[0] as PluginPage).NodeInfoName + "SA";
			}
		}
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}
		
		void OSCToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (PluginTabs == null)
				(MainPanel.Controls[0] as PluginPage).OSC = !(MainPanel.Controls[0] as PluginPage).OSC;
			else
				(PluginTabs.TabPages[PluginTabs.SelectedIndex].Controls[0] as PluginPage).OSC = !(PluginTabs.TabPages[PluginTabs.SelectedIndex].Controls[0] as PluginPage).OSC;
		}
		
		void DebugToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (PluginTabs == null)
				(MainPanel.Controls[0] as PluginPage).Debug = !(MainPanel.Controls[0] as PluginPage).Debug;
			else
				(PluginTabs.TabPages[PluginTabs.SelectedIndex].Controls[0] as PluginPage).Debug = !(PluginTabs.TabPages[PluginTabs.SelectedIndex].Controls[0] as PluginPage).Debug;			
		}
	}
	
	public class PlugInfo
	{
		private string FPath;
		private string FClassName;
		
		public string Path
		{
			get {return FPath;}
			set {FPath = value;}
		}
		public string ClassName
		{
			get {return FClassName;}
			set {FClassName = value;}
		}
	}
}
