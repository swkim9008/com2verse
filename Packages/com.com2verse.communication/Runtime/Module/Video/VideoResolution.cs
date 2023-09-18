/*===============================================================
* Product:		Com2Verse
* File Name:	VideoResolution.cs
* Developer:	urun4m0r1
* Date:			2022-11-02 15:42
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.IO;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Communication
{
	/// <inheritdoc />
	/// <summary>
	/// 해당 문서 참조 <a href="https://docs.google.com/spreadsheets/d/1BaWRYkCDplNMFTKSmwpHK_jpxL9ZkzKIw4e7biZGEhc/edit#gid=1518563565">TableData</a>
	/// </summary>
	public sealed class VideoResolution : Singleton<VideoResolution>
	{
		public Dictionary<int, VideoResolutionSettings>? Table { get; private set; }
		public VideoResolutionSettings? this[int index] => Table?[index];

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private VideoResolution() { }

		public void TryInitialize()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				var result = TableDataManager.Instance.Get<TableVideoResolutionSettings>();
				Table = result.Datas ?? throw new InvalidDataException("VideoResolutionSettings is null");
			}
			IsInitializing = false;
			IsInitialized  = true;
		}
	}
}
