/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUserData.cs
* Developer:	ikyoung
* Date:			2023-07-04 13:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Network
{
	[ServiceID(300)]
	public sealed class MiceUserData : BaseUserData
	{
		[field: SerializeField, ReadOnly] public  string EmployeeID { get; set; } = string.Empty;
		[field: SerializeField, ReadOnly] private string _userName  = string.Empty;
		public override string UserName
		{
			get => _userName;
			set => _userName = value;
		}
	}
}