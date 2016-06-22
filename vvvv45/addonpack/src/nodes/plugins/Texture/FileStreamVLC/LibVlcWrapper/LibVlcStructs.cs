//    nVLC
//    
//    Author:  Roman Ginzburg
//
//    nVLC is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published by
//    the Free Software Foundation, either version 2.1+ of the License, or
//    (at your option) any later version.
//
//    nVLC is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Lesser General Public License for more details.
//     
// ========================================================================

using System;
using System.Runtime.InteropServices;

namespace LibVlcWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_log_message_t
    {
        public UInt32 sizeof_msg;
        public Int32 i_severity;
        public IntPtr psz_type;
        public IntPtr psz_name;
        public IntPtr psz_header;
        public IntPtr psz_message;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_media_stats_t
    {
        /* Input */
        public int i_read_bytes;
        public float f_input_bitrate;

        /* Demux */
        public int i_demux_read_bytes;
        public float f_demux_bitrate;
        public int i_demux_corrupted;
        public int i_demux_discontinuity;

        /* Decoders */
        public int i_decoded_video;
        public int i_decoded_audio;

        /* Video Output */
        public int i_displayed_pictures;
        public int i_lost_pictures;

        /* Audio output */
        public int i_played_abuffers;
        public int i_lost_abuffers;

        /* Stream output */
        public int i_sent_packets;
        public int i_sent_bytes;
        public float f_send_bitrate;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct libvlc_media_track_info_t
    {
        [FieldOffset(0)]
        public UInt32 i_codec;

        [FieldOffset(4)]
        public int i_id;

        [FieldOffset(8)]
        public libvlc_track_type_t i_type;

        [FieldOffset(12)]
        public int i_profile;

        [FieldOffset(16)]
        public int i_level;

        [FieldOffset(20)]
        public audio audio;

        [FieldOffset(20)]
        public video video;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct audio
    {
        public int i_channels;
        public int i_rate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct video
    {
        public int i_height;
        public int i_width;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct libvlc_event_t
    {
        [FieldOffset(0)]
        public libvlc_event_e type;

        [FieldOffset(4)]
        public IntPtr p_obj;

        [FieldOffset(8)]
        public media_meta_changed media_meta_changed;

        [FieldOffset(8)]
        public media_subitem_added media_subitem_added;

        [FieldOffset(8)]
        public media_duration_changed media_duration_changed;

        [FieldOffset(8)]
        public media_parsed_changed media_parsed_changed;

        [FieldOffset(8)]
        public media_freed media_freed;

        [FieldOffset(8)]
        public media_state_changed media_state_changed;

        [FieldOffset(8)]
        public media_player_position_changed media_player_position_changed;

        [FieldOffset(8)]
        public media_player_time_changed media_player_time_changed;

        [FieldOffset(8)]
        public media_player_title_changed media_player_title_changed;

        [FieldOffset(8)]
        public media_player_seekable_changed media_player_seekable_changed;

        [FieldOffset(8)]
        public media_player_pausable_changed media_player_pausable_changed;

        [FieldOffset(8)]
        public media_list_item_added media_list_item_added;

        [FieldOffset(8)]
        public media_list_will_add_item media_list_will_add_item;

        [FieldOffset(8)]
        public media_list_item_deleted media_list_item_deleted;

        [FieldOffset(8)]
        public media_list_will_delete_item media_list_will_delete_item;

        [FieldOffset(8)]
        public media_list_player_next_item_set media_list_player_next_item_set;

        [FieldOffset(8)]
        public media_player_snapshot_taken media_player_snapshot_taken;

        [FieldOffset(8)]
        public media_player_length_changed media_player_length_changed;

        [FieldOffset(8)]
        public vlm_media_event vlm_media_event;

        [FieldOffset(8)]
        public media_player_media_changed media_player_media_changed;
    }

    /* media descriptor */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_meta_changed
    {
        public libvlc_meta_t meta_type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_subitem_added
    {
        public IntPtr new_child;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_duration_changed
    {
        public long new_duration;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_parsed_changed
    {
        public int new_status;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_freed
    {
        public IntPtr md;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_state_changed
    {
        public libvlc_state_t new_state;
    }

    /* media instance */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_position_changed
    {
        public float new_position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_time_changed
    {
        public long new_time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_title_changed
    {
        public int new_title;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_seekable_changed
    {
        public int new_seekable;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_pausable_changed
    {
        public int new_pausable;
    }

    /* media list */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_item_added
    {
        public IntPtr item;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_will_add_item
    {
        public IntPtr item;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_item_deleted
    {
        public IntPtr item;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_will_delete_item
    {
        public IntPtr item;
        public int index;
    }

    /* media list player */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_player_next_item_set
    {
        public IntPtr item;
    }

    /* snapshot taken */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_snapshot_taken
    {
        public IntPtr psz_filename;
    }

    /* Length changed */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_length_changed
    {
        public long new_length;
    }

    /* VLM media */
    [StructLayout(LayoutKind.Sequential)]
    public struct vlm_media_event
    {
        public IntPtr psz_media_name;
        public IntPtr psz_instance_name;
    }

    /* Extra MediaPlayer */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_media_changed
    {
        public IntPtr new_media;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_module_description_t
    {
        public IntPtr psz_name;
        public IntPtr psz_shortname;
        public IntPtr psz_longname;
        public IntPtr psz_help;
        public IntPtr p_next;
    }
}