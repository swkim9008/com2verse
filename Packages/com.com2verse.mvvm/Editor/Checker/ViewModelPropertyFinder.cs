/*===============================================================
* Product:		Com2Verse
* File Name:	PropertyFinder.cs
* Developer:	tlghks1009
* Date:			2022-11-19 12:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.UI
{
    public sealed class ViewModelPropertyFinder : EditorWindow
    {
        private class BinderInfo
        {
            public string Property;
            public string FullPathOfGameObject;
        }

        [MenuItem("Com2Verse/ViewModel/ViewModel Property Finder")]
        static void OpenWindow()
        {
            var window = GetWindow<ViewModelPropertyFinder>();
            window.titleContent = new GUIContent("ViewModel Property Finder");
            window.maxSize = new Vector2(800, 100);
            window.maxSize = new Vector2(800, 100);
            window.Show();
        }

        private float _layoutWidth = 200f, _buttonLayoutWidth = 90f;
        private string _searchKey;

        private readonly MVVMTrie<BinderInfo> _trie = MVVMTrie<BinderInfo>.CreateNew(MVVMTrie<BinderInfo>.TrieSettings.Default);

        private void OnGUI()
        {
            DrawSearchField();
        }


        private void InitializeBinderProperties()
        {
            var selectionObjects = Selection.gameObjects;

            foreach (var selectionObject in selectionObjects)
            {
                var childObjectOfSelectionObject = selectionObject.GetComponentsInChildren<DataBinder>();

                foreach (var dataBinder in childObjectOfSelectionObject)
                {
                    SerializedObject serializedObject = new SerializedObject(dataBinder);

                    var pair = new MVVMTrie<BinderInfo>.Pair();
                    var binderInfo = new BinderInfo
                    {
                        Property = serializedObject.FindProperty("_sourcePath.property").stringValue,
                        FullPathOfGameObject = MVVMUtil.GetFullPathInHierarchy(dataBinder.transform),
                    };

                    pair.Key = binderInfo.Property;
                    pair.Value = binderInfo;

                    _trie.Insert(pair);
                }
            }
        }


        private void DrawSearchField()
        {
            EditorGUILayout.LabelField("Hierachy에서 선택된 오브젝트의 하위에 있는 DataBinder를 검사 합니다.");

            EditorGUILayout.LabelField("ViewModel 내에서 사용되는 Property의 이름을 넣어주세요.");

            EditorGUILayout.BeginHorizontal();

            _searchKey = EditorGUILayout.TextField(_searchKey, EditorStyles.toolbarSearchField, GUILayout.Width(_layoutWidth));

            if (GUILayout.Button("검색", GUILayout.Width(_buttonLayoutWidth)))
            {
                _trie.Clear();

                InitializeBinderProperties();

                Search();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("검색 결과는 Console창에 나옵니다.");
        }

        private void Search()
        {
            if (_trie.TotalCount == 0)
            {
                return;
            }

            var find = _trie.FindAll(_searchKey);

            if (find == null)
            {
                return;
            }

            foreach (var binderInfo in find)
            {
                C2VDebug.Log($"Property : {binderInfo.Property} - Path : {binderInfo.FullPathOfGameObject}");
            }
        }
    }
}
