/*===============================================================
* Product:		Com2Verse
* File Name:	HighlightEffectEditor.cs
* Developer:	ljk
* Date:			2022-08-23 15:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Rendering.Effect;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Rendering.Effect
{
	[CustomEditor(typeof(HighlightMask))]	
	public sealed class HighlightEffectEditor : Editor
	{
		private string _partsMaskPath = "Rendering/D16_SKIN_HighlightMask";
		private string _textureLoadError = "";
		private string _targetTestIndex = "";
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			HighlightMask maskhelper = target as HighlightMask;

			GUILayout.Space(30);
			GUILayout.Label("-- 테스트 --");
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("SetupTexture", GUILayout.Width(200)))
			{
				Texture2D tx = Resources.Load<Texture2D>(_partsMaskPath);
				if (tx == null)
					_textureLoadError = "경로 확인";
				else
				{
					_textureLoadError = "";
					maskhelper.SetupTexture(tx);
				}
			}

			_partsMaskPath = GUILayout.TextField(_partsMaskPath,GUILayout.Width(400));
			if(_textureLoadError.Length > 0)
				GUILayout.Label(_textureLoadError);
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5);
			GUILayout.Label("콤마(,)구분자로 여러 인덱스 동시에");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("인덱스 테스트",GUILayout.Width(200)))
			{
				string[] sep = _targetTestIndex.Split(",");
				if (sep != null && sep.Length > 0)
				{
					List<int> integers = new List<int>();

					for (int i = 0; i < sep.Length; i++)
					{
						if (int.TryParse(sep[i], out int target))
						{
							integers.Add(target);
						}
					}

					if (integers.Count > 0)
					{
						if(integers.Count == 1)
							maskhelper.HighLightIndex(integers[0]);
						else
							maskhelper.HighLightIndexes(integers);
					}
				}
			}

			_targetTestIndex = GUILayout.TextField(_targetTestIndex,GUILayout.Width(400));
			GUILayout.EndHorizontal();
		}
	}
}
