/*===============================================================
 * Product:		Com2Verse
 * File Name:	MediaTrackConnector.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication
{
	public abstract class MediaTrackConnector : ConnectionController
	{
		public ICommunicationUser Owner { get; }
		public eTrackType         Type  { get; }

		protected MediaTrackConnector(ICommunicationUser owner, eTrackType trackType)
		{
			Owner = owner;
			Type  = trackType;
		}

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		protected override string GetLogCategory()
		{
			var baseCategory = base.GetLogCategory();

			return ZString.Format(
				"{0} ({1})"
			  , baseCategory, Type);
		}

		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo  = base.GetDebugInfo();
			var ownerInfo = Owner.GetInfoText();

			return ZString.Format(
				"{0}\n: Owner = {1}\n: Type = {2}"
			  , baseInfo, ownerInfo, Type);
		}
#endregion // Debug
	}
}
