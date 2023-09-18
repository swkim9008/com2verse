/*===============================================================
* Product:		Com2Verse
* File Name:	PlatformUtility.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Newtonsoft.Json;

namespace Com2Verse.PlatformControl
{
    // Application
    public enum eApplicationEventType
    {
        START_ENTER,
        END_ENTER,
        START_RESTORE,
        END_RESTORE,
        CHANGE_SCREEN_MODE
    }
  
    public enum eResizeState
    {
        APPLICATION,
        RESIZING,
        WORKSPACE
    }
    
    
    
    // Process
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct PacketMessage
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Action;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Sender;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Receiver;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4096)]
        public string Message;
    }

    public struct Message
    {
        public string DeviceId;
        public string Name;
        public long   AccountId;
        public string C2VAccessToken;
        public string C2VRefreshToken;
        public string ServiceAccessToken;
        public string ServiceRefreshToken;
    }

    public enum eReceiveMessage
    {
        PING,
        QUICK_CONNECT,
        NORMAL,
        ALL,
    }

    public enum eConnectType
    {
        DISCONNECTING,
        DISCONNECTED,
        CONNECTING_HOST,
        CONNECTING_USER,
        CONNECTED,
        CANCELED
    }

    public class SocketUtils
    {
        public static string Name       { get; set; }
        public static string DaemonPath { get; set; }
        public static int    SocketPort { get; set; }
        public static string Ip = "127.0.0.1";
        
        public static void Initialize()
        {
#if UNITY_EDITOR
            Name       = "Editor";
            DaemonPath = "C:\\TestBuild\\DaemonTest\\Daemon\\DaemonConsole.exe";
            SocketPort = 50000;
#else
            Name       = "Com2Verse";
            DaemonPath = "..\\Daemon\\DaemonConsole.exe";
            SocketPort = 50100;
#endif
        }
    }
}
