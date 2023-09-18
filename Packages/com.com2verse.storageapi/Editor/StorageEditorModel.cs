/*===============================================================
* Product:		Com2Verse
* File Name:	StorageEditorModel.cs
* Developer:	jhkim
* Date:			2023-05-26 15:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2VerseEditor.UGC.UIToolkitExtension;
using UnityEngine;

namespace Com2Verse
{
    public class StorageEditorModel : EditorWindowExModel
    {
        [SerializeField] private string _requestUrl;
        [SerializeField] private string _serviceName = "test_service";
        [SerializeField] private string _accessToken;
        [SerializeField] private string _path;
        [SerializeField] private string _fileName;
        [SerializeField] private long _length;
        [SerializeField] private string _md5;
        [SerializeField] private string _downloadFileName;

        public string RequestUrl
        {
            set => _requestUrl = value;
        }

        public string ServiceName => _serviceName;
        public string AccessToken => _accessToken;
        public string Path => _path;
        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }
        public long Length
        {
            get => _length;
            set => _length = value;
        }

        public string Md5
        {
            get => _md5;
            set => _md5 = value;
        }
        public string FilePath { get; set; }
        public string DownloadFileName => _downloadFileName;
    }
}
