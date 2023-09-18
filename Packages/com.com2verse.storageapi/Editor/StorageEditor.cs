/*===============================================================
* Product:		Com2Verse
* File Name:	StorageEditor.cs
* Developer:	jhkim
* Date:			2023-05-26 15:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using Com2Verse.Logger;
using Com2Verse.StorageApi;
using UnityEditor;
using Com2VerseEditor.UGC.UIToolkitExtension;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace Com2Verse
{
    public class StorageEditor : EditorWindowEx
    {
        private static readonly string UploadFileUrl = "https://test-api2.com2verse.com/file/api/v1/file/";
        private StorageEditorModel _model;

#region UI Toolkit
        [MenuItem("Com2Verse/Tools/스토리지 API", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<StorageEditor>();
            window.SetConfig(window, new Vector2Int(456, 443), "스토리지 API");
        }
        public override string MetaGuid => "bc4a1dea5ee53b84c8a68cbe3a0526f8";
        public override string ModelGuid => "8156605d92db14e4db0b77a8129c8d75";

        protected override void OnStart(VisualElement root)
        {
            base.OnStart(root);

            Initialize();
        }

        protected override void OnDraw(VisualElement root)
        {
            base.OnDraw(root);
        }

        protected override void OnClear(VisualElement root)
        {
            base.OnClear(root);
        }
#endregion // UI Toolkit

#region Initialize
        void Initialize()
        {
            _model = LoadModel<StorageEditorModel>();

            SetButtonOnClick("btnSelectFile", async (btn) => await OnSelectFile(btn));
            SetButtonOnClick("btnCopyHash", OnCopyHash);
            SetButtonOnClick("btnHashCheckFile", async (btn) => await OnHashTestFile(btn));
            SetButtonOnClick("btnDownloadUrl", async (btn) => await OnDownloadUrl(btn));

            async UniTask OnSelectFile(Button btn)
            {
                var path = EditorUtility.OpenFilePanel("파일을 선택 해 주세요", string.Empty, string.Empty);
                if (File.Exists(path))
                {
                    if (string.IsNullOrWhiteSpace(_model.AccessToken))
                    {
                        EditorUtility.DisplayDialog("알림", "토큰이 유효하지 않습니다.", "확인");
                        return;
                    }

                    var fileInfo = new FileInfo(path);
                    Util.TryGenerateHash(path, out var hash);
                    WebApi.Util.Instance.AccessToken = _model.AccessToken;

                    _model.FileName = fileInfo.Name;
                    _model.Length = fileInfo.Length;
                    _model.Md5 = hash;
                    _model.FilePath = path;

                    var bytes = await File.ReadAllBytesAsync(_model.FilePath);
                    StorageApi.Helper.ServiceName = _model.ServiceName;
                    StorageApi.Helper.StoragePath = _model.Path;
                    var success = await StorageApi.Helper.UploadAsync(_model.FileName, bytes);
                    C2VDebug.Log($"파일 업로드 결과...{success}");
                }
            }

            void OnCopyHash(Button btn)
            {
                GUIUtility.systemCopyBuffer = _model.Md5;
                EditorUtility.DisplayDialog("알림", "Hash값이 복사되었습니다.", "확인");
            }

            async UniTask OnHashTestFile(Button btn)
            {
                var path = EditorUtility.OpenFilePanel("파일을 선택 해 주세요", string.Empty, string.Empty);
                if (File.Exists(path))
                {
                    Util.TryGenerateHash(path, out var hash);
                    EditorUtility.DisplayDialog("알림", hash, "확인");
                }
            }

            async UniTask OnDownloadUrl(Button btn)
            {
                WebApi.Util.Instance.AccessToken = _model.AccessToken;
                StorageApi.Helper.ServiceName = _model.ServiceName;
                StorageApi.Helper.StoragePath = _model.Path;

                var result = await StorageApi.Helper.GetDownloadUrlAsync(_model.DownloadFileName);
                if (result.Item1)
                {
                    EditorUtility.DisplayDialog("알림", $"다운로드 URL 요청 성공\n클립보드로 복사되었습니다.", "확인");
                    GUIUtility.systemCopyBuffer = result.Item2;
                }
                else
                {
                    EditorUtility.DisplayDialog("알림", "다운로드 URL 요청 실패", "확인");
                }
            }
        }
#endregion // Initialize

#region Util
        private T LoadModel<T>() where T : class
        {
            T result = null;
            if (metaData != null)
            {
                if (modelObject is T modelObj)
                    result = modelObj;
                else if (metaData.modelObject is T metaModelObj)
                    result = metaModelObj;
            }

            return result;
        }

        private VisualElement InstantiateFromUxml(string name)
        {
            var item = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Project/Editor/Util/StorageEditor/{name}.uxml");
            return item != null ? item.Instantiate() : null;
        }

        private void SetButtonOnClick(string name, Action<Button> onClick)
        {
            var btn = rootVisualElement.Q<Button>(name);
            if (btn == null) return;
            btn.clickable = new Clickable(() => onClick?.Invoke(btn));
        }

        private void SetButtonEnable(string name, bool enable)
        {
            var btn = rootVisualElement.Q<Button>(name);
            if (btn == null) return;
            btn.SetEnabled(enable);
        }
#endregion // Util
    }
}
