// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	MiceEditableUserInfo.cs
//  * Developer:	wlemon
//  * Date:		2023-04-14 오후 5:42
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Collections.Generic;
using Cysharp.Text;
using Google.Protobuf.WellKnownTypes;

namespace Com2Verse.Mice
{
	public class MiceEditableUserInfo : MiceUserInfo
	{
		public MiceEditableUserInfo(MiceUserInfo userInfo) : base(userInfo) { }

		public MiceEditableUserInfo(MiceWebClient.Entities.AccountEntity data) : base(data) { }

		public void SetFirstName(string value) => FirstName = value;

		public void SetLastName(string  value) => LastName = value;

		public void SetAdditionalName(string value) => AdditionalName = value;

		public void SetAffiliation(string value) => Affiliation = value;

		public void SetEmail(string value) => Email = value;

		public void SetEmail(string emailID, string emailDomain)
		{
			if (string.IsNullOrEmpty(emailID)) return;
			if (string.IsNullOrEmpty(emailDomain)) return;

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.Clear();
			stringBuilder.AppendFormat("{0}@{1}", emailID, emailDomain);
			SetEmail(stringBuilder.ToString());
		}

		public void SetPhone(string value) => Phone = value;

		public void SetFreeListCount(int count)
		{
			while (count > FreeList.Count)
			{
				FreeList.Add(new Free());
			}

			while (count < FreeList.Count)
			{
				FreeList.RemoveAt(FreeList.Count - 1);
			}
		}

		public void ClearFreeList()
		{
			FreeList.Clear();
		}

		public void SetFree(int index, string title, string content)
		{
			if (index >= FreeList.Count) return;

			var free = FreeList[index];
			free.Title      = title;
			free.Content    = content;
			FreeList[index] = free;
		}

		public void SetIsPublic(bool   value) => IsPublic = value;

		public void SetPhotoUrl(string value) => PhotoUrl = value;

		public void SetPhotoThumbnailUrl(string value) => PhotoThumbnailUrl = value;


	}
}
