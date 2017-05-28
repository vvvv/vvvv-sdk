using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.Runtime;

namespace CoreTests
{
	public abstract class ProjectTest
	{
		[TestFixtureSetUp]
		public void Init()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			SourceTemplateProject = CreateTemplateProject(SourceTemplateDir);
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
			CleanupDirectories();
		}
		
		protected void SetupDirectories()
		{
			CopyFilesRecursively(SourceTemplateDir, TemplateDir);
		}
		
		protected void CleanupDirectories()
		{
			// Make sure test dirs are cleaned up.
			if (Directory.Exists(TestDir))
				Directory.Delete(TestDir, true);
			
			if (Directory.Exists(TemplateDir))
				Directory.Delete(TemplateDir, true);
		}
		
		public static void CopyFilesRecursively(string source, string target)
		{
			CopyFilesRecursively(new DirectoryInfo(source), new DirectoryInfo(target));
		}
		
		public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
		{
			foreach (DirectoryInfo dir in source.GetDirectories())
				CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
			foreach (FileInfo file in source.GetFiles())
			{
				var destFileName = Path.Combine(target.FullName, file.Name);
				file.CopyTo(destFileName, true);
				
				// Make sure it's not read-only
				var destFile = new FileInfo(destFileName);
				destFile.IsReadOnly = false;
			}
		}
		
		public string TemplateDir
		{
			get
			{
				return SourceTemplateDir + "Working";
			}
		}
		
		public string SourceTemplateDir
		{
			get
			{
				var currentDir = Path.GetFullPath(Environment.CurrentDirectory + @"\..\..");
				return Path.Combine(currentDir, "Templates");
			}
		}
		
		public string TestDir
		{
			get
			{
				return Path.Combine(Environment.CurrentDirectory, "Test");
			}
		}
		
		protected IProject SourceTemplateProject
		{
			get;
			private set;
		}
		
		protected abstract IProject CreateTemplateProject(string baseDir);
		
		protected abstract IProject CreateProject(Uri location);
		
		protected abstract Uri TestLocation
		{
			get;
		}
		
		protected abstract Uri CloneLocation
		{
			get;
		}
		
		[Test]
		public void TestLoadProject()
		{
			SetupDirectories();
			
			try
			{
				using (var project = CreateTemplateProject(TemplateDir))
				{
					// Initial state (1)
					Assert.IsFalse(project.IsLoaded);
					Assert.IsFalse(project.IsDirty);
					Assert.AreEqual(0, project.Documents.Count);
					Assert.AreEqual(0, project.References.Count);
					
					// Load should bring us to state (3)
					bool loadEventRaised = false;
					
					project.Loaded += delegate(object sender, EventArgs e)
					{
						loadEventRaised = true;
					};
					
					// Load it.
					Assert.DoesNotThrow(() => project.Load(), "Load() caused exception.");
					
					Assert.IsTrue(loadEventRaised, "Loaded event wasn't raised.");
					Assert.IsTrue(project.IsLoaded, "IsLoaded should be true after Load().");
					Assert.IsFalse(project.IsDirty, "IsDirty should be false after Load().");
					
					// Check if load was ok.
					Assert.Greater(project.Documents.Count, 0);
					Assert.Greater(project.References.Count, 0);
				}
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestUnloadProject()
		{
			SetupDirectories();
			
			try
			{
				using (var project = CreateTemplateProject(TemplateDir))
				{
					// TODO: This is not good. We depend on Load(). How to solve?
					project.Load();
					
					// Unload should bring us to state (1)
					bool unloadEventRaised = false;
					
					project.Unloaded += delegate(object sender, EventArgs e)
					{
						unloadEventRaised = true;
					};
					
					// Unload it.
					Assert.DoesNotThrow(() => project.Unload(), "Unload() caused exception.");
					
					Assert.IsTrue(unloadEventRaised, "Unloaded event wasn't raised.");
					Assert.IsFalse(project.IsLoaded, "IsLoaded should be false after Unload().");
					Assert.IsFalse(project.IsDirty, "IsDirty should be false after Unload().");
					
					// Check if unload was ok.
					Assert.AreEqual(0, project.Documents.Count);
					Assert.AreEqual(0, project.References.Count);
				}
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestSaveProject()
		{
			SetupDirectories();
			
			try
			{
				using (var project = CreateTemplateProject(TemplateDir))
				{
					// TODO: This is not good. We depend on Load(). How to solve?
					project.Load();
					
					// Save the project -> state (3)
					bool saveEventRaised = false;
					
					project.Saved += delegate(object sender, EventArgs e)
					{
						saveEventRaised = true;
					};
					
					// Save it.
					Assert.DoesNotThrow(() => project.Save(), "Save() caused exception.");
					
					Assert.IsTrue(saveEventRaised, "Saved event wasn't raised.");
					Assert.IsTrue(project.IsLoaded, "IsLoaded should be true after Save().");
					Assert.IsFalse(project.IsDirty, "IsDirty should be false after Save().");
					
					// Test if same content.
					var expectedDir = new DirectoryInfo(SourceTemplateProject.Location.GetLocalDir());
					var actualDir = new DirectoryInfo(project.Location.GetLocalDir());
					DirectoriesContainSameFiles(expectedDir, actualDir, false);
				}
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestSaveToProject()
		{
			SetupDirectories();
			
			try
			{
				using (var project = CreateTemplateProject(SourceTemplateDir))
				{
					// TODO: This is not good. We depend on Load(). How to solve?
					project.Load();
					
					// Save the project -> state (3)
					bool saveEventRaised = false;
					
					project.Saved += delegate(object sender, EventArgs e)
					{
						saveEventRaised = true;
					};
					
					// Save it.
					Assert.DoesNotThrow(() => project.SaveTo(CloneLocation), "SaveTo() caused exception.");
					
					Assert.IsFalse(saveEventRaised, "Saved event was raised after SaveTo().");
					Assert.IsTrue(project.IsLoaded, "IsLoaded should be true after SaveTo().");
					Assert.IsFalse(project.IsDirty, "IsDirty should be false after SaveTo().");
					Assert.IsTrue(File.Exists(CloneLocation.LocalPath), "Can't find project file after SaveTo().");
					
					// Test if same content.
					var expectedDir = new DirectoryInfo(project.Location.GetLocalDir());
					var actualDir = new DirectoryInfo(CloneLocation.GetLocalDir());
					
					var newProject = CreateProject(CloneLocation);
					Assert.DoesNotThrow(() => newProject.Load(), "Project copy can't be loaded.");
					
					Assert.IsFalse(newProject.IsReadOnly, "IsReadOnly should be false after SaveTo().");
				}
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestCompileProject()
		{
			SetupDirectories();
			
			try
			{
				using (var project = CreateTemplateProject(TemplateDir))
				{
					// TODO: This is not good. We depend on Load(). How to solve?
					project.Load();
					
					bool projectCompiledSuccessfullyEventRaised = false;
					
					project.ProjectCompiledSuccessfully += delegate(object sender, CompilerEventArgs args)
					{
						projectCompiledSuccessfullyEventRaised = true;
					};
					
					bool compiledCompletedEventRaised = false;
					
					project.CompileCompleted += delegate(object sender, CompilerEventArgs args)
					{
						compiledCompletedEventRaised = true;
					};
					
					// Compile it.
					Assert.DoesNotThrow(() => project.Compile(), "Compile() caused exception.");
					
					Assert.IsTrue(projectCompiledSuccessfullyEventRaised, "ProjectCompiledSuccessfully event wasn't raised after Compile().");
					Assert.IsTrue(compiledCompletedEventRaised, "CompiledCompleted event wasn't raised after Compile().");
				}
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestLoadAndDisposeProject()
		{
			SetupDirectories();
			
			try
			{
				var project = CreateTemplateProject(TemplateDir);
				
				Assert.DoesNotThrow(() => project.Load());
				
				bool disposeEventRaised = false;
				
				project.Disposed += delegate(object sender, EventArgs e) 
				{
					disposeEventRaised = true;
				};
				
				Assert.DoesNotThrow(() => project.Dispose());
				Assert.IsTrue(disposeEventRaised, "Disposed event was raised after Dispose().");
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		[Test]
		public void TestLoadAndDisposeProjectTwice()
		{
			SetupDirectories();
			
			try
			{
				var project = CreateTemplateProject(TemplateDir);
				
				Assert.DoesNotThrow(() => project.Load());
				
				bool disposeEventRaised = false;
				
				project.Disposed += delegate(object sender, EventArgs e) 
				{
					disposeEventRaised = true;
				};
				
				Assert.DoesNotThrow(() => project.Dispose());
				Assert.IsTrue(disposeEventRaised, "Disposed event was raised after Dispose().");
				
				project = CreateTemplateProject(TemplateDir);
				
				Assert.DoesNotThrow(() => project.Load());
				
				disposeEventRaised = false;
				
				project.Disposed += delegate(object sender, EventArgs e) 
				{
					disposeEventRaised = true;
				};
				
				Assert.DoesNotThrow(() => project.Dispose());
				Assert.IsTrue(disposeEventRaised, "Disposed event was raised after Dispose().");
			}
			finally
			{
				CleanupDirectories();
			}
		}
		
		private void DirectoriesContainSameFiles(DirectoryInfo expected, DirectoryInfo actual, bool checkFileContent)
		{
			var expectedDirs = expected.GetDirectories();
			var actualDirs = actual.GetDirectories();
			
			Assert.AreEqual(expectedDirs.Length, actualDirs.Length, "Subdirectory count of {0} doesn't match {1}.", actual, expected);
			
			for (int i = 0; i < expectedDirs.Length; i++)
			{
				Assert.AreEqual(expectedDirs[i].Name, actualDirs[i].Name, "{0} doesn't contain same subdirectories as {1}.", actual, expected);
				DirectoriesContainSameFiles(expectedDirs[i], actualDirs[i], checkFileContent);
			}
			
			var expectedFiles = expected.GetFiles();
			var actualFiles = actual.GetFiles();
			
			Assert.AreEqual(expectedFiles.Length, actualFiles.Length, "File count of {0} doesn't match {1}.", actual, expected);
			
			for (int i = 0; i < expectedFiles.Length; i++)
			{
				Assert.AreEqual(expectedFiles[i].Name, actualFiles[i].Name);
				Assert.IsFalse(actualFiles[i].IsReadOnly, "Copied files should not be read-only.");
				if (checkFileContent)
				    FileAssert.AreEqual(expectedFiles[i], actualFiles[i], "File content of {0} differs from {1}.", actualFiles[i], expectedFiles[i]);
			}
		}
	}
}
