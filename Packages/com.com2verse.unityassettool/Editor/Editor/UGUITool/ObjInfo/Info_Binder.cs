using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Binder = Com2Verse.UI.Binder;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_Binder : IObjInfo
    {
        public GameObject gameObject;
        public Binder binder;
        public Binder[] binders;

        GUIStyle labelStyle = new GUIStyle();
        GUIStyle fieldStyle = new GUIStyle();

        FieldInfo commentFieldInfo = null;
        FieldInfo bindingModeFieldInfo = null;
        FieldInfo targetPathFieldInfo = null;
        FieldInfo sourcePathFieldInfo = null;
        FieldInfo eventPathFieldInfo = null;

        //NavigationBinder
        FieldInfo sequenceListFieldInfo = null;
        Dictionary<Binder, List<NavigationBinder.Sequence>> dicSequenceList = new Dictionary<Binder, List<NavigationBinder.Sequence>>();

        //CollectionBinder
        FieldInfo poolCountFieldInfo = null;
        FieldInfo maxPoolFieldInfo = null;
        FieldInfo prefabFieldInfo = null;
        FieldInfo IsItemOrderedFieldInfo = null;
        FieldInfo IsAscendingOrderFieldInfo = null;
        FieldInfo poolRootFieldInfo = null;
        FieldInfo itemRootFieldInfo = null;

        private bool[] filter;
        public void SetFilter(bool[] _filter) { this.filter = _filter; }    //Data, Command, Navigation, Collection

        private string matchString;


        public Info_Binder(GameObject _obj)
        {
            this.binder = _obj.GetComponent<Binder>();
            this.binders = _obj.GetComponents<Binder>();
            this.gameObject = _obj;

            this.labelStyle.alignment = TextAnchor.MiddleRight;
            this.labelStyle.normal.textColor = Color.gray;

            this.fieldStyle.normal.textColor = Color.white;
            this.fieldStyle.richText = true;

            this.commentFieldInfo = typeof(Binder).GetField("_comment", BindingFlags.NonPublic | BindingFlags.Instance);
            this.bindingModeFieldInfo = typeof(Binder).GetField("_bindingMode", BindingFlags.NonPublic | BindingFlags.Instance);
            this.targetPathFieldInfo = typeof(Binder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            this.sourcePathFieldInfo = typeof(Binder).GetField("_sourcePath", BindingFlags.NonPublic | BindingFlags.Instance);
            this.eventPathFieldInfo = typeof(Binder).GetField("_eventPath", BindingFlags.NonPublic | BindingFlags.Instance);

            this.sequenceListFieldInfo = typeof(NavigationBinder).GetField("_sequenceList", BindingFlags.NonPublic | BindingFlags.Instance);
            this.dicSequenceList.Clear();

            this.poolCountFieldInfo = typeof(CollectionBinder).GetField("_poolCount", BindingFlags.NonPublic | BindingFlags.Instance);
            this.maxPoolFieldInfo = typeof(CollectionBinder).GetField("_maxPoolCount", BindingFlags.NonPublic | BindingFlags.Instance);
            this.prefabFieldInfo = typeof(CollectionBinder).GetField("_prefab", BindingFlags.NonPublic | BindingFlags.Instance);
            this.IsItemOrderedFieldInfo = typeof(CollectionBinder).GetField("_isItemOrdered", BindingFlags.NonPublic | BindingFlags.Instance);
            this.IsAscendingOrderFieldInfo = typeof(CollectionBinder).GetField("_isAscendingOrder", BindingFlags.NonPublic | BindingFlags.Instance);
            this.poolRootFieldInfo = typeof(CollectionBinder).GetField("_poolRoot", BindingFlags.NonPublic | BindingFlags.Instance);
            this.itemRootFieldInfo = typeof(CollectionBinder).GetField("_itemRoot", BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var binder in binders)
            {
                if (binder is NavigationBinder)
                {
                    List<NavigationBinder.Sequence> _ = (List<NavigationBinder.Sequence>)this.sequenceListFieldInfo.GetValue(binder);
                    this.dicSequenceList.Add(binder, _);
                }
            }
        }

        public string GetDescription()
        {
            return "Binder";
        }

        public void Render()
        {
            if (this.binders.Length < 1) return;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.ObjectField(this.binder.gameObject, typeof(GameObject), true, GUILayout.Width(250));
            EditorGUILayout.LabelField($"Binder Count: {this.binders.Length}  ", this.labelStyle, GUILayout.Width(250));
            EditorGUILayout.EndVertical();

            foreach (Binder binder in binders)
            {
                switch (binder)
                {
                    case DataBinder dataBinder:
                    {
                        if (this.filter[0] == false) break;
                        if (this.matchString.Length > 3)
                        {
                            if (!(((Binder.BindingPath)targetPathFieldInfo.GetValue(binder)).property).ToLower().Contains(this.matchString.ToLower()) &&
                                !(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).property).ToLower().Contains(this.matchString.ToLower())) break;
                        }


                        EditorGUILayout.BeginVertical(GUILayout.Width(400));
                        GUI.color = Color.white;
                        EditorGUILayout.LabelField(binder.GetType().Name, GUILayout.Width(400));
                        GUI.color = Color.white;
                        string comment = commentFieldInfo != null ? (string)(commentFieldInfo.GetValue(binder)) : String.Empty;
                        EditorGUILayout.TextArea(comment, GUILayout.Height(40), GUILayout.Width(400));

                        if (this.bindingModeFieldInfo != null)
                        {
                            GUI.color = ((Com2Verse.UI.Binder.eBindingMode)(bindingModeFieldInfo.GetValue(binder)) == Binder.eBindingMode.ONE_WAY_TO_TARGET) ? Color.gray: Color.white;
                            EditorGUILayout.EnumPopup((Com2Verse.UI.Binder.eBindingMode)(bindingModeFieldInfo.GetValue(binder)), GUILayout.Width(400));
                            GUI.color = Color.white;
                        }

                        EditorGUILayout.LabelField($"<color=#c0c0c0ff>{(((Binder.BindingPath)targetPathFieldInfo.GetValue(binder)).propertyOwner)}</color>/<b>{(((Binder.BindingPath)targetPathFieldInfo.GetValue(binder)).property)}</b>", this.fieldStyle, GUILayout.Width(400));
                        EditorGUILayout.LabelField($"<color=#c0c0c0ff>{(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).propertyOwner)}</color>/<b>{(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).property)}</b>", this.fieldStyle, GUILayout.Width(400));
                        EditorGUILayout.EndVertical();
                    }
                        break;
                    case CommandBinder commandBinder:
                    {
                        if (this.filter[1] == false) break;
                        if (this.matchString.Length > 3)
                        {
                            if (!(((Binder.BindingPath)eventPathFieldInfo.GetValue(binder)).property).ToLower().Contains(this.matchString.ToLower()) &&
                                !(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).property).ToLower().Contains(this.matchString.ToLower())) break;
                        }

                        EditorGUILayout.BeginVertical(GUILayout.Width(400));
                        GUI.color = Color.cyan;
                        EditorGUILayout.LabelField(binder.GetType().Name, GUILayout.Width(400));
                        GUI.color = Color.white;
                        string comment = commentFieldInfo != null ? (string)(commentFieldInfo.GetValue(binder)) : String.Empty;
                        EditorGUILayout.TextArea(comment, GUILayout.Height(40), GUILayout.Width(400));
                        EditorGUILayout.LabelField($"<color=#c0c0c0ff>{(((Binder.BindingPath)targetPathFieldInfo.GetValue(binder)).propertyOwner)}</color>", this.fieldStyle, GUILayout.Width(400));
                        EditorGUILayout.LabelField($"<b>{(((Binder.BindingPath)eventPathFieldInfo.GetValue(binder)).property)}</b>", this.fieldStyle, GUILayout.Width(400));
                        EditorGUILayout.LabelField($"<color=#c0c0c0ff>{(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).propertyOwner)}</color>/<b>{(((Binder.BindingPath)sourcePathFieldInfo.GetValue(binder)).property)}</b>", this.fieldStyle, GUILayout.Width(400));
                        EditorGUILayout.EndVertical();
                    }
                        break;
                    case NavigationBinder navigationBinder:
                    {
                        if (this.filter[2] == false) break;
                        EditorGUILayout.BeginVertical(GUILayout.Width(400));
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField(binder.GetType().Name, GUILayout.Width(400));
                        GUI.color = Color.white;

                        string comment = commentFieldInfo != null ? (string)(commentFieldInfo.GetValue(binder)) : String.Empty;
                        EditorGUILayout.TextArea(comment, GUILayout.Height(40), GUILayout.Width(400));

                        int sequenceNum = 1;
                        foreach (NavigationBinder.Sequence item in this.dicSequenceList[binder])
                        {
                            EditorGUILayout.BeginHorizontal("box");

                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField($"Sequence {sequenceNum.ToString("D2")}", this.labelStyle, GUILayout.Width(100));
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.EnumPopup(item.commandType, GUILayout.Width(300));
                            if (item.commandType == NavigationBinder.eCommandType.NONE) continue;

                            EditorGUILayout.EnumPopup(item.targetType, GUILayout.Width(300));

                            if (item.targetType == NavigationBinder.eTargetType.OTHER)
                            {
                                var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item.assetReference.AssetGUID), typeof(UnityEngine.Object));
                                EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), false, GUILayout.Width(300));
                            }

                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            sequenceNum++;
                        }
                        EditorGUILayout.EndVertical();
                    }
                        break;
                    case CollectionBinder collectionBinder:
                    {
                        if (this.filter[3] == false) break;
                        EditorGUILayout.BeginVertical(GUILayout.Width(400));
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField(binder.GetType().Name, GUILayout.Width(400));
                        GUI.color = Color.white;

                        EditorGUILayout.TextField("Pool Count", $"{this.poolCountFieldInfo.GetValue(binder)} / {this.maxPoolFieldInfo.GetValue(binder)}", GUILayout.Width(400));
                        var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(((AssetReference)this.prefabFieldInfo.GetValue(binder)).AssetGUID), typeof(UnityEngine.Object));
                        EditorGUILayout.ObjectField("Prefab", asset, typeof(UnityEngine.Object), false, GUILayout.Width(400));

                        EditorGUILayout.Toggle($"Is Item Ordered", (bool)this.IsItemOrderedFieldInfo.GetValue(binder), GUILayout.Width(400));
                        EditorGUILayout.Toggle($"Is Ascending Order", (bool)this.IsAscendingOrderFieldInfo.GetValue(binder), GUILayout.Width(400));

                        EditorGUILayout.ObjectField("Pool Root", (Transform)this.poolRootFieldInfo.GetValue(binder), typeof(Transform), true, GUILayout.Width(400));
                        EditorGUILayout.ObjectField("Item Root", (Transform)this.itemRootFieldInfo.GetValue(binder), typeof(Transform), true, GUILayout.Width(400));
                        EditorGUILayout.EndVertical();
                    }
                        break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        public void MatchString(string str)
        {
            this.matchString = str;
        }
    }
}




//this.commandTypeFieldInfo = typeof(NavigationBinder).GetField("commandType", BindingFlags.NonPublic | BindingFlags.Instance);
//this.targetTypeFieldInfo = typeof(NavigationBinder).GetField("targetType", BindingFlags.NonPublic | BindingFlags.Instance);
//리플렉션 <<<
//FieldInfo fieldInfo = this.binder.GetType().GetField("_bindingMode", BindingFlags.NonPublic | BindingFlags.Instance);
//Debug.Log($">>>>{fieldInfo.Name}");

//foreach (FieldInfo field in fieldInfos)
//{
//    Debug.Log(field.Name);

//    if (field.Name == "_bindingMode")
//    {
//        Debug.Log($">>>>{field.Name}");
//    }
//}
//Debug.Log($"Method --------------------------------------");
//MethodInfo[] methodInfos = this.binder.GetType().GetMethods();
//foreach (MethodInfo method in methodInfos)
//{
//    Debug.Log(method.Name);
//}
//Debug.Log($"Property --------------------------------------");

//this.propertyInfos = this.binder.GetType().GetProperties();
//foreach (PropertyInfo property in propertyInfos)
//{
//    if (property.PropertyType == typeof(Com2Verse.UI.Binder.eBindingMode))
//    {
//        Debug.Log(property.Name);
//    }
//}
