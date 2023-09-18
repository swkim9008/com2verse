/*===============================================================
* Product:		Com2Verse
* File Name:	AnimatorBuilder_MenuItem.cs
* Developer:	eugene9721
* Date:			2022-07-14 16:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2VerseEditor.AvatarAnimation
{
	public static class AnimatorBuilder
	{
		[MenuItem("Com2Verse/Animation/Get Animation List", false, 1)]
		private static void GetAnimationList()
		{
			var animations = AnimationEditorUtils.LoadAnimationClipsAtSelectedFiles();
			if (!AnimationEditorUtils.SelectedObjectCheck(animations)) return;

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"총 {animations.Count.ToString()}개의 애니메이션이 선택되었습니다.");

			for (var index = 0; index < animations.Count; ++index)
				stringBuilder.AppendLine($"{(index + 1).ToString()}. {animations[index].name}");

			Debug.Log(stringBuilder.ToString());
		}

		[MenuItem("Com2Verse/Animation/Extract AnimationClips(legacy)", false, 3)]
		private static void ExtractAnimationClips()
		{
			var selectedObject = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
			if (!AnimationEditorUtils.FolderConditionCheck(selectedObject)) return;

			var selectedFolderName = selectedObject[0].name;
			var basePath           = AssetDatabase.GetAssetPath(selectedObject[0]);
			var fbxFolderPath      = Path.Combine(basePath, AnimatorBuilderDefine.AnimFBXFolderName);

			List<(eAnimationClipType, AnimationClip)> animationClips = new();
			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
			{
				var clipTypeName      = animationClipType.ToString().ToLower();
				var fbxTypeFolderPath = Path.Combine(fbxFolderPath, clipTypeName);
				if (!Directory.Exists(fbxTypeFolderPath)) continue;
				var fbxClipPath = Directory.GetFiles(fbxTypeFolderPath);

				List<AnimationClip> loadClips = new();
				foreach (var fileName in fbxClipPath)
				{
					loadClips.Clear();
					AnimationEditorUtils.LoadAnimationClips(fileName, loadClips);
					foreach (var clip in loadClips)
					{
						if (clip.name.StartsWith(AnimatorBuilderDefine.PreviewHeader)) continue;
						if (clip.name.Contains(AnimatorBuilderDefine.PreviewAnimation)) continue;
						animationClips.Add((animationClipType, clip));
					}
				}
			}

			var window = EditorWindow.GetWindow<AnimationClipExtractorLegacy>(nameof(AnimationClipExtractorLegacy));
			if (window == null) return;

			window.Initialize(animationClips, basePath, selectedFolderName);
			window.Show();
		}

		[MenuItem("Com2Verse/Animation/Clip Setting/Set loop at model animations")]
		private static void SetLoopAtModelAnimations()
		{
			var modelImporters = AnimationEditorUtils.LoadModelImporterAtSelectedFiles();
			if (!AnimationEditorUtils.SelectedObjectCheck(modelImporters)) return;

			var response = EditorUtility.DisplayDialog("SetLoopAtModelAnimations", "정말 변경하시겠습니까?", "OK", "Cancel");
			if (!response) return;

			foreach (var modelImporter in modelImporters)
			{
				var clipAnimations                                 = modelImporter.clipAnimations;
				foreach (var clip in clipAnimations) clip.loopTime = true;

				modelImporter.clipAnimations = clipAnimations;
				modelImporter.SaveAndReimport();
			}

			AssetDatabase.SaveAssets();
		}

		[MenuItem("Com2Verse/Animation/Clip Setting/Set loop at animation clips")]
		private static void SetLoopAtAnimationClips()
		{
			var animations = AnimationEditorUtils.LoadAnimationClipsAtSelectedFiles();
			if (!AnimationEditorUtils.SelectedObjectCheck(animations)) return;

			var response = EditorUtility.DisplayDialog("SetLoopAtAnimationClips", "정말 변경하시겠습니까?", "OK", "Cancel");
			if (!response) return;

			foreach (var animation in animations)
			{
				var newSettings = AnimationUtility.GetAnimationClipSettings(animation);
				newSettings.loopTime = true;
				AnimationUtility.SetAnimationClipSettings(animation, newSettings);
				EditorUtility.SetDirty(animation);
			}

			AssetDatabase.SaveAssets();
		}
	}
}
