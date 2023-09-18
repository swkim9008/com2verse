// from http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
// 포럼에서 주워다가 폐곡선 옵션 추가

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Effects/UILineRendererSimple")]
[RequireComponent(typeof(CanvasRenderer))]
public class UILineRendererSimple : MaskableGraphic
{
    public enum eShapeType
    {
        FreeLine,
        AlignedLine,
        Rect,
        Circle,
    }
    public enum eAlignType { Left, Right, Up, Down }
    public enum eUVType { Line, uSingle, uTile , vTile }
    private enum eSegmentType { Start, Middle, End, }

    [SerializeField]
    Texture m_Texture;
    [SerializeField]
    Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);



    public eShapeType lineType = eShapeType.Rect;
    public eAlignType alignType = eAlignType.Down;
    public bool isClose;    //폐곡선
    public Vector3[] Points = new Vector3[] { };
    public float lineThickness = 10;
    public float lineOffset = 0;
    [Range(0, 360)]
    public int angleOffset = 0;

    public eUVType uvType = eUVType.uSingle;

    private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;
    private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0, 1);
    private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
    private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
    private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1, 0);
    private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1, 1);

    private static readonly Vector2[] startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };
    private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
    private static readonly Vector2[] endUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };

    private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    /// <summary>
    /// Texture to be used.
    /// </summary>
    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value) return;
            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// UV rectangle used by the texture.
    /// </summary>
    public Rect uvRect
    {
        get
        {
            return m_UVRect;
        }
        set
        {
            if (m_UVRect == value) return;
            m_UVRect = value;
            SetVerticesDirty();
        }
    }


    void SetPoints(eShapeType _lineType)
    {
        Rect rect = this.rectTransform.rect;

        switch (_lineType)
        {
            case eShapeType.FreeLine:
                break;
            case eShapeType.AlignedLine:
                {
                    Vector3 v1 = new Vector3();
                    Vector3 v2 = new Vector3();

                    switch (this.alignType)
                    {
                        case eAlignType.Up:
                            {
                                v1.x = rect.xMin;
                                v1.y = rect.yMax + this.lineOffset;
                                v2.x = rect.xMax;
                                v2.y = rect.yMax + this.lineOffset;
                            }
                            break;
                        case eAlignType.Down:
                            {
                                v1.x = rect.xMin;
                                v1.y = rect.yMin + this.lineOffset;
                                v2.x = rect.xMax;
                                v2.y = rect.yMin + this.lineOffset;
                            }
                            break;
                        case eAlignType.Left:
                            {
                                v1.x = rect.xMin + this.lineOffset;
                                v1.y = rect.yMax;
                                v2.x = rect.xMin + this.lineOffset;
                                v2.y = rect.yMin;
                            }
                            break;
                        case eAlignType.Right:
                            {
                                v1.x = rect.xMax + this.lineOffset;
                                v1.y = rect.yMax;
                                v2.x = rect.xMax + this.lineOffset;
                                v2.y = rect.yMin;
                            }
                            break;
                    }

                    this.Points = new Vector3[2] { v1, v2 };
                    this.isClose = false;
                }
                break;
            case eShapeType.Rect:
                {
                    Vector2 leftDown = new Vector2(rect.xMin - this.lineOffset, rect.yMin - this.lineOffset);
                    Vector2 rightDown = new Vector2(rect.xMax + this.lineOffset, rect.yMin - this.lineOffset);
                    Vector2 rightUp = new Vector2(rect.xMax + this.lineOffset, rect.yMax + this.lineOffset);
                    Vector2 LeftUp = new Vector2(rect.xMin - this.lineOffset, rect.yMax + this.lineOffset);

                    this.Points = new Vector3[4] { leftDown, rightDown, rightUp, LeftUp };
                    this.isClose = true;
                }
                break;
            case eShapeType.Circle:
                {
                    float step = 360f / this.Points.Length;

                    for (int i = 0; i < this.Points.Length; i++)
                    {
                        float radian = Mathf.Deg2Rad * step * i + Mathf.Deg2Rad * this.angleOffset;
                        float x = Mathf.Cos(radian) * (rect.width * 0.5f + this.lineOffset);
                        float y = Mathf.Sin(radian) * (rect.height * 0.5f + this.lineOffset);

                        this.Points[i] = new Vector3(x, y, 0);
                    }
                    this.isClose = true;
                }
                break;
            default:
                break;
        }
    }


    protected override void OnPopulateMesh(VertexHelper _vertexHelper)
    {
        SetPoints(this.lineType);

        if (this.Points == null || this.Points.Length < 1) return;
        _vertexHelper.Clear();

        List<UIVertex[]> uiVertexList = new List<UIVertex[]>();   //쿼드를 만들기 위해 4개씩 묶음으로 만든다.

        for (var i = 1; i < this.Points.Length; i++)
        {
            Vector3 start = this.Points[i - 1];  //세그먼트 시작점 0 1 2
            Vector3 end = this.Points[i];        //세그먼트 끝점 1 2 3

            uiVertexList.Add(CreateLineSegment(start, end, eSegmentType.Middle, i - 1));
        }
        

        //폐곡선 처리
        if (this.isClose && 2 < this.Points.Length)
        {
            uiVertexList.Add(CreateLineSegment(this.Points[this.Points.Length - 1], this.Points[0], eSegmentType.Middle, Points.Length - 1));
        }

        for (var i = 0; i < uiVertexList.Count; i++)
        {
            int vCount = isClose ? uiVertexList.Count : uiVertexList.Count - 1;     //닫힌 곡선일 경우 전체 버텍스 카운트 만큼 돈다.

            if (i < vCount)
            {
                int start = i;
                int end = isClose && i == uiVertexList.Count - 1 ? 0 : i + 1;

                var vec1 = uiVertexList[start][1].position - uiVertexList[start][2].position;
                var vec2 = uiVertexList[end][2].position - uiVertexList[end][1].position;

                var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                // Positive sign means the line is turning in a 'clockwise' direction
                var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                // Calculate the miter point
                var miterDistance = this.lineThickness / (2 * Mathf.Tan(angle / 2));
                var miterPointA = uiVertexList[start][2].position - vec1.normalized * miterDistance * sign;
                var miterPointB = uiVertexList[start][3].position + vec1.normalized * miterDistance * sign;

                if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                {
                    uiVertexList[start][2].position = miterPointA;
                    uiVertexList[start][3].position = miterPointB;

                    uiVertexList[end][0].position = miterPointB;
                    uiVertexList[end][1].position = miterPointA;
                }
            }
        }

        //폐곡선 옵션위해 모든 버텍스 정리되고 나서 쿼드 그리기
        foreach (UIVertex[] uIVertices in uiVertexList)
        {
            _vertexHelper.AddUIVertexQuad(uIVertices);
        }

        //좌표 찍어보기
        //for (int j = 0; j < uiVertexList.Count; j++)
        //{
        //    UIVertex[] uIVertexs = uiVertexList[j];
        //    string aa = string.Format(">>{0}:\t {1}\t{2}\t{3}\t{4}", j, uIVertexs[0].position, uIVertexs[1].position, uIVertexs[2].position, uIVertexs[3].position);
        //    Debug.Log(aa);
        //}
    }


    
    /// <summary>
    /// 두 점을 받아서 두께만큼 벌려 네개의 점을 만들어 넘겨준다.
    /// UV 타입에 따라 UV를 생성하여 적용한다.
    /// </summary>
    /// <param name="start">시자점</param>
    /// <param name="end">끝점</param>
    /// <param name="type">Start, Middle, End</param>
    /// <param name="_segmentIndex"></param>
    /// <returns></returns>
    private UIVertex[] CreateLineSegment(Vector3 start, Vector3 end, eSegmentType type, int _segmentIndex)
    {
        Vector2[] uvs = new Vector2[] { };

        switch (this.uvType)
        {
            case eUVType.Line:
                uvs = middleUvs;
                if (type == eSegmentType.Start) { uvs = startUvs; }
                else if (type == eSegmentType.End) { uvs = endUvs; }
                break;
            case eUVType.uSingle:
                { 
                    float uTile = _segmentIndex % this.Points.Length;
                    float width = this.isClose ? 1.0f / this.Points.Length : 1.0f / (this.Points.Length - 1);

                    float uStart = uTile * width * uvRect.width;
                    float uEnd = uStart + width * uvRect.width;

                    Vector2 uv0 = new Vector2(uStart + uvRect.x, uvRect.yMin);
                    Vector2 uv1 = new Vector2(uStart + uvRect.x, uvRect.yMax);
                    Vector2 uv2 = new Vector2(uEnd + uvRect.x, uvRect.yMax);
                    Vector2 uv3 = new Vector2(uEnd + uvRect.x, uvRect.yMin);
                
                    uvs = new[] { uv0, uv1, uv2, uv3 };
                    //Debug.Log(uTile + "::" + uTile * width);
                }
                break;
            case eUVType.uTile:
                {
                    Vector2 uv0 = new Vector2(uvRect.xMin, uvRect.yMin);
                    Vector2 uv1 = new Vector2(uvRect.xMin, uvRect.yMax);
                    Vector2 uv2 = new Vector2(uvRect.xMax, uvRect.yMax);
                    Vector2 uv3 = new Vector2(uvRect.xMax, uvRect.yMin);

                    uvs = new[] { uv0, uv1, uv2, uv3 };
                }
                break;
            case eUVType.vTile:
                {
                    Vector2 uv0 = new Vector2(uvRect.xMin, uvRect.yMax);
                    Vector2 uv1 = new Vector2(uvRect.xMax, uvRect.yMax);
                    Vector2 uv2 = new Vector2(uvRect.xMax, uvRect.yMin);
                    Vector2 uv3 = new Vector2(uvRect.xMin, uvRect.yMin);

                    uvs = new[] { uv0, uv1, uv2, uv3 };
                }
                break;
            default:
                break;
        }


        Vector3 offset = new Vector3(start.y - end.y, end.x - start.x, end.z - start.z).normalized * this.lineThickness * 0.5f;
        Vector3 offset2 = new Vector3(start.y - end.y, end.x - start.x, start.z - end.z).normalized * this.lineThickness * 0.5f;

        var v1 = start - offset;
        var v2 = start + offset2;
        var v3 = end + offset2;
        var v4 = end - offset;

        return SetVbo(new Vector3[] { v1, v2, v3, v4 }, uvs);
    }

    public UIVertex[] SetVbo(Vector3[] _vertices, Vector2[] _uvs)
    {
        UIVertex[] vertexsArray = new UIVertex[4];   //4개 점

        for (int i = 0; i < _vertices.Length; i++)
        {
            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = _vertices[i];
            vert.uv0 = _uvs[i];

            vertexsArray[i] = vert;
        }
        return vertexsArray;
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(UILineRendererSimple))]
[CanEditMultipleObjects]
public class UILineRendererSimpleEditor : Editor
{
    SerializedProperty m_Material;
    SerializedProperty m_Texture;
    SerializedProperty m_UVRect;
    SerializedProperty m_Maskable;
    SerializedProperty m_Color;
    SerializedProperty m_RaycastTarget;

    SerializedProperty lineType;
    SerializedProperty alignType;
    SerializedProperty isClose;
    SerializedProperty Points;
    SerializedProperty lineThickness;
    SerializedProperty lineOffset;
    SerializedProperty angleOffset;
    SerializedProperty uvType;

    private GUIStyle buttonStyle;
    private bool useGizmo = false;


    void OnEnable()
    {
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_Texture = serializedObject.FindProperty("m_Texture");
        m_UVRect = serializedObject.FindProperty("m_UVRect");
        m_Maskable = serializedObject.FindProperty("m_Maskable");

        m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        m_RaycastTarget.boolValue = false;      //RaycastTarget은 항상 Off;

        lineType = serializedObject.FindProperty("lineType");

        alignType = serializedObject.FindProperty("alignType");
        isClose = serializedObject.FindProperty("isClose");
        Points = serializedObject.FindProperty("Points");

        lineThickness = serializedObject.FindProperty("lineThickness");
        lineOffset = serializedObject.FindProperty("lineOffset");
        angleOffset = serializedObject.FindProperty("angleOffset");
        uvType = serializedObject.FindProperty("uvType");
    }

    private void OnSceneGUI()
    {
        if (!useGizmo) return;
        UILineRendererSimple t = target as UILineRendererSimple;
        if (t == null || t.Points == null) return;
        
        // grab the center of the parent
        Vector3 center = t.transform.position;
        
        if (t.lineType != UILineRendererSimple.eShapeType.FreeLine) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;

        for (int i = 0; i < t.Points.Length; i++)
        {
            Vector3 pos = t.Points[i] - center;
            Handles.Label(pos, $"{ i }", style);

            //Vector3 tmp = Handles.PositionHandle(t.Points[i] + center, Quaternion.identity);
            //t.Points[i] = tmp - center;

            
            t.Points[i] = Handles.PositionHandle(pos, Quaternion.identity) + center;
        }
        
        t.SetAllDirty();
    }

    //MeshFilter meshFilter;
    
    public override void OnInspectorGUI()
    {
        buttonStyle = new GUIStyle(GUI.skin.button);

        serializedObject.Update();
        
        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(lineType);
        EditorGUILayout.Space(5);

        if (this.lineType.enumValueIndex == 0) //Free Line 일때만 보인다.
        {
            EditorGUILayout.PropertyField(Points);
            if (3 <= this.Points.arraySize)
            {
                EditorGUILayout.PropertyField(isClose);
            }
            useGizmo = GUILayout.Toggle(useGizmo, "USE GIZMO", buttonStyle);


            //this.meshFilter = (MeshFilter)EditorGUILayout.ObjectField("Mesh", this.meshFilter, typeof(MeshFilter) );


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("GET MESH Vertices", buttonStyle)) { TEST(1); };
            if (GUILayout.Button("GET MESH Vertices -z", buttonStyle)) { TEST(-1); };
            EditorGUILayout.EndHorizontal();

        }

        if (this.lineType.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(alignType);
        }

        if (this.lineType.enumValueIndex == 3) //Cricle
        {
            this.Points.arraySize = EditorGUILayout.IntSlider("Segment", this.Points.arraySize, 3, 360);
            EditorGUILayout.PropertyField(angleOffset);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(lineThickness);
        if (this.lineType.enumValueIndex != 0) //Free Line 일때만 보인다.
        {
            EditorGUILayout.PropertyField(lineOffset);
        }
        EditorGUILayout.Space(10);

        

        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_Texture);
        EditorGUILayout.PropertyField(uvType);
        EditorGUILayout.PropertyField(m_UVRect);
        //EditorGUILayout.PropertyField(m_RaycastTarget); //항상 fasle
        EditorGUILayout.PropertyField(m_Maskable);

        serializedObject.ApplyModifiedProperties();
    }




    void TEST(float _zFlip = 1)
    {
        GameObject tmpObj = Selection.activeObject as GameObject;
        if (null == tmpObj) return;

        Mesh mesh = tmpObj.GetComponent<MeshFilter>().sharedMesh;
        //Mesh mesh = this.meshFilter.mesh;
        if (null == mesh) return;

        Vector3[] vertices = mesh.vertices;


        var bottom = vertices.ToList().Where(pos => pos.y < 0).OrderBy(pos => pos.x).ToList();
        var top = vertices.ToList().Where(pos => pos.y > 0).OrderByDescending(pos => pos.x).ToList();


        List<Vector3> sortedList = new List<Vector3>();
        sortedList.AddRange(bottom);
        sortedList.AddRange(top);

        List<Vector3> finalList = new List<Vector3>();
        foreach (Vector3 pos in sortedList)
        {
            Vector3 tmpPos = new Vector3((float)pos.x, (float)pos.y, (float)pos.z * _zFlip);

            finalList.Add(tmpObj.transform.TransformPoint(tmpPos * 100));
        }


        UILineRendererSimple t = target as UILineRendererSimple;
        t.Points = finalList.ToArray();

        t.gameObject.transform.position = tmpObj.transform.position;

        foreach (Vector3 v in finalList)
        {
            Debug.Log(v);
        }
    }


}
#endif
