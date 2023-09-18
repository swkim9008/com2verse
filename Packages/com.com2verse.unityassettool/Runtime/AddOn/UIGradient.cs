using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


[AddComponentMenu("UI/Effects/UIGradient")]
[RequireComponent(typeof (Graphic))]
public class UIGradient : BaseMeshEffect
{
    public enum GradientMode { Global, Local }

    public enum GradientDir
    {
        Vertical,
        Horizontal,
        DiagonalLeftToRight,
        DiagonalRightToLeft
        //Free
    }

    public enum Blend
    {
        Override,
        Add,
        Multiply
    }

    [SerializeField]
    private GradientMode gradientMode = GradientMode.Global;

    [SerializeField]
    private GradientDir gradientDir = GradientDir.Vertical;

    [SerializeField]
    private Blend blendMode = Blend.Multiply;

    //public bool overwriteAllColor = false;
    public Color color1 = Color.white;
    public Color color2 = Color.black;

    [SerializeField]
    [Range(-1, 1)]
    private float offset = 0f;

    private Graphic targetGraphic;
    private Rect meshRect = new Rect();

    //bool isText = false;

    //[SerializeField]
    //UnityEngine.Gradient _effectGradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) } };
    #region Properties
    public Blend BlendMode { get { return blendMode; } set { blendMode = value; } }
    public float Offset  { get { return offset; } set { offset = value; } }
    #endregion


    protected override void Start()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    public void Refresh() => this.targetGraphic?.SetVerticesDirty();

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
        if (!IsActive() || _vertexHelper.currentVertCount == 0) return;

        List<UIVertex> vertexList = new List<UIVertex>();
        _vertexHelper.GetUIVertexStream(vertexList);

        this.meshRect = GetMeshRect(vertexList);        //바운더리 계산

        UIVertex uiVertex = new UIVertex();

        for (int i = 0; i < _vertexHelper.currentVertCount; i++)
        {
            _vertexHelper.PopulateUIVertex(ref uiVertex, i);

            if (gradientMode == GradientMode.Global)
            {
                //if (gradientDir == GradientDir.DiagonalLeftToRight || gradientDir == GradientDir.DiagonalRightToLeft)
                //{
                //#if UNITY_EDITOR
                //    Debug.LogWarning("Diagonal dir is not supported in Global mode");
                //#endif
                //    gradientDir = GradientDir.Vertical;
                //    Offset = 0f;
                //}

                switch (gradientDir)
                {
                    case GradientDir.Vertical:
                        { 
                            uiVertex.color = BlendColor(uiVertex.color, Color.Lerp(color2, color1, ((uiVertex.position.y - this.meshRect.yMin) / this.meshRect.height) - Offset));
                        }
                        break;
                    case GradientDir.Horizontal:
                        {
                            uiVertex.color = BlendColor(uiVertex.color, Color.Lerp(color1, color2, ((uiVertex.position.x - this.meshRect.xMin) / this.meshRect.width) - Offset));
                        }
                        break;
                    case GradientDir.DiagonalLeftToRight:
                        {
                            uiVertex.color *= (i % 4 == 0) ? color1 : ((i - 2) % 4 == 0 ? color2 : Color.Lerp(color2, color1, 0.5f - Offset));
                        }
                        break;
                    case GradientDir.DiagonalRightToLeft:
                        {
                            uiVertex.color *= ((i - 1) % 4 == 0) ? color1 : ((i - 3) % 4 == 0 ? color2 : Color.Lerp(color2, color1, 0.5f - Offset));
                        }
                        break;
                }
            }
            else
            {
                switch (gradientDir)
                {
                    case GradientDir.Vertical:
                        {
                            uiVertex.color = BlendColor(uiVertex.color, (i % 4 == 0 || (i - 1) % 4 == 0) ? color1 : color2);
                        }
                        break;
                    case GradientDir.Horizontal:
                        {
                            uiVertex.color = BlendColor(uiVertex.color, (i % 4 == 0 || (i - 3) % 4 == 0) ? color1 : color2);
                        }
                        break;
                    case GradientDir.DiagonalLeftToRight:
                        {
                            uiVertex.color *= (i % 4 == 0) ? color1 : ((i - 2) % 4 == 0 ? color2 : Color.Lerp(color2, color1, 0.5f - Offset));
                        }
                        break;
                    case GradientDir.DiagonalRightToLeft:
                        {
                            uiVertex.color *= ((i - 1) % 4 == 0) ? color1 : ((i - 3) % 4 == 0 ? color2 : Color.Lerp(color2, color1, 0.5f - Offset));
                        }
                        break;
                }
            }

            _vertexHelper.SetUIVertex(uiVertex, i);
        }
    }

    //private bool CompareCarefully(Color col1, Color col2)
    //{
    //    if (Mathf.Abs(col1.r - col2.r) < 0.003f && Mathf.Abs(col1.g - col2.g) < 0.003f && Mathf.Abs(col1.b - col2.b) < 0.003f && Mathf.Abs(col1.a - col2.a) < 0.003f)
    //        return true;
    //    return false;
    //}


    private Color BlendColor(Color colorA, Color colorB)
    {
        switch (this.BlendMode)
        {
            default: return colorB;
            case Blend.Add: return colorA + colorB;
            case Blend.Multiply: return colorA * colorB;
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(UIGradient))]
[CanEditMultipleObjects]
public class UIGradientEditor : Editor
{
    SerializedProperty gradientMode;
    SerializedProperty gradientDir;
    SerializedProperty blendMode;

    SerializedProperty color1;
    SerializedProperty color2;

    SerializedProperty offset;
  
    void OnEnable()
    {
        gradientMode = serializedObject.FindProperty("gradientMode");
        gradientDir = serializedObject.FindProperty("gradientDir");
        
        blendMode = serializedObject.FindProperty("blendMode");
        color1 = serializedObject.FindProperty("color1");
        color2 = serializedObject.FindProperty("color2");

        offset = serializedObject.FindProperty("offset");
    }

    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(gradientMode);
        EditorGUILayout.PropertyField(gradientDir);
        EditorGUILayout.PropertyField(blendMode);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(color1);
        EditorGUILayout.PropertyField(color2);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Swap", GUILayout.Width(50)))
        {
            Color tmp = color1.colorValue;
            color1.colorValue = color2.colorValue;
            color2.colorValue = tmp;
        };
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(offset);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif



/*바운더리 계산*/
//float top = vertexList[0].position.y;
//float bottom = vertexList[0].position.y;
//float tmpY = 0f;

//float left = vertexList[0].position.x;
//float right = vertexList[0].position.x;
//float tmpX = 0f;

//for (int j = 1; j < vertexList.Count; j++)
//{
//    tmpX = vertexList[j].position.x;
//    if (tmpX > right) { right = tmpX; }
//    else if (tmpX < left) { left = tmpX; }

//    tmpY = vertexList[j].position.y;
//    if (tmpY > top) { top = tmpY; }
//    else if (tmpY < bottom) { bottom = tmpY; }
//}

//float uiElementHeight = top - bottom;
//float uiElementWidth = right - left;

//if (gradientMode == GradientMode.Global)
//{

//    if (gradientDir == GradientDir.DiagonalLeftToRight || gradientDir == GradientDir.DiagonalRightToLeft)
//    {
//        #if UNITY_EDITOR
//        Debug.LogError("Diagonal dir is not supported in Global mode");
//        #endif
//        gradientDir = GradientDir.Vertical;
//    }

//    float left = vertexList[0].position.x;
//    float right = vertexList[0].position.x;
//    float x = 0f;

//    for (int i = 1; i < vertexList.Count; i++)
//    {
//        x = vertexList[i].position.x;
//        if (x > right) { right = x; }
//        else if (x < left) { left = x; }
//    }

//    float bottom = vertexList[0].position.y;
//    float top = vertexList[0].position.y;
//    float y = 0f;

//    for (int i = 1; i < vertexList.Count; i++)
//    {
//        y = vertexList[i].position.y;
//        if (y > top) { top = y; }
//        else if (y < bottom) { bottom = y; }
//    }


//    float uiElementHeight = top - bottom;
//    float uiElementWidth = right - left;

//    for (int i = 0; i < _vertexHelper.currentVertCount; i++)
//    {
//        _vertexHelper.PopulateUIVertex(ref uiVertex, i);

//        //if (!overwriteAllColor && startVertex.color != targetGraphic.color) continue;

//        switch (gradientDir)
//        {
//            case GradientDir.Vertical:
//                //uiVertex.color = uiVertex.color * Color.Lerp(color2, color1, ((uiVertex.position.y - bottomY) / uiElementHeight) - Offset);
//                uiVertex.color = BlendColor(uiVertex.color,  Color.Lerp(color2, color1, ((uiVertex.position.y - bottom) / uiElementHeight) - Offset));
//                break;
//            case GradientDir.Horizontal:
//                //uiVertex.color = uiVertex.color * Color.Lerp(color1, color2, (uiVertex.position.x - left) / uiElementWidth);
//                uiVertex.color = BlendColor(uiVertex.color, Color.Lerp(color1, color2, ((uiVertex.position.x - left) / uiElementWidth) - Offset));
//                break;
//        }

//        _vertexHelper.SetUIVertex(uiVertex, i);
//    }
//}
//else
//{








//enum color mode Additive, Multiply, Overwrite



//public Color32 topColor = Color.white;
//public Color32 bottomColor = Color.black;

//public override void ModifyMesh(VertexHelper helper)
//{
//    if (!IsActive() || helper.currentVertCount == 0)
//        return;

//    List<UIVertex> vertices = new List<UIVertex>();
//    helper.GetUIVertexStream(vertices);

//    float bottomY = vertices[0].position.y;
//    float topY = vertices[0].position.y;

//    for (int i = 1; i < vertices.Count; i++)
//    {
//        float y = vertices[i].position.y;
//        if (y > topY)
//        {
//            topY = y;
//        }
//        else if (y < bottomY)
//        {
//            bottomY = y;
//        }
//    }

//    float uiElementHeight = topY - bottomY;

//    UIVertex v = new UIVertex();

//    for (int i = 0; i < helper.currentVertCount; i++)
//    {
//        helper.PopulateUIVertex(ref v, i);
//        v.color = Color32.Lerp(bottomColor, topColor, (v.position.y - bottomY) / uiElementHeight);
//        helper.SetUIVertex(v, i);
//    }
//}

//float bottomY = gradientDir == GradientDir.Vertical ? vertexList[vertexList.Count - 1].position.y : vertexList[vertexList.Count - 1].position.x;
//float topY = gradientDir == GradientDir.Vertical ? vertexList[0].position.y : vertexList[0].position.x;

//public UnityEngine.Gradient EffectGradient
//{
//    get { return _effectGradient; }
//    set { _effectGradient = value; }
//}

//public Type GradientType
//{
//    get { return _gradientType; }
//    set { _gradientType = value; }
//}
