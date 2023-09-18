/*===============================================================
* Product:		Com2Verse
* File Name:	BundleLoader.cs
* Developer:	jhkim
* Date:			2022-06-17 11:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Com2Verse.AssetSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Data
{
	public static class DataLoader
	{
		private static readonly string LocalTablePath = Path.Combine(Application.dataPath!, "External/LocalTable");
		[NotNull] private static readonly string TableName = "Table";
		private static bool IsValidDataTable(Type type) => IsValidDataTable(type?.Name);
		private static bool IsValidDataTable(string typeName) => !string.IsNullOrWhiteSpace(typeName) && typeName.StartsWith(TableName);
		private static string GetBundleName<T>() => GetBundleName(typeof(T));
		private static string GetBundleName(Type type) => $"{type.Name.Replace("Table", string.Empty)}.bytes";

		public static async UniTask<T> LoadFromBundleAsync<T>() where T : Loader<T> => await LoadFromBundleAsync<T>(GetBundleName<T>());

		public static async UniTask<T> LoadFromBundleAsync<T>(string assetName) where T : Loader<T>
		{
			if (IsValidDataTable(typeof(T)))
			{
#if UNITY_EDITOR
				var path = Path.Combine(LocalTablePath!, assetName);
				if (File.Exists(path))
				{
					var bytes = await File.ReadAllBytesAsync(path);
					if (bytes != null)
						return await Loader<T>.LoadAsync(bytes);
				}

				var textAsset = await C2VAddressables.LoadAssetAsync<TextAsset>(assetName).ToUniTask();
#else
				var textAsset = await C2VAddressables.LoadAssetAsync<TextAsset>(assetName).ToUniTask();
#endif
				if (textAsset && textAsset.bytes != null)
					return await Loader<T>.LoadAsync(textAsset.bytes);
			}

			return null;
		}
		
		public static async UniTask<Loader.ResultInfo[]> LoadFromBundlesAsync(params Type[] types)
		{
			var tasks = new List<Task<Loader.ResultInfo>>();
			foreach (var type in types)
			{
				if (IsValidDataTable(type))
				{
					var assetName = GetBundleName(type);
#if UNITY_EDITOR
					var path = Path.Combine(LocalTablePath!, assetName);
					if (File.Exists(path))
					{
						tasks.Add(LoadLocalFile(type, path));
						continue;
					}
					tasks.Add(LoadAddressableFile(type, assetName));
#else
					tasks.Add(LoadAddressableFile(type, assetName));
#endif
				}
			}

			var addressableResults = await Task.WhenAll(tasks)!;
			tasks.Clear();
			foreach (var result in addressableResults!)
			{
				if (result.Data != null)
					tasks.Add(Loader.LoadAsync(result.Type, (byte[])result.Data));
			}

			if (tasks.Count == 0)
				return Array.Empty<Loader.ResultInfo>();

			var resultInfos = await Task.WhenAll(tasks)!;
			return resultInfos;
		}

		private static async Task<Loader.ResultInfo> LoadAddressableFile(Type type, string assetName)
		{
			var asset = await C2VAddressables.LoadAssetAsync<TextAsset>(assetName).ToUniTask();
			return new Loader.ResultInfo { Success = true, Data = asset.bytes, Type = type };
		}

		private static async Task<Loader.ResultInfo> LoadLocalFile(Type type, string path)
		{
			var bytes = await File.ReadAllBytesAsync(path);
			return new Loader.ResultInfo { Success = true, Data = bytes, Type = type };
		}

		public static async UniTask<Loader.ResultInfo[]> LoadFromBundleAsync(params (Type, string)[] loadInfos)
		{
			var tasks = new List<Task<Loader.ResultInfo>>();
			foreach (var info in loadInfos)
			{
				var type = info.Item1;
				if (IsValidDataTable(type))
				{
					var assetName = info.Item2;
#if UNITY_EDITOR
					var path = Path.Combine(LocalTablePath!, assetName);
					if (File.Exists(path))
					{
						var bytes = await File.ReadAllBytesAsync(path);
						if (bytes != null)
							tasks.Add(Loader.LoadAsync(type, bytes));
						continue;
					}
					var textAsset = await C2VAddressables.LoadAssetAsync<TextAsset>(assetName).ToUniTask();
#else
					var textAsset = await C2VAddressables.LoadAssetAsync<TextAsset>(assetName).ToUniTask();
#endif
					if (textAsset && textAsset.bytes != null) 
						tasks.Add(Loader.LoadAsync(type, textAsset.bytes));
				}
			}

			if (tasks.Count == 0)
				return Array.Empty<Loader.ResultInfo>();

			var resultInfos = await Task.WhenAll(tasks)!;
			return resultInfos;
		}
	}
}
