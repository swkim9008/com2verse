/*===============================================================
* Product:		Com2Verse
* File Name:	MetaCircle.cs
* Developer:	ljk
* Date:			2022-07-28 11:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Rendering.Utility;
using UnityEngine;

namespace Com2Verse.Rendering.Interactable
{	
	[Serializable]
	public class BlobColorResonance
	{
		public eEffectState _targetState;
		public Color _minColor;
		public Color _maxColor;
		public float _curvePeriod;
		public AnimationCurve _appearanceCurve;
	}

	
	public sealed class Blob : InteractableEffect
	{
		private const int VERTEX_COUNT_BASELINE = 256;
		private List<Boundary> _areaBound = new List<Boundary>();
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;
		private LineRenderer _lineRenderer;
		private Mesh _visibleMesh;
		private Vector3[] _vertexToDraw;
		private List<TransformNode> _hullTarget;
		
		public eShapeRenderType _blobRendererType = eShapeRenderType.MESH;
		public eShapeEvaluateType _blobEvaluateType = eShapeEvaluateType.LERP;
		
		[Range(0,0.4f)][Header("블랍 내부로 향하는 장력")]
		public float _shrinkAmount = 0.05f;

		[Range(0.5f,3f)][Header("장력 계수")]
		public float _shrinkExp = 1.5f;

		[Header("메시 표면 재질")]
		public Material _surfaceMaterial;
		
		[Header(" 블랍 표현 (성능 사용)")]
		public bool _gumBlob = false;
		
		public TransformNode AddTarget(Transform target,float size)
		{
			if (_hullTarget == null)
				_hullTarget = new List<TransformNode>();

			TransformNode transformNode = new TransformNode() { _center = target, _size = size };
			
			_hullTarget.Add(transformNode);

			return transformNode;
		}
		
		public TransformNode AddTarget(Vector3 target,float size)
		{
			if (_hullTarget == null)
				_hullTarget = new List<TransformNode>();

			TransformNode transformNode = new TransformNode() { _moveTarget = target, _size = size , _isEvaluatedByController = true};
			
			_hullTarget.Add(transformNode);

			return transformNode;
		}

		public void SetMaterialRenderQueue(int renderQueue)
		{
			_surfaceMaterial.renderQueue = renderQueue-10;
			if (_lineRenderer != null && _lineRenderer.material != null)
				_lineRenderer.material.renderQueue = renderQueue;
		}
		
		public void RemoveTarget(TransformNode target)
		{
			_hullTarget.Remove(target);
		}

		public override void SetRenderState(bool render)
		{
			if (_render == render)
				return;
			base.SetRenderState(render);

			_meshRenderer.forceRenderingOff = !render;
			if (_lineRenderer != null)
				_lineRenderer.forceRenderingOff = !render;
		}
		
		private void Prepare()
		{
			#region set_renderers

			if (_blobRendererType == eShapeRenderType.MESH || _blobRendererType == eShapeRenderType.BOTH)
			{
				_meshRenderer = gameObject.GetComponent<MeshRenderer>();
				_meshFilter = gameObject.GetComponent<MeshFilter>();
			
				if (_meshRenderer == null)
					_meshRenderer = gameObject.AddComponent<MeshRenderer>();
				if (_meshFilter == null)
					_meshFilter = gameObject.AddComponent<MeshFilter>();

				if (_visibleMesh == null || _visibleMesh.vertexCount != 5+1)
				{
					_visibleMesh = new Mesh();
					_visibleMesh.name = "Generated - MetaCircle";
				}
			
				Vector3[] vertices = new Vector3[5+1];
				int[] triangles = new int[(5+1 )* 3]; // 한면만
				Vector3[] normals = new Vector3[5+1]; // 혹시 빛을 처리할수 있으니 노멀

				vertices[5] =  transform.position; // 마지막 버텍스는 원의 중앙
				normals[5] = Vector3.up; // 평면이므로 다 Up
			
				triangles[5*3] = 5;
				triangles[5*3 + 1] =  1;
				triangles[5*3 + 2] = 0;
			
				for (int i = 0; i < 5; i++)
				{
					Vector3 evalPos = transform.position;
					Vector3 circleNormal = new Vector3(Mathf.Cos(Mathf.PI * 2 * (i / (float)5)), 0,
						Mathf.Sin(Mathf.PI * 2 * (i / (float)5)));
				
					vertices[i] = evalPos + circleNormal;
					normals[i] = Vector3.up;
					if (i == 0)
					{
						triangles[i * 3] = 5;
						triangles[i * 3 + 1] = 0;
						triangles[i * 3 + 2] = 5-1;
					}
					else
					{
						triangles[i * 3] = 5;
						triangles[i * 3 + 1] = i + 1;
						triangles[i * 3 + 2] = i;
					}
				}
				// 허리가 더 잘록해지는 요구사항이 올 경우 , Triangulation 이 별도로 필요할 수 있다.
			
				_visibleMesh.vertices = vertices;
				_visibleMesh.triangles = triangles;
				_visibleMesh.normals = normals;
			
				_meshFilter.mesh = _visibleMesh;
				_meshRenderer.material = _surfaceMaterial;
			}
			_meshRenderer.material = Instantiate(_surfaceMaterial);
			_lineRenderer = gameObject.GetComponent<LineRenderer>();
			if (_blobRendererType == eShapeRenderType.LINE || _blobRendererType == eShapeRenderType.BOTH)
			{
				if (_lineRenderer == null)
					_lineRenderer = gameObject.AddComponent<LineRenderer>();
				_lineRenderer.positionCount = 5;
				_lineRenderer.loop = true;
				
				_lineRenderer.material = Instantiate(_lineRenderer.sharedMaterial);
			}
			else
			{
				if (_lineRenderer != null)
					_lineRenderer.enabled = false;
			}
			
			_meshRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * ShapeUtility.FAR);
			
			_vertexToDraw = new Vector3[5 + 1];

			gameObject.layer = LayerMask.NameToLayer("Object");
			gameObject.FlagRenderingLayerMask(true,RenderStateUtility.eRenderingLayerMask.UNUSED_12);
			
			#endregion set_renderers
		}

		private void MeshDataRecreation(Vector3[] vertices)
		{
			Vector3[] normals = new Vector3[vertices.Length];
			int[] triangles = new int[vertices.Length*3];

			int outersAscenterIndex = vertices.Length - 1;
			triangles[(outersAscenterIndex) * 3] = outersAscenterIndex;
			triangles[(outersAscenterIndex) * 3 + 1] = 1;
			triangles[(outersAscenterIndex) * 3 + 2] = 0;
			
			for (int i = 0; i < vertices.Length-1; i++)
			{
				if (i == 0)
				{
					triangles[i * 3] =  outersAscenterIndex;
					triangles[i * 3 + 1] = 0;
					triangles[i * 3 + 2] = vertices.Length-2;
				}
				else
				{
					triangles[i * 3] = outersAscenterIndex;
					triangles[i * 3 + 1] = i + 1;
					triangles[i * 3 + 2] = i;
				}

				normals[i] = Vector3.up;
			}
			_visibleMesh.Clear();
			_visibleMesh.vertices = vertices;
			_visibleMesh.triangles = triangles;
			_visibleMesh.normals = normals;
		}
		
		public override void Init()
		{
			base.Init();
			// make circle
			Prepare();
		}

		public override void Evaluate(float time)
		{
			if(!_render)
				return;

			if (_hullTarget != null && _hullTarget.Count != _areaBound.Count)
			{
				_areaBound.Clear();
				for (int i = 0; i < _hullTarget.Count; i++)
				{
					_areaBound.Add(new Boundary(_hullTarget[i],i,transform));
				}
			}
			
			if(_hullTarget == null || _hullTarget.Count == 0)
				return;
			
			if (_gumBlob)
			{
				List<Vector4> boundPoints = new List<Vector4>();
				for (int i = 0; i < _areaBound.Count; i++)
				{
					_areaBound[i].RefreshBounds();
					boundPoints.AddRange(_areaBound[i]._basePointVector4s);
				}
				
				List<Vector4> convexHull = ShapeUtility.CalculateConvexHull(boundPoints);
				List<Vector3> smoothHull = ShapeUtility.CalculateSmoothContour(_vertexToDraw[0],convexHull, VERTEX_COUNT_BASELINE, _shrinkAmount,_shrinkExp, true);
				
				//TODO: Lerp 구문의 동작 수정 
				if (_blobEvaluateType == eShapeEvaluateType.LERP)
				{
					_vertexToDraw = smoothHull.ToArray();
				}
				else
					_vertexToDraw = smoothHull.ToArray();

				if (_blobRendererType == eShapeRenderType.MESH || _blobRendererType == eShapeRenderType.BOTH)
				{
					MeshDataRecreation(_vertexToDraw);
				}

				if (_blobRendererType == eShapeRenderType.LINE || _blobRendererType == eShapeRenderType.BOTH)
				{
					_lineRenderer.positionCount = _vertexToDraw.Length - 1;
					_lineRenderer.SetPositions(_vertexToDraw);
				}
					
				
				#if UNITY_EDITOR
				ShapeUtility.DebugPolyLine(convexHull,Color.red,Vector3.up*0.1f,0.01f,true,convexHull.Count-1);
				ShapeUtility.DebugPolyLine(smoothHull,Color.green,Vector3.up*0.15f,0.01f,true,smoothHull.Count-1);
				#endif
				
			}
			else
			{
				List<Vector3> boundPoints = new List<Vector3>();
				for (int i = 0; i < _areaBound.Count; i++)
				{
					_areaBound[i].RefreshBounds();
					boundPoints.AddRange(_areaBound[i]._basePointVector3s);
				}
				Vector3[] convexHull = ShapeUtility.CalculateConvexHull(boundPoints).ToArray();
				MeshDataRecreation(convexHull);
				if (_blobRendererType == eShapeRenderType.BOTH || _blobRendererType == eShapeRenderType.LINE)
				{
					if (!_lineRenderer.enabled)
						_lineRenderer.enabled = true;
					
					_lineRenderer.positionCount = convexHull.Length - 1;
					_lineRenderer.SetPositions(convexHull);
				}
				
			}
		}

		public override void Draw()
		{
			// draw shape
		}

		public void SetMaterialPropertyBlock(MaterialPropertyBlock meshProperty,MaterialPropertyBlock lineProperty)
		{
			_meshRenderer.SetPropertyBlock(meshProperty);
			if(_blobRendererType == eShapeRenderType.BOTH || _blobRendererType == eShapeRenderType.LINE)
				_lineRenderer.SetPropertyBlock(lineProperty);
		}

		private void Update()
		{
			Evaluate(0);
			Draw();
		}
	}
}
