using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Effects/UISkew")]
[RequireComponent(typeof(Graphic))]
public class UISkew : BaseMeshEffect
{
    public enum SkewDir
    {
        Horizontal = 0,
        Vertical = 1,
        
    }

    [SerializeField] private SkewDir skewDir = SkewDir.Horizontal;
    [SerializeField] private float skewValue = 0f;
    [SerializeField] float widthOffset = 0;
    [SerializeField] float positionOffset = 0;

    private Graphic targetGraphic;
    [SerializeField] Rect meshRect = new Rect();

    #region Properties

    public float SkewValue { get { return skewValue; } set { skewValue = value; } }
    public float WidthOffset { get { return widthOffset; } set { widthOffset = value; } }
    public float PositionOffset { get { return positionOffset; } set { positionOffset = value; } }
    #endregion


    protected override void Start()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    /*버텍스리스트 바운더리 계산*/
    public Rect GetMeshRect(List<UIVertex> _vertexList)
    {
        float yMax = _vertexList[0].position.y;
        float yMin = _vertexList[0].position.y;
        float tmpY = 0f;

        float xMin = _vertexList[0].position.x;
        float xMax = _vertexList[0].position.x;
        float tmpX = 0f;

        for (int j = 1; j < _vertexList.Count; j++)
        {
            tmpX = _vertexList[j].position.x;
            if (tmpX > xMax) { xMax = tmpX; }
            else if (tmpX < xMin) { xMin = tmpX; }

            tmpY = _vertexList[j].position.y;
            if (tmpY > yMax) { yMax = tmpY; }
            else if (tmpY < yMin) { yMin = tmpY; }
        }

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }


    public override void ModifyMesh(VertexHelper _vertexHelper)
    {
        if (!IsActive() || _vertexHelper.currentVertCount == 0)
        {
            return;
        }

        List<UIVertex> vertexList = new List<UIVertex>();
        _vertexHelper.GetUIVertexStream(vertexList);

        this.meshRect = GetMeshRect(vertexList);        //바운더리 계산

        UIVertex uiVertex = new UIVertex();

        for (int i = 0; i < _vertexHelper.currentVertCount; i++)
        {
            _vertexHelper.PopulateUIVertex(ref uiVertex, i);

            if (this.skewDir == SkewDir.Horizontal)
            {
                float rate = (uiVertex.position.y - meshRect.yMin) / this.meshRect.height;
                float value = Mathf.Lerp(0, SkewValue, rate) + PositionOffset;
                
                uiVertex.position.x += 0 < uiVertex.position.x ? value + WidthOffset: value - WidthOffset;
            }
            else if (this.skewDir == SkewDir.Vertical)
            {
                float rate = (uiVertex.position.x - this.meshRect.xMin) / this.meshRect.width;
                float value = Mathf.Lerp(0, SkewValue, rate) + PositionOffset;

                uiVertex.position.y += 0 < uiVertex.position.y ? value + WidthOffset: value - WidthOffset;
            }

            _vertexHelper.SetUIVertex(uiVertex, i);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UISkew))]
[CanEditMultipleObjects]
public class UISkewEditor : Editor
{
    SerializedProperty skewDir;
    SerializedProperty skewValue;
    SerializedProperty widthOffset;
    SerializedProperty positionOffset;

    SerializedProperty meshRect;

    private bool useAngle;
    private float skewAngle;

    void OnEnable()
    {
        skewDir = serializedObject.FindProperty("skewDir");
        skewValue = serializedObject.FindProperty("skewValue");
        widthOffset = serializedObject.FindProperty("widthOffset");
        positionOffset = serializedObject.FindProperty("positionOffset");

        meshRect = serializedObject.FindProperty("meshRect");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(skewDir);

        this.useAngle = EditorGUILayout.Toggle("Use Angle", useAngle);
        EditorGUILayout.BeginHorizontal();
        if (this.useAngle)
        {
            this.skewAngle = EditorGUILayout.Slider("Skew Angle", this.skewAngle, -89, 89);

            float triHeight = this.skewDir.enumValueIndex == 0 ? this.meshRect.rectValue.height : this.meshRect.rectValue.width;

            this.skewValue.floatValue = triHeight / Mathf.Tan(Mathf.Deg2Rad * (90 - this.skewAngle));
        }
        else
        {
            EditorGUILayout.PropertyField(skewValue);
        }
        if (GUILayout.Button("Reverse", GUILayout.Width(100)))
        {
            this.skewValue.floatValue *= -1;
            this.skewAngle *= -1;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(widthOffset);

        float limit = this.skewDir.enumValueIndex == 0 ? - this.meshRect.rectValue.width * 0.5f : -this.meshRect.rectValue.height * 0.5f; 

        if (widthOffset.floatValue < limit)
        {
            widthOffset.floatValue = limit;
        }


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(positionOffset);
        if (GUILayout.Button("Center Align", GUILayout.Width(100)))
        {
            this.positionOffset.floatValue = -this.skewValue.floatValue * 0.5f;
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif



//커스텀 에디터에 붙여야하는데 좀 애매하구먼...괜찮겠지 뭐...^^;;;
//if (this.useAngle)
//{
//    SkewValue = this.meshRect.height / Mathf.Tan(Mathf.Deg2Rad * (90 - skewAngle));
//}
//else
//{
//    SkewAngle = Mathf.Tan(skewValue / this.meshRect.height) * Mathf.Rad2Deg;
//}

//[System.Serializable]
//public class UISkewProperties
//{
//    public enum SkewDir
//    {
//        Vertical,
//        Horizontal,
//    }

//    [SerializeField]
//    private SkewDir skewDir = SkewDir.Horizontal;

//    public bool centerAlign = false;

//    [SerializeField]
//    //[Range(-1, 1)]
//    private float _skewValue = 0f;

//    [SerializeField]
//    private bool _useAngle = false;

//    [SerializeField]
//    private float _skewAngle = 0f;

//    public bool reverse = false;

//    [SerializeField]
//    float _skewOffset = 0;
//    [SerializeField]
//    float _positionOffset = 0;

//    private Graphic targetGraphic;


//    #region Properties
//    public bool UseAngle { get { return _useAngle; } set { _useAngle = value; } }

//    public float SkewValue { get { return _skewValue; } set { _skewValue = value; } }
//    public float SkewAngle { get { return _skewAngle; } set { _skewAngle = value; } }
//    public float SkewOffset { get { return _skewOffset; } set { _skewOffset = value; } }

//    public float PositionOffset { get { return _positionOffset; } set { _positionOffset = value; } }
//    #endregion
//}