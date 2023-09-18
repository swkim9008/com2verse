/*===============================================================
* Product:		Com2Verse
* File Name:	Com2VerseApi.cs
* Developer:	jhkim
* Date:			2023-03-29 10:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Com2Verse.WebApi;
using UnityEditor;
using Com2VerseEditor.UGC.UIToolkitExtension;
using Com2VerseEditor.UGC.UIToolkitExtension.Containers;
using Com2VerseEditor.UGC.UIToolkitExtension.Controls;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using Button = UnityEngine.UIElements.Button;
using Util = Com2Verse.HttpHelper.Util;
using WebApiUtil = Com2Verse.WebApi.Util;

namespace Com2verseEditor.WebApi
{
    public class Com2VerseApi : EditorWindowEx
    {
#region Variables
        [NotNull] private Com2VerseApiModel _model;
        private static readonly string SwaggerInfoJsonPath = "Packages/com.com2verse.c2vapi/Editor/SwaggerInfo.json";
        private Swagger.SwaggerInfo[] _swaggerInfos = null;

        private int _prevSelectedApiIndex = -1;
        private string _selectedApiKey = string.Empty;
        private int _selectedApiIndex = -1;
        private readonly Dictionary<string, string> _urlParams = new();
        private readonly Dictionary<string, string> _requestBodyParams = new();
        private Swagger.PropertyInfo[] _requestBodyProperties;
        private Dictionary<string, Swagger.SwaggerApi> _swaggerApis = new();

        private Button _btnSendRequest;
        private static StyleColor _noemalTextColor = new StyleColor(new Color(210, 210, 210));
        private static StyleColor _errorTextColor = new StyleColor(Color.red);
#endregion // Variables

#region EditorWindowEx
        [MenuItem("Com2Verse/Tools/컴투버스 Web API", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<Com2VerseApi>();
            if (window == null) return;

            window.SetConfig(window, new Vector2Int(1200, 1000), "컴투버스 Web API");
        }

        protected override void OnStart(VisualElement root)
        {
            base.OnStart(root);

            if (metaData != null)
            {
                if (modelObject is Com2VerseApiModel modelObj)
                    _model = modelObj;
                else if (metaData.modelObject is Com2VerseApiModel metaModelObj)
                    _model = metaModelObj;
            }
            else
            {
                C2VDebug.LogWarning("Com2VerseApiModel Not found");
                return;
            }

            Initialize();
        }
#endregion // EditorWindowEx

#region Init
        void Initialize()
        {
            LoadSwaggerInfo();
            InitAuth();
            InitTopMenu();
            InitApiList();
            InitRequestUI();
            InitToolBarUI();
            ClearSelectedApiIdx();
            RefreshUI();
        }

        void LoadSwaggerInfo()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(SwaggerInfoJsonPath);
            if (json == null) return;

            _swaggerInfos = JsonConvert.DeserializeObject<Swagger.SwaggerInfo[]>(json.text);
            RefreshAuthUI();
        }
#endregion // Init

#region TopMenu
        void InitTopMenu()
        {
            SetButtonOnClick("btnChangeSavePath", OnChangeSavePath);
            SetButtonOnClick("btnResetSavePath", OnResetSavePath);

            void OnChangeSavePath(Button btn)
            {
                var path = EditorUtility.OpenFolderPanel("저장 경로 선택", string.Empty, string.Empty);
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    if (path.Contains("Assets/"))
                    {
                        var idx = path.IndexOf("Assets/", StringComparison.CurrentCulture);
                        path = path.Substring(idx);
                        _model.SavePath = path;
                    }
                }
            }

            void OnResetSavePath(Button btn)
            {
                _model.SavePath = Com2VerseApiModel.DefaultSavePath;
            }
        }
#endregion // TopMenu

#region Auth UI
        private void InitAuth()
        {
            SetButtonOnClick("btnClearAuth", OnClearAuth);
            SetButtonOnClick("btnLoginAndAuth", async btn => await OnLoginAndAuthAsync(btn));
            SetButtonOnClick("btnServiceAuth", async btn => await OnServiceAuthAsync(btn));

            void OnClearAuth(Button btn)
            {
                _model.C2VAccessToken = string.Empty;
            }

            async UniTask OnLoginAndAuthAsync(Button btn)
            {
                if (string.IsNullOrWhiteSpace(_model.C2VId)) return;

                btn.SetEnabled(false);

                var response = await Com2VerseLoginAsync(_model.C2VId);
                if (response == null)
                {
                    C2VDebug.LogWarning($"Login Response is NULL");
                }
                else
                {
                    if (response.code != 200)
                    {
                        C2VDebug.LogWarning($"Login Response = {response.msg} ({response.code})");
                    }
                    else
                    {
                        _model.C2VAccessToken = response.data.c2vAccessToken;
                        _model.C2VRefreshToken = response.data.c2vRefreshToken;
                    }
                }

                btn.SetEnabled(true);
            }

            async UniTask OnServiceAuthAsync(Button btn)
            {
                if (string.IsNullOrWhiteSpace(_model.C2VAccessToken)) return;

                await ServicePeekAsync();
            }
        }

        private void RefreshAuthUI()
        {
            var linkView = rootVisualElement.Q<VisualElementEx>("viewLinks");
            linkView?.hierarchy.Clear();

            if (_swaggerInfos == null) return;

            foreach (var info in _swaggerInfos)
            {
                var swaggerItem = InstantiateFromUxml("SwaggerItem");
                if (swaggerItem == null) continue;

                var btnOpenSwagger = swaggerItem.Q<Button>("btnOpenSwagger");
                var btnRefreshApi = swaggerItem.Q<Button>("btnRefreshApi");
                var enableAuth = swaggerItem.Q<Toggle>("enableAuth");

                btnOpenSwagger.text = info.Name;
                btnOpenSwagger.clickable = new Clickable(() => OnOpenUrl(info.SwaggerUrl));

                btnRefreshApi.text = "API 갱신";
                btnRefreshApi.clickable = new Clickable(async () =>
                {
                    var swaggerInfo = info;
                    swaggerInfo.UseAuth = enableAuth.value;
                    await OnParseSwaggerApi(btnRefreshApi, swaggerInfo);
                });

                enableAuth.RegisterValueChangedCallback(evt =>
                {
                    var swaggerInfo = info;
                    swaggerInfo.UseAuth = evt.newValue;
                });

                linkView?.hierarchy.Add(swaggerItem);
            }
        }

        void OnOpenUrl(string url) => Application.OpenURL(url);

        async UniTask OnParseSwaggerApi(Button btn, Swagger.SwaggerInfo info)
        {
            btn.SetEnabled(false);

            var swaggerInfo = await Swagger.ParseSwaggerInfo(info.JsonUrl);
            if (swaggerInfo == null)
            {
                btn.SetEnabled(true);
                return;
            }

            swaggerInfo.Name = info.Name;
            swaggerInfo.Namespace = info.Namespace;
            swaggerInfo.ApiUrl = info.ApiUrl;
            swaggerInfo.UseAuth = info.UseAuth;

            if (_swaggerApis.ContainsKey(swaggerInfo.Name))
                _swaggerApis.Remove(swaggerInfo.Name);
            _swaggerApis.Add(swaggerInfo.Name, swaggerInfo);

            RefreshApiGroup();
            btn.SetEnabled(true);
        }
#endregion // Auth UI

#region ApiList UI
        private void InitApiList()
        {
            SetButtonOnClick("btnOpenSwaggerInfo", OnOpenSwaggerInfo);
            SetButtonOnClick("btnRefreshSwagger", OnRefreshSwagger);
            SetButtonOnClick("btnClearSwagger", OnClearSwaggerApis);

            var apiList = rootVisualElement.Q<ListViewEx>("listApis");
            var idx = 0;

            apiList.itemsSource = Array.Empty<Swagger.ApiInfo>();
            apiList.makeItem = () =>
            {
                var row = new VisualElementEx();
                row.style.flexDirection = FlexDirection.Row;
                row.style.flexGrow = 1;
                row.style.justifyContent = Justify.FlexStart;
                row.style.alignItems = Align.Center;

                var name = new LabelEx();
                name.name = "name";

                var path = new LabelEx();
                path.name = "path";

                row.Add(name);
                row.Add(path);
                return row;
            };

            apiList.bindItem = (item, i) =>
            {
                var apiInfos = GetSelectedApiInfos();
                if (apiInfos.Length < i) return;

                var apiInfo = apiInfos[i];

                var name = item.Q<LabelEx>("name");
                var path = item.Q<LabelEx>("path");

                var tag = apiInfo.Tags.Length > 0 ? $"[{apiInfo.Tags[0]}]" : string.Empty;
                name.text = $"{tag} {apiInfo.Summary} ({apiInfo.RequestType})";
                path.text = apiInfo.ApiPath;
            };

            apiList.onSelectedIndicesChange -= OnSelectionChanged;
            apiList.onSelectedIndicesChange += OnSelectionChanged;

            void OnSelectionChanged(IEnumerable<int> selectedIndices)
            {
                foreach (var idx in selectedIndices)
                {
                    _selectedApiIndex = idx;
                    RefreshUI();
                    return;
                }
            }

            RefreshApiGroup();

            void OnOpenSwaggerInfo(Button btn)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(SwaggerInfoJsonPath);
                AssetDatabase.OpenAsset(asset);
            }

            void OnRefreshSwagger(Button btn)
            {
                LoadSwaggerInfo();
            }

            void OnClearSwaggerApis(Button btn)
            {
                _swaggerApis.Clear();
                RefreshApiGroup();
                RefreshUI();
            }
        }
        private void RefreshApiGroup()
        {
            ClearSelectedApiIdx();

            var apiGroup = rootVisualElement.Q<RadioButtonGroupEx>("apiGroup");
            apiGroup.hierarchy.Clear();

            foreach (var (key, swaggerApi) in _swaggerApis)
            {
                var button = new RadioButtonEx();
                button.style.flexGrow = 1;
                button.text = key;
                button.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        _selectedApiKey = key;
                        RefreshApiList();
                    }
                });
                apiGroup.hierarchy.Add(button);
            }
        }

        private void RefreshApiList()
        {
            var apiList = rootVisualElement.Q<ListViewEx>("listApis");
            IList itemSource = Array.Empty<Swagger.ApiInfo>();
            if (IsValidApiKey())
                itemSource = GetSelectedApiInfos();
            apiList.itemsSource = itemSource;
        }
#endregion // ApiList UI

#region Request
        private void InitRequestUI()
        {
            SetButtonOnClick("btnSendRequest", async (btn) => await OnSendRequestAsync(btn));

            async UniTask OnSendRequestAsync(Button btn)
            {
                _btnSendRequest = btn;
                await SendRequestAsync();
            }
        }

        private void BindParameters(IList list)
        {
            var paramList = rootVisualElement.Q<VisualElementEx>("paramItems");
            paramList.hierarchy.Clear();

            var apiInfo = GetSelectedApiInfo();
            if (!apiInfo.HasValue) return;
            var parameters = apiInfo.Value.Parameters;
            if (parameters.Length < 1) return;

            _urlParams.Clear();

            for (var i = 0; i < list.Count; i++)
            {
                var item = new VisualElementEx();
                AddParameter(item, parameters[i]);
                paramList.hierarchy.Add(item);
            }

            void AddParameter(VisualElement item, Swagger.Parameter parameter)
            {
                if (parameter.IsRef && TryGetComponentInfo(parameter.Ref, out var componentInfo))
                {
                    AddRefWithInput(item, parameter.Name, componentInfo, (propName, value) => OnValueChanged(parameter.Name, value, propName));
                }
                else
                {
                    var row = TextInputRow.CreateNew($"{parameter.Name} ({parameter.GetSchemaDataType()})");
                    row.SetValidateType(parameter.GetDataType());
                    row.SetValueChangedCallback(evt => OnValueChanged(parameter.Name, evt.newValue));
                    item.Add(row.Layout);
                }
            }

            void OnValueChanged(string key, string value, string propName = "")
            {
                if (!(string.IsNullOrWhiteSpace(propName) || key.Equals(propName)))
                    key = $"{key}.{propName}";

                if (_urlParams.ContainsKey(key))
                    _urlParams[key] = value;
                else
                    _urlParams.Add(key, value);
            }
        }
        private void BindRequestBody(IList list)
        {
            var requestBodyItems = rootVisualElement.Q<VisualElementEx>("requestBodyItems");
            requestBodyItems.hierarchy.Clear();

            if (_requestBodyProperties == null || _requestBodyProperties.Length != list?.Count) return;
            _requestBodyParams.Clear();

            foreach (var property in _requestBodyProperties)
            {
                var item = new VisualElementEx();
                AddProperty(item, property, string.Empty);
                requestBodyItems.hierarchy.Add(item);
            }

            void AddProperty(VisualElement item, Swagger.PropertyInfo propertyInfo, string keyPrefix)
            {
                var key = GetRequestBodyFieldName(keyPrefix, propertyInfo.Name);
                if (propertyInfo.IsRef && TryGetComponentInfo(propertyInfo.Ref, out var componentInfo))
                {
                    AddRefWithInput(item, propertyInfo.Name, componentInfo, (propName, value) => OnValueChanged(key, value, propName));
                }
                else
                {
                    var row = TextInputRow.CreateNew($"{propertyInfo.Name} ({propertyInfo.GetPropertyType()})");
                    row.SetValidateType(propertyInfo.GetDataType());
                    row.SetValueChangedCallback(evt => OnValueChanged(key, evt.newValue));
                    item.Add(row.Layout);
                }
            }

            void OnValueChanged(string key, string value, string propName = "")
            {
                if (!(string.IsNullOrWhiteSpace(propName) || key.Equals(propName)))
                    key = $"{key}.{propName}";

                if (_requestBodyParams.ContainsKey(key))
                    _requestBodyParams[key] = value;
                else
                    _requestBodyParams.Add(key, value);
            }
        }

        private async UniTask SendRequestAsync()
        {
            var selectedApiInfoObj = GetSelectedApiInfo();
            if (!selectedApiInfoObj.HasValue) return;
            if (string.IsNullOrWhiteSpace(_model.C2VAccessToken)) return;

            var selectedApiInfo = selectedApiInfoObj.Value;
            _btnSendRequest?.SetEnabled(false);

            var url = GetRequestUrl(selectedApiInfo);
            C2VDebug.Log($"SEND REQUEST = [{selectedApiInfo.RequestType}] {url}");

            WebApiUtil.Instance.AccessToken = _model.C2VAccessToken;
            if (!WebApiUtil.Instance.TrySetAuthToken())
            {
                C2VDebug.LogWarning($"invalid accessToken token : {WebApiUtil.Instance.AccessToken}");
                return;
            }

            var requestBuilder = HttpRequestBuilder.CreateNew(selectedApiInfo.RequestType, url);
            if (selectedApiInfo.HasRequestBody)
            {
                var requestContent = selectedApiInfo.RequestBody.Value.RequestContents.First();
                if (TryGetComponentInfo(requestContent.GetRequestContentDataType(), out var componentInfo))
                {
                    var jRoot = new JObject();
                    AddComponentInfo(jRoot, componentInfo, string.Empty);
                    requestBuilder.SetContent(jRoot.ToString());
                    requestBuilder.SetContentType(Client.Constant.ContentJson);
                    C2VDebug.Log($"REQUEST BODY = {jRoot}");
                }
            }

            var response = await Client.Message.RequestStringAsync(requestBuilder.Request);
            C2VDebug.Log($"RESPONSE ({response.StatusCode}) = {response.Value}");

            _btnSendRequest?.SetEnabled(true);

            string GetRequestUrl(Swagger.ApiInfo apiInfo)
            {
                var path = apiInfo.ApiPath;
                if (apiInfo.HasParameter)
                {
                    var hasTrailParam = false;
                    for (var i = 0; i < apiInfo.Parameters.Length; i++)
                    {
                        var parameter = apiInfo.Parameters[i];
                        var key = parameter.Name;
                        if (_urlParams.ContainsKey(key))
                        {
                            var paramValue = _urlParams[key];
                            var newPath = path.Replace($"{{{key}}}", paramValue);

                            if (newPath == path)
                            {
                                if (!hasTrailParam)
                                {
                                    path = $"{path}?";
                                    hasTrailParam = true;
                                }
                                else
                                {
                                    path = $"{path}&";
                                }

                                path = $"{path}{key}={_urlParams[key]}";
                            }
                            else
                            {
                                path = newPath;
                            }
                        }
                    }
                }

                var apiUrl = $"{GetSelectedApiUrl()}{path}";
                return apiUrl;
            }

            void AddComponentInfo(JToken jParent, Swagger.ComponentInfo componentInfo, string keyPrefix)
            {
                switch (componentInfo)
                {
                    case Swagger.PropertyComponent propertyComponent:
                    {
                        foreach (var property in propertyComponent.Properties)
                        {
                            var key = GetRequestBodyFieldName(keyPrefix, property.Name);
                            if (property.IsRef)
                            {
                                if (TryGetComponentInfo(property.GetPropertyType(), out var subComponentInfo))
                                {
                                    if (subComponentInfo.IsEnumComponent)
                                    {
                                        AddProperty(jParent, key);
                                    }
                                    else
                                    {
                                        var jSubComponent = new JObject();
                                        AddComponentInfo(jSubComponent, subComponentInfo, key);
                                        Add(property.Name, jSubComponent);
                                    }
                                }
                            }
                            else if (_requestBodyParams.ContainsKey(key))
                            {
                                AddProperty(jParent, key);
                            }
                        }
                        break;
                    }
                    case Swagger.EnumComponent _:
                    {
                        AddProperty(jParent, keyPrefix);
                        break;
                    }
                }

                void Add(string name, JToken item)
                {
                    if (string.IsNullOrWhiteSpace(name)) return;

                    switch (item)
                    {
                        case JObject jObj:
                            jParent[name] = jObj;
                            break;
                        case JProperty jProperty:
                        {
                            if (jParent is JObject obj)
                                obj.Add(jProperty);
                        }
                            break;
                    }
                }

                void AddProperty(JToken parent, string key)
                {
                    if (parent is not JObject) return;

                    if (_requestBodyParams.ContainsKey(key))
                    {
                        var value = _requestBodyParams[key];
                        var keyName = GetKeyName();
                        (parent as JObject).Add(new JProperty(keyName, value));
                    }

                    string GetKeyName()
                    {
                        var idx = key.LastIndexOf(".");
                        return idx == -1 ? key : key.Substring(idx + 1);
                    }
                }
            }
        }

        private string GetRequestBodyFieldName(string prefix, string name) => string.IsNullOrWhiteSpace(prefix) ? name : $"{prefix}.{name}";
#endregion // Request

#region Response
        private void BindResponse(Swagger.Response? responseObj)
        {
            var responseItems = rootVisualElement.Q<VisualElementEx>("responseItems");
            responseItems.hierarchy.Clear();

            if (!responseObj.HasValue || responseObj.Value.IsVoid) return;

            var response = responseObj.Value;
            if (response.IsRef && TryGetComponentInfo(response.Ref, out var componentInfo))
                AddRefWithLabel(responseItems, response.Type, componentInfo);
            else
                responseItems.Add(TextLabel.CreateNew(response.Type).Layout);
        }
#endregion // Response

#region Components
        private void BindComponentInfos(IList list)
        {
            var componentList = rootVisualElement.Q<VisualElementEx>("componentItems");
            componentList.hierarchy.Clear();

            var componentInfos = GetComponentInfos();
            if (componentInfos.Length != list.Count) return;

            for (var i = 0; i < list.Count; i++)
            {
                var foldout = new FoldoutEx();
                foldout.value = false;
                foldout.text = componentInfos[i].Name;

                AddComponent(foldout, componentInfos[i]);
                componentList.hierarchy.Add(foldout);
            }

            void AddComponent(VisualElement item, Swagger.ComponentInfo component)
            {
                switch (component)
                {
                    case Swagger.PropertyComponent propertyComponent:
                    {
                        foreach (var property in propertyComponent.Properties)
                        {
                            if (property.IsRef && TryGetComponentInfo(property.Ref, out var componentInfo))
                            {
                                AddRefWithLabel(item, property.Name, componentInfo);
                            }
                            else
                            {
                                var row = TextLabel.CreateNew($"{property.Name} ({property.GetPropertyType()})");
                                item.Add(row.Layout);
                            }
                        }

                        break;
                    }
                    case Swagger.EnumComponent enumComponent:
                    {
                        foreach (var (enumValue, enumName) in enumComponent.Items)
                        {
                            var row = TextLabel.CreateNew($"{enumName} = {Convert.ToString(enumValue)}");
                            item.Add(row.Layout);
                        }
                    }
                        break;
                }
            }
        }
#endregion // Components

#region Toolbar
        private void InitToolBarUI()
        {
            SetButtonOnClick("btnCodeGenerate", OnCodeGenerate);
            SetButtonOnClick("btnCodeDelete", OnCodeDelete);
            SetButtonOnClick("btnAssetRefresh", OnAssetRefresh);

            void WriteToFile(string dir, string fileName, string text)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var filePath = Path.Combine(dir, $"{fileName}.cs");
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllText(filePath, text);
            }

            void OnCodeGenerate(Button btn)
            {
                var apiNameSuffix = "_API";
                var apiNames = string.Empty;
                if (_swaggerApis.Count > 0)
                    apiNames = _swaggerApis.Select(api => $"{api.Value.Name} ({api.Value.Namespace}{apiNameSuffix}.cs)").Aggregate((l, r) => $"{l}\n{r}");

                if (EditorUtility.DisplayDialog("API 코드를 생성합니다.", $"코드 생성 후 문제가 발생할 수 있습니다.\n기존 코드를 백업 해 주세요.\n-----------------------------------\n{apiNames}", "예", "아니오"))
                {
                    string code;
                    var path = Path.Combine(Application.dataPath, _model.SavePath.Replace("Assets/", string.Empty));

                    foreach (var (_, value) in _swaggerApis)
                    {
                        var fileName = $"{value.Namespace}{apiNameSuffix}";
                        code = CodeGenerator.GenerateSwaggerApi(value);

                        if (string.IsNullOrWhiteSpace(code)) break;
                        if (string.IsNullOrWhiteSpace(_model.SavePath)) break;

                        WriteToFile(path, fileName, code);
                    }

                    code = CodeGenerator.GenerateUtilClass();
                    WriteToFile(path, "Util", code);
                }
            }

            void OnCodeDelete(Button btn)
            {
                if (EditorUtility.DisplayDialog("코드 삭제", "WebAPI를 사용중인 코드에 영향을 미칠 수 있습니다.\n생성된 코드를 삭제하시겠습니까?", "예", "아니오"))
                {
                    var path = Path.Combine(Application.dataPath, _model.SavePath.Replace("Assets/", string.Empty));
                    if (Directory.Exists(path))
                    {
                        var filePaths = Directory.GetFiles(path);
                        foreach (var filePath in filePaths)
                        {
                            if (filePath.EndsWith("asmdef")) continue;

                            File.Delete(filePath);
                        }
                    }
                }
            }

            void OnAssetRefresh(Button btn) => AssetDatabase.Refresh();
        }
#endregion // Toolbar

#region UI Refresh
        private void RefreshUI()
        {
            RefreshComponentsUI();
            RefreshRequestUI();
            RefreshApiList();
        }

        private void RefreshComponentsUI()
        {
            var layout = rootVisualElement.Q<VisualElementEx>("layoutComponents");
            layout.style.display = GetComponentInfos().Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void RefreshRequestUI()
        {
            var selectedIdx = _selectedApiIndex;
            var layout = rootVisualElement.Q<VisualElementEx>("layoutRequestApi");
            layout.style.display = selectedIdx == -1 ? DisplayStyle.None : DisplayStyle.Flex;

            if (selectedIdx == _prevSelectedApiIndex || selectedIdx == -1)
                return;

            _selectedApiIndex = selectedIdx;
            _prevSelectedApiIndex = selectedIdx;

            if (!IsValidApiKey()) return;

            var apiInfo = GetSelectedApiInfos()[_selectedApiIndex];

            _model.SelectedParamName = apiInfo.Summary;
            RefreshApiInfo();

            _btnSendRequest?.SetEnabled(IsAvailableToSend());
        }

        private void RefreshApiInfo()
        {
            var apiInfoObj = GetSelectedApiInfo();
            if (!apiInfoObj.HasValue) return;

            var apiInfo = apiInfoObj.Value;
            var baseUrl = GetSelectedApiUrl();

            _model.ApiCategory = apiInfo.Tags[0];
            _model.ApiRequestType = apiInfo.RequestType.ToString();
            _model.ApiUrl = $"{baseUrl}/{apiInfo.ApiPath}";

            if (apiInfo.Parameters != null && apiInfo.Parameters.Length > 0)
                BindParameters(apiInfo.Parameters);
            else
                BindParameters(Array.Empty<Swagger.Parameter>());

            if (apiInfo.RequestBody.HasValue)
            {
                var refName = apiInfo.RequestBody.Value.RequestContents[0].Ref;
                if (TryGetComponentInfo(refName, out var componentInfo))
                {
                    if (componentInfo is Swagger.PropertyComponent propertyComponent)
                    {
                        _requestBodyProperties = propertyComponent.Properties;
                        BindRequestBody(_requestBodyProperties);
                    }
                }
            }
            else
            {
                BindRequestBody(Array.Empty<Swagger.PropertyInfo>());
            }

            BindResponse(apiInfo.Response);

            var componentInfos = GetComponentInfos();
            BindComponentInfos(componentInfos);
        }
#endregion // UI Refresh

#region Model
        private bool IsValidApiKey()
        {
            if (IsValidApiKey(_selectedApiKey)) return true;
            _selectedApiKey = string.Empty;
            return false;
        }

        private bool IsValidApiKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (_swaggerApis.ContainsKey(key)) return true;
            return false;
        }
        private Swagger.ApiInfo[] GetSelectedApiInfos()
        {
            if (!IsValidApiKey()) return Array.Empty<Swagger.ApiInfo>();
            return _swaggerApis[_selectedApiKey].ApiInfos;
        }

        private bool TryGetComponentInfo(string name, out Swagger.ComponentInfo info)
        {
            info = null;

            if (!IsValidApiKey()) return false;
            if (string.IsNullOrWhiteSpace(name) || !name.Contains("/")) return false;

            name = name.Split("/")[^1];
            return _swaggerApis[_selectedApiKey].TryGetComponent(name, out info);
        }
        private Swagger.ApiInfo? GetSelectedApiInfo()
        {
            var apiInfos = GetSelectedApiInfos();
            if (apiInfos.Length < _selectedApiIndex) return null;
            return apiInfos[_selectedApiIndex];
        }

        private string GetSelectedApiUrl()
        {
            if (!IsValidApiKey()) return string.Empty;
            return _swaggerApis[_selectedApiKey].ApiUrl;
        }

        private Swagger.ComponentInfo[] GetComponentInfos()
        {
            if (!IsValidApiKey()) return Array.Empty<Swagger.ComponentInfo>();
            return _swaggerApis[_selectedApiKey].ComponentInfos ?? Array.Empty<Swagger.ComponentInfo>();
        }

        private void ClearSelectedApiIdx()
        {
            _selectedApiIndex = -1;
        }
#endregion // Model

#region Util
        private static ulong GenerateHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hashValue = BitConverter.ToUInt64(hashBytes, 0);
                return hashValue;
            }
        }
        private bool IsAvailableToSend()
        {
            if (string.IsNullOrWhiteSpace(_model.ApiUrl)) return false;

            return !string.IsNullOrWhiteSpace(_model.C2VAccessToken);
        }

        private VisualElement InstantiateFromUxml(string name)
        {
            var item = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Packages/com.com2verse.c2vapi/Editor/UI/{name}.uxml");
            return item != null ? item.Instantiate() : null;
        }
#endregion // Util

#region Auth
        private async UniTask<AuthInfo.LoginResponse> Com2VerseLoginAsync(string id)
        {
            var idHash = GenerateHash(_model.C2VId);
            var request = new AuthInfo.LoginRequest
            {
                Did = "0",
                HiveToken = "0",
                HiveValidate = false,
                PId = Convert.ToString(idHash),
                Platform = 20827,
                AccessExpires = 0,
                RefreshExpires = 0,
            };

            var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, AuthInfo.C2VAuthSignURL);
            builder.SetContent(JsonUtility.ToJson(request));
            builder.SetContentType(AuthInfo.ContentJson);

            var responseString = await Client.Message.RequestStringAsync(builder.Request);
            var response = JsonUtility.FromJson<AuthInfo.LoginResponse>(responseString.Value);
            return response;
        }

        private async UniTask ServicePeekAsync()
        {
            var did = "0";
            var serviceType = "1";
            var code = "A1B2C3D4";

            var url = Util.MakeUrlWithParam(AuthInfo.C2VServicePeekURL, new (string, string)[]
            {
                ("service-type", serviceType),
                ("code", code),
                ("did", did),
            });

            Client.Auth.SetTokenAuthentication(Util.MakeTokenAuthInfo(_model.C2VAccessToken));

            var response = await Client.GET.RequestAsync<AuthInfo.ServicePeekResponse>(url);
            if (response == null) return;

            C2VDebug.Log($"Service URI: {response.Value.data.serviceUri}");
            GUIUtility.systemCopyBuffer = response.Value.data.serviceUri;
            Application.OpenURL(response.Value.data.serviceUri);
        }
#endregion // Auth

#region UI Toolkit
        private interface ILayout
        {
            public VisualElementEx Layout { get; }
        }

        private record TextLabel : ILayout
        {
            private readonly LabelEx _label;
            public VisualElementEx Layout { get; private set; }
            public static TextLabel CreateNew(string text) => new TextLabel(text);

            private TextLabel(string text)
            {
                Layout = new VisualElementEx();

                _label = new LabelEx();
                _label.text = text;

                Layout.Add(_label);
            }
        }

        private record TextInputRow : ILayout
        {
            private readonly LabelEx _tag;
            private readonly TextFieldEx _input;
            private Swagger.eDataType _validateType = Swagger.eDataType.NONE;
            public VisualElementEx Layout { get; private set; }

            public static TextInputRow CreateNew(string tag) => new TextInputRow(tag);

            private TextInputRow(string tag)
            {
                Layout = new VisualElementEx();
                Layout.style.flexDirection = FlexDirection.Row;
                Layout.style.justifyContent = Justify.SpaceBetween;
                Layout.style.alignItems = Align.Center;

                _tag = new LabelEx();
                _tag.text = tag;
                _tag.style.flexGrow = 1;
                _tag.style.flexBasis = 10;

                _input = new TextFieldEx();
                _input.style.flexGrow = 1;
                _input.style.flexBasis = 90;

                Layout.Add(_tag);
                Layout.Add(_input);
            }

            public void SetValueChangedCallback(EventCallback<ChangeEvent<string>> onValueChanged)
            {
                if (onValueChanged == null) return;

                _input.UnregisterValueChangedCallback(onValueChanged);
                _input.RegisterValueChangedCallback(onValueChanged);
            }

            public void SetValidateType(Swagger.eDataType validateType)
            {
                _validateType = validateType;
                _input.UnregisterValueChangedCallback(OnValidateInput);
                _input.RegisterValueChangedCallback(OnValidateInput);
            }

            void OnValidateInput(ChangeEvent<string> evt) => OnValidateInput(evt, _validateType);

            private void OnValidateInput(ChangeEvent<string> evt, Swagger.eDataType validateType)
            {
                var isValid = true;
                switch (validateType)
                {
                    case Swagger.eDataType.NONE:
                        break;
                    case Swagger.eDataType.INT32:
                        isValid = Int32.TryParse(evt.newValue, out _);
                        break;
                    case Swagger.eDataType.INT64:
                        isValid = Int64.TryParse(evt.newValue, out _);
                        break;
                    case Swagger.eDataType.BOOLEAN:
                        isValid = Boolean.TryParse(evt.newValue, out _);
                        break;
                    case Swagger.eDataType.STRING:
                        break;
                    default:
                        break;
                }

                _tag.style.color = isValid ? _noemalTextColor : _errorTextColor;
            }
        }

        private void SetButtonOnClick(string name, Action<Button> onClick)
        {
            var btn = rootVisualElement.Q<Button>(name);
            if (btn == null) return;
            btn.clickable = new Clickable(() => onClick?.Invoke(btn));
        }
        void AddRefWithLabel(VisualElement item, string name, Swagger.ComponentInfo componentInfo)
        {
            AddRef(item, name, componentInfo, (propName, propType, _) =>
            {
                var row = TextLabel.CreateNew($"{propName} ({propType})");
                row.Layout.style.flexGrow = 1;
                row.Layout.style.justifyContent = Justify.Center;
                return row;
            });
        }
        void AddRefWithInput(VisualElement item, string name, Swagger.ComponentInfo componentInfo, Action<string, string> onValueChanged = null)
        {
            AddRef(item, name, componentInfo, (propName, propType, validateType) =>
            {
                var row = TextInputRow.CreateNew($"{propName} ({propType})");
                row.SetValidateType(validateType);
                row.SetValueChangedCallback(evt => onValueChanged?.Invoke(propName, evt.newValue));
                row.Layout.style.flexGrow = 1;
                row.Layout.style.justifyContent = Justify.Center;
                return row;
            });
        }

        void AddRef<T>(VisualElement item, string name, Swagger.ComponentInfo componentInfo, Func<string, string, Swagger.eDataType, T> onCreateRow) where T : class, ILayout
        {
            var foldout = new FoldoutEx();
            foldout.value = false;
            foldout.text = $"{name} ({componentInfo.Name})";

            switch (componentInfo)
            {
                case Swagger.PropertyComponent propertyComponent:
                {
                    foreach (var property in propertyComponent.Properties)
                    {
                        if (property.IsRef && TryGetComponentInfo(property.Ref, out var subComponentInfo))
                        {
                            if (propertyComponent.Name != subComponentInfo.Name)
                                AddRef<T>(foldout, property.Name, subComponentInfo, onCreateRow);
                        }
                        else
                        {
                            var validateType = property.GetDataType();
                            var row = onCreateRow?.Invoke(property.Name, property.GetPropertyType(), validateType);
                            foldout.Add(row.Layout);
                        }
                    }

                    break;
                }
                case Swagger.EnumComponent enumComponent:
                {
                    foreach (var (enumValue, enumName) in enumComponent.Items)
                    {
                        var row = TextLabel.CreateNew($"{enumName} = {Convert.ToString(enumValue)}");
                        foldout.Add(row.Layout);
                    }

                    foldout.Add(onCreateRow?.Invoke(name, "ENUM", Swagger.eDataType.INT32)?.Layout);
                }
                    break;
            }

            item.Add(foldout);
        }
#endregion // UI Toolkit
    }
}
