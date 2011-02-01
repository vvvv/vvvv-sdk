using System;
using System.IO;
using NUnit.Framework;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;

namespace CoreTests
{
	[TestFixture]
	public class CSProjectTest : ProjectTest
	{
		protected override Uri TestLocation 
		{
			get 
			{
				return new Uri(TestDir + "/TestCSProject/TestProject.csproj");
			}
		}
		
		protected override Uri CloneLocation 
		{
			get 
			{
				return new Uri(TestDir + "/ClonedCSProject/TestProject.csproj");
			}
		}
		
		protected override IProject CreateTemplateProject(string baseDir)
		{
			return new CSProject("cs_project", new Uri(baseDir + "/Template/Template.csproj"));
		}
		
		protected override IProject CreateProject(Uri location)
		{
			return new CSProject("cs_project", location);
		}
	}
}
