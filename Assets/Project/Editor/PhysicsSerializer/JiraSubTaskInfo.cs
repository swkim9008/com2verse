/*===============================================================
* Product:		Com2Verse
* File Name:	JiraSubTaskInfo.cs
* Developer:	haminjeong
* Date:			2023-09-14 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2VerseEditor
{
	[CreateAssetMenu(fileName = "JiraSubTaskInfo", menuName = "Com2Verse/Create Jira SubTask")]
	public sealed class JiraSubTaskInfo : ScriptableObject
	{
		public string Token;
		public string Summary;
		public string Description;
	}
}
