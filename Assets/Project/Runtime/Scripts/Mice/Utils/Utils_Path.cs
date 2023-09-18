/*===============================================================
* Product:		Com2Verse
* File Name:	Utils.Path.cs
* Developer:	sprite
* Date:			2023-07-03 15:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Logger;

namespace Com2Verse.Mice
{
    public static partial class Utils
    {
        public static class Path
        {
            private const string FOLDER_COM2VERSE   = "Com2Verse";
            private const string FOLDER_DOWNLOADS   = "Downloads";
            private const string FOLDER_SCREENSHOTS = "Screenshots";

            public static string Com2Verse      { get; private set; }
            public static string Downloads      { get; private set; }
            public static string Screenshots    { get; private set; }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            private static void OnLoad()
            {
                var myDoc   = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                Com2Verse   = System.IO.Path.Combine(myDoc,     FOLDER_COM2VERSE);
                Downloads   = System.IO.Path.Combine(Com2Verse, FOLDER_DOWNLOADS);
                Screenshots = System.IO.Path.Combine(Com2Verse, FOLDER_SCREENSHOTS);

                C2VDebug.LogCategory("Com2Verse.Mice.Utils", $"<Path> MyDocuments = '{myDoc}'");
                C2VDebug.LogCategory("Com2Verse.Mice.Utils", $"<Path> Com2Verse   = '{Com2Verse}'");
                C2VDebug.LogCategory("Com2Verse.Mice.Utils", $"<Path> Downloads   = '{Downloads}'");
                C2VDebug.LogCategory("Com2Verse.Mice.Utils", $"<Path> Screenshots = '{Screenshots}'");
            }
        }
    }
}
