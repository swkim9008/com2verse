/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUserInfo.cs
* Developer:	wlemon
* Date:			2023-04-13 18:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.QrCode;
using Com2Verse.QrCode.Schemas;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Mice
{
	public partial class MiceUserInfo : MiceBaseInfo
	{
		public static readonly int ImageSize    = 512;
		public static readonly int ThubnailSize = 32;

		public struct Free
		{
			public int    Seq;
			public string Title;
			public string Content;
		}

		public long                                       AccountId         { get; protected set; }
		public int                                        DomainId          { get; protected set; }
		public string                                     PhotoUrl          { get; protected set; }
		public string                                     PhotoThumbnailUrl { get; protected set; }
		public string                                     FirstName         { get; protected set; }
		public string                                     LastName          { get; protected set; }
		public string                                     AdditionalName    { get; protected set; }
		public string                                     Affiliation       { get; protected set; }
		public string                                     Email             { get; protected set; }
		public string                                     Phone             { get; protected set; }
		public List<Free>                                 FreeList          { get; protected set; }
		public bool                                       IsPublic          { get; protected set; }
		public MiceWebClient.eMiceAccountCardExchangeCode ExchangeCode      { get; protected set; }
		public bool                                       IsExchanged       => ExchangeCode == MiceWebClient.eMiceAccountCardExchangeCode.MUTUAL_FOLLOW;
		public List<long>                                 SurveyAgrees      { get; protected set; }

		public string Name
		{
			get
			{
				var stringBuilder = ZString.CreateStringBuilder();
				stringBuilder.AppendFormat("{0}{1}", LastName, FirstName);
				return stringBuilder.ToString();
			}
		}

		public MiceUserInfo(MiceUserInfo otherUserInfo)
		{
			Set(otherUserInfo);
		}

		public MiceUserInfo(MiceWebClient.Entities.AccountEntity accountEntity)
		{
			Set(accountEntity);
		}

		public void Set(MiceWebClient.Entities.AccountEntity accountEntity)
		{
			AccountId         = accountEntity.AccountId;
			DomainId          = accountEntity.DomainId;
			PhotoUrl          = accountEntity.PhotoUrl;
			PhotoThumbnailUrl = accountEntity.PhotoThumbnailUrl;
			FirstName         = accountEntity.GivenName;
			LastName          = accountEntity.Surname;
			AdditionalName    = accountEntity.MiddleName;
			Affiliation       = accountEntity.CompanyName;
			Email             = accountEntity.MailAddress;
			Phone             = accountEntity.TelNo;
			if (FreeList == null) FreeList = new List<Free>();
			FreeList.Clear();
			if (accountEntity.Details != null)
			{
				foreach (var detail in accountEntity.Details)
				{
					FreeList.Add(new Free()
					{
						Title   = detail.DetailName,
						Content = detail.DetailValue
					});
				}
			}

			IsPublic     = accountEntity.IsPublic;
			ExchangeCode = accountEntity.ExchangeCode;
			SurveyAgrees = new List<long>(accountEntity.SurveyAgrees);
		}

		public void Set(MiceUserInfo otherUserInfo)
		{
			AccountId         = otherUserInfo.AccountId;
			DomainId          = otherUserInfo.DomainId;
			PhotoUrl          = otherUserInfo.PhotoUrl;
			PhotoThumbnailUrl = otherUserInfo.PhotoThumbnailUrl;
			FirstName         = otherUserInfo.FirstName;
			LastName          = otherUserInfo.LastName;
			AdditionalName    = otherUserInfo.AdditionalName;
			Affiliation       = otherUserInfo.Affiliation;
			Email             = otherUserInfo.Email;
			Phone             = otherUserInfo.Phone;
			if (FreeList == null) FreeList = new List<Free>();
			FreeList.Clear();
			FreeList.AddRange(otherUserInfo.FreeList);
			IsPublic     = otherUserInfo.IsPublic;
			ExchangeCode = otherUserInfo.ExchangeCode;
			SurveyAgrees = new List<long>(otherUserInfo.SurveyAgrees);
		}

		public MiceWebClient.Entities.AccountInfo ToAccountInfo()
		{
			var details = new List<MiceWebClient.Entities.AccountDetailEntity>();
			foreach (var free in FreeList)
			{
				details.Add(new MiceWebClient.Entities.AccountDetailEntity()
				{
					AccountId   = AccountId,
					DetailSeq   = free.Seq,
					DetailName  = free.Title,
					DetailValue = free.Content
				});
			}

			return new MiceWebClient.Entities.AccountInfo()
			{
				AccountId   = AccountId,
				Surname     = LastName,
				GivenName   = FirstName,
				MiddleName  = AdditionalName,
				TelNo       = Phone,
				CompanyName = Affiliation,
				MailAddress = Email,
				IsPublic    = IsPublic,
				Details     = details,
			};
		}

		public void GetEmail(out string emailID, out string emailDomain)
		{
			if (string.IsNullOrEmpty(Email))
			{
				emailID     = string.Empty;
				emailDomain = string.Empty;
			}
			else
			{
				var temp = Email.Split('@');
				emailID     = temp.Length > 0 ? temp[0] : string.Empty;
				emailDomain = temp.Length > 1 ? temp[1] : string.Empty;
			}
		}

		public bool IsFreeActive(int index)
		{
			return index < FreeList.Count;
		}

		public void GetFree(int index, out string title, out string content)
		{
			if (FreeList.Count > index)
			{
				title   = FreeList[index].Title;
				content = FreeList[index].Content;
			}
			else
			{
				title   = string.Empty;
				content = string.Empty;
			}
		}

		public string GetFree(int index)
		{
			GetFree(index, out var title, out var content);
			if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
			{
				var stringBuilder = ZString.CreateStringBuilder();
				stringBuilder.AppendFormat("{0} : {1}", title, content);
				return stringBuilder.ToString();
			}
			else if (!string.IsNullOrEmpty(title))
			{
				return title;
			}
			else if (!string.IsNullOrEmpty(content))
			{
				return content;
			}
			else
			{
				return string.Empty;
			}
		}

		public Texture2D GenerateQrCode()
		{
			var result = QrCodeMaker.Generate(new VCardSchema
			{
				Organization = Affiliation,
				FirstName    = FirstName,
				LastName     = LastName,
				PhoneNumber  = Phone,
				Email        = Email,
				MiddleName   = string.Empty,
				Photo        = string.Empty,
			});
			return result;
		}

		public bool CheckIsCompletedSurvey(long surveyNo)
		{
			return SurveyAgrees.Contains(surveyNo);
		}

		public void CompleteSurvey(long surveyNo)
		{
			Mice.MiceWebClient.User.AgreePost_SurveyNo(surveyNo).Forget();
			SurveyAgrees.Add(surveyNo);
		}

		public void SetExchangeCode(MiceWebClient.eMiceAccountCardExchangeCode exchangeCode)
		{
			ExchangeCode = exchangeCode;
		}
	}
}
