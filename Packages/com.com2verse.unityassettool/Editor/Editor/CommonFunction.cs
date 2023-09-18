using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Com2VerseEditor.UnityAssetTool
{
    public class Common
    {
        //GUIStyle.
        public static GUIStyle DangerLabel = null;
        public static GUIStyle WarningLabel = null;
        public static GUIStyle NormalLabel = null;
        public static GUIStyle NotifyLabel = null;
        public static GUIStyle SubTitleLabel = null;

        public static string GetAssetPath(Object _target)
        {
            string assetPath = AssetDatabase.GetAssetPath(_target);
            return assetPath;
        }

        public static Mesh[] GetMeshesInFolder(Object _folder)
        {
            string[] folderPath = new string[1] { AssetDatabase.GetAssetPath(_folder) };
            Debug.Log(" --- Folder : " + folderPath[0]);
            string[] assetGUIDs = AssetDatabase.FindAssets("t:model", folderPath);
            Mesh[] meshes = new Mesh[assetGUIDs.Length];            

            for ( int loop = 0, max = assetGUIDs.Length ; loop < max ; loop++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[loop]);
                meshes[loop] = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
                Debug.Log("   - Mesh : " + meshes[loop]);
            }

            return meshes;
        }

        public static void DefineGUIStyle() {

            // 위험 라벨.---.
            if (DangerLabel == null) {
                DangerLabel = new GUIStyle("Label");
                DangerLabel.normal.textColor = Color.red;
            }

            // 경고 라벨.---.
            if (WarningLabel == null) {
                WarningLabel = new GUIStyle("Label");
                WarningLabel.normal.textColor = Color.yellow;
            }

            // 일반 라벨.---.
            if (NormalLabel == null) {
                NormalLabel = new GUIStyle("Label");
            }

            // 알림 라벨.---.
            if (NotifyLabel == null) {
                NotifyLabel = new GUIStyle("Label");
                NotifyLabel.normal.textColor = Color.white;
                NotifyLabel.alignment = TextAnchor.MiddleCenter;
            }

            // 서브타이틀 라벨.---.
            if (SubTitleLabel == null) {
                SubTitleLabel = new GUIStyle("Label");
                SubTitleLabel.normal.textColor = Color.gray;
                SubTitleLabel.fontStyle = FontStyle.Bold;
                SubTitleLabel.alignment = TextAnchor.MiddleCenter;
            }
        }

        public static Object[] CopyFile(Object[] _source, string _addInfo)
        {
            Object[] resultObject = new Object[_source.Length];
            for (int loop = 0, max = _source.Length; loop < max; loop++){
                string sourcePath = AssetDatabase.GetAssetPath(_source[loop]);
                string sourceDir = Path.GetDirectoryName(sourcePath);
                string sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
                string sourceExtendion = Path.GetExtension(sourcePath);

                string newPath = sourceDir + "/" + sourceFileName + _addInfo + sourceExtendion;
                AssetDatabase.CopyAsset(sourcePath, newPath);

                resultObject[loop] = AssetDatabase.LoadAssetAtPath(newPath, typeof(Object));
            }
            return resultObject;
        }



        // 폴더에서 에셋 긁어오기.
        public static List<T> GetAssetInDirectory<T>(string _path, string _filter) where T : UnityEngine.Object
        {
            string[] findAssets = AssetDatabase.FindAssets(_filter, new string[] { _path });
            List<T> result = new List<T>();
            for (int loop = 0; loop < findAssets.Length; loop++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(findAssets[loop]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                result.Add(asset);
            }
            return result;
        }


        

        public static bool ShowProgressBar(string _title, string _info, int _type, float _max, float _current)
        {
            string progressInfo = string.Empty;
            float progress = _current / _max;

            switch (_type)
            {
                case 1:
                    progressInfo = $"분석중 : ({_current}/{_max}) {_info} ";
                    break;
            }

            if (EditorUtility.DisplayCancelableProgressBar(_title, progressInfo, progress))
            {
                return true;
            }

            return false;
        }

    }
}
