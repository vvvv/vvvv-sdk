using System;
using System.IO;
using NUnit.Framework;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;

namespace CoreTests
{
	[TestFixture]
	public class FXProjectTest : ProjectTest
	{
		private readonly string FIncludePath = Path.GetFullPath(@"..\..\..\..\lib\nodes\");
		
		protected override Uri TestLocation 
		{
			get 
			{
				return new Uri(TestDir + "/TestProject/Project.fx");
			}
		}
		
		protected override Uri CloneLocation 
		{
			get 
			{
				return new Uri(TestDir + "/ClonedProject/Project.fx");
			}
		}
		
		protected override IProject CreateTemplateProject(string baseDir)
		{
			return new FXProject("fx_project", new Uri(baseDir + "/EffectTemplate/Attractor3d.fx"), FIncludePath);
		}
		
		protected override IProject CreateProject(Uri location)
		{
			return new FXProject("fx_project", location, FIncludePath);
		}
	}
}
