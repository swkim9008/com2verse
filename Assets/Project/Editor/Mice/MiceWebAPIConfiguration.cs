/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIConfiguration.cs
* Developer:	sprite
* Date:			2023-05-16 19:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;
using Com2Verse.Logger;
using System;
using System.Linq;
using UnityEngine;
using Com2Verse.Network;
using Com2Verse.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Com2VerseEditor.Mice
{
    public class MiceWebAPIConfiguration : EditorWindow
    {
        [MenuItem("Com2Verse/MiceWebAPIBuilder/Config", priority = 120001)]
        private static void OnMiceWebAPIBuilder_Config()
        {
            var wnd = EditorWindow.GetWindow<MiceWebAPIConfiguration>(true);
            wnd.minSize = new Vector2(500, 350);
            wnd.maxSize = wnd.minSize;
            wnd.Show();
        }

        private void OnEnable()
        {
            _styleLabel = null;

            _configurationMap = MiceWebAPIConfiguration.LoadConfigJson();

            _targetServer.Load();

            if (_configurationMap.TryGetValue(_targetServer.Section, out var infos))
            {
                var info = infos.FirstOrDefault(e => string.Compare(e.displayName, _targetServer.Name) == 0);
                if (info != null)
                {
                    info.TestServerAddress();
                }
            }

            this.RefreshWindowCaption();
        }

        private void RefreshWindowCaption()
        {
            this.titleContent = new GUIContent
            (
                $"MiceWebAPIBuilder - {(_targetServer.Modified ? "* " : "")}" +
                $"[{_targetServer.Section}|{_targetServer.Name}]"
            );
        }

#region Server Address Management
        public enum TestState
        {
            UNKNOWN,
            TESTING,
            COMPLETED,
        }

        public class ServerInfo
        {
            const string PROGRESS = @"←↑→↓";
            public Configuration Configuration { get; private set; }
            public bool IsValid => !string.IsNullOrEmpty(this.Configuration.MiceServerAddress);
            public bool IsLive { get; private set; }
            public string displayName => this.Configuration.ServerLists[Mathf.Min(this.Configuration.ServerLists.Length - 1, Configurator.Instance.ServerType)].Name;
            public string displayAddress
            {
                get
                {
                    var state = '?';
                    if (this.TestState == TestState.TESTING)
                    {
                        state = PROGRESS[Mathf.FloorToInt((float)_prgsCount / 10)];
                        _prgsCount = ++_prgsCount % (PROGRESS.Length * 10);
                    }
                    else if(this.TestState == TestState.COMPLETED)
                    {
                        state = this.IsLive ? 'O' : 'X';
                    }

                    return $"{this.Configuration.MiceServerAddress} ({state})";
                }
            }
            public bool IsTesting => this.TestState == TestState.TESTING;
            public TestState TestState { get; private set; }

            private int _prgsCount;
            private UnityWebRequestAsyncOperation _ao;

            public ServerInfo(Configuration configuration)
            {
                this.Configuration = configuration;
                this.IsLive = false;
                this.TestState = TestState.UNKNOWN;
                _prgsCount = 0;
            }

            public void TestServerAddress()
            {
                if (this.TestState == TestState.TESTING) return;

                this.IsLive = false;
                this.TestState = TestState.TESTING;

                var url = $"{this.Configuration.MiceServerAddress}{MiceWebAPIBuilder.MICE_WEB_API_SWAGGER}";
                var uwr = UnityWebRequest.Get(url);
                _ao = uwr.SendWebRequest();
                if (_ao.isDone)
                {
                    OnCompleted();
                }
                else
                {
                    _ao.completed += _ => OnCompleted();
                }

                void OnCompleted()
                {
                    this.TestState = TestState.COMPLETED;
                    this.IsLive = _ao.webRequest.result == UnityWebRequest.Result.Success;
                    _ao.webRequest.Dispose();
                    _ao = null;
                }
            }
        }

        private Dictionary<string, ServerInfo[]> _configurationMap;

        public static Dictionary<string, ServerInfo[]> LoadConfigJson()
        {
            const string CFG_FILE = "Config.json";

            Dictionary<string, ServerInfo[]> configurationMap = null;

            try
            {
                var configFile = System.IO.File.ReadAllText(DirectoryUtil.GetStreamingAssetPath(CFG_FILE));
                var serverConfigurations = JsonConvert.DeserializeObject<ServerConfigurations>(configFile);

                configurationMap = typeof(ServerConfigurations)
                    .GetFields()
                    .ToDictionary(e => e.Name, e => (e.GetValue(serverConfigurations) as Configuration[]).Select(el => new ServerInfo(el)).ToArray());
            }
            catch (Exception e)
            {
                C2VDebug.LogError(e.Message);
            }

            return configurationMap;
        }

        public static string GetMiceServerIP(string section, string name)
        {
            var map = MiceWebAPIConfiguration.LoadConfigJson();
            if (map != null && map.TryGetValue(section, out var cfgs) && cfgs.Length > 0)
            {
                var item = cfgs.FirstOrDefault(e => string.Equals(e.displayName, name));
                if (item != null)
                {
                    var cfg = item.Configuration;
                    if (cfg != null)
                    {
                        return cfg.MiceServerAddress;
                    }
                }
            }

            return string.Empty;
        }

        public static string GetMiceServerIP()
        {
            var target = new TargetServer();
            target.Load();
            return MiceWebAPIConfiguration.GetMiceServerIP(target.Section, target.Name);
        }
#endregion // Server Address Management

#region Server Selection Info
        private static readonly string PREFS_KEY = "MICE_WEB_API_CONFIGURATION_TARGET_SERVER";

        internal class TargetServer
        {
            public string Section { get; private set; }
            public string Name { get; private set; }
            public bool Modified
                => string.Compare(this.Section, _section) != 0 || string.Compare(this.Name, _name) != 0;

            private string _section;
            private string _name;

            public void Load()
            {
                var str = EditorPrefs.GetString(PREFS_KEY, "DevConfigurations|mice");
                this.Section = str.Split('|')[0];
                this.Name = str.Split('|')[1];
                this._section = this.Section;
                this._name = this.Name;
            }

            public void Save()
            {
                EditorPrefs.SetString(PREFS_KEY, $"{this.Section}|{this.Name}");
                this._section = this.Section;
                this._name = this.Name;
            }

            public bool Set(string section, string name)
            {
                if (string.Compare(this.Section, section) != 0)
                {
                    this.Section = section;
                }
                if (string.Compare(this.Name, name) != 0)
                {
                    this.Name = name;
                }

                return this.Modified;
            }

            public void Reset()
            {
                this.Section = this._section;
                this.Name = this._name;
            }
        }
#endregion // Server Selection Info

#region UI
        private GUIStyle _styleLabel;
        private TargetServer _targetServer = new TargetServer();
        private Vector2 _scrollPos = Vector2.zero;
        private ServerInfo _testing;

        private void OnGUI()
        {
            if (_styleLabel == null)
            {
                _styleLabel = new GUIStyle(EditorStyles.label);
                _styleLabel.richText = true;
            }

            GUI.enabled = _testing == null || !_testing.IsTesting;

            if (_configurationMap == null)
            {
                EditorGUILayout.LabelField("Empty contents!");
            }
            else
            {
                using (var sv = new EditorGUILayout.ScrollViewScope(_scrollPos, GUILayout.Height(300)))
                {
                    _scrollPos = sv.scrollPosition;
                    sv.handleScrollWheel = true;

                    foreach (var pair in _configurationMap)
                    {
                        var selSection = string.Equals(_targetServer.Section, pair.Key);
                        EditorGUILayout.LabelField($"[{pair.Key}]".ToColoredText(Color.green, selSection), _styleLabel);
                        using (new EditorGUI.IndentLevelScope())
                        {
                            for (int i = 0, cnt = pair.Value.Length; i < cnt; i++)
                            {
                                var item = pair.Value[i];
                                var cfg  = item.Configuration;
                                var nm   = item.displayName;
                                if (string.IsNullOrEmpty(cfg.MiceServerAddress)) continue;

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    var selCFG = selSection && string.Equals(_targetServer.Name, nm);

                                    GUI.changed = false;
                                    {
                                        selCFG = EditorGUILayout.ToggleLeft(item.displayName.ToColoredText(Color.green, selCFG), selCFG, _styleLabel);
                                        EditorGUILayout.LabelField(item.displayAddress.ToColoredText(Color.green, selCFG), _styleLabel);
                                    }
                                    if (GUI.changed && selCFG)
                                    {
                                        _targetServer.Set(pair.Key, nm);
                                        this.RefreshWindowCaption();

                                        _testing = item;
                                        item.TestServerAddress();
                                        this.Repaint();
                                    }
                                }
                            }
                        }
                    }

                    if (_testing != null)
                    {
                        if (_testing.IsTesting)
                        {
                            this.Repaint();
                        }
                        else
                        {
                            if (!_testing.IsLive)
                            {
                                _targetServer.Reset();
                                this.RefreshWindowCaption();
                            }
                            _testing = null;
                            this.Repaint();
                        }
                    }
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = _targetServer.Modified && (_testing == null || !_testing.IsTesting);
                    if(GUILayout.Button("저 장"))
                    {
                        _targetServer.Save();

                        this.RefreshWindowCaption();
                    }
                    GUI.enabled = true;

                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }

            GUI.enabled = true;
        }
    }

    internal static class StringExtensions
    {
        public static string ToColoredText(this string source, Color color, bool flag = true)
            => flag ? $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{source}</color>" : source;
    }
#endregion // UI
}
