using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Effects/UITaper")]
[RequireComponent(typeof (Graphic))]
public class UITaper : BaseMeshEffect
{
    public enum TaperDir
    {
        top = 0,
        bottom = 1,
        left = 2,
        right = 4
    }

    [SerializeField] private TaperDir taperDir = TaperDir.top;
    [SerializeField] private float taperValue = 0f;

    public bool usePercent = false;
    [SerializeField] private float _taperPercent = 0f;

    private Graphic targetGraphic;
    [SerializeField] Rect meshRect = new Rect();

    #region Properties
    public float TaperValue  { get { return taperValue; } set { taperValue = value; } }
    //public float TaperPercent { get { return _taperPercent; } set { _taperPercent = value; } }
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
            float rate = 0;
            float value = 0;

            switch (this.taperDir)
            {
                case TaperDir.top:
                    rate = (uiVertex.position.y - this.meshRect.yMin) / this.meshRect.height;
                    value = Mathf.Lerp(0, TaperValue, rate);

                    uiVertex.position.x += 0 < uiVertex.position.x ? -value : value;

                    break;
                case TaperDir.bottom:
                    rate = (this.meshRect.yMax - uiVertex.position.y) / this.meshRect.height;
                    value = Mathf.Lerp(0, TaperValue, rate);

                    uiVertex.position.x += 0 < uiVertex.position.x ? -value : value;

                    break;
                case TaperDir.left:
                    rate = (this.meshRect.xMax - uiVertex.position.x) / this.meshRect.width;
                    value = Mathf.Lerp(0, TaperValue, rate);

                    uiVertex.position.y += 0 < uiVertex.position.y ? -value : value;

                    break;
                case TaperDir.right:
                    rate = (uiVertex.position.x - this.meshRect.xMin) / this.meshRect.width;
                    value = Mathf.Lerp(0, TaperValue, rate);

                    uiVertex.position.y += 0 < uiVertex.position.y ? -value : value;
                    break;
                default:
                    break;
            }

            _vertexHelper.SetUIVertex(uiVertex, i);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UITaper))]
[CanEditMultipleObjects]
public class UITaperEditor : Editor
{
    SerializedProperty taperDir;
    SerializedProperty taperValue;

    SerializedProperty meshRect;

    private bool usePercent;
    private float taperPercent;

    void OnEnable()
    {
        taperDir = serializedObject.FindProperty("taperDir");
        taperValue = serializedObject.FindProperty("taperValue");

        meshRect = serializedObject.FindProperty("meshRect");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(taperDir);

        EditorGUILayout.PropertyField(taperValue);

        float limit = this.taperDir.enumValueIndex < 2 ? this.meshRect.rectValue.width * 0.5f : this.meshRect.rectValue.height * 0.5f;

        if (taperValue.floatValue > limit)
        {
            taperValue.floatValue = limit;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif