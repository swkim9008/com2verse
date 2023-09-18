/*===============================================================
* Product:		Com2Verse
* File Name:	TableDataManager.cs
* Developer:	haminjeong
* Date:			2023-08-02 13:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.Data
{
	public sealed class TableDataManager : Singleton<TableDataManager>, IDisposable
	{
		[UsedImplicitly] private TableDataManager() { }

		private readonly Dictionary<Type, object> _tableDictionary = new();

		[CanBeNull] public T Get<T>() where T : Loader<T> => _tableDictionary!.TryGetValue(typeof(T), out var value) ? value as T : null;

		public async UniTask InitializeAsync()
		{
			var assembly = Assembly.Load("Com2Verse.TableData");
			List<Type> types = new();
			foreach (var type in assembly.GetTypes())
			{
				if (!type.Name.Contains("Table") || type.Name.Contains("Formatter")) continue;
				types.Add(type);
			}
			await LoadTableAsync(types);
		}

		public async UniTask InitializeSettingTableAsync()
		{
			var assembly = Assembly.Load("Com2Verse.TableData");
			List<Type> types = new();
			foreach (var type in assembly.GetTypes())
			{
				if (!type.Name.Contains("Table") || type.Name.Contains("Formatter")) continue;
				if (type.Name == "TableSetting")
				{
					types.Add(type);
					break;
				}
			}
			await LoadTableAsync(types);
		}

		public void Dispose()
		{
			_tableDictionary!.Clear();
		}

		private async UniTask LoadTableAsync(List<Type> type)
		{
			var resultInfos = await DataLoader.LoadFromBundlesAsync(type!.ToArray());

			foreach (var resultInfo in resultInfos)
			{
				if (resultInfo.Success)
					_tableDictionary!.TryAdd(resultInfo.Type, resultInfo.Data);
			}
		}
	}
}
