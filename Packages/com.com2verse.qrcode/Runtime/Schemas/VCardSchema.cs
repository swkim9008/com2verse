/*===============================================================
* Product:		Com2Verse
* File Name:	VCardSchema.cs
* Developer:	klizzard
* Date:			2023-04-05 12:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.QrCode.Helpers;
using UnityEngine;

namespace Com2Verse.QrCode.Schemas
{
	public class VCardSchema
	{
		public string Organization { get; set; } = "";
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string MiddleName { get; set; } = "";
		public string Email { get; set; } = "";
		public string PhoneNumber { get; set; } = "";
		public string Photo { get; set; } = "";
	}
}
