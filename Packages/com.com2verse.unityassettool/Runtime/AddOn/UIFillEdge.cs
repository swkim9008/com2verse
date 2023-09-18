using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Effects/UIFillEdge")]
[RequireComponent(typeof(Image))]
public class UIFillEdge : BaseMeshEffect
{
    private Image targetImage;
    //private Rect meshRect = new Rect();

    //private List<bool> isFillEdges = new List<bool> { true, true, true, false, true, true, true, true, false };
    [SerializeField] private bool leftBottom = true;
    [SerializeField] private bool leftMiddle = true;
    [SerializeField] private bool leftTop = true;

    [SerializeField] private bool centerBottom = true;
    [SerializeField] private bool centerTop = true;

    [SerializeField] private bool rightBottom = true;
    [SerializeField] private bool rightMiddle = true;
    [SerializeField] private bool rightTop = true;

    [SerializeField] private bool isFit = true;

    protected override void Start()
    {
        this.targetImage = GetComponent<Image>();
    }

    private List<UIVertex> RemoveUIVertex(List<UIVertex> _uIVertices, List<bool> _isFillEdges, bool _fillCenter)
    {
        bool hasShadowMesh = _isFillEdges.Count > 108 ? true : false;

        for (int i = _isFillEdges.Count - 1; -1 < i; i--)
        {
            if (!_isFillEdges[i])
            {
                _uIVertices.RemoveRange(i * 6, 6);
            }
        }

        return _uIVertices;
    }


    public override void ModifyMesh(VertexHelper _vertexHelper)
    {
        this.targetImage ??= GetComponent<Image>();
        if (this.targetImage == null) return;
        if (this.targetImage.type != Image.Type.Sliced) return;
        if (!IsActive() || _vertexHelper.currentVertCount == 0) return;

        List<bool> isFillEdges = new List<bool> { leftBottom, leftMiddle, leftTop, centerBottom, true, centerTop, rightBottom, rightMiddle, rightTop };

        List<UIVertex> vertexList = new List<UIVertex>();
        _vertexHelper.GetUIVertexStream(vertexList);

        _vertexHelper.Clear();  //지우고

        List<UIVertex> newVertexList = RemoveUIVertex(vertexList, isFillEdges, this.targetImage.fillCenter);  //VertexList에서 Edge Tri 제거

        _vertexHelper.AddUIVertexTriangleStream(newVertexList);    //제거된 VertexzList로 Draw

        RectTransform rectTransform = this.transform as RectTransform;
        float xDistance = rectTransform.rect.width / 2.0f;
        float yDistance = rectTransform.rect.height / 2.0f;

        UIVertex uiVertex = new UIVertex();

        if (isFit)
        {
            for (int i = 0; i < _vertexHelper.currentVertCount; i++)
            {
                _vertexHelper.PopulateUIVertex(ref uiVertex, i);
            
                if (!(leftTop || leftMiddle || leftBottom) && uiVertex.position.x < 0) { uiVertex.position.x = -xDistance; }
                if (!(rightTop || rightMiddle || rightBottom) && uiVertex.position.x > 0) { uiVertex.position.x = xDistance; }
                if (!(leftTop || centerTop || rightTop) && uiVertex.position.y > 0) { uiVertex.position.y = yDistance; }
                if (!(leftBottom || centerBottom || rightBottom) && uiVertex.position.y < 0) { uiVertex.position.y = -yDistance; }
         
                _vertexHelper.SetUIVertex(uiVertex, i);
            }
        }
    }

    //private bool CheckUseShadow()
    //{
    //    Shadow shadow = GetComponent<Shadow>();
    //    return (shadow.enabled && shadow != null) ? true : false;
    //}
}


#if UNITY_EDITOR
[CustomEditor(typeof(UIFillEdge))]
[CanEditMultipleObjects]
public class UIFillEdgeEditor : Editor
{
    SerializedProperty leftBottom;
    SerializedProperty leftMiddle;
    SerializedProperty leftTop;

    SerializedProperty centerBottom;
    SerializedProperty centerTop;

    SerializedProperty rightBottom;
    SerializedProperty rightMiddle;
    SerializedProperty rightTop;

    SerializedProperty isFit;

    private GUIStyle buttonStyle;

    void OnEnable()
    {
        leftBottom = serializedObject.FindProperty("leftBottom");
        leftMiddle = serializedObject.FindProperty("leftMiddle");
        leftTop = serializedObject.FindProperty("leftTop");

        centerBottom = serializedObject.FindProperty("centerBottom");
        centerTop = serializedObject.FindProperty("centerTop");

        rightBottom = serializedObject.FindProperty("rightBottom");
        rightMiddle = serializedObject.FindProperty("rightMiddle");
        rightTop = serializedObject.FindProperty("rightTop");

        isFit = serializedObject.FindProperty("isFit");
    }


    public override void OnInspectorGUI()
    {
        buttonStyle = new GUIStyle(GUI.skin.button);
        serializedObject.Update();
        
        EditorGUILayout.BeginHorizontal();
        leftTop.boolValue = GUILayout.Toggle(leftTop.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        centerTop.boolValue = GUILayout.Toggle(centerTop.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        rightTop.boolValue = GUILayout.Toggle(rightTop.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        leftMiddle.boolValue = GUILayout.Toggle(leftMiddle.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        isFit.boolValue = GUILayout.Toggle(isFit.boolValue, "FIT", buttonStyle, GUILayout.Height(40));
        rightMiddle.boolValue = GUILayout.Toggle(rightMiddle.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        leftBottom.boolValue = GUILayout.Toggle(leftBottom.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        centerBottom.boolValue = GUILayout.Toggle(centerBottom.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        rightBottom.boolValue = GUILayout.Toggle(rightBottom.boolValue, "   ", buttonStyle, GUILayout.Height(40));
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("* Image 컴포넌트 바로 밑에 붙여서 사용");
        GUILayout.Label("* Image Type 이 Sliced 때만 동작");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

