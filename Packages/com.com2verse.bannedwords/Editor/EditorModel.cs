/*===============================================================
* Product:		Com2Verse
* File Name:	EditorModel.cs
* Developer:	jhkim
* Date:			2023-03-10 10:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2VerseEditor.UGC.UIToolkitExtension;
using UnityEngine;

namespace Com2Verse.BannedWords
{
	public class EditorModel : EditorWindowExModel
	{
		[SerializeField]
		private string _appId = "com.com2us.aaa";
		[SerializeField]
		private string _gameName = "default";
		[SerializeField]
		private string _revision = "0";
		[SerializeField]
		private bool _isStaging = false;

		[SerializeField]
		private string _lang = "All";
		[SerializeField]
		private string _country = "All";
		[SerializeField]
		private string _usage = "All";

		[SerializeField]
		private string _inputBannedWord;
		[SerializeField]
		private string _inputReplace = "*";
		[SerializeField]
		private string _inputBannedWordResult;
		[SerializeField]
		private string _inputFind;

		public string AppId => _appId;
		public string GameName => _gameName;
		public string Revision => _revision;
		public bool IsStaging
		{
			get => _isStaging;
			set => _isStaging = value;
		}

		public string InputBannedWord
		{
			get => _inputBannedWord;
			set => _inputBannedWord = value;
		}

		public string InputReplace
		{
			get => _inputReplace;
			set => _inputReplace = value;
		}

		public string InputBannedWordResult
		{
			get => _inputBannedWordResult;
			set => _inputBannedWordResult = value;
		}

		public string InputFind
		{
			get => _inputFind;
			set => _inputFind = value;
		}

		public string Lang
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_lang))
					_lang = "All";
				return _lang;
			}
			set => _lang = value;
		}

		public string Country
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_country))
					_country = "All";
				return _country;
			}
			set => _country = value;
		}

		public string Usage
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_usage))
					_usage = "All";
				return _usage;
			}
			set => _usage = value;
		}
		public BannedWordsInfo Info { get; set; }
	}
}
