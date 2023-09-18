/*===============================================================
* Product:		Com2Verse
* File Name:	Define.cs
* Developer:	jhkim
* Date:			2023-03-10 18:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.BannedWords
{
	internal sealed class Define : Base
	{
		private BannedWordsDefines _defines;
		internal override string MakeFileName(AppDefine appDefine) => "bannedwords_define";
		public async UniTask InitializeAsync()
		{
			if (_defines == null)
			{
				if (IsCached(default))
				{
					var jsonStr = await LoadCacheAsync(default);
					try
					{
						_defines = JsonUtility.FromJson<BannedWordsDefines>(jsonStr);
					}
					catch (Exception e)
					{
						C2VDebug.LogWarning(e);
						_defines = new BannedWordsDefines();
					}
				}
				else
				{
					_defines = new BannedWordsDefines();
				}
			}
		}
		public string GetRevision(AppDefine appDefine)
		{
			if (!IsValidAppDefine(appDefine)) return "0";

			CheckAndCreateDefine();
			var find = _defines.Find(appDefine);
			return find.HasValue ? find.Value.Revision : "0";
		}

		public void UpdateAppDefine(AppDefine appDefine)
		{
			if (!IsValidAppDefine(appDefine)) return;

			CheckAndCreateDefine();
			_defines.Update(appDefine);

			SaveAsync().Forget();
		}

		private async UniTask SaveAsync() => await SaveCacheAsync(default, _defines.ToJson());
		private void CheckAndCreateDefine() => _defines ??= new BannedWordsDefines();
	}

	[Serializable]
	public class BannedWordsDefines
	{
		[SerializeField]
		private List<AppDefine> _appDefines;

		public IReadOnlyList<AppDefine> AppDefines;
		public BannedWordsDefines()
		{
			_appDefines = new List<AppDefine>();
		}

		public AppDefine? Find(AppDefine define)
		{
			foreach (var appDefine in _appDefines)
			{
				if (appDefine.AppId == define.AppId && appDefine.Game == define.Game && appDefine.IsStaging == define.IsStaging)
					return appDefine;
			}

			return null;
		}

		public void Update(AppDefine define)
		{
			Remove(define);
			_appDefines.Add(define);
		}

		private void Remove(AppDefine define)
		{
			for (var i = 0; i < _appDefines.Count; i++)
			{
				if (_appDefines[i].AppId == define.AppId && _appDefines[i].Game == define.Game && _appDefines[i].IsStaging == define.IsStaging)
				{
					_appDefines.RemoveAt(i);
					return;
				}
			}
		}

		public string ToJson() => JsonUtility.ToJson(this);
	}
}
