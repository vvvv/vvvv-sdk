using System;
using System.Collections.Generic;
using System.IO;
using VVVV.PluginInterfaces.V1;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Collections;
using System.Linq;
using VVVV.Utils.Linq;

namespace VVVV.Nodes
{


    #region Dir Node

    /// <summary>
    /// Description of RS232.
    /// </summary>
    /// 
    public class Dir : IDisposable, IPlugin
    {

        #region Field declaration
        // PLUGIN HOST
        ///////////////////////
        private IPluginHost FHost;
        private bool FDisposed = false;

        //Input Pins
        private IStringIn FDirectory;
        private IValueIn FSubdirectories;
        private IValueIn FShortFilenameIn;
        private IStringIn FMask;
        private IEnumIn FMaskRule;
        private IValueIn FCountIn;
        private IEnumIn FCountOrder;
        private IValueIn FUpdate;
        


        // Output Pins
        private IStringOut FFiles;
        private IStringOut FShortFilenameOut;
        private IValueOut FCountOut;
        private IStringOut FMessage;


        //Search
        private SearchProcess FSearch = null;

        #endregion Field Declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Dir()
        {

        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (FDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (FHost != null)
                        FHost.Log(TLogType.Message, PluginInfo.Name + " has been deleted");
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }

            FDisposed = true;
        }


        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in WebTypes derived from this class.
        /// </summary>
        ~Dir()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion constructor / desconstructor


        #region pluginInfo

        public bool AutoEvaluate
        {
            get { return false; }
        }

        /// <summary>
        /// PluginInfo
        /// </summary>
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Dir";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "File";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "Advanced";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "Files, Directory, Search";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        #endregion pluginInfo


        #region Pin Creation

        public void SetPluginHost(IPluginHost Host)
        {
            FHost = Host;

            //INPUT-PINS
            FHost.CreateStringInput("Mask", TSliceMode.Dynamic, TPinVisibility.True, out FMask);
            FMask.SetSubType("*.*", false);
            
            FHost.CreateStringInput("Directory", TSliceMode.Single, TPinVisibility.True, out FDirectory);
            FDirectory.SetSubType2("", int.MaxValue, string.Empty, TStringType.Directory);

            FHost.CreateValueInput("Include Subdirectories", 1, null, TSliceMode.Single, TPinVisibility.True, out FSubdirectories);
            FSubdirectories.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Show Short Filename", 1, null, TSliceMode.Single, TPinVisibility.False, out FShortFilenameIn);
            FShortFilenameIn.SetSubType(0, 1, 1, 1, false, true, true);

            FHost.UpdateEnum("Sort Order", "Name", new string[] { "Name", "FullName", "FileSize", "Extension", "LastAccess", "LastWriteTime", "CreationTime" });
            FHost.CreateEnumInput("Sort Order", TSliceMode.Single, TPinVisibility.True, out FMaskRule);
            FMaskRule.SetSubType("Sort Order");
            
            FHost.CreateValueInput("Count", 1, null, TSliceMode.Single, TPinVisibility.True, out FCountIn);
            FCountIn.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            FHost.UpdateEnum("Count Selection", "First", new string[] { "First", "Last"});
            FHost.CreateEnumInput("Count Selection", TSliceMode.Single, TPinVisibility.True, out FCountOrder);
            FCountOrder.SetSubType("Count Selection");

            FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out FUpdate);
            FUpdate.SetSubType(0, 1, 1, 0, true, false, true);


            // OUTPUT-PINS
            FHost.CreateStringOutput("Filenames", TSliceMode.Dynamic, TPinVisibility.True, out FFiles);
            FFiles.SetSubType("", true);

            FHost.CreateStringOutput("Short Filenames", TSliceMode.Dynamic, TPinVisibility.True, out FShortFilenameOut);
            FShortFilenameOut.SetSubType("", false);

            FHost.CreateValueOutput("File Count", 1, null, TSliceMode.Single, TPinVisibility.True, out FCountOut);
            FCountOut.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            FHost.CreateStringOutput("Message", TSliceMode.Single, TPinVisibility.Hidden, out FMessage);
            FMessage.SetSubType("OK",false);
        }

        #endregion Pin Creation


        #region Configurate

        public void Configurate(IPluginConfig pInput)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        #endregion Configurate


        #region Evaluate

        /// <summary>
        /// The Mainloop
        /// </summary>
        public void Evaluate(int SpreadMax)
        {
            // Get Node Settings
            string DirectoryPath = "";
            double IncludeSubdirectories = 0;
            List<string> Mask = new List<string>();
            string MaskRule = "";
            string CountOrder = "";
            bool Update = false;
            SetMessage("Please Update");
            double CountIn = 0;

            //Check if any pin is changed and setup a new search;
            if (FDirectory.PinIsChanged || FSubdirectories.PinIsChanged || FMask.PinIsChanged || FMaskRule.PinIsChanged || FCountIn.PinIsChanged || FShortFilenameIn.PinIsChanged || FCountOrder.PinIsChanged || FUpdate.PinIsChanged)
            {
                //Get the DirectoryPath String an check if it null, empty or exist. if not the function is returned
                FDirectory.GetString(0, out DirectoryPath);
                if(String.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath))
                {
                    FFiles.SliceCount = 0;
                    FShortFilenameOut.SliceCount = 0;
                    FCountOut.SetValue(0, 0);
                    
                    SetMessage("Please Enter a correct directory Path..");
                    return;
                }

                FSubdirectories.GetValue(0, out IncludeSubdirectories);
                FMaskRule.GetString(0, out MaskRule);;        
                FCountIn.GetValue(0, out CountIn);
                FCountOrder.GetString(0, out CountOrder);

                char[] invalidPathChars = Path.GetInvalidPathChars();
                var n1 = ".." + Path.DirectorySeparatorChar;
                var n2 = ".." + Path.AltDirectorySeparatorChar;
                //Get all Serach Masks 
                for (int i = 0; i < FMask.SliceCount; i++)
                {
                    string MaskSlice;
                    FMask.GetString(i,out MaskSlice);
                    
                    if(!String.IsNullOrEmpty(MaskSlice))
                    {
                        //the remarks here explain several things that are not allowed for the searchPattern:
                        //https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx
                        if (MaskSlice.None(c => invalidPathChars.Contains(c)) && !MaskSlice.EndsWith("..") && !MaskSlice.Contains(n1) && !MaskSlice.Contains(n2))
                            Mask.Add(MaskSlice);
                    }
                }

                //if there is no Mask create one, otherwise the Function getFiles crashes
                if (Mask.Count == 0)
                {
                    Mask.Add("*.*");
                }

                //Create a new Search with the given setup. If there exist a Search Object we are still Searching. 
                if (FSearch == null)
                {
                    //Set if the Subdirectories are included or not and Create a new Search Process Object which handle all Files Searching and Sorting in a new Thread
                    if (IncludeSubdirectories <= 0)
                        FSearch = new SearchProcess(DirectoryPath, Mask.ToArray(), SearchOption.TopDirectoryOnly, MaskRule, CountIn,CountOrder);
                    else
                        FSearch = new SearchProcess(DirectoryPath, Mask.ToArray(), SearchOption.AllDirectories,MaskRule, CountIn, CountOrder);

                    if (FSearch != null)
                    {
                        //Start the Searching.. 
                        SetMessage(FSearch.Search());
                    }
                }
                else
                {
                    SetMessage("Still Searching... Please Wait...");
                }
            }

            //Read the Resulte of the SearchProcess
            if (FSearch != null)
            {
                string Message = FSearch.GetStatus();
                SetMessage(Message);

                string[] FullFilesNames = FSearch.GetFullFileNames();


                if (FullFilesNames != null)
                {
                    double IncludeShortFilename = 0;
                    FShortFilenameIn.GetValue(0, out IncludeShortFilename);

                    FFiles.SliceCount = FullFilesNames.Length;
                    FCountOut.SetValue(0, FullFilesNames.Length);

                    for (int i = 0; i < FullFilesNames.Length; i++)
                    {
                        string FilePath = FullFilesNames[i];
                        FFiles.SetString(i, FilePath);

                        if (IncludeShortFilename >= 0.5)
                        {
                            string[] ShortFilename = FSearch.GetShortFileNames();

                            FShortFilenameOut.SliceCount = ShortFilename.Length;
                            FShortFilenameOut.SetString(i, ShortFilename[i]);
                        }
                        else
                        {
                            FShortFilenameOut.SliceCount = 0;
                        }
                    }
                    FSearch = null;
                } 
            }
        }

        private void SetMessage(string Message)
        {
            FMessage.SetString(0, Message);
            if (Message != "Please Update")
            {
                FHost.Log(TLogType.Debug, Message);
            }
        }

        #endregion Evaluate
    }

    #endregion Dir Node


    #region Search Process Class

    /// <summary>
    /// Search and Sort Files in a given Directory
    /// </summary>
    class SearchProcess
    {

        #region FieldDeclartion

        //Threading
        Thread SearchThread;
        Object Lock = new Object();
        

        //Search and Sort
        string[] FFullFileName;
        string[] FShortFilename;
        string FDirectory = "";
        string[] FMask;
        string FStatus;
        string FMaskRule;
        SearchOption FSearchOption;
        double FCount;
        string FCountOrder;


        #endregion FieldDeclartion


        #region Constructor

        /// <summary> 
        /// Constructor. Set the Filed Values and Init an new Thread.
        /// </summary>
        /// <param name="DirectoryPath">The Path to Search for Files</param>
        /// <param name="Mask">The File Mask which kind of Files are search</param>
        /// <param name="SearchOption">The Search option fot the GetFile()</param>
        /// <param name="MaskRule">The Rule which shows how are the the Files Sorted</param>
        /// <param name="Count">Show how many files are Searched for</param>
        /// <param name="CountOrder">Identfies if the first or last files are Shown</param>
        public SearchProcess(string DirectoryPath, string[] Mask, SearchOption SearchOption,string MaskRule, double Count, string CountOrder)
        {
            FStatus = "Init Search";
            SearchThread = new Thread(new ThreadStart(Run));
            SearchThread.IsBackground = true;

            FDirectory = DirectoryPath;
            FMask = Mask;
            FMaskRule = MaskRule;
            FSearchOption = SearchOption;
            FCount = Count;
            FCountOrder = CountOrder;
        }

        #endregion Constructor


        #region Getter Functions

        /// <summary>
        /// Returns all File Pathes
        /// </summary>
        /// <returns></returns>
        public string[] GetFullFileNames()
        {
            //if the Thread has locked the FFullFilename it can not acces via vvvv.
            if (Monitor.TryEnter(Lock))
            {
                try
                {
                    return FFullFileName;
                }
                finally
                {
                    Monitor.Exit(Lock);
                }

                
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all ShortFileNames
        /// </summary>
        /// <returns></returns>
        public string[] GetShortFileNames()
        {
            //if the Thread has locked the FShortFilename it can not acces via vvvv.
            if (Monitor.TryEnter(Lock))
            {
                try
                {
                    return FShortFilename;
                }
                finally
                {
                    Monitor.Exit(Lock);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the State of the Searching
        /// </summary>
        /// <returns></returns>
        public string GetStatus()
        {
            //if the Thread has locked the FStatus it can not acces via vvvv.
            if (Monitor.TryEnter(Lock))
            {
                try
                {
                    return FStatus;
                }
                finally
                {
                    Monitor.Exit(Lock);
                }
            }
            else
            {
                return "Searching...";
            }
        }

        #endregion Getter Functions


        #region Search Process

        /// <summary>
        /// Init a new Search if there is no Thread running. 
        /// </summary>
        /// <returns>State of the Search</returns>
        public string Search()
        {
            if (Monitor.TryEnter(Lock))
            {
                try
                {
                    if (FStatus == "Init Search")
                    {
                        FStatus = "Searching...";
                        SearchThread.Start();
                        return "Start Searching";
                    }
                    else
                    {
                        return "Still Searching. Please Wait...";
                    }
                }
                finally
                {
                    Monitor.Exit(Lock);
                }
            }
            else
            {
                return "Still Searching. Please Wait...";
            }
        }


        /// <summary>
        /// The Search is Started at this Point in a new Thread.
        /// </summary>
        private void Run()
        {
            
            //Lockes the Fiels Variables that they are only used in this Thread an not tried to accessed via the vvvv Thread
            Monitor.Enter(Lock);
            try
            {

                ArrayList SortedFiles = new ArrayList();
                IComparer Comparer;

                //Decieds which kind of Sorting is selected
                switch (FMaskRule)
                {
                    case "Name":
                        Comparer = new FileComparer(FileComparer.CompareBy.Name);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "LastAccess":
                        Comparer = new FileComparer(FileComparer.CompareBy.LastAccessTime);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "LastWriteTime":
                        Comparer = new FileComparer(FileComparer.CompareBy.LastWriteTime);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "CreationTime":
                        Comparer = new FileComparer(FileComparer.CompareBy.CreationTime);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "FileSize":
                        Comparer = new FileComparer(FileComparer.CompareBy.FileSize);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "Extension":
                        Comparer = new FileComparer(FileComparer.CompareBy.Extension);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;

                    case "FullName":
                        Comparer = new FileComparer(FileComparer.CompareBy.FullName);
                        SortedFiles = LoadAndSortFiles(Comparer);
                        break;
                    default:
                        break;
                }


                //Receises the List of FileInfos if only a specific number of files are requested
                if (FCount > 0)
                {
                    try
                    {
                        //Decied if the First or the Last are requested
                        if (FCountOrder == "First")
                        {
                            SortedFiles.RemoveRange((int)FCount, SortedFiles.Count - (int)FCount);
                        }
                        else
                        {
                            SortedFiles.RemoveRange(0, SortedFiles.Count - (int)FCount);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        FStatus = ex.Message;
                    }
                }

                //Creates an string Array out of the List<InfoY because of Performance issue in vvvv
                List<string> FullFileNames = new List<string>();
                List<string> ShortFileNames = new List<string>();

                foreach (FileInfo FInfo in SortedFiles)
                {
                    FullFileNames.Add(FInfo.FullName);
                    ShortFileNames.Add(FInfo.Name);
                }

                FFullFileName = new string[FullFileNames.Count];
                FullFileNames.CopyTo(FFullFileName);

                FShortFilename = new string[ShortFileNames.Count];
                ShortFileNames.CopyTo(FShortFilename);
            }
            finally
            {
                //Releases the Thread Blocking
                FStatus = "OK";
                Monitor.Exit(Lock);
            }
        }

        /// <summary>
        /// Loads the Files form the Directory and Sorted it
        /// </summary>
        /// <param name="Comparer"></param>
        /// <returns></returns>
        private ArrayList LoadAndSortFiles(IComparer Comparer)
        {
            ArrayList Infos = new ArrayList();

            //Does the Search for each Mask
            foreach (string Mask in FMask)
            {
                try
                {
                    //Loads the files
                    string[] Files;
                    Files = Directory.GetFiles(FDirectory, Mask, FSearchOption);
                    
                    //create for all files an Info
                    foreach (string FilePath in Files)
                    {
                        FileInfo Info = new FileInfo(FilePath);
                        Infos.Add(Info);
                    }
                }
                catch(Exception ex)
                {
                    FStatus = ex.Message;
                }
                   
            }

            //Sortes the Fileinfo based on the Comparer
            Infos.Sort(Comparer);
            return Infos;
        }


        #endregion Search Process
    }

    #endregion Search Process Class


    #region FileComparer Interface

    /// <summary>
    /// The Comparer Interface for the Fileinfo
    /// </summary>
    public class FileComparer:IComparer    
    {

        int _CompareBy = (int)CompareBy.Name;

        public enum CompareBy
        {
            Name /* a-z */,
            LastWriteTime /* oldest to newest */,
            CreationTime  /* oldest to newest */,
            LastAccessTime /* oldest to newest */,
            FileSize /* smallest first */,
            Extension /* a-z */,
            FullName /* a-z */
        }

        public FileComparer(CompareBy compareBy)
        {
            _CompareBy = (int)compareBy;
        }

        int IComparer.Compare(Object x, Object y)
        {
            int output = 0;

            FileInfo fx = (FileInfo)x;
            FileInfo fy = (FileInfo)y;

            switch (_CompareBy)
            {
                case (int)CompareBy.LastWriteTime:
                    output = DateTime.Compare(fx.LastWriteTime, fy.LastWriteTime);
                    break;
                case (int)CompareBy.CreationTime:
                    output = DateTime.Compare(fx.CreationTime, fy.CreationTime);
                    break;
                case (int)CompareBy.LastAccessTime:
                    output = DateTime.Compare(fx.LastAccessTime, fy.LastAccessTime);
                    break;
                case (int)CompareBy.FileSize:
                    output = Convert.ToInt32(fx.Length - fy.Length);
                    break;
                case (int)CompareBy.Extension:
                    output = (new CaseInsensitiveComparer()).Compare(fx.Extension, fy.Extension);
                    break;
                case (int)CompareBy.FullName:
                    output = (new CaseInsensitiveComparer()).Compare(fx.FullName, fy.FullName);
                    break;
                case (int)CompareBy.Name:
                default:
                    output = (new CaseInsensitiveComparer()).Compare(fx.Name, fy.Name);
                    break;
            }
            return output;
        }
    }

    #endregion FileComparer 
}

