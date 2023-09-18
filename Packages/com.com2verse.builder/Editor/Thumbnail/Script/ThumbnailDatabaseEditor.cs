/*===============================================================
* Product:		Com2Verse
* File Name:	ThumbnailDatabase.cs
* Developer:	yangsehoon
* Date:			2023-03-31 12:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Builder;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Com2VerseEditor.Builder
{
	[CustomEditor(typeof(ThumbnailDatabase))]
	public class ThumbnailDatabaseEditor : Editor
	{
		private readonly int _shadowOffsetPropertyId = Shader.PropertyToID("_ShadowWorldOffset");
		private readonly string _fileNameTemplate = "{0}/{1}.thumb.{2}";
		private readonly string _targetSceneName = "ThumbnailStudio";
		private Camera _mainCamera;
		private Light _mainLight;
		
		private static bool _overwriteExist = false;
		private RenderTexture _backgroundRenderTexture;
		private bool _showThumbnailList = false;
		private bool _showOptions = true;
		private bool _showThumbnailPreview = true;
		private bool _autoRefresh = false;
		private bool _showCameraOption = true;
		private bool _showLightingOption = true;
		private bool _showSavingOption = true;
		private Vector2 _thumbScrollPos;
		private bool _optionChanged = false;

		private GameObject _shadowStage = null;
		private Material _shadowMaterial = null;
		private Transform _previewStage = null;
		private GameObject _previewTargetPrefab = null;
		private GameObject _previewTargetInstance = null;
		private Texture2D _previewTexture = null;

		private RenderTexture _defaultRenderTexture = null;
		private RenderTexture _currentRenderTexture = null;
		
		private Color _defaultAmbientColor;
		private AmbientMode _defaultAmbientMode;
		private float _defaultShadowDistance;
		
		private ThumbnailDatabase _data;

		private string FileExtension => _data.Option.IncludeAlpha ? "png" : "jpeg";
		
		private void OnEnable()
		{
			var currentScene = EditorSceneManager.GetActiveScene();
			if (currentScene.name.Equals(_targetSceneName))
			{
				_data = (serializedObject.targetObject as ThumbnailDatabase);
				var roots = currentScene.GetRootGameObjects();
				foreach (var root in roots)
				{
					if (root.name.Equals("PreviewStage"))
					{
						_previewStage = root.transform;
					}
					else if (root.name.Equals("Stage"))
					{
						_shadowStage = root;
						_shadowMaterial = _shadowStage.GetComponent<MeshRenderer>().sharedMaterial;
					}
					else if (root.name.Equals("Directional Light"))
					{
						_mainLight = root.gameObject.GetComponent<Light>();
					}
				}
				
				_mainCamera = Camera.main;
				_defaultRenderTexture = _mainCamera.targetTexture;
				_currentRenderTexture = _defaultRenderTexture;

				_defaultAmbientColor = RenderSettings.ambientLight;
				_defaultAmbientMode = RenderSettings.ambientMode;
				_defaultShadowDistance = QualitySettings.shadowDistance;

				ClearPreviewObject();
				_backgroundRenderTexture = new RenderTexture((int)_data.Option.ImageSize.x, (int)_data.Option.ImageSize.y, 24);
			}
		}

		private void OnDisable()
		{
			if (EditorSceneManager.GetActiveScene().name.Equals(_targetSceneName))
			{
				ClearPreviewObject();
				
				if (_backgroundRenderTexture)
					_backgroundRenderTexture.Release();
			}
		}

		private Vector3 GetExtents(GameObject targetObject)
		{
			var filters = targetObject.GetComponentsInChildren<MeshFilter>();
			Vector3 min = Vector3.positiveInfinity;
			Vector3 max = Vector3.negativeInfinity;
			
			foreach (var filter in filters)
			{
				var vertices = filter.sharedMesh.vertices;
				foreach (var vertex in vertices)
				{
					var worldVertex = filter.transform.TransformPoint(vertex);
					min = Vector3.Min(min, worldVertex);
					max = Vector3.Max(max, worldVertex);
				}
			}

			Bounds boundingBox = new Bounds();
			boundingBox.SetMinMax(min, max);
			
			return boundingBox.extents;
		}

		private void Clear(ThumbnailDatabase data)
		{
			data.Sprites.Clear();
			data.ThumbnailMap.Clear();

			AssetDatabase.DeleteAsset(data.ThumbnailPath);
		}

		private Texture2D TakeShot(GameObject instance)
		{
			int width = (int)_data.Option.ImageSize.x;
			int height = (int)_data.Option.ImageSize.y;

			Texture2D backgroundTexture = null;
			if (_data.Option.BackgroundImage != null)
			{
				Graphics.Blit(_data.Option.BackgroundImage.texture, _backgroundRenderTexture);
				
				backgroundTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { alphaIsTransparency = true };
				backgroundTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
				backgroundTexture.Apply();
			}

			_mainLight.color = _data.Option.DirectionalLightColor;
			_mainCamera.fieldOfView = _data.Option.FOV;
			_mainLight.transform.rotation = Quaternion.Euler(_data.Option.DirectionalLightRotation);
			_mainCamera.transform.rotation = Quaternion.Euler(_data.Option.CameraRotation);
			ThumbnailCamera.AlignCamera(_mainCamera, instance, _data.Option.IterationCount, _data.Option.FitRatio);

			RenderTexture.active = _mainCamera.targetTexture;
			_mainCamera.Render();
			
			Texture2D mainTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { alphaIsTransparency = true };
			mainTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			mainTexture.Apply();

			if (backgroundTexture != null)
			{
				var mainPixelData = mainTexture.GetPixelData<Color32>(0);
				var backgroundPixelData = backgroundTexture.GetPixelData<Color32>(0);

				if (mainPixelData.Length == backgroundPixelData.Length)
				{
					for (int i = 0; i < mainPixelData.Length; i++)
					{
						double alpha = mainPixelData[i].a / 255f;
						mainPixelData[i] = new Color32((byte)(mainPixelData[i].r * alpha + backgroundPixelData[i].r * (1 - alpha)),
						                               (byte)(mainPixelData[i].g * alpha + backgroundPixelData[i].g * (1 - alpha)),
						                               (byte)(mainPixelData[i].b * alpha + backgroundPixelData[i].b * (1 - alpha)),
						                                     (byte)Mathf.Max(mainPixelData[i].a, backgroundPixelData[i].a));
					}

					mainTexture.SetPixelData(mainPixelData, 0, 0);
					mainTexture.Apply();
				}
				else
				{
					UnityEngine.Debug.LogWarning("Background texture size mismatch");
				}

				mainPixelData.Dispose();
				backgroundPixelData.Dispose();
			}

			return mainTexture;
		}

		private void Register(string address, Texture2D texture)
		{
			string path = string.Format(_fileNameTemplate, _data.ThumbnailPath, address, FileExtension);
			byte[] data = _data.Option.IncludeAlpha ? texture.EncodeToPNG() : texture.EncodeToJPG();
			
			File.WriteAllBytes(path, data);
		}

		private void ImportAsset(List<string> fileList)
		{
			AssetDatabase.ImportAsset(_data.ThumbnailPath, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);

			string extension = FileExtension;
			foreach (var address in fileList)
			{
				string path = string.Format(_fileNameTemplate, _data.ThumbnailPath, address, extension);

				TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
				importer.textureType = TextureImporterType.Sprite;
				AssetDatabase.WriteImportSettingsIfDirty(path);
				
				Texture2D assetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				_data.Sprites.Add(assetTexture);
				var newSprite = Sprite.Create(assetTexture, new Rect(0, 0, _data.Option.ImageSize.x, _data.Option.ImageSize.y), Vector2.one / 2);
				_data.ThumbnailMap.Add(address, newSprite);
			}
		}

		private void ShowThumbnailList()
		{
			if (_data.Sprites.Count > 0)
			{
				if (_data.ThumbnailMap.Count == 0)
				{
					try
					{
						_data.Initialize();
					}
					catch
					{
						EditorGUILayout.HelpBox("Exception while loading thumbnail data.", MessageType.Error);
						return;
					}
				}
			}
			else
			{
				EditorGUILayout.LabelField("No Thumbnail Data Exists");
				return;
			}
			
			_showThumbnailList = EditorGUILayout.Foldout(_showThumbnailList, "Saved Thumbnail Data (Read Only)");
			if (!_showThumbnailList) return;
			
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();

			_thumbScrollPos = EditorGUILayout.BeginScrollView(_thumbScrollPos, GUILayout.Height(320));
			int nullCheck = 0;
			foreach (var thumbnailData in _data.ThumbnailMap)
			{
				EditorGUILayout.BeginHorizontal();

				try
				{
					if (thumbnailData.Value == null)
						nullCheck++;
					EditorGUILayout.ObjectField(thumbnailData.Key, thumbnailData.Value, typeof(Sprite), false);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}

				EditorGUILayout.EndHorizontal();
			}
			
			if (nullCheck == _data.ThumbnailMap.Count)
				_data.Initialize();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		private void GenerateObjectThumbnail(List<string> fileList)
		{
			ClearPreviewObject();
			
			foreach (var prefab in _data.TargetObjects)
			{
				string thumbnailKey = $"{prefab.name}.prefab";
				if (!_overwriteExist && _data.ThumbnailMap.ContainsKey(thumbnailKey))
				{
					continue;
				}

				var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

				try
				{
					instance.transform.position += Vector3.up * GetExtents(instance).y / 2;

					Register(thumbnailKey, TakeShot(instance));
					fileList.Add(thumbnailKey);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log(e.Message);
				}

				DestroyImmediate(instance);
			}
		}

		private void GenerateMaterialThumbnail(List<string> fileList)
		{
			GameObject shadowCaster = null;
			if (_data.Option.IncludeShadow)
			{
				shadowCaster = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;
				shadowCaster.transform.rotation = Quaternion.Euler(0, 180, 0);
				shadowCaster.transform.position += Vector3.up / 2;
				shadowCaster.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			}
			
			foreach (var material in _data.TargetMaterials)
			{
				string thumbnailKey = $"{material.name}.mat";
				if (!_overwriteExist && _data.ThumbnailMap.ContainsKey(thumbnailKey))
				{
					continue;
				}

				var instance = GameObject.CreatePrimitive(PrimitiveType.Quad) as GameObject;

				try
				{
					instance.transform.rotation = Quaternion.Euler(0, 180, 0);
					instance.transform.position += Vector3.up / 2;
					instance.GetComponent<Renderer>().sharedMaterial = material;

					Register(thumbnailKey, TakeShot(instance));
					fileList.Add(thumbnailKey);
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log(e.Message);
				}

				DestroyImmediate(instance);
			}

			if (_data.Option.IncludeShadow)
			{
				DestroyImmediate(shadowCaster);
			}
		}

		private void DrawGeneratingOptions()
		{
			_showOptions = EditorGUILayout.Foldout(_showOptions, "Generating Option");

			if (_showOptions)
			{
				var option = serializedObject.FindProperty("_option");
				EditorGUI.indentLevel++;

				_showCameraOption = EditorGUILayout.Foldout(_showCameraOption, "Camera");
				if (_showCameraOption)
				{
					EditorGUI.indentLevel++;
					option.FindPropertyRelative("_cameraRotation").vector3Value = EditorGUILayout.Vector3Field("Camera Rotation (In Degrees)", _data.Option.CameraRotation);
					option.FindPropertyRelative("_fieldOfView").floatValue = EditorGUILayout.Slider("Field Of View", _data.Option.FOV, 10f, 70f);
					option.FindPropertyRelative("_fitRatio").floatValue = EditorGUILayout.Slider("Viewport Fit Ratio", _data.Option.FitRatio, 0.001f, 1);
					option.FindPropertyRelative("_iterationCount").intValue = EditorGUILayout.IntSlider("Center Align Quality", _data.Option.IterationCount, 1, 3);
					EditorGUI.indentLevel--;
				}

				_showLightingOption = EditorGUILayout.Foldout(_showLightingOption, "Lighting & Rendering");
				if (_showLightingOption)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.LabelField("Main Light");
					EditorGUI.indentLevel++;
					option.FindPropertyRelative("_directionalLightRotation").vector3Value = EditorGUILayout.Vector3Field("Directional Light Rotation (In Degrees)", _data.Option.DirectionalLightRotation);
					option.FindPropertyRelative("_directionalLightColor").colorValue = EditorGUILayout.ColorField("Directional Light Color", _data.Option.DirectionalLightColor);
					option.FindPropertyRelative("_includeShadow").boolValue = EditorGUILayout.Toggle("Include Shadow", _data.Option.IncludeShadow);
					if (_data.Option.IncludeShadow)
					{
						option.FindPropertyRelative("_shadowOffset").vector3Value = EditorGUILayout.Vector3Field("Shadow World Offset", _data.Option.ShadowOffset);
						_shadowMaterial.SetVector(_shadowOffsetPropertyId, _data.Option.ShadowOffset);
					}
					EditorGUI.indentLevel--;
					option.FindPropertyRelative("_ambientLightColor").colorValue = EditorGUILayout.ColorField("Ambient Light Color", _data.Option.AmbientLightColor);
					option.FindPropertyRelative("_backgroundImage").objectReferenceValue = EditorGUILayout.ObjectField("Background Image (optional)", _data.Option.BackgroundImage, typeof(Sprite), false) as Sprite;
					EditorGUI.indentLevel--;
				}

				_showSavingOption = EditorGUILayout.Foldout(_showSavingOption, "Saving");
				if (_showSavingOption)
				{
					EditorGUI.indentLevel++;
					option.FindPropertyRelative("_includeAlpha").boolValue = EditorGUILayout.Toggle("Include Alpha In Result Image File", _data.Option.IncludeAlpha);
					option.FindPropertyRelative("_imageSize").vector2Value = EditorGUILayout.Vector2Field("Thumbnail Image size", _data.Option.ImageSize);
					EditorGUI.indentLevel--;
				}
				
				EditorGUI.indentLevel--;

				if (serializedObject.hasModifiedProperties)
				{
					_optionChanged = true;
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		private void DrawPreviewField()
		{
			_showThumbnailPreview = EditorGUILayout.Foldout(_showThumbnailPreview, "Thumbnail Preview");
			if (!_showThumbnailPreview) return;
			
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.BeginVertical();
			var target = EditorGUILayout.ObjectField("GO Thumbnail Preview", _previewTargetPrefab, typeof(GameObject), false) as GameObject;
			_autoRefresh = EditorGUILayout.Toggle("Auto Refresh when Option Changed", _autoRefresh);
			if (_autoRefresh && _optionChanged)
			{
				RefreshPreview();
			}
			if (!_autoRefresh && GUILayout.Button("Refresh Preview Manually"))
			{
				RefreshPreview();
			}
			EditorGUILayout.EndVertical();

			if (!ReferenceEquals(target, _previewTargetPrefab))
			{
				ClearPreviewObject();

				_previewTargetPrefab = target;
				_previewTargetInstance = (PrefabUtility.InstantiatePrefab(_previewTargetPrefab) as GameObject);
				_previewTargetInstance.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				_previewTargetInstance.transform.SetParent(_previewStage);

				RefreshPreview();
			}

			GUILayout.Box(_previewTexture);
			
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}

		private void RefreshPreview()
		{
			if (_previewTargetInstance)
			{
				ApplyRenderingOptions();
				
				RenderTexture.active = _mainCamera.targetTexture;
				_previewTexture = TakeShot(_previewTargetInstance);
				
				RestoreDefaultSettings();
			}
			else
			{
				_previewTexture = null;
			}
		}

		private void ClearPreviewObject()
		{
			if (!ReferenceEquals(_previewStage, null))
			foreach (Transform child in _previewStage)
			{
				DestroyImmediate(child.gameObject);
				_previewTargetPrefab = null;
			}
		}
		
		private void ApplyRenderingOptions()
		{
			_shadowStage.SetActive(_data.Option.IncludeShadow);
			_shadowMaterial.SetVector(_shadowOffsetPropertyId, _data.Option.ShadowOffset);

			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = _data.Option.AmbientLightColor;

			if (_mainCamera.targetTexture.width != _data.Option.ImageSize.x || _mainCamera.targetTexture.height != _data.Option.ImageSize.y)
			{
				if (_data.Option.ImageSize.x > 1 && _data.Option.ImageSize.y > 1)
				{
					if (_currentRenderTexture != _defaultRenderTexture)
					{
						_currentRenderTexture.Release();
					}

					if (_backgroundRenderTexture)
						_backgroundRenderTexture.Release();

					_backgroundRenderTexture = new RenderTexture((int)_data.Option.ImageSize.x, (int)_data.Option.ImageSize.y, 24);
					_currentRenderTexture = new RenderTexture((int)_data.Option.ImageSize.x, (int)_data.Option.ImageSize.y, 24);
					_currentRenderTexture.antiAliasing = 8;
					_defaultRenderTexture = _mainCamera.targetTexture;
					_mainCamera.targetTexture = _currentRenderTexture;
				}
			}

			QualitySettings.shadowDistance = 10000;
		}
		
		private void RestoreDefaultSettings()
		{
			_shadowStage.SetActive(true);
			_shadowMaterial.SetVector(_shadowOffsetPropertyId, Vector4.zero);
			
			RenderSettings.ambientLight = _defaultAmbientColor;
			RenderSettings.ambientMode = _defaultAmbientMode;
			
			_mainCamera.targetTexture = _defaultRenderTexture;
			RenderTexture.active = _defaultRenderTexture;
			
			if (_currentRenderTexture != _defaultRenderTexture)
				_currentRenderTexture.Release();
			_currentRenderTexture = _defaultRenderTexture;

			QualitySettings.shadowDistance = _defaultShadowDistance;
		}

		public override void OnInspectorGUI()
		{
			if (!EditorSceneManager.GetActiveScene().name.Equals(_targetSceneName))
			{
				EditorGUILayout.LabelField("썸네일 추출기는 ThumbnailStudio 씬에서 사용 가능합니다.");
				if (GUILayout.Button("Open ThumbnailStudio"))
				{
					EditorSceneManager.OpenScene($"Packages/com.com2verse.builder/Editor/Thumbnail/MISC/{_targetSceneName}.unity");
				}
				
				return;
			}

			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_thumbnailPath"), new GUIContent("Thumbnail Image Save Path"));
			if (GUILayout.Button("Clear Data + Assets"))
			{
				Clear(_data);
			}
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Generate Target");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetObjects"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetMaterials"));
			EditorGUILayout.Separator();
			EditorGUI.indentLevel--;
			
			DrawGeneratingOptions();
			EditorGUILayout.Separator();
			DrawPreviewField();
			EditorGUILayout.Separator();

			_overwriteExist = EditorGUILayout.Toggle("overwrite Exists Thumbnail", _overwriteExist);
			if (GUILayout.Button("Generate Thumbnails", GUILayout.Height(48)))
			{
				if (_overwriteExist)
					Clear(_data);
				else
					_data.Initialize();

				ApplyRenderingOptions();

				if (!Directory.Exists(_data.ThumbnailPath))
				{
					Directory.CreateDirectory(_data.ThumbnailPath);
					AssetDatabase.ImportAsset(_data.ThumbnailPath);
				}

				List<string> fileList = new List<string>();
				GenerateObjectThumbnail(fileList);
				GenerateMaterialThumbnail(fileList);
				
				ImportAsset(fileList);

				RestoreDefaultSettings();

				EditorUtility.SetDirty(serializedObject.targetObject);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				_data.Initialize();
			}

			ShowThumbnailList();

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();

			_optionChanged = false;
		}
	}
}
