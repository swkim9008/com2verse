/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEditorDisplayProgress.cs
* Developer:	tlghks1009
* Date:			2023-03-09 12:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesEditorDisplayProgress
	{
		public string Name { get; set; }
		public int TotalCount { get; set; }
		public int Current { get; set; }
		public float Progress => (float) Current / TotalCount;


		public C2VAddressablesEditorDisplayProgress()
		{
			Reset();
		}

		public void DisplayProgressBar()
		{
			var progressTitle = $"Addressable 최적화중... ({Current}/{TotalCount})";
			var progressInfo = $"{Name}";
			var progress = Progress;

			EditorUtility.DisplayProgressBar(progressTitle, progressInfo, progress);

			if (progress >= 1)
			{
				Reset();
			}
		}

		public void Reset()
		{
			Name = string.Empty;
			TotalCount = 0;
			Current = 0;

			EditorUtility.ClearProgressBar();
		}
	}
}
