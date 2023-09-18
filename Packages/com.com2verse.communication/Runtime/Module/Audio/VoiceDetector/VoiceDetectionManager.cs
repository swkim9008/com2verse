/*===============================================================
* Product:		Com2Verse
* File Name:	VoiceDetectionManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-16 16:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.IO;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Communication
{
	/// <inheritdoc />
	public sealed class VoiceDetectionManager : Singleton<VoiceDetectionManager>
	{
		public VoiceDetectionSettings? Settings { get; private set; }

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private VoiceDetectionManager() { }

		public void TryInitialize()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				var result = TableDataManager.Instance.Get<TableVoiceDetectionSettings>();
				Settings = result.Datas?[Define.DefaultTableIndex] ?? throw new InvalidDataException("VoiceDetectionSettings is null");
			}
			IsInitializing = false;
			IsInitialized  = true;
		}
	}
}
