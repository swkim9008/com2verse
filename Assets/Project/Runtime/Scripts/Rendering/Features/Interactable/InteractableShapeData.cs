/*===============================================================
* Product:		Com2Verse
* File Name:	InteractableShapeData.cs
* Developer:	ljk
* Date:			2022-08-09 10:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.Rendering.Interactable
{
	public enum eShapeRenderType
	{
		MESH,
		LINE,
		BOTH
	}

	public enum eShapeEvaluateType
	{
		IMMEDIATE,
		LERP
	}

	public enum eEffectState
	{
		CONNECTING,
		DISCONNECTING,
		PING,
		DEFAULT,
		MAX
	}
	
	public class Boundary
 	{
 		private readonly int BOUND_VERTICES = 32;
 		private static Vector3[] _preCalculatedOffsets;
 		private float _index;
 		
 		public TransformNode _target;
 		public Vector4[] _basePointVector4s;
 		public Vector3[] _basePointVector3s;
 		
 		public Boundary(TransformNode target,float index,Transform effector)
 		{
 			_target = target;
 			_basePointVector4s = new Vector4[BOUND_VERTICES];
 			_basePointVector3s = new Vector3[BOUND_VERTICES];
 			_index = index;
 			
 			// 매 프레임, 매 바운더리마다 계산을 피하기 위해 원 버텍스 값 캐싱
 			if (_preCalculatedOffsets == null)
 			{
 				_preCalculatedOffsets = new Vector3[BOUND_VERTICES];
 				for (int i = 0; i < BOUND_VERTICES; i++)
 					_preCalculatedOffsets[i] = new Vector3(Mathf.Cos(Mathf.PI * (i / (float)BOUND_VERTICES) * 2f), 0,
 						Mathf.Sin(Mathf.PI * (i / (float)BOUND_VERTICES) * 2f));
 			}
 			
 			RefreshBounds();
 		}
 		public void RefreshBounds()
 		{
 			for(int i = 0; i < _basePointVector3s.Length; i++)
 			{
 				_basePointVector3s[i] = _target.Position + _preCalculatedOffsets[i]*_target._size;
 				_basePointVector4s[i] = _basePointVector3s[i];
 				_basePointVector4s[i].w = _index;
 			}
 		}
 	}
 
 	public class TransformNode
 	{
 		public const float NODE_MIN_SIZE = 0.05f;
 		public Transform _center;
 		public Vector3 _moveTarget;
 		public bool _isEvaluatedByController = false;
 		public float _evaluationScore;
        public float _size = NODE_MIN_SIZE;
 		public Vector3 Position
 		{
 			get
 			{
 				if (_isEvaluatedByController || _center == null)
 					return _moveTarget;
 				else
 					return _center.position;
 			}
 		}
 	}
    
    public interface iTransformConnection<T> where T : ConnectionNode
    {
	    public T Connect(Transform connectorTransform);
	    public void Disconnect(T connector);

	    public void DestroyConnection();

	    public void InitConnection();

	    public void SetEffectState(eEffectState effectState);
    }

    public abstract class ConnectionNode : IDisposable
    {
	    public TransformNode _bodyVolume; 
	    public TransformNode _connectTarget;
	    public float _weight;
	    public abstract TransformNode GetBody();
	    public abstract void SetTarget(TransformNode target);

	    public abstract void SetWeight(float weight);

	    public abstract bool Evaluate();

	    public virtual void Dispose()
	    {
		    _bodyVolume = null;
		    _connectTarget = null;
	    }
    }
}
