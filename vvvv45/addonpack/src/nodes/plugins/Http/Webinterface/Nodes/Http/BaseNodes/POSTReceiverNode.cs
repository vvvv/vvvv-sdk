using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface;

namespace VVVV.Nodes.Http.BaseNodes
{
	public abstract class POSTReceiverNode
	{
		//Host
        protected IPluginHost FHost;
		
		private WebinterfaceSingelton FWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
		protected string FNodeId;
		protected bool FChangedNodeId = true;
		protected List<string> FSliceId = new List<string>();
		protected int FSpreadMax = 0;
		protected SortedList<int, string> FSavedResponses = new SortedList<int, string>();
		protected string FNodePath;
		protected bool FChangedSpreadSize = false;
		protected bool FReceivedNewString = false;
		protected List<string> FReceivedString = new List<string>();
		protected bool FInitFlag = true;

		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			FHost.GetNodePath(true, out FNodePath);
			FNodeId = HTMLToolkit.CreatePageID(FNodePath);

			try
			{
				FWebinterfaceSingelton.HostPath = FNodePath;

				CreateBasePins();
			}
			catch (Exception ex)
			{
				FHost.Log(TLogType.Error, "Error in POSTReceiverNode (Http) by Pin Initialisation" + Environment.NewLine + ex.Message);
			}
		}

		protected abstract void CreateBasePins();

		public void Evaluate(int SpreadMax)
		{
			try
			{
				string currentNodePath;

				FHost.GetNodePath(true, out currentNodePath);
				FChangedNodeId = FNodePath != currentNodePath;
				if (FChangedNodeId)
				{
					FNodePath = currentNodePath;
					FNodeId = HTMLToolkit.CreatePageID(FNodePath);
				}

				FChangedSpreadSize = FSpreadMax != SpreadMax;

				if (FChangedSpreadSize)
				{
					if (FSliceId.Count > SpreadMax)
					{
						FSliceId.RemoveRange(SpreadMax, FSliceId.Count - SpreadMax);
					}
					else
					{
						for (int i = FSpreadMax; i < SpreadMax; i++)
						{
							FSliceId.Add(HTMLToolkit.CreateSliceID(FNodePath, i));
						}
					}
				}

				#region ReceivedData

				FReceivedNewString = CheckIfNodeReceivedData();


				if (FReceivedNewString || FInitFlag || FChangedSpreadSize)
				{
					for (int i = 0; i < SpreadMax; i++)
					{
						if (FInitFlag)
						{
							FReceivedString.Add(GetNewDataFromServer(i));
						}
						else if (i >= FReceivedString.Count)
						{
							FReceivedString.Add(GetNewDataFromServer(i));
						}
						else
						{
							FReceivedString[i] = GetNewDataFromServer(i);
						}
					}

					FInitFlag = false;
				}

				#endregion ReceivedData

				BaseEvaluate(SpreadMax);
				FChangedNodeId = false;
				FSpreadMax = SpreadMax;
			}
			catch (Exception ex)
			{
				FHost.Log(TLogType.Error, "in Node with Id: " + FNodePath + Environment.NewLine + ex.Message);
			}
		}

		protected abstract void BaseEvaluate(int SpreadMax);

		#region Get data from WebinterfaceSingelton

		protected bool CheckIfNodeReceivedData()
		{
			if (FWebinterfaceSingelton.CheckIfNodeIdReceivedValues(FNodeId))
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		protected string GetNewDataFromServer(int SliceNumber)
		{

			string tContent = null;

			FWebinterfaceSingelton.getNewBrowserData(FSliceId[SliceNumber], FNodePath, SliceNumber, out tContent);

			if (tContent == null && FSavedResponses.ContainsKey(SliceNumber))
			{
				FSavedResponses.TryGetValue(SliceNumber, out tContent);
			}

			if (FSavedResponses.ContainsKey(SliceNumber))
			{
				FSavedResponses.Remove(SliceNumber);
				FSavedResponses.Add(SliceNumber, tContent);
			}
			else
			{
				FSavedResponses.Add(SliceNumber, tContent);
			}

			FWebinterfaceSingelton.AddListToSave(FNodePath, FSavedResponses);

			return tContent;
		}

		#endregion Get data from WebinterfaceSingelton
	}
}
