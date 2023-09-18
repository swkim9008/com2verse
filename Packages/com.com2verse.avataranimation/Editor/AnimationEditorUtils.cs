/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationEditorUtils.cs
* Developer:	eugene9721
* Date:			2023-02-23 14:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using System.Collections.Generic;
using Com2Verse.AvatarAnimation;
using Com2Verse.Logger;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Windows;

namespace Com2VerseEditor.AvatarAnimation
{
	public static class AnimationEditorUtils
	{
		private static readonly string PreviewAnimation = "__preview__Take 001";

		/// <summary>
		/// State 콜랙션에 포함된 Motion목록들을 가져옴
		/// </summary>
		/// <param name="states">Motion을 가져올 State콜랙션</param>
		/// <returns>콜랙션의 Motion 목록</returns>
		public static List<Motion> GetMotionsInStates(ICollection<AnimatorState> states)
		{
			var motions = new List<Motion>();
			foreach (var state in states)
				if (state.motion as BlendTree)
				{
					var blendTree = state.motion as BlendTree;
					if (blendTree == null) continue;
					motions.AddRange(GetBlendTreeMotionList(blendTree));
				}
				else
				{
					if (state.motion != null) motions.Add(state.motion);
				}

			return motions;
		}

		public static void GetStateMachineStates(AnimatorStateMachine machine, ICollection<AnimatorState> states)
		{
			if (machine == null || machine.stateMachines == null || machine.states == null)
			{
				C2VDebug.LogWarning(nameof(AnimationEditorUtils), "machine is null");
				return;
			}

			foreach (var childStateMachine in machine.stateMachines)
			{
				if (childStateMachine.stateMachine == null) continue;

				GetStateMachineStates(childStateMachine.stateMachine, states);
			}

			foreach (var state in machine.states)
				states.Add(state.state);
		}

		public static List<Motion> GetBlendTreeMotionList(BlendTree blendTree)
		{
			var motions = new List<Motion>();
			if (blendTree.children == null) return motions;

			foreach (var child in blendTree.children)
				motions.Add(child.motion);

			return motions;
		}

		public static List<ModelImporter>? LoadModelImporterAtSelectedFiles()
		{
			Object[]? selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			if (selectedAsset == null) return null;

			List<ModelImporter> modelImporters = new();
			foreach (var objects in selectedAsset)
			{
				var assetPath = AssetDatabase.GetAssetPath(objects);
				if (assetPath == null) continue;

				var importer = AssetImporter.GetAtPath(assetPath);
				if (importer is ModelImporter modelImporter)
					modelImporters.Add(modelImporter);
			}

			return modelImporters;
		}

		public static List<AnimationClip>? LoadAnimationClipsAtSelectedFiles() => LoadAnimationClipsAtObjects(Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets));

		public static List<AnimationClip>? LoadAnimationClipsAtObjects(Object[]? objects)
		{
			if (objects == null) return null;

			List<AnimationClip> animationClips = new();
			foreach (var fbxObject in objects)
			{
				var assetPath = AssetDatabase.GetAssetPath(fbxObject);
				if (assetPath == null) continue;

				LoadAnimationClips(assetPath, animationClips);
			}

			return animationClips;
		}

		public static void LoadAnimationClips(string path, ICollection<AnimationClip> clips)
		{
			var objects = AssetDatabase.LoadAllAssetsAtPath(path);
			if (objects == null) return;

			foreach (var obj in objects)
			{
				var clip = obj as AnimationClip;
				if (clip != null && clip.name != PreviewAnimation)
					clips.Add(clip);
			}
		}

		public static string AnimationNameWithoutAvatarType(string animationName) => animationName.Substring(AnimationDefine.AvatarCodeLength + 1);

		public static bool SelectedObjectCheck<T>(ICollection<T>? animations) where T : Object
		{
			if (animations == null || animations.Count == 0)
			{
				C2VDebug.LogWarningCategory(nameof(AnimationEditorUtils), $"{typeof(T)}를 가져올 파일들을 선택해주세요.");
				return false;
			}

			return true;
		}

		public static bool FolderConditionCheck(Object[]? selectedObject)
		{
			if (selectedObject == null)
			{
				C2VDebug.LogWarningCategory(nameof(AnimationEditorUtils), "선택한 오브젝트 정보가 없습니다.");
				return false;
			}

			if (selectedObject.Length > 1)
			{
				C2VDebug.LogWarningCategory(nameof(AnimationEditorUtils), "1개의 최상위 폴더만 선택해주세요");
				return false;
			}

			if (selectedObject.Length <= 0)
			{
				C2VDebug.LogWarningCategory(nameof(AnimationEditorUtils), "변환할 폴더를 선택해주세요");
				return false;
			}

			if (!IsAssetAFolder(selectedObject[0]))
			{
				C2VDebug.LogWarningCategory(nameof(AnimationEditorUtils), "파일이 아닌 폴더를 선택해주세요");
				return false;
			}

			return true;
		}

		public static bool IsAssetAFolder(Object obj)
		{
			if (obj == null) return false;

			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if (path == null) return false;

			return path.Length > 0 && Directory.Exists(path);
		}
	}
}
