/*===============================================================
* Product:		Com2Verse
* File Name:	IVolume.cs
* Developer:	urun4m0r1
* Date:			2022-04-08 17:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	/// <summary>
	/// Volume controller which you can set volume or change mute state.
	/// </summary>
	public interface IVolume
	{
		/// <summary>
		/// Normalized volume level.
		/// Setting this value to zero will not affect <see cref="IsAudible"/> state.
		/// </summary>
		/// <value>0f ~ 1f</value>
		float Level { get; set; }

		/// <summary>
		/// Boolean value indicating whether the audio is audible or not.
		/// Does not affect the <see cref="Level"/>.
		/// </summary>
		bool IsAudible { get; set; }

		event Action<float>? LevelChanged;
		event Action<bool>?  AudibleChanged;
	}
}
