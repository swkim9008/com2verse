/*===============================================================
* Product:		Com2Verse
* File Name:	Com2VerseApiModel.cs
* Developer:	jhkim
* Date:			2023-03-29 10:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using Com2VerseEditor.UGC.UIToolkitExtension;
using UnityEngine;

namespace Com2verseEditor.WebApi
{
	public class Com2VerseApiModel : EditorWindowExModel
	{
#region Variables
		public static readonly string DefaultSavePath = "../Packages/com.com2verse.c2vapi/Runtime";

		[SerializeField] private string _savePath = DefaultSavePath;
		[SerializeField] private string _c2vId;
		[SerializeField] private string _c2vAccessToken;
		[SerializeField] private string _c2vRefreshToken;

		[SerializeField] private string _serviceAuthUrl;
		[SerializeField] private string _serviceAccessToken;
		[SerializeField] private string _serviceRefreshToken;
		[SerializeField] private string _selectedParamName;
		[SerializeField] private string _apiUrl;
		[SerializeField] private string _apiCategory;
		[SerializeField] private string _apiRequestType;
#endregion // Variables

#region Properties
		public string SavePath
		{
			get => _savePath;
			set => _savePath = value;
		}
		public string C2VId => _c2vId;

		public string C2VAccessToken
		{
			get => _c2vAccessToken;
			set => _c2vAccessToken = value;
		}

		public string C2VRefreshToken
		{
			get => _c2vRefreshToken;
			set => _c2vRefreshToken = value;
		}

		public string ServiceAuthUrl
		{
			get => _serviceAuthUrl;
			set => _serviceAuthUrl = value;
		}
		public string ServiceAccessToken
		{
			get => _serviceAccessToken;
			set => _serviceAccessToken = value;
		}

		public string ServiceRefreshToken
		{
			get => _serviceRefreshToken;
			set => _serviceRefreshToken = value;
		}
		public string SelectedParamName
		{
			set => _selectedParamName = value;
		}

		public string ApiUrl
		{
			get => _apiUrl;
			set => _apiUrl = value;
		}

		public string ApiCategory
		{
			set => _apiCategory = value;
		}

		public string ApiRequestType
		{
			set => _apiRequestType = value;
		}
#endregion // Properties
	}
}
#endif // UNITY_EDITOR
