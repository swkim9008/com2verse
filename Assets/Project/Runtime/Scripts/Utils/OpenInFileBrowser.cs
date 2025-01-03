﻿/*===============================================================
* Product:		Com2Verse
* File Name:	OpenInFileBrowser.cs
* Developer:	seaman2000
* Date:			2023-04-05 13:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows;

namespace Com2Verse
{

    // http://wiki.unity3d.com/index.php/OpenInFileBrowser
    // CC BY-SA 3.0 http://creativecommons.org/licenses/by-sa/3.0/
    
    public static class OpenInFileBrowser
    {
        public static bool IsInMacOS
        {
            get
            {
                return UnityEngine.SystemInfo.operatingSystem.IndexOf("Mac OS") != -1;
            }
        }

        public static bool IsInWinOS
        {
            get
            {
                return UnityEngine.SystemInfo.operatingSystem.IndexOf("Windows") != -1;
            }
        }

        public static void OpenInMac(string path)
        {
            bool openInsidesOfFolder = false;

            // try mac
            string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

            if (System.IO.Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            if (!macPath.StartsWith("\""))
            {
                macPath = "\"" + macPath;
            }

            if (!macPath.EndsWith("\""))
            {
                macPath = macPath + "\"";
            }

            string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

            try
            {
                System.Diagnostics.Process.Start("open", arguments);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        public static void OpenInWin(string path)
        {
            bool openInsidesOfFolder = false;

            // try windows
            string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            if (System.IO.Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        public static void Open(string path)
        {
            Application.OpenURL(path);
            /* IL2CPP에서 Exception 발생
            if (IsInWinOS)
            {
                OpenInWin(path);
            }
            else if (IsInMacOS)
            {
                OpenInMac(path);
            }
            else // couldn't determine OS
            {
                OpenInWin(path);
                OpenInMac(path);
            }*/
        }
    }
}
