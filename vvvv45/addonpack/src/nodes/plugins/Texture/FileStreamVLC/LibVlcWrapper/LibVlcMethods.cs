//    nVLC
//    
//    Author:  Roman Ginzburg
//
//    nVLC is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    nVLC is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//     
// ========================================================================

using System;
using System.Runtime.InteropServices;
using System.Security;

//needed for loading the file with extra search paths
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.IO;

namespace LibVlcWrapper
{
    [SuppressUnmanagedCodeSecurity]
    public static class LibVlcMethods
    {
    	#region constructor/destructor
    	static LibVlcMethods() {
			/* // the resolveeventhandler doesn't get called for unmanaged dll's
    		try {
				System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve += new ResolveEventHandler( MyAssemblyResolveHandler );
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show( "PROBLEM setting AppDomain:\n" + e.Message, "Vlc plugin error.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
			}
    		*/
    		
			string libvlcdllPath = FindFilePath( "libvlc.dll", "libvlc_searchpath.txt" );
			if ( libvlcdllPath != null ) {
				string pathEnvVar = Environment.GetEnvironmentVariable( "PATH" );
				Environment.SetEnvironmentVariable( "PATH", pathEnvVar + ";" + libvlcdllPath );
			}
			else {
				throw new Exception( "The libvlc.dll file could not be found in any of the paths specified in libvlc_searchpath.txt, so probably, loading the Vlc plugin will fail." );
				//MessageBox.Show( "The libvlc.dll file could not be found in any of the paths specified in libvlc_searchpath.txt, so probably, loading the Vlc plugin will fail.", "Vlc plugin error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);				
			}
		}

    	/*
		static public System.Reflection.Assembly MyAssemblyResolveHandler(object source, ResolveEventArgs e) {
			//System.Windows.Forms.MessageBox.Show( "LibVlcWrapper.MyAssemblyResolveHandler" + "\n" + e.Name, "MyAssemblyResolveHandler", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
			if ( e.Name == "libvlc.dll" ) {
				return Assembly.LoadFrom( FindFilePath( "libvlc.dll", "libvlc_searchpath.txt") + "libvlc.dll" );
			}
			return null;
    	}
		*/
    	#endregion constructor/destructor

		#region MediaRenderer static helper functions
		/// <summary>
		/// The function returns the file path of the assembly (the dll file) this class resides in. 
		/// </summary>
		static public string AssemblyDirectory
		{
		    get
		    {
		        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
		        UriBuilder uri = new UriBuilder(codeBase);
		        string path = Uri.UnescapeDataString(uri.Path);
		        return Path.GetDirectoryName(path);
		    }
		}

		/// <summary>
		/// This function will look for the file in all of the folders defined in the searchPathFile, 
		/// and returns the FIRST path where the given fileName is found. It will also look in the 
		/// same directory as this dll itself.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="searchPathFileName"></param>
		/// <returns></returns>
		private static string FindFilePath(string fileName, string searchPathFileName) {
			//if the libvlc dll is not found in any folder of the PATH environment variable,
			//search also in directories specified in the text-file

			string sameDirAsCallingCode = AssemblyDirectory + "\\";
			if ( File.Exists( sameDirAsCallingCode + fileName) ) {
				return sameDirAsCallingCode;
			}
			
			//const string searchPathFileName = "libvlc_searchpath.txt";
			string searchPathFilePath = sameDirAsCallingCode + searchPathFileName;
			
			//string searchpath = searchPathFilePath + "\n";
			try {
				foreach ( string row in File.ReadAllLines( searchPathFilePath ) ) {
					//ignore lines starting with # and ignore empty lines
					if ( ! ( row.Length == 0 || row.StartsWith("#") ) ) {
						string currentPath = row + ( row.EndsWith( "\\" ) ? "" : "\\" );
						if ( row.StartsWith(".") ) {
							//relative path
							currentPath = AssemblyDirectory + "\\" + currentPath;
						}
						else {
							//absolute path								
						}

						if ( File.Exists( currentPath + fileName) ) {
							//ideally check if the version is ok to use
							return currentPath;
						}
					}
				}
			}
			catch (IOException) {
				throw new Exception( "A file named " + searchPathFilePath + " should exist (in the same folder as the Vlc node's dll). This file, which contains paths where the plugin should look for the libvlc.dll (and others) could not be opened, so probably, loading the Vlc plugin will fail." );
				//MessageBox.Show( "A file named " + searchPathFilePath + " should exist (in the same folder as the Vlc node's dll). This file, which contains paths where the plugin should look for the libvlc.dll (and others) could not be opened, so probably, loading the Vlc plugin will fail.", "Vlc plugin error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return null;
		}

        #endregion MediaRenderer static helper functions
		
		
        #region libvlc.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern string libvlc_errmsg();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_clearerr();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(IntPtr libvlc_instance_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_retain(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_add_intf(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_set_exit_handler(IntPtr p_instance, IntPtr callback, IntPtr opaque);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_wait(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_set_user_agent(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] name, [MarshalAs(UnmanagedType.LPArray)] byte[] http);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern string libvlc_get_version();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_event_attach(IntPtr p_event_manager, libvlc_event_e i_event_type, IntPtr f_callback, IntPtr user_data);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_event_detach(IntPtr p_event_manager, libvlc_event_e i_event_type, IntPtr f_callback, IntPtr user_data);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_event_type_name(libvlc_event_e event_type);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 libvlc_get_log_verbosity(IntPtr libvlc_instance_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_set_log_verbosity(IntPtr libvlc_instance_t, UInt32 level);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_log_open(IntPtr libvlc_instance_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_log_close(IntPtr libvlc_log_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 libvlc_log_count(IntPtr libvlc_log_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_log_clear(IntPtr libvlc_log_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_log_get_iterator(IntPtr libvlc_log_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_log_iterator_free(IntPtr libvlc_log_iterator_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 libvlc_log_iterator_has_next(IntPtr libvlc_log_iterator_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_log_iterator_next(IntPtr libvlc_log_iterator_t, ref libvlc_log_message_t p_buffer);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_audio_filter_list_get(IntPtr p_instance);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_video_filter_list_get(IntPtr p_instance);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_module_description_list_release(IntPtr p_list);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_clock();

        #endregion

        #region libvlc_media.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_location(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_as_node(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option(IntPtr libvlc_media_inst, [MarshalAs(UnmanagedType.LPArray)] byte[] ppsz_options);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option_flag(IntPtr p_md, [MarshalAs(UnmanagedType.LPArray)] byte[] ppsz_options, int i_flags);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_retain(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_release(IntPtr libvlc_media_inst);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_media_get_mrl(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_duplicate(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_media_get_meta(IntPtr p_md, libvlc_meta_t e_meta);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_set_meta(IntPtr p_md, libvlc_meta_t e_meta, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_value);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_save_meta(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_state_t libvlc_media_get_state(IntPtr p_meta_desc);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_get_stats(IntPtr p_md, out libvlc_media_stats_t p_stats);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_subitems(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_event_manager(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_media_get_duration(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_parse(IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_parse_async(IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_media_is_parsed(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_set_user_data(IntPtr p_md, IntPtr p_new_user_data);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_get_user_data(IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_get_tracks_info(IntPtr media, out IntPtr tracks);

        #endregion

        #region libvlc_media_discoverer.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_discoverer_new_from_name(IntPtr p_inst, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_discoverer_release(IntPtr p_mdis);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_media_discoverer_localized_name(IntPtr p_mdis);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_discoverer_media_list(IntPtr p_mdis);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_discoverer_event_manager(IntPtr p_mdis);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_discoverer_is_running(IntPtr p_mdis);


        #endregion

        #region libvlc_media_library.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_library_new(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_library_release(IntPtr p_mlib);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_library_retain(IntPtr p_mlib);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_library_load(IntPtr p_mlib);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_library_media_list(IntPtr p_mlib);

        #endregion

        #region libvlc_media_player.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new(IntPtr p_libvlc_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr libvlc_media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_release(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_retain(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_get_media(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_media(IntPtr libvlc_media_player_t, IntPtr libvlc_media_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_event_manager(IntPtr libvlc_media_player_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_is_playing(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_play(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_pause(IntPtr mp, int do_pause);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_pause(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_stop(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_callbacks(IntPtr mp, IntPtr @lock, IntPtr unlock, IntPtr display, IntPtr opaque);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_format(IntPtr mp, [MarshalAs(UnmanagedType.LPArray)] byte[] chroma, int width, int height, int pitch);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr libvlc_mediaplayer, IntPtr libvlc_drawable);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_get_hwnd(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_media_player_get_length(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_media_player_get_time(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_time(IntPtr libvlc_mediaplayer, Int64 time);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_media_player_get_position(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_position(IntPtr p_mi, float f_pos);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_chapter(IntPtr p_mi, int i_chapter);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_get_chapter(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_get_chapter_count(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_media_player_will_play(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_get_chapter_count_for_title(IntPtr p_mi, int i_title);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_title(IntPtr p_mi, int i_title);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_get_title(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_get_title_count(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_previous_chapter(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_next_chapter(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_media_player_get_rate(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_set_rate(IntPtr p_mi, float rate);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_state_t libvlc_media_player_get_state(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_media_player_get_fps(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_has_vout(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_media_player_is_seekable(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_media_player_can_pause(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_next_frame(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_track_description_release(IntPtr p_track_description);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_toggle_fullscreen(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_set_fullscreen(IntPtr p_mi, bool b_fullscreen);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_get_fullscreen(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_key_input(IntPtr p_mi, bool on);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_mouse_input(IntPtr p_mi, bool on);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_size(IntPtr p_mi, uint num, out uint px, out uint py);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_cursor(IntPtr p_mi, uint num, out int px, out int py);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_video_get_scale(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_scale(IntPtr p_mi, float f_factor);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_video_get_aspect_ratio(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_aspect_ratio(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_aspect);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_spu(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_spu_count(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_video_get_spu_description(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_set_spu(IntPtr p_mi, int i_spu);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_set_subtitle_file(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_subtitle);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_video_get_title_description(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_video_get_chapter_description(IntPtr p_mi, int i_title);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_video_get_crop_geometry(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_crop_geometry(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_geometry);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_teletext(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_teletext(IntPtr p_mi, int i_page);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_toggle_teletext(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_track_count(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_video_get_track_description(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_track(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_set_track(IntPtr p_mi, int i_track);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_take_snapshot(IntPtr p_mi, uint stream, [MarshalAs(UnmanagedType.LPArray)] byte[] filePath, UInt32 i_width, UInt32 i_height);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_deinterlace(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mode);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_marquee_int(IntPtr p_mi, libvlc_video_marquee_option_t option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_video_get_marquee_string(IntPtr p_mi, libvlc_video_marquee_option_t option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_marquee_int(IntPtr p_mi, libvlc_video_marquee_option_t option, int i_val);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_marquee_string(IntPtr p_mi, libvlc_video_marquee_option_t option, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_text);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_logo_int(IntPtr p_mi, libvlc_video_logo_option_t option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_logo_int(IntPtr p_mi, libvlc_video_logo_option_t option, int value);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_logo_string(IntPtr p_mi, libvlc_video_logo_option_t option, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_value);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_adjust_int(IntPtr p_mi, libvlc_video_adjust_option_t option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_adjust_int(IntPtr p_mi, libvlc_video_adjust_option_t option, int value);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_video_get_adjust_float(IntPtr p_mi, libvlc_video_adjust_option_t option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_adjust_float(IntPtr p_mi, libvlc_video_adjust_option_t option, float value);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_audio_output_list_get(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_output_list_release(IntPtr p_list);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_output_set(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_output_device_count(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_audio_output);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_audio_output_device_longname(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_audio_output, int i_device);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        public static extern string libvlc_audio_output_device_id(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_audio_output, int i_device);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_output_device_set(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_audio_output, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_device_id);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_audio_output_device_types_t libvlc_audio_output_get_device_type(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_output_set_device_type(IntPtr p_mi, libvlc_audio_output_device_types_t device_type);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_toggle_mute(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_get_volume(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_set_volume(IntPtr p_mi, int volume);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_set_mute(IntPtr p_mi, bool mute);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool libvlc_audio_get_mute(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_get_track_count(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_audio_get_track_description(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_get_track(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_set_track(IntPtr p_mi, int i_track);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_audio_output_channel_t libvlc_audio_get_channel(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_set_channel(IntPtr p_mi, libvlc_audio_output_channel_t channel);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_audio_get_delay(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_set_delay(IntPtr p_mi, Int64 i_delay);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_navigate(IntPtr p_mi, libvlc_navigate_mode_t navigate);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_format_callbacks(IntPtr p_mi, IntPtr setup, IntPtr cleanup);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_set_callbacks(IntPtr p_mi, IntPtr play, IntPtr pause, IntPtr resume, IntPtr flush, IntPtr drain, IntPtr opaque);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_set_volume_callback(IntPtr p_mi, IntPtr volume);

        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_set_format_callbacks(IntPtr p_mi, IntPtr setup, IntPtr cleanup);
                                  
        [MinimalLibVlcVersion("1.2.0")]
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_audio_set_format(IntPtr p_mi, [MarshalAs(UnmanagedType.LPArray)] byte[] format, int rate, int channels);
                          

        #endregion

        #region libvlc_media_list.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_new(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_release(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_retain(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_set_media(IntPtr p_ml, IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_media(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_add_media(IntPtr p_ml, IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_insert_media(IntPtr p_ml, IntPtr p_md, int i_pos);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_remove_index(IntPtr p_ml, int i_pos);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_count(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_item_at_index(IntPtr p_ml, int i_pos);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_index_of_item(IntPtr p_ml, IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_is_readonly(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_lock(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_unlock(IntPtr p_ml);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_event_manager(IntPtr p_ml);

        #endregion

        #region libvlc_media_list_player.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_player_new(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_release(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_list_player_event_manager(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_set_media_player(IntPtr p_mlp, IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_set_media_list(IntPtr p_mlp, IntPtr p_mlist);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_play(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_pause(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_player_is_playing(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern libvlc_state_t libvlc_media_list_player_get_state(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_player_play_item_at_index(IntPtr p_mlp, int i_index);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_player_play_item(IntPtr p_mlp, IntPtr p_md);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_stop(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_player_next(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_list_player_previous(IntPtr p_mlp);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_list_player_set_playback_mode(IntPtr p_mlp, libvlc_playback_mode_t e_mode);

        #endregion

        #region libvlc_vlm.h

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_vlm_release(IntPtr p_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_add_broadcast(IntPtr p_instance,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] psz_input,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] psz_output,
                                               int i_options,
                                               [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] ppsz_options,
                                               int b_enabled,
                                               int b_loop);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_add_vod(IntPtr p_instance,
                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_input,
                                         int i_options,
                                         [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] ppsz_options,
                                         int b_enabled,
                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mux);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_del_media(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_set_enabled(IntPtr p_instance,
                                             [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                             int b_enabled);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_set_output(IntPtr p_instance,
                                            [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                            [MarshalAs(UnmanagedType.LPArray)] byte[] psz_output);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_set_input(IntPtr p_instance,
                                           [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                           [MarshalAs(UnmanagedType.LPArray)] byte[] psz_input);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_add_input(IntPtr p_instance,
                                           [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                           [MarshalAs(UnmanagedType.LPArray)] byte[] psz_input);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_set_loop(IntPtr p_instance,
                                          [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                          int b_loop);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_set_mux(IntPtr p_instance,
                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mux);


        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_change_media(IntPtr p_instance,
                                              [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                              [MarshalAs(UnmanagedType.LPArray)] byte[] psz_input,
                                              [MarshalAs(UnmanagedType.LPArray)] byte[] psz_output,
                                              int i_options,
                                              [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] ppsz_options,
                                              int b_enabled,
                                              int b_loop);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_play_media(IntPtr p_instance,
                                             [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_stop_media(IntPtr p_instance,
                                             [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_pause_media(IntPtr p_instance,
                                             [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_seek_media(IntPtr p_instance,
                                            [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                            float f_percentage);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string libvlc_vlm_show_media(IntPtr p_instance,
                                                   [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern float libvlc_vlm_get_media_instance_position(IntPtr p_instance,
                                                               [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                                               int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_time(IntPtr p_instance,
                                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                                         int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_length(IntPtr p_instance,
                                                           [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                                           int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_rate(IntPtr p_instance,
                                                         [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name,
                                                         int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_title(IntPtr p_instance,
                                                          [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name, int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_chapter(IntPtr p_instance,
                                                            [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name, int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_vlm_get_media_instance_seekable(IntPtr p_instance,
                                                             [MarshalAs(UnmanagedType.LPArray)] byte[] psz_name, int i_instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_vlm_get_event_manager(IntPtr p_instance);


        #endregion
    }
}