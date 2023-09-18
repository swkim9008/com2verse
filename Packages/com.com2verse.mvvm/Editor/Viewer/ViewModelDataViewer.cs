using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Com2VerseEditor.UI
{
    public class ViewModelDataViewer : EditorWindow
    {
        [MenuItem("Com2Verse/ViewModel/ViewModel Data Viewer")]
        static void OpenWindow()
        {
            var window = GetWindow<ViewModelDataViewer>();
            window.titleContent = new GUIContent("Data Viewer");
            window.minSize = new Vector2(1000, 300);
            window.Show();
        }

        enum eColumnType
        {
            PROPERTY_NAME = 0,
            PROPERTY_TYPE = 1,
            PROPERTY_VALUE = 2
        }


        private MultiColumnHeader _multiColumnHeader;
        private MultiColumnHeaderState _multiColumnHeaderState;
        private MultiColumnHeaderState.Column[] _columns;

        private Vector2 _scrollPosition;
        private ViewModel _selectedViewModel;

        private readonly Dictionary<int, bool> _foldOutContainer = new();
        private readonly Dictionary<string, BindableMember<PropertyInfo>> _propertyDict = new();
        private readonly MVVMTrie<BindableMember<PropertyInfo>> _trie = MVVMTrie<BindableMember<PropertyInfo>>.CreateNew(MVVMTrie<BindableMember<PropertyInfo>>.TrieSettings.Default);
        private List<BindableMember<PropertyInfo>> _bindableMemberList = new();

        private float _yScrollOffset = 21f;
        private float _minWidthOfColumns = 250f;
        private float _layoutWidth = 200f;
        private int _leftPadding = 10;
        private int _buttonLayoutWidth = 90;
        private int _columnIndex;

        private readonly Color _lighterColor = Color.white * 0.3f;
        private readonly Color _darkerColor = Color.white * 0.1f;
        private readonly Color _collectionColor = Color.white * 0.6f;

        private string _selectedViewModelName;
        private string _searchKey;

        private Rect _rowRect = new Rect();
        private int _foldOutIndexer;

        private bool _isFoldAll, _isExpandAll;

        private PlayModeStateChange _playModeStateChange;

        private void OnEnable()
        {
            if (Application.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }


        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            _playModeStateChange = playModeStateChange;

            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode ||
                playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                _multiColumnHeader = null;
                _multiColumnHeaderState = null;
                _columns = null;
                _selectedViewModel = null;
                _propertyDict.Clear();

                _selectedViewModelName = string.Empty;
                _searchKey = string.Empty;
                _foldOutContainer.Clear();
                _foldOutIndexer = 0;
            }
        }


        private void PrepareColumns()
        {
            this._columns = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false, // At least one column must be there.
                    autoResize = true,
                    minWidth = _minWidthOfColumns,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("PropertyName"),
                    headerTextAlignment = TextAlignment.Left,
                },
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = _minWidthOfColumns,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("Type"),
                    headerTextAlignment = TextAlignment.Left,
                },
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = _minWidthOfColumns,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("Value"),
                    headerTextAlignment = TextAlignment.Left,
                },
            };

            _multiColumnHeaderState = new MultiColumnHeaderState(columns: _columns);
            _multiColumnHeader = new MultiColumnHeader(state: _multiColumnHeaderState);
            _multiColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();

            _multiColumnHeader.ResizeToFit();
        }


        private void OnGUI()
        {
            if (_multiColumnHeader == null)
                PrepareColumns();

            EditorGUILayout.BeginHorizontal();
            {
                RefreshViewModels();
                RefreshProperties();

                DrawSearchField();

                GUILayout.FlexibleSpace();
                DrawOptions();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            Rect windowRect = GUILayoutUtility.GetLastRect();
            windowRect.width = this.position.width;
            windowRect.height = this.position.height;

            float columnHeight = EditorGUIUtility.singleLineHeight;
            Rect columnRectPrototype = new Rect(source: windowRect)
            {
                height = columnHeight,
            };

            var positionalRectAreaOfScrollView = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
            var viewRect = new Rect(source: windowRect)
            {
                xMax = _columns!.Sum((column) => column.width),
                yMax = _columnIndex * _yScrollOffset
            };

            this._scrollPosition = GUI.BeginScrollView(
                position: positionalRectAreaOfScrollView,
                scrollPosition: this._scrollPosition,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false
            );

            this._multiColumnHeader.OnGUI(rect: columnRectPrototype, 0.0f);

            if (_selectedViewModel != null)
            {
                DrawContext(columnRectPrototype, columnHeight);
            }

            GUI.EndScrollView(handleScrollWheel: true);
            Repaint();
        }


        private void RefreshViewModels()
        {
            var newViewModelList = MasterViewModel.ViewModelDict?.Values.ToList();

            if (_playModeStateChange is PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.EnteredEditMode)
            {
                newViewModelList?.Clear();
            }

            if (newViewModelList?.Count > 0)
            {
                var viewModelNames = new List<string>();

                foreach (var viewModel in newViewModelList)
                {
                    viewModelNames.Add(viewModel.GetType().Name);
                }

                var oldIndex = Array.IndexOf(viewModelNames.ToArray(), _selectedViewModelName);
                if (oldIndex == -1) oldIndex = 0;
                var newIndex = EditorGUILayout.Popup(oldIndex, viewModelNames.ToArray(), GUILayout.Width(_layoutWidth));

                _selectedViewModelName = viewModelNames[newIndex];
                _selectedViewModel = newViewModelList[newIndex];

                if (_selectedViewModel.DontDestroyOnLoad)
                {
                    EditorGUILayout.LabelField("**DontDestroyOnLoad**");
                }
            }
            else
            {
                _selectedViewModel = null;
                _selectedViewModelName = string.Empty;
                _searchKey = string.Empty;
                _foldOutContainer.Clear();
            }
        }

        private void DrawContext(Rect columnRectPrototype, float columnHeight)
        {
            if (_bindableMemberList == null)
            {
                return;
            }

            _foldOutIndexer = 0;
            _columnIndex = 0;

            foreach (var bindableMember in _bindableMemberList)
            {
                var propertyName = bindableMember.MemberName;
                var propertyTypeName = bindableMember.MemberType.Name;
                var propertyInfo = bindableMember.MemberInfo;

                _rowRect = columnRectPrototype;
                _rowRect.y += columnHeight * (_columnIndex + 1);

                EditorGUI.DrawRect(_rowRect, _columnIndex % 2 == 0 ? _darkerColor : _lighterColor);

                int accumulatedLeftPadding = _leftPadding;
                if (bindableMember.MemberInfo.PropertyType.IsGenericType)
                {
                    DrawCollectionItem(accumulatedLeftPadding, columnHeight, _selectedViewModel,
                                       bindableMember.MemberInfo);
                }
                else
                {
                    DrawColumn((int) eColumnType.PROPERTY_NAME, accumulatedLeftPadding, _rowRect.y, propertyName);
                    DrawColumn((int) eColumnType.PROPERTY_TYPE, accumulatedLeftPadding, _rowRect.y, propertyTypeName);

                    try
                    {
                        DrawColumn((int) eColumnType.PROPERTY_VALUE, accumulatedLeftPadding, _rowRect.y, propertyInfo.GetValue(_selectedViewModel)?.ToString());
                    }
                    catch (Exception)
                    {
                        DrawColumn((int) eColumnType.PROPERTY_VALUE, accumulatedLeftPadding, _rowRect.y, "Please check the View model.");
                    }
                }

                _columnIndex++;
            }
        }

        private void DrawColumn(int columnIndex, int leftPadding, float yPos, string context)
        {
            if (_multiColumnHeader.IsColumnVisible(columnIndex))
            {
                int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = yPos;

                GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(left: leftPadding, right: 10, top: 2, bottom: 2)
                };

                EditorGUI.LabelField
                (
                    position: _multiColumnHeader.GetCellRect(visibleColumnIndex, columnRect),
                    label: new GUIContent(context),
                    nameFieldGUIStyle
                );
            }
        }


        private void DrawCollectionItem(int accumulatedLeftPadding, float columnHeight, object viewModel,
                                        PropertyInfo propertyInfo)
        {
            if (!propertyInfo.PropertyType.IsGenericType)
                return;

            var foldOutRect = _rowRect;
            foldOutRect.x = accumulatedLeftPadding / 2f;

            accumulatedLeftPadding += _leftPadding;


            MakeFoldOutKeyAndDrawFoldOut(foldOutRect, propertyInfo.Name, result =>
            {
                if (result)
                    DrawCollectionFoldOut(accumulatedLeftPadding, columnHeight, viewModel, propertyInfo);
            });
        }


        private void DrawCollectionFoldOut(int accumulatedLeftPadding, float columnHeight, object viewModel,
                                           PropertyInfo propertyInfo)
        {
            var foldOutRect = _rowRect;
            foldOutRect.x = accumulatedLeftPadding;

            accumulatedLeftPadding += _leftPadding;

            if (propertyInfo.GetValue(viewModel) is INotifyCollectionChanged collection)
            {
                foreach (var itemSource in collection.ItemsSource)
                {
                    _rowRect.y += columnHeight;
                    foldOutRect.y = _rowRect.y;
                    MakeFoldOutKeyAndDrawFoldOut(foldOutRect, propertyInfo.Name, result =>
                    {
                        if (result)
                            DrawColumns(accumulatedLeftPadding, columnHeight, itemSource);
                    });
                    _columnIndex++;
                }
            }
        }

        private void DrawColumns(int accumulatedLeftPadding, float columnHeight, object viewModel)
        {
            accumulatedLeftPadding += _leftPadding;

            var properties = viewModel.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetGetMethod(false) == null)
                    continue;

                if (property.PropertyType.IsGenericType)
                {
                    DrawCollectionFoldOut(accumulatedLeftPadding, columnHeight, viewModel, property);
                }
                else
                {
                    EditorGUI.DrawRect(_rowRect, _columnIndex % 2 == 0 ? _darkerColor : _lighterColor);

                    _rowRect.y += columnHeight;

                    DrawColumn((int) eColumnType.PROPERTY_NAME, accumulatedLeftPadding, _rowRect.y, property.Name);
                    DrawColumn((int) eColumnType.PROPERTY_TYPE, _leftPadding, _rowRect.y, property.PropertyType.ToString());
                    DrawColumn((int) eColumnType.PROPERTY_VALUE, _leftPadding, _rowRect.y, property.GetValue(viewModel)?.ToString());

                    _columnIndex++;
                }
            }
        }

        private void RefreshProperties()
        {
            if (_selectedViewModel == null) return;

            var selectedViewModel = _selectedViewModel;
            var properties = new List<BindableMember<PropertyInfo>>();
            var bindableMemberList = Finder.GetProperties(selectedViewModel.GetType(), properties, (property) =>
                                                              property.GetGetMethod(false) != null);

            _trie.Clear();

            foreach (var bindableMember in bindableMemberList)
            {
                if (bindableMember?.MemberInfo == null ||
                    bindableMember.MemberType?.GetInterface(nameof(ICommand)) != null)
                    continue;

                var pair = new MVVMTrie<BindableMember<PropertyInfo>>.Pair
                {
                    Key = bindableMember.MemberName,
                    Value = bindableMember
                };

                _trie.Insert(pair);
            }
        }


        private void MakeFoldOutKeyAndDrawFoldOut(Rect foldOutRect, string propertyName, Action<bool> result)
        {
            _foldOutIndexer++;
            if (!_foldOutContainer.TryGetValue(_foldOutIndexer, out bool foldOut))
            {
                _foldOutContainer.Add(_foldOutIndexer, true);
            }

            _foldOutContainer[_foldOutIndexer] =
                EditorGUI.Foldout(foldOutRect, _foldOutContainer[_foldOutIndexer], propertyName);

            result?.Invoke(_foldOutContainer[_foldOutIndexer]);
        }


        private void DrawSearchField()
        {
            _searchKey = EditorGUILayout.TextField(_searchKey, EditorStyles.toolbarSearchField,GUILayout.Width(_layoutWidth));

            _bindableMemberList = _trie.FindAll(_searchKey);
        }


        private void DrawOptions()
        {
            if (GUILayout.Button("모두 펼치기", GUILayout.Width(_buttonLayoutWidth)))
            {
                SetActiveAllFoldOut(true);
            }


            if (GUILayout.Button("모두 닫기", GUILayout.Width(_buttonLayoutWidth)))
            {
                SetActiveAllFoldOut(false);
            }
        }

        private void SetActiveAllFoldOut(bool display)
        {
            var foldOutContainer = new Dictionary<int, bool>(_foldOutContainer);

            foreach (var foldOutContainerKey in foldOutContainer.Keys)
            {
                _foldOutContainer[foldOutContainerKey] = display;
            }
        }
    }
}
