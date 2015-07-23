using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Internals.Bullet;

using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="DestroyBody", Category="Bullet", Version = "DX9", Author="vux", 
		Help="Destroys an existing bullet body (rigid or soft)",AutoEvaluate=true)]
	public class BulletDestroyBodyNode : AbstractBodyInteractionNode<CollisionObject>
	{
		protected override void ProcessObject(CollisionObject obj, int slice)
		{
			BodyCustomData bd = (BodyCustomData)obj.UserObject;
			bd.MarkedForDeletion = true;	
		}
	}
}
