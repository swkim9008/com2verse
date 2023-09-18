/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtension.cs
* Developer:	ikyoung
* Date:			2023-04-04 17:10
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.Mice
{
	public static partial class MiceExtensions
	{
		public static string ToName(this eMiceAreaType areaType)
		{
			return $"MICE {areaType}";
		}

		public static eMiceAreaType ToMiceAreaType(this eSpaceCode code)
		{
			switch (code)
			{
				case eSpaceCode.MICE_LOBBY:
					return eMiceAreaType.LOBBY;
				case eSpaceCode.MICE_LOUNGE:
					return eMiceAreaType.LOUNGE;
				case eSpaceCode.MICE_CONFERENCE_HALL:
					return eMiceAreaType.HALL;
				case eSpaceCode.MICE_MEET_UP:
					return eMiceAreaType.MEET_UP;
				default:
					return eMiceAreaType.NONE;
			}
		}

		public static void QuitApplication(this object _) => Utils.QuitApplication();


		public static UniTask WithCancellationToken(this UniTask task, CancellationToken cancellationToken)
			=> UniTask.WhenAny
			(
				task,
				UniTask.WaitUntil(() => cancellationToken.IsCancellationRequested, cancellationToken: cancellationToken)
			);

		public static UniTask<T> WithCancellationToken<T>(this UniTask<T> task, CancellationToken cancellationToken)
			=> UniTask.WhenAny
			(
				task,
				UniTask.WaitUntil(() => cancellationToken.IsCancellationRequested, cancellationToken: cancellationToken)
			)
			.ContinueWith(v => v.hasResultLeft ? v.result : default);


		public static eConferenceObjectType GetConferenceObjectType(this Protocols.ObjectState objState)
		{
			foreach (var tag in objState.ObjectTags)
			{
				if (!tag.Key.Equals(TagDefine.Key.ConferenceObjectType)) continue;

				if (tag.Value.Equals(TagDefine.Value.Listener))
				{
					return eConferenceObjectType.LISTENER;
				}
				else if (tag.Value.Equals(TagDefine.Value.Speaker))
				{
					return eConferenceObjectType.SPEAKER;
				}
				else
				{
					return eConferenceObjectType.NONE;
				}
			}

			return eConferenceObjectType.NONE;
		}
		
		public static string ToWebLanguageCode(this UI.Localization.eLanguage systemLanguageCode)
		{
			switch (systemLanguageCode)
			{
				case UI.Localization.eLanguage.KOR:
					return "ko";
				case UI.Localization.eLanguage.ENG:
					return "en";
				default:
					return "ko";
			}
		}

		public static MiceWebClient.eMiceLangCode ToMiceLanguageCode(this UI.Localization.eLanguage systemLanguageCode)
		{
			switch (systemLanguageCode)
			{
				case UI.Localization.eLanguage.KOR:
					return MiceWebClient.eMiceLangCode.KO;
				case UI.Localization.eLanguage.ENG:
					return MiceWebClient.eMiceLangCode.EN;
				default:
					return MiceWebClient.eMiceLangCode.KO;
			}
		}
	}
}
