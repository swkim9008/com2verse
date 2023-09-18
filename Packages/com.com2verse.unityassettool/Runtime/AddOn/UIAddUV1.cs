using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/UIAddUV1", 16)]
    /// <summary>
    /// An IVertexModifier which sets the raw vertex position into UV1 of the generated verts.
    /// </summary>
    public class UIAddUV1 : BaseMeshEffect
    {
        protected UIAddUV1()
        { }


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
            List<UIVertex> vertexList = new List<UIVertex>();
            _vertexHelper.GetUIVertexStream(vertexList);
            Rect meshRect = GetMeshRect(vertexList);        //바운더리 계산

            UIVertex vert = new UIVertex();
            for (int i = 0; i < _vertexHelper.currentVertCount; i++)
            {
                _vertexHelper.PopulateUIVertex(ref vert, i);

                float u = Mathf.Lerp(0, 1, ((vert.position.x - meshRect.xMin) / meshRect.width));
                float v = Mathf.Lerp(0, 1, ((vert.position.y - meshRect.yMin) / meshRect.height));

                vert.uv1 = new Vector2(u, v);
                
                _vertexHelper.SetUIVertex(vert, i);
            }
        }
    }
}