// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SpaceManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-06 오후 3:18
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com2Verse.Builder
{
	public class SpaceManager : DestroyableMonoSingleton<SpaceManager>
	{
		public enum eSpaceSize
		{
			SMALL = 0,
			MEDIUM,
			LARGE_A,
			LARGE_B,
			XLARGE,
			XXLARGE
		}

		private const float DefaultWallHeight = 4;
		public const float GridSize = 0.5f;
		public const int SelectableObjectLayer = 6;
		public const int GroundLayer = 7;
		public const int StackBaseObjectLayer = 8;
		public const int FloorRayHitLayer = 9;

		private readonly Vector2 _negativeZ = new Vector2(0, -1);
		private readonly Vector2 _negativeX = new Vector2(-1, 0);
		private readonly Vector2 _positiveZ = new Vector2(0, 1);
		private readonly Vector2 _positiveX = new Vector2(1, 0);

		public string SpaceID
		{
			get
			{
				if (string.IsNullOrEmpty(_spaceId))
				{
					// (TODO) use server issued id
					_spaceId = System.DateTime.Now.ToFileTime().ToString();
				}

				return _spaceId;
			}
			set
			{
				_spaceId = value;
			}
		}
		public GameObject MapRoot => _mapRoot;
		public GameObject TrashCan => _trashcan;

		public bool CullWall
		{
			get => _cullWall;
			set
			{
				if (!value)
				{
					foreach (var wall in Walls)
					{
						wall.gameObject.SetActive(true);
					}
				}
				else
				{
					CheckWall(true);
				}

				_cullWall = value;
			}
		}
		public BuilderWall[] Walls { get; set; } = new BuilderWall[0];
		public BuilderFloor[] Floors { get; set; } = new BuilderFloor[0];
		public eBuilderSpaceType SpaceType { get; set; }
		public long TemplateId { get; set; } = 0;

		private Dictionary<long, SpaceTemplate> _spaceTemplates = new Dictionary<long, SpaceTemplate>();
		private Dictionary<long, SpaceTemplateArrangement> _spaceTemplatesArrangements = new Dictionary<long, SpaceTemplateArrangement>();
		private bool _cullWall = false;
		private string _spaceId = string.Empty;
		private GameObject _misc;
		private GameObject _mapRoot;
		private GameObject _trashcan;
		private Transform _floorParent;
		private Transform _wallParent;
		private GameObject _wallObject;
		private GameObject _cornerObject;

		private CameraMouseController _cameraMouseController;

		private void Awake()
		{
			_misc = new GameObject("MISC");
			_mapRoot = new GameObject("MapRoot");
			_trashcan = new GameObject("TrashCan");
			_trashcan.SetActive(false);
			_floorParent = new GameObject("Floor").transform;
			_wallParent = new GameObject("Wall").transform;
			
			_cameraMouseController = GameObject.Find("MainVirtualCamera").AddComponent<CameraMouseController>();
			
			_wallObject = CreateWallObject(new Vector2(1, DefaultWallHeight + 0.5f));
			_cornerObject = CreateWallObject(new Vector2(0.125f, DefaultWallHeight + 0.5f));
			
			ActionSystem.Instance.Initialize();
		}

		private void Update()
		{
			CheckWall(false);
		}

		public void LoadSpaceData()
		{
			_spaceTemplates = TableDataManager.Instance.Get<TableSpaceTemplate>()!.Datas;
			_spaceTemplatesArrangements = TableDataManager.Instance.Get<TableSpaceTemplateArrangement>()!.Datas;
		}

		public async UniTask LoadTemplateObject(bool loadArrangements = false)
		{
			if (_spaceTemplates.TryGetValue(TemplateId, out var template))
			{
				await C2VAddressables.LoadSceneAsync($"{template.SpaceTemplateName}.unity", LoadSceneMode.Additive).ToUniTask();

				if (loadArrangements)
				{
					foreach (var arrangement in _spaceTemplatesArrangements.Values)
					{
						if (arrangement.TemplateID == TemplateId)
						{
							await LoadObject(arrangement.BaseObjectID, arrangement.SpaceObjectID, arrangement.Position, Quaternion.Euler(arrangement.Rotation));
						}
					}
				}
			}
			else
			{
				C2VDebug.LogError($"{TemplateId} : 존재하지 않는 템플릿 입니다.");
			}
		}

		private async UniTask LoadObject(long baseObjectId, long spaceObjectId, Vector3 position, Quaternion rotation)
		{
			// Load object
			var model = await BuilderAssetManager.Instance.GetAssetAsync(eInventoryItemCategory.OBJECT, baseObjectId);
			if (model.LoadedAsset.IsReferenceNull()) return;

			GameObject rootObject = new GameObject(BuilderInventoryManager.Instance.GetPrefabName(baseObjectId));
			rootObject.transform.SetPositionAndRotation(position, rotation);
			rootObject.transform.SetParent(MapRoot.transform);
			Instantiate(model.LoadedAsset, rootObject.transform);
				
			var children = rootObject.GetComponentsInChildren<Transform>(true);
			foreach (var child in children)
			{
				child.gameObject.layer = SelectableObjectLayer;
			}
							
			var builderObject = rootObject.AddComponent<BuilderObject>();
			if (BuilderAssetManager.Instance.ItemListMap[eInventoryItemCategory.OBJECT].TryGetValue(baseObjectId, out var item))
			{
				builderObject.SerializationId = spaceObjectId;
				BuilderObject.IndexCounter = Math.Max(BuilderObject.IndexCounter, spaceObjectId + 1);
				builderObject.Init(rootObject, item);
			}
			else
			{
				throw new System.Exception($"#{baseObjectId} :오브젝트 시스템에 등록되지 않은 아이템입니다.");
			}
		}

		private float XZDistanceSqr(Vector3 r, Vector3 l)
		{
			return (r.x - l.x) * (r.x - l.x) + (r.z - l.z) * (r.z - l.z);
		}
		
		private void CheckWall(bool force)
		{
			if (force || (CullWall && CameraMouseController.Instance.Moving))
			{
				Transform mainCam = _cameraMouseController.MainCamera.transform;

				foreach (var wall in Walls)
				{
					wall.GroupCullingCalculated = false;
					if (!wall.gameObject.activeSelf) wall.gameObject.SetActive(true);
				}

				foreach (var wall in Walls)
				{
					if (wall.IsCorner) continue;
					
					if (!wall.GroupCullingCalculated)
					{
						Vector3 raycastTargetPosition = wall.transform.position - Vector3.up * 1.5f;
						bool hide = XZDistanceSqr(mainCam.position, wall.transform.position) < 1f || Physics.Raycast(raycastTargetPosition, raycastTargetPosition - mainCam.position, float.PositiveInfinity, 1 << FloorRayHitLayer);

						IterNeighbor(wall, true);
						IterNeighbor(wall, false);

						void IterNeighbor(BuilderWall wall, bool left)
						{
							while (!wall.IsReferenceNull())
							{
								if (hide)
								{
									wall.GroupCullingCalculated = true;
									if (wall.gameObject.activeSelf) wall.gameObject.SetActive(false);
								}
								if (left)
									wall = wall.NeighborLeft;
								else
									wall = wall.NeighborRight;
							}
						}
					}
				}
			}
		}
		
		public void ActiveCamera(bool state)
		{
			_cameraMouseController.enabled = state;
		}

		private GameObject CreateWallObject(Vector2 scale)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.SetActive(false);
			cube.transform.SetParent(_misc.transform);
			var meshFilter = cube.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;

			var vertices = mesh.vertices;
			var newVertices = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				newVertices[i] = new Vector3(vertices[i].x * scale.x, vertices[i].y * scale.y, vertices[i].z * scale.x);
			}

			cube.GetComponent<BoxCollider>().size = new Vector3(scale.x, scale.y, scale.x);

			var newMesh = new Mesh();
			newMesh.SetVertices(newVertices);
			newMesh.SetTriangles(mesh.triangles, 0);

			newMesh.RecalculateNormals();

			List<Vector2> uvList = new();
			List<Vector2> newUVList = new();
			mesh.GetUVs(0, uvList);
			foreach (var uv in uvList)
			{
				newUVList.Add(new Vector2(uv.x * scale.x, uv.y * scale.y));
			}

			newMesh.SetUVs(0, newUVList);
			meshFilter.sharedMesh = newMesh;

			return cube;
		}

		private GameObject CreateFloorObject(Vector2 scale, Vector2 worldPositionDelta)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var meshFilter = cube.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;

			var vertices = mesh.vertices;
			var newVertices = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				newVertices[i] = new Vector3(vertices[i].x * scale.x, vertices[i].y / 8, vertices[i].z * scale.y);
			}

			cube.GetComponent<BoxCollider>().size = new Vector3(scale.x, 0.125f, scale.y);

			var newMesh = new Mesh();
			newMesh.SetVertices(newVertices);
			newMesh.SetTriangles(mesh.triangles, 0);

			newMesh.RecalculateNormals();

			List<Vector2> uvList = new();
			List<Vector2> newUVList = new();
			mesh.GetUVs(0, uvList);
			foreach (var uv in uvList)
			{
				newUVList.Add(new Vector2(uv.x * scale.x, uv.y * scale.y) + worldPositionDelta);
			}

			newMesh.SetUVs(0, newUVList);
			meshFilter.sharedMesh = newMesh;

			return cube;
		}

		public void CreateSpaceFromTemplate(long templateId)
		{
			SpaceType = eBuilderSpaceType.TEMPLATE;
			TemplateId = templateId;
			
			BuilderObject.IndexCounter = 1;
			BuilderWall.IndexCounter = 0;
			BuilderFloor.IndexCounter = 0;

			Walls = new BuilderWall[0];
			Floors = new BuilderFloor[0];
		}

		public void CreateSpace(eSpaceSize size)
		{
			SpaceType = eBuilderSpaceType.EMPTY;
			
			BuilderObject.IndexCounter = 1;
			BuilderWall.IndexCounter = 0;
			BuilderFloor.IndexCounter = 0;
			
			switch (size)
			{
				case eSpaceSize.SMALL:
					Floors = new BuilderFloor[1];
					Floors[0] = CreateFloor(10, 10, Vector2.zero);

					Walls = new BuilderWall[44];
					for (int i = 0; i < 10; i++)
					{
						Walls[0 + i * 4] = CreateWall(new Vector2(-4.5f + i, 5.5f), _negativeZ);
						Walls[1 + i * 4] = CreateWall(new Vector2(5.5f, 4.5f - i), _negativeX);
						Walls[2 + i * 4] = CreateWall(new Vector2(4.5f - i, -5.5f), _positiveZ);
						Walls[3 + i * 4] = CreateWall(new Vector2(-5.5f, -4.5f + i), _positiveX);

						// setting neighbors
						for (int j = 0; j < 4; j++)
						{
							if (j + (i - 1) * 4 >= 0)
							{
								Walls[j + (i - 1) * 4].NeighborRight = Walls[j + i * 4];
								Walls[j + i * 4].NeighborLeft = Walls[j + (i - 1) * 4];
							}
						}
					}

					Walls[40] = CreateCorner(new Vector2(-5.5f, 5.5f) + new Vector2(0.4375f, -0.4375f), _negativeZ);
					Walls[39].NeighborRight = Walls[40]; Walls[0].NeighborLeft = Walls[40];
					Walls[41] = CreateCorner(new Vector2(5.5f, 5.5f) + new Vector2(-0.4375f, -0.4375f), _negativeX);
					Walls[36].NeighborRight = Walls[41]; Walls[1].NeighborLeft = Walls[41];
					Walls[42] = CreateCorner(new Vector2(5.5f, -5.5f) + new Vector2(-0.4375f, 0.4375f), _positiveZ);
					Walls[37].NeighborRight = Walls[42]; Walls[2].NeighborLeft = Walls[42];
					Walls[43] = CreateCorner(new Vector2(-5.5f, -5.5f) + new Vector2(0.4375f, 0.4375f), _positiveX);
					Walls[38].NeighborRight = Walls[43]; Walls[3].NeighborLeft = Walls[43];
					break;
				case eSpaceSize.MEDIUM:
					Floors = new BuilderFloor[1];
					Floors[0] = CreateFloor(10, 20, Vector2.zero);
					break;
				case eSpaceSize.LARGE_A:
					Floors = new BuilderFloor[1];
					Floors[0] = CreateFloor(10, 30, Vector2.zero);
					break;
				case eSpaceSize.LARGE_B:
					Floors = new BuilderFloor[2];
					Floors[0] = CreateFloor(10, 10, new Vector2(-2, 8));
					Floors[1] = CreateFloor(10, 20, new Vector2(8, 3));
					break;
				case eSpaceSize.XLARGE:
					Floors = new BuilderFloor[1];
					Floors[0] = CreateFloor(20, 20, Vector2.zero);
					break;
				case eSpaceSize.XXLARGE:
					Floors = new BuilderFloor[1];
					Floors[0] = CreateFloor(20, 30, Vector2.zero);
					break;
			}
		}

		public BuilderFloor CreateFloor(float x, float z, Vector2 initialPosition)
		{
			GameObject floor = CreateFloorObject(new Vector2(x, z), initialPosition);
			floor.layer = StackBaseObjectLayer;
			floor.transform.SetParent(_floorParent);
			floor.transform.position = new Vector3(initialPosition.x, -0.4375f, initialPosition.y);
			BuilderFloor floorData = floor.AddComponent<BuilderFloor>();
			floorData.InnerNormalDirection = Vector3.up;
			floorData.FloorScale = new Vector2(x, z);

			GameObject floorRayMask = new GameObject("RayMask");
			floorRayMask.layer = FloorRayHitLayer;
			floorRayMask.transform.SetParent(floor.transform);
			floorRayMask.transform.localPosition = Vector3.zero;
			var box = floorRayMask.AddComponent<BoxCollider>();
			box.size = new Vector3(x, 0.125f, z);

			return floorData;
		}

		public BuilderWall CreateWall(Vector2 initialPosition, Vector2 direction)
		{
			GameObject wall = Instantiate(_wallObject);
			wall.SetActive(true);
			wall.layer = StackBaseObjectLayer;
			wall.transform.SetParent(_wallParent);
			BuilderWall wallData = wall.AddComponent<BuilderWall>();
			wallData.InnerNormalDirection = new Vector3(direction.x, 0, direction.y);
			wall.transform.position = new Vector3(initialPosition.x + direction.x * 0.4375f, DefaultWallHeight / 2 - 0.25f, initialPosition.y + direction.y * 0.4375f);
			wall.transform.localScale = new Vector3(direction.x == 0 ? 1 : 0.125f, 1, direction.y == 0 ? 1 : 0.125f);

			return wallData;
		}

		public BuilderWall CreateCorner(Vector2 position, Vector2 direction)
		{
			GameObject wall = Instantiate(_cornerObject);
			wall.SetActive(true);
			wall.transform.SetParent(_wallParent);
			BuilderWall wallData = wall.AddComponent<BuilderWall>();
			wallData.InnerNormalDirection = new Vector3(direction.x, 0, direction.y);
			wallData.IsCorner = true;
			wall.transform.position = new Vector3(position.x, DefaultWallHeight / 2 - 0.25f, position.y);

			return wallData;
		}

		public void AddObject(GameObject targetModel, BuilderInventoryItem data)
		{
			var action = new AddObjectAction(targetModel, targetModel.transform.parent, TrashCan.transform);
			var objectComponent = targetModel.AddComponent<BuilderObject>();
			objectComponent.Init(targetModel, data);
			action.ObjectId = data.BaseObjectId;
			var children = targetModel.GetComponentsInChildren<Transform>(true);
			foreach (var child in children)
			{
				child.gameObject.layer = SelectableObjectLayer;
			}

			action.Do();
		}

		public void ChangeMaterial(GameObject targetObject, Material targetMaterial, BuilderInventoryItem data)
		{
			var currentRenderer = targetObject.GetComponent<MeshRenderer>();
			var wallFloor = targetObject.GetComponent<BaseWallObject>();
			var wallComponent = wallFloor as BuilderWall;
			if (wallComponent.IsReferenceNull())
				new ChangeMaterialAction(targetObject, currentRenderer, wallFloor.AssignedMaterial, targetMaterial, data.BaseObjectId).Do(true);
			else
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					List<ReversibleAction> groupActions = new List<ReversibleAction>();

					wallComponent.PropagateAction((wall) =>
					{
						groupActions.Add(new ChangeMaterialAction(wall.gameObject, wall.CurrentRenderer, wall.AssignedMaterial, targetMaterial, data.BaseObjectId));
					});

					new ActionGroup(groupActions).Do(true);
				}
				else
				{
					new ChangeMaterialAction(targetObject, currentRenderer, wallFloor.AssignedMaterial, targetMaterial, data.BaseObjectId).Do(true);
				}
			}
		}
	}
}
