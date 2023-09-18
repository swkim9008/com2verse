/*===============================================================
* Product:		Com2Verse
* File Name:	BlobConnection.cs
* Developer:	ljk
* Date:			2022-08-02 14:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Rendering.Utility;
using UnityEngine;

namespace Com2Verse.Rendering.Interactable
{
	
	// 개별 커넥션 노드
	public class BlobConnector : ConnectionNode
	{
		public TransformNode _headVolume;  // blob Head
		private BlobConnection _mainConnection; 
		private Blob _individualBlob; // 내 바디 형태 렌더링 블랍
		private Transform _targetCache;
		private float _weighTarget;
		private bool _killingReserved = false;
		public BlobConnector(Transform body, BlobConnection mainBound ,float initSize)
		{
			_targetCache = body;
			_individualBlob = mainBound.MakeBlob();
			_bodyVolume = _individualBlob.AddTarget(body, initSize);
			_headVolume = _individualBlob.AddTarget(body.position, initSize);
			_mainConnection = mainBound;
			//_individualBlob.SetMaterialRenderQueue(2999);
			SetWeight(0);
			_weight = 0;
		}

		public override TransformNode GetBody()
		{
			return _bodyVolume;
		}
		
		public override void SetTarget(TransformNode target)
		{
			_connectTarget = target;
		}
		
		/// <summary>
		/// 그룹 또는 개별 연결 정도 ( normalize 된 점수 )
		/// </summary>
		/// <param name="connectivity"></param>
		public override void SetWeight(float weight)
		{
			if (_killingReserved)
				return;
			_weighTarget = Mathf.Clamp01( weight );
		}

		public void KillSmoothly()
		{
			SetWeight(0);
			_killingReserved = true;
		}


		public override bool Evaluate()
		{
			bool dyingOrAlone = _killingReserved || (!_mainConnection._disableMainRenderer && _mainConnection.Connectors == 1 && _weighTarget < 1);
			
			_weight = ShapeUtility.Lerp(_weight, _weighTarget, Mathf.Clamp01(Time.deltaTime * _mainConnection._softColloidMoveSpeed));
			_bodyVolume._size = _mainConnection._bodySizeOverConnection.Evaluate(_weight)*_mainConnection._maxBodyRadius;
			
			Vector3 targetPos = _headVolume.Position;
			float targetHeadSize = _headVolume._size;

			if (_connectTarget == null) // blob 노이즈?
			{
				targetPos = _bodyVolume.Position;
				targetHeadSize = TransformNode.NODE_MIN_SIZE;
			}
			else
			{
				float distanceVal = _mainConnection._distanceOverConnection.Evaluate(_weight);
				targetPos = Vector3.Lerp(_bodyVolume.Position,_connectTarget.Position, distanceVal);
				targetHeadSize = _mainConnection._headSizeOverConnection.Evaluate(_weight) *
				                 _mainConnection._maxHeadRadius;
			}
			
			if (_mainConnection._softColloidMovement)
			{
				// 커넥션 단절이거나, 다른 노드가 단절되어서 혼자 남는 순간인 경우, 수축 커브를 참조한다
				if (dyingOrAlone)
				{
					_headVolume._moveTarget = Vector3.Lerp(_headVolume._moveTarget, targetPos, _mainConnection._retractionCurve.Evaluate(Mathf.Clamp01(Time.deltaTime*_mainConnection._softColloidMoveSpeed)));
					_headVolume._size = ShapeUtility.Lerp(_headVolume._size, targetHeadSize, Time.deltaTime * _mainConnection._softColloidMoveSpeed);	
				}
				else
				{
					_headVolume._moveTarget = Vector3.Lerp(_headVolume._moveTarget, targetPos, Time.deltaTime * _mainConnection._softColloidMoveSpeed);
					_headVolume._size = ShapeUtility.Lerp(_headVolume._size, targetHeadSize, Time.deltaTime * _mainConnection._softColloidMoveSpeed);
				}
			}
			else
			{
				_headVolume._moveTarget = targetPos;
				_headVolume._size = targetHeadSize;
			}

			bool renderOn = _weight < 1 || _mainConnection.Connectors == 1 || !Mathf.Approximately(_headVolume._size, targetHeadSize) || _mainConnection._disableMainRenderer;
			
			_individualBlob.SetRenderState(  renderOn );

			if (_targetCache == null || (_killingReserved && Mathf.Abs(_weight) < 0.01f )) // 타겟 삭제 감지. 혹은 접속종료
			{
				_mainConnection.DisposeNodeFromConnection(this);
				_killingReserved = false;
				SetTarget(null);
				Dispose();
				return false;
			}

			return true;
		}
		
		public override void Dispose()
		{
			// dispose
			base.Dispose();
			_headVolume = null;
			GameObject.Destroy(_individualBlob.gameObject);
		}

		public void SetMaterialPropertyBlock(MaterialPropertyBlock meshProperty,MaterialPropertyBlock lineProperty)
		{
			_individualBlob.SetMaterialPropertyBlock(meshProperty,lineProperty);
		}
	}

	public sealed class BlobConnection : InteractableEffect , iTransformConnection<ConnectionNode>
	{
		private List<BlobConnector> _connectors;
		private List<TransformNode> _blobAreaSnaps;
		private Blob _mainBound;

		private MaterialPropertyBlock _blobMeshMaterialProperty;
		private MaterialPropertyBlock _blobLineMaterialProperty;

		private eEffectState _effectState = eEffectState.DEFAULT;
		private float _resonanceColorRunning = 0;
		private Color _resonanceMinColor;
		private Color _resonanceMaxColor;
		private BlobColorResonance _currentResonance;

		[Header("렌더링 정보 프리팹(Blob)")] public GameObject _blobPrefab;
		
		[Header("헤드(노치) 최대 크기")] public float _maxHeadRadius = 0.5f;
		[Header("바디 최대 크기")] public float _maxBodyRadius = 1f;

		[Header("접속률 당 헤드 변화량")] public AnimationCurve _headSizeOverConnection;
		[Header("접속률 당 바디 변화량")] public AnimationCurve _bodySizeOverConnection;
		[Header("접속률 당 거리 변화량")] public AnimationCurve _distanceOverConnection;
		[Header("접속단절 수축커브")] public AnimationCurve _retractionCurve;

		[Header("영역 변화 부드럽게 처리")] public bool _softColloidMovement;
		[Header("영역 변화 추종성")] public float _softColloidMoveSpeed = 15;

		[Header("주 영역 강제 비활성화(연결 안함)")] public bool _disableMainRenderer = true;
		
		[Header("상황별 영역 컬러 변화 정의")]
		public List<BlobColorResonance> _blobColorResonances = new List<BlobColorResonance>()
		{
			new BlobColorResonance(){ _targetState = eEffectState.DEFAULT,
				_minColor = new Color(0.7f,1,0,0.2f),
				_maxColor = new Color(0.7f,1,0,0.2f),
				_curvePeriod = 0 },
			new BlobColorResonance(){ _targetState = eEffectState.CONNECTING,
				_minColor = new Color(0.7f,0.7f,0.7f,0.2f),
				_maxColor = new Color(0.2f,0.2f,0.7f,0.2f),
				_curvePeriod = 1 },
			new BlobColorResonance(){ _targetState = eEffectState.DISCONNECTING,
				_minColor = new Color(0.7f,0.7f,0.7f,0.2f),
				_maxColor = new Color(0.7f,0.2f,0.2f,0.2f),
				_curvePeriod = 1 }
		};
		
		[Header("컬러변화 전환 시간")]
		public float _resonanceTransitionSpeed = 1;
		
		public int Connectors
		{
			get => (_connectors == null ? 0 : _connectors.Count);
		}
		
		#region Mono

		private void Update()
		{
			Evaluate(0);
		}
		
		#endregion Mono
		
		public ConnectionNode Connect(Transform connector)
		{
			if (_connectors == null)
				_connectors = new List<BlobConnector>();
			if (_blobAreaSnaps == null)
				_blobAreaSnaps = new List<TransformNode>();
			
			BlobConnector blobConnector = new BlobConnector(connector,this,TransformNode.NODE_MIN_SIZE);
			_connectors.Add(blobConnector);

			if (!_disableMainRenderer)
			{
				if (_mainBound == null)
				{
					_mainBound = MakeBlob();
				//	_mainBound.SetMaterialRenderQueue(3000);
					// 메인 블랍은, 개별 서브 블랍들보다 렌더큐를 높게 잡아 Merge될 때 표면이 겹쳐 보이는걸 막는다
				
				}

				_blobAreaSnaps.Add(_mainBound.AddTarget(_connectors[0].GetBody().Position, TransformNode.NODE_MIN_SIZE));
				_mainBound.gameObject.SetActive(_connectors.Count != 0);
			}
			
			
			SetEffectState(eEffectState.DEFAULT);
			_resonanceMinColor = _currentResonance._minColor;
			_resonanceMaxColor = _currentResonance._maxColor;
			_blobMeshMaterialProperty = new MaterialPropertyBlock();
			_blobLineMaterialProperty = new MaterialPropertyBlock();
			
			return blobConnector;
		}

		public Blob MakeBlob()
		{
			Blob blob = Instantiate(_blobPrefab, transform).GetComponent<Blob>();
			blob.Init();
			return blob;
		}

		public void Disconnect(ConnectionNode connector)
		{
			if (connector == null)
				return;
		
			(connector as BlobConnector).KillSmoothly();
		}

		public void DisposeNodeFromConnection(ConnectionNode connector)
		{
			int index = _connectors.IndexOf(connector as BlobConnector );
			
			_connectors.RemoveAt(index);
			if (!_disableMainRenderer)
			{
				_mainBound.RemoveTarget(connector._bodyVolume);
				_mainBound.RemoveTarget(_blobAreaSnaps[index]);
				_blobAreaSnaps.RemoveAt(index);
				_mainBound.gameObject.SetActive(_connectors.Count != 0);
			}
		
		
		}
		
		public void DestroyConnection()
		{
			// TODO : implement connection close
		}

		public void InitConnection()
		{
			if (_connectors == null) return;
			for (int i = _connectors.Count-1; i >= 0 ; i--)
			{
				if(_connectors[i] == null)
					continue;
				Disconnect(_connectors[i]);
			}
			
			SetEffectState(eEffectState.DEFAULT);
			_resonanceMinColor = _currentResonance._minColor;
			_resonanceMaxColor = _currentResonance._maxColor;
		}

		public void SetEffectState(eEffectState effectState)
		{
			_effectState = effectState;
			_currentResonance = _blobColorResonances.Find(x => x._targetState == _effectState);
		}

		/// <summary>
		/// 접속되어있는 ( weight 가 1이면 접속중이라고 가정 ) 노드 중 입력 노드로부터 가장 가까운 노드 반환
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private ConnectionNode GetClosestBodyInConnection(ConnectionNode node)
		{
			return _connectors.OrderBy(x => { 
				return (x == node || x._weight < 1 ) ? ShapeUtility.FAR : (x._bodyVolume.Position - node._bodyVolume.Position).sqrMagnitude;
			}).First();
		}

		void UpdateColor()
		{
			_resonanceColorRunning += Time.deltaTime;

			_resonanceMinColor = Color.Lerp(_resonanceMinColor, _currentResonance._minColor,
				Time.deltaTime * _resonanceTransitionSpeed);
			_resonanceMaxColor = Color.Lerp(_resonanceMaxColor, _currentResonance._maxColor,
				Time.deltaTime * _resonanceTransitionSpeed);

			Color targetColor = _resonanceMinColor;
			if (_currentResonance._curvePeriod > 0)
			{
				_resonanceColorRunning %= _currentResonance._curvePeriod;
				float evalTime = _resonanceColorRunning / _currentResonance._curvePeriod;
				targetColor = Color.Lerp(_resonanceMinColor, _resonanceMaxColor ,_currentResonance._appearanceCurve.Evaluate(evalTime));
			}
			
			_blobMeshMaterialProperty.SetColor("_Color",targetColor);
			_blobLineMaterialProperty.SetColor("_Color",targetColor);
			if(!_disableMainRenderer)
				_mainBound.SetMaterialPropertyBlock(_blobMeshMaterialProperty,_blobLineMaterialProperty);
		}
		
		public override void Evaluate(float time)
		{
			if (_connectors != null)
			{
				for (int i = 0; i < _connectors.Count; i++)
				{
					if (!_disableMainRenderer)
					{
						ConnectionNode closestNode = GetClosestBodyInConnection(_connectors[i]);
						TransformNode closestBlob = _blobAreaSnaps[i];

						Vector3 nodeTarget = Vector3.Lerp(closestNode._bodyVolume.Position,
							_connectors[i]._bodyVolume.Position,
							_distanceOverConnection.Evaluate( _connectors[i]._weight ));
						float nodeTargetHeadSize =  Mathf.Max(TransformNode.NODE_MIN_SIZE, 
							_headSizeOverConnection.Evaluate(_connectors[i]._weight)*_maxHeadRadius );
					
						if (_softColloidMovement)
						{
							closestBlob._moveTarget = Vector3.Lerp(closestBlob._moveTarget, nodeTarget, Time.deltaTime * _softColloidMoveSpeed);
							closestBlob._size =
								ShapeUtility.Lerp(closestBlob._size, nodeTargetHeadSize, Time.deltaTime * _softColloidMoveSpeed);
						}
						else
						{
							closestBlob._moveTarget = nodeTarget;
							closestBlob._size = nodeTargetHeadSize;
						}
						if (_connectors[i].Evaluate())
						{
							_connectors[i].SetTarget(closestNode._bodyVolume);
							_connectors[i].SetMaterialPropertyBlock(_blobMeshMaterialProperty,_blobLineMaterialProperty);
						}
					}
					else
					{
						if (_connectors[i].Evaluate())
						{
							_connectors[i].SetMaterialPropertyBlock(_blobMeshMaterialProperty,_blobLineMaterialProperty);
						}
					}
				}
				UpdateColor();
			}
		}

		public override void Draw()
		{
			
		}
	}
}
