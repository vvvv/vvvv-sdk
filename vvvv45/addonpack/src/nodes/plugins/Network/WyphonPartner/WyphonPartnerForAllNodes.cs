/*
 * Created by SharpDevelop.
 * User: frederik
 * Date: 18/12/2012
 * Time: 9:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Wyphon;

namespace VVVV.Nodes.Network
{
	/// <summary>
	/// A WyphonPartner will contain STATIC variables, so each instance of VVVV can have only 1 WyphonPartner.
	/// 
	/// Thus makes it easy to have multiple nodes that all share the same WyphonPartner.
	/// </summary>
	public class WyphonPartnerForAllNodes
	{
		private static string wyphonPartnerForAllNodesName = "vvvv";

		private static WyphonPartner wyphonPartnerForAllNodes;
		
		public static string wyphonPartnerName {
			get { return wyphonPartnerForAllNodesName; }
			//set { wyphonPartnerForAllNodesName = Value; }
		}
			
		public static WyphonPartner wyphonPartner {
			get { 
				if (wyphonPartnerForAllNodes == null) {
					wyphonPartnerForAllNodes = new WyphonPartner( wyphonPartnerForAllNodesName ); 
				}
				return wyphonPartnerForAllNodes; }
		}
		
		public WyphonPartnerForAllNodes()
		{
		}
	}
}
