using System;
using System.Runtime.InteropServices;

namespace LD2000
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Point
	{
		public int X; //X coordinate
		public int Y; //Y coordinate
		public int Z; //Z (distance) coordinate
		public int BB; //Beam Brush  (0-100)
		public int Color; //RGB color, in Windows format: &H00BBGGRR
		public int X3D; //X coord of 3D projection of this point
		public int Y3D; //Y coord of 3D projection of this point
		public int Grp; //group number this point belongs with
		public int VOtype; //PT_VECTOR or PT_CORNER
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Frame
	{
		//True if vector-oriented frame, false if point-oriented
		public int VectorFlag;
		//100%=normal speed, 50%=slower than DFreq, 150%=faster
		public int ScanRate;
		//True if abstract, false if graphic frame
		public int AbstractFlag;
		//Number of points in the frame
		public int NumPoints;
		//23 character memo field plus a null
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
		public string FrameNote;
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct FrameEx
	{
		//Indicates that this frame has changed since the last time it was saved. This is read-only. 
		public Int32 ChangedFlag;
		//Indicates that this frame is stored internally as 3D data. This is read-only.
		public Int32 ThreeDFlag;
		//Indicates that this frame stores beam brush points internally. This is read-only.
		public Int32 BeamBrushFlag;
		//Indicates that this frame is to be rendered using the vector renderer. This is read/write.
		public Int32 VectorFlag;
		//Indicates that this frame has additional abstract information and that this should be rendered as an abstract.
		public Int32 AbstractFlag;
		//Indicates that this frame has DMX data in addition to point and perhaps abstract data.
		public Int32 DMXFlag;
		//Indicates that this frame is a raster frame. No special internal handling is done at this time.
		public Int32 RasterFlag;
		//Indicates that this frame was rendered by 3D Studio MAX.
		public Int32 MaxRenderedFlag;
		//Indicates that this frame is secured.
		public Int32 SecureFrameFlag;
		//Reserved for future use
		public Int32 Reserved3;
		//Palette that this frame will use unless overridden by Track.
		public Int32 PreferredPalette;
		//Projection zone that this frame will be projected onto unless overridden by track.
		public Int32 PreferredProjectionZone;
		//Number of frames to the end of the animation. Range is 0 to 65535.
		public Int32 AnimationCount;
		//Number, usually bit-encoded, that describes the frame. This is only 16-bits internally.
		public Int32 ClipartClass;
		//Scan rate for this frame. If positive, handled as a multiplier. If negative, treated as exact sample rate in 1K increments.
		public Int32 ScanRate;
		//Number of data points in this frame.
		public Int32 NumPoints;
		//23 character memo field plus a null
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
		public string FrameNote;
	}
		
	/// <summary>
	/// Functions provided by LD2000.dll used or may used by the LD2000Node.
	/// This list is not complete. For a full reference please have a look
	/// at the LD2000 SDK.
	/// </summary>
	public class LD
	{
		#region CONSTANTS
		
		#region LD_STATUS
		//The CheckStatus command returns a status or "error" code.
		//The following constants are defined for use in processing
		//CheckStatus's result.

		//Normal return value -- everything is OK.
		public const short LDSTATUS_OK = 0;

		//The following (-1, -2) can appear at anytime except
		//from InitialQMCheck. In other words, the QM board is
		//installed, but it is not working properly. If these
		//occur, there is a great likelihood that the QM is gone
		//and therefore the user that
		//
		//First-in, first-out queue read zero or non-even number of bytes.
		//Indicates incorrect processing of a QM command.
		public const short LDSTATUS_FIFO_READ_ERROR = -1;
		//QM32 is not responding properly (non-FIFO read error)
		public const short LDSTATUS_QM32_SOFTWARE_ERROR = -2;

		//Returned after successful file loads
		//to tell what format was loaded
			//Pangolin LSD1 frame format
		public const short LDSTATUS_LSD_LOADED = -11;
			//Pangolin LD for Amiga frame or palette format
		public const short LDSTATUS_LDA_LOADED = -12;
			//ILDA frame format
		public const short LDSTATUS_ILDA_LOADED = -13;
			//Aura Technologies/Laser Fantasy frame format
		public const short LDSTATUS_DEC3_LOADED = -14;
			//Pangolin LD for Windows frame or palette format
		public const short LDSTATUS_LDB_LOADED = -15;
			//Pangolin secure frame format
		public const short LDSTATUS_LDSECURE_LOADED = -16;

		//Returned by ConvertToPointFrame
		//if trying to create a point-oriented frame
		//from a frame that is already point-oriented.
		public const short LDSTATUS_ALREADY_POINT_ORIENTED = -31;

		//Returned by CheckSession if Q32 is loaded but no session exists
		public const short LDSTATUS_NO_SESSION_IN_PROGRESS = -101;

		//Returned by LFileRequest and LPaletteRequest
		//if file dialog is canceled
		public const short LDSTATUS_FILE_REQUEST_CANCEL = -201;

		//File loading/saving errors
		public const short LDSTATUS_FILE_NOT_FOUND = -401;
		public const short LDSTATUS_WRONG_FILE_TYPE = -402;
		public const short LDSTATUS_DISK_FULL = -403;
		public const short LDSTATUS_DISK_WRITE_PROTECTED = -404;
		public const short LDSTATUS_FILE_WRITE_PROTECTED = -405;
		public const short LDSTATUS_MISC_FILE_ERROR = -406;
			//Supplied filename is over 128 characters
		public const short LDSTATUS_STRING_TOO_LONG = -407;

		//Wrong frame or point number
		public const short LDSTATUS_FRAME_OUT_OF_RANGE = -501;
		public const short LDSTATUS_POINT_OUT_OF_RANGE = -502;
		public const short LDSTATUS_TDC_OUT_OF_RANGE = -511;
		public const short LDSTATUS_TRANSITION_OUT_OF_RANGE = -512;
		public const short LDSTATUS_EFFECT_OUT_OF_RANGE = -513;
		public const short LDSTATUS_SCENE_OUT_OF_RANGE = -514;
		public const short LDSTATUS_MODULE_OUT_OF_RANGE = -515;
		public const short LDSTATUS_SHOW_OUT_OF_RANGE = -516;
		public const short LDSTATUS_STRUCTURE_NOT_FOUND = -519;

		public const short LDSTATUS_EFFECT_DELETED = -530;
		public const short LDSTATUS_SCENE_DELETED = -531;
		public const short LDSTATUS_MODULE_DELETED = -532;
		public const short LDSTATUS_SHOW_DELETED = -533;
		public const short LDSTATUS_STRUCTURE_DELETED = -539;

		//Returned if try to delete something but Showtime is using it
		public const short LDSTATUS_EFFECT_IN_USE = -540;
		public const short LDSTATUS_STRUCTURE_IN_USE = -549;

		//No free Windows memory
		public const short LDSTATUS_NO_IBM_MEMORY = -601;
		//Can't open a window (e.g., Debug, File Request)
		//because Windows won't let us.
		public const short LDSTATUS_CANT_OPEN_WINDOW = -602;

		//No free QM200032 memory.
		public const short LDSTATUS_NO_QM32_MEMORY = -702;
		//Not enough memory to load a file. Nothing gets loaded.
		public const short LDSTATUS_FILE_TOO_LARGE = -703;

		//Attempted to call a DLL command which exists
		//but is not yet implemented.
		public const short LDSTATUS_NOT_IMPLEMENTED = -801;

		//Error communicating with the QM200032
		//(long timeout during file loading/saving and
		//activearray functions).
		//Can indicate it has shaken loose or otherwise
		//is no longer working properly.
		public const short LDSTATUS_QM32_ERROR = -901;

		//Following will only be returned by InitialQMCheck
		//if there is a problem (otherwise, a 0 will be returned).
		//InitialQMCheck puts up its own message, informing you
		//that one of these three was found.
		//
		//QM32 board is not in the computer
		public const short LDSTATUS_QM32_NOT_PRESENT = -1001;
		//QM32 is not responding properly probably due to
		//another ISA bus card using the same I/O address
		public const short LDSTATUS_QM32_ADDRESS_CONFLICT = -1002;
		//QM32 is not responding properly for an unknown reason
		public const short LDSTATUS_QM32_INITIALIZATION_ERROR = -1003;
		
		#endregion
		
		#endregion
		
		#region SESSION CONTROL
		
		/// <summary>
		/// The QM2000 board is a self-contained computer. It runs its own "session".
		/// These commands begin the session, check to see if one is already in progress,
		/// and end the session. There are also some checks to keep track of the
		/// software date (for the Windows dll and on the QM board) and the QM serial number.
		/// 
		/// If a session is already running, it closes the former session, wipes out all memory,
		/// and starts a new session. Therefore, it is a good thing to use CheckSession first
		/// if a session is running and you want to keep the old frames.
		/// </summary>
		/// <param name="maxframes">maximum number of frames you want</param>
		/// <param name="maxpoints">maximum number of points per frame</param>
		/// <param name="maxbuffer">maximum buffer size for all tracks (set to 8192 or below)</param>
		/// <param name="numberofundos">number of undos desired (LD uses 5)</param>
		/// <param name="Status">status code</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void BeginSession(int maxframes, int maxpoints, int maxbuffer, int numberofundos, ref int Status);
		
		/// <summary>
		/// The QM2000 board is a self-contained computer. It runs its own "session".
		/// These commands begin the session, check to see if one is already in progress,
		/// and end the session. There are also some checks to keep track of the
		/// software date (for the Windows dll and on the QM board) and the QM serial number.
		/// 
		/// If a session is already running, it closes the former session, wipes out all memory,
		/// and starts a new session. Therefore, it is a good thing to use CheckSession first
		/// if a session is running and you want to keep the old frames.
		/// </summary>
		/// <param name="version"></param>
		/// <param name="maxframes">maximum number of frames you want</param>
		/// <param name="maxpoints">maximum number of points per frame</param>
		/// <param name="maxbuffer">maximum buffer size for all tracks (set to 8192 or below)</param>
		/// <param name="numberofundos">number of undos desired (LD uses 5)</param>
		/// <param name="Status">status code</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void BeginSessionEx(ref int version, ref int maxframes, ref int maxpoints, ref int maxbuffer, ref int numberofundos, ref int Status);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void InitialQMCheck(ref int Status);
		
		/// <summary>
		/// Returns session status: either LDSTATUS_NO_SESSION_IN_PROGRESS or LDSTATUS_OK
		/// </summary>
		/// <param name="Status">LDSTATUS_NO_SESSION_IN_PROGRESS or LDSTATUS_OK</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void CheckSession(ref int Status);
		
		/// <summary>
		/// Returns session status (just like CheckSession): either LDSTATUS_NO_SESSION_IN_PROGRESS or LDSTATUS_OK
		/// If a session is already running (status = LDSTATUS_OK) then the current settings
		/// (maxframes, maxpoints, etc.) are returned so you can set your system to match.
		/// </summary>
		/// <param name="maxframes">maximum number of frames</param>
		/// <param name="maxpoints">maximum number of points per frame</param>
		/// <param name="maxbuffer">maximum buffer size for all tracks</param>
		/// <param name="numberofundos">number of undos</param>
		/// <param name="Status">LDSTATUS_NO_SESSION_IN_PROGRESS or LDSTATUS_OK</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void CheckSessionSettings(ref int maxframes, ref int maxpoints, ref int maxbuffer, ref int numberofundos, ref int Status);
		
		/// <summary>
		/// No parameters needed. Ends the session (clearing frames) but leaves QM90 program still
		/// downloaded and running on the QM90 board.
		/// </summary>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void EndSession();
		
		/// <summary>
		/// No parameters needed. Wipes out the QM90 program, as if you just turned on the computer.
		/// The QM90 program will be downloaded to the QM90 board the next time you start LD,
		/// or run CheckSession or BeginSession.
		/// </summary>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void RebootQM32();
		
		/// <summary>
		/// This but resets all *display* parameters at the system level.
		/// This is more reliable than Reset All and it also will reset more parameters in the future as they get added.
		/// Note that this only resets display parameters, not the current working frame or anything non-display related.
		/// This is used in LD's Reset button
		/// </summary>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ResetLD();
		
		/// <summary>
		/// Parameter returns LDSTATUS_OK (i.e., zero) if everything is OK,
		/// or a status error code (defined in LDGLOBAL.BAS) if there is a problem.
		/// </summary>
		/// <param name="Status">LDSTATUS_OK or error code</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void GetLDStatus(ref int Status);
		
		/// <summary>
		/// //Parameters are date codes in form YYYYMMDD, telling when the item was last revised.
		/// Example: 19940205 is Feb. 5, 1994.
		/// 1) File LD2000.DLL   (Windows dynamic link library)
		/// 2) File LD.Q32   (QM200032 library. Note that the file and variable name use "Q90", NOT "QM90")
		/// 3) ROM chip      (QM200032 hardware board)
		/// </summary>
		/// <param name="LDdlldate">revision date of windows dynamic link library</param>
		/// <param name="LDq32date">revision date of QM200032 library</param>
		/// <param name="boarddate">revision date of QM200032 hardware board</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void GetDates(ref int LDdlldate, ref int LDq32date, ref int boarddate);
		
		/// <summary>
		/// Parameter is serial number of QM2000 board.
		/// </summary>
		/// <param name="serialnumber">serial number of QM2000 board</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void GetSerialNumber(ref int serialnumber);

		#endregion
		
		#region DISPLAY CONTROL
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void DisplayFreq3(int desiredPointpps, int desiredVectorpps, int desiredAbstractpps, ref int actualPointpps, ref int actualVectorpps, ref int actualAbstractpps);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void DisplayFrame(int fr);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void DisplayObjectSettings(int VOvisiblebegin, int VOvisiblemid, int VOvisibleend, int VOvisibledensity, int VOblankbegin, int VOblankend, int VOblankdensity);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void DisplaySkew(int intertrackpts, int blankcolorshift, int beambrushshift);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void DisplayUpdate();
		
		#endregion
		
		#region WORKING ENVIRONMENT
		
		/// <summary>
		/// All subsequent LD commands will affect the current working scanner(s)
		/// Add these numbers to get the working scanner pair or pairs:
		/// 1 = pair #1, 2 = pair #2, 4 = pair #3, 8 = pair #4.
		/// For example, to work with scanners 1 and 3:
		///   SetWorkingScanners scanner1 + scanner3
		/// which is the same as
		///   SetWorkingScanners 5
		/// </summary>
		/// <param name="scannercode">1 = pair #1, 2 = pair #2, 4 = pair #3, 8 = pair #4</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void SetWorkingScanners(int scannercode);
		
		/// <summary>
		/// Similar to above; all subsequent calls will affect the working tracks.
		/// Add the track numbers: 1=trackA, 2=trackB, 4=trackC, 8=trackD, etc. up to 32 tracks just like LD/Amiga.
		/// </summary>
		/// <param name="trackcode">1=trackA, 2=trackB, 4=trackC, 8=trackD, etc. up to 32 tracks</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void SetWorkingTracks(int trackcode);
		
		/// <summary>
		/// Similar to above; all subsequent calls will affect the working frame.
		/// Note that you can have only one working frame!
		/// </summary>
		/// <param name="fr">frame number</param>
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void SetWorkingFrame(int fr);
		
		#endregion
		
		#region FRAME EDITING
		
		public const int PT_VECTOR = 0; // normal point
		public const int PT_CORNER = 4096; // corner point
		public const int PT_TRAVELBLANK = 16384; // ???
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void WriteFrameFastEx(ref FrameEx SUPPLY_LDFrameEx, ref Point SUPPLY_LDpt_Array);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ReadNumPoints(ref int ptsinframe);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ReadFrameStruct(int frameNumber, ref Frame LDfr);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ReadFrameStructEx(int frameNumber, ref FrameEx LDfr);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ReadFrame(ref Frame LDfr, ref Point LDpt);
		
		[DllImport("LD2000.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern void ReadFrameEx(ref FrameEx LDfr, ref Point LDpt);
		
		#endregion
	}
}
