/*===============================================================
* Product:		Com2Verse
* File Name:	TagColorScriptableObject.cs
* Developer:	ksw
* Date:			2023-04-07 12:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse
{
	[CreateAssetMenu(fileName = "ColorData", menuName = "ColorData")]
	public class ColorScriptableObject : ScriptableObject
	{
		[SerializeField] private Color canceled;
		public                   Color CanceledColor => canceled;

		[SerializeField] private Color canceledBg;
		public                   Color CanceledBgColor => canceledBg;
		
		[SerializeField] private Color ongoing;
		public                   Color OnGoingColor => ongoing;
		
		[SerializeField] private Color ongoingBg;
		public                   Color OnGoingBgColor => ongoingBg;
		
		[SerializeField] private Color startedSoon;
		public                   Color StartedSoonColor => startedSoon;
		
		[SerializeField] private Color startedSoonBg;
		public                   Color StartedSoonBgColor => startedSoonBg;
		
		[SerializeField] private Color waitJoinRequest;
		public                   Color WaitJoinRequestColor => waitJoinRequest;

		[SerializeField] private Color waitJoinRequestBg;
		public                   Color WaitJoinRequestBgColor => waitJoinRequestBg;

		[SerializeField] private Color waitJoinReceived;
		public                   Color WaitJoinReceivedColor => waitJoinReceived;

		[SerializeField] private Color waitJoinReceivedBg;
		public                   Color WaitJoinReceivedBgColor => waitJoinReceivedBg;
		
		[SerializeField] private Color waitJoinInquiry;
		public                   Color WaitJoinInquiryColor => waitJoinInquiry;

		[SerializeField] private Color waitJoinInquiryBg;
		public                   Color WaitJoinInquiryBgColor => waitJoinInquiryBg;
		
		[SerializeField] private Color myConnecting;
		public                   Color MyConnectingColor => myConnecting;

		[SerializeField] private Color myConnectingBg;
		public                   Color MyConnectingBgColor => myConnectingBg;
		
		[SerializeField] private Color backgroundColor;
		public                   Color ExpiredBackgroundColor => backgroundColor;
	}
}
