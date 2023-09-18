/*===============================================================
* Product:		Com2Verse
* File Name:	LocalSave.cs
* Developer:	jhkim
* Date:			2022-10-20 09:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Globalization;
using System.IO;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
// ReSharper disable HeuristicUnreachableCode

#pragma warning disable CS0162 // Unreachable code detected

namespace Com2Verse.Utils
{
	public static class LocalSave
	{
		public enum eSavePath
		{
			TEMP,
			PERSISTENT_DATA,
		}

		private static readonly string LogCategory = "LocalSave";

		[NotNull] public static readonly TempLocalSave Temp = new();
		[NotNull] public static readonly TempLocalSave TempGlobal = new();
		[NotNull] public static readonly PersistentLocalSave Persistent = new();
		[NotNull] public static readonly PersistentLocalSave PersistentGlobal = new();
#region Global Function
		public static void DeleteAllLocalSave()
		{
			LocalSaveInternal.DeleteLocalSave(eSavePath.TEMP);
			LocalSaveInternal.DeleteLocalSave(eSavePath.PERSISTENT_DATA);
		}

		public static void SetId(string id)
		{
			Temp.SetId(id);
			Persistent.SetId(id);
		}

		public static void ClearId()
		{
			Temp.ClearId();
			Persistent.ClearId();
		}
#endregion // Global Function

#region LocalSave Internal
		private static class LocalSaveInternal
		{
#region Variables
			private const bool UseEncryption = true;
			private static readonly string FileExt = ".sav";
			private static string _id = string.Empty;
#endregion // Variables

#region Public Methods
#region Load (Sync)
			// Load Internal (Sync)
			public static T LoadJson<T>(string key, eSavePath savePathType) where T : class => LoadJson<T>(GetPath(key, savePathType));
			public static string LoadString(string key, eSavePath savePathType) => LoadInternal(GetPath(key, savePathType), s => s);
			public static int LoadInt(string key, eSavePath savePathType)
			{
				var result = LoadInternal(GetPath(key, savePathType), OnParseData);
				return result;

				int OnParseData(string data)
				{
					int.TryParse(data, out var intResult);
					return intResult;
				}
			}
			public static float LoadFloat(string key, eSavePath savePathType)
			{
				var result = LoadInternal(GetPath(key, savePathType), OnParseData);
				return result;

				float OnParseData(string data)
				{
					float.TryParse(data, out var floatResult);
					return floatResult;
				}
			}
#endregion // Load (Sync)
#region Load (Async)
			// Load Internal (Async)
			public static async UniTask<T> LoadJsonAsync<T>(string key, eSavePath savePathType, Action<T> onLoaded) where T : class => await LoadJsonAsync(GetPath(key, savePathType), onLoaded);
			public static async UniTask<string> LoadStringAsync(string key, eSavePath savePathType, Action<string> onLoaded = null)
			{
				var result = await LoadInternalAsync(GetPath(key, savePathType), s => s);
				onLoaded?.Invoke(result);
				return result;
			}
			public static async UniTask<int> LoadIntAsync(string key, eSavePath savePathType, Action<int> onLoaded = null)
			{
				var result = await LoadInternalAsync(GetPath(key, savePathType), OnParseData);
				onLoaded?.Invoke(result);
				return result;

				int OnParseData(string data)
				{
					int.TryParse(data, out var intResult);
					return intResult;
				}
			}
			public static async UniTask<float> LoadFloatAsync(string key, eSavePath savePathType, Action<float> onLoaded = null)
			{
				var result = await LoadInternalAsync(GetPath(key, savePathType), OnParseData);
				onLoaded?.Invoke(result);
				return result;

				float OnParseData(string data)
				{
					float.TryParse(data, out var floatResult);
					return floatResult;
				}
			}
#endregion // Load (Async)
#region Save
			public static bool Save(string key, eSavePath savePathType, string serialized)
			{
				if (string.IsNullOrWhiteSpace(serialized)) return false;

				var path = GetPath(key, savePathType);
				CreateDir(path);

				try
				{
					if (UseEncryption)
					{
						var base64Str = Security.Instance.EncryptAes(serialized);
						return FileHandleManager.WriteToStream(path, base64Str);
					}
					else
					{
						return FileHandleManager.WriteToStream(path, serialized);
					}
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Save Failed...\n{e}");
					return false;
				}
			}
			public static async UniTask SaveAsync(string key, eSavePath savePathType, string serialized)
			{
				if (string.IsNullOrWhiteSpace(serialized)) return;

				var path = GetPath(key, savePathType);
				CreateDir(path);

				try
				{
					if (UseEncryption)
					{
						var base64Str = await Security.Instance.EncryptAesAsync(serialized);
						await FileHandleManager.WriteToStreamAsync(path, base64Str);
					}
					else
					{
						await FileHandleManager.WriteToStreamAsync(path, serialized);
					}
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Save Failed...\n{e}");
				}
			}
#endregion // Save
#region Delete
			public static void Delete(string key, eSavePath savePathType)
			{
				var path = GetPath(key, savePathType);
				FileHandleManager.DeleteFileAsync(path).Forget();
			}
			public static async UniTask<bool> DeleteAsync(string key, eSavePath savePathType)
			{
				var path = GetPath(key, savePathType);
				var result = await FileHandleManager.DeleteFileAsync(path);
				return result;
			}
#endregion // Delete

			public static void SetId(string id) => _id = id ?? string.Empty;
			public static void ClearId() => _id = string.Empty;

			public static bool IsExist(string key, eSavePath savePathType) => File.Exists(GetPath(key, savePathType));
			public static string ToJson<T>(T data) where T : class => data == null ? string.Empty : JsonConvert.SerializeObject(data);
			[NotNull]
			public static string GetBaseDir(eSavePath savePathType)
			{
				switch (savePathType)
				{
					case eSavePath.PERSISTENT_DATA:
						return DirectoryUtil.GetPersistentDataPath("LocalSave");
					case eSavePath.TEMP:
					default:
						return DirectoryUtil.GetTempPath("LocalSave");
				}
			}
#endregion // Public Methods

#region Private Methods
#region Load
			private static T LoadJson<T>(string path, Action<T> onLoaded = null) where T : class
			{
				if (!File.Exists(path))
				{
					onLoaded?.Invoke(null);
					return null;
				}

				try
				{
					var jsonStr = GetJsonStr(path);
					if (string.IsNullOrWhiteSpace(jsonStr))
					{
						C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...Json is Empty");
						onLoaded?.Invoke(null);
						return null;
					}

					var obj = JsonConvert.DeserializeObject<T>(jsonStr);
					onLoaded?.Invoke(obj);
					return obj;
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...\n{e}");
					onLoaded?.Invoke(null);
					return null;
				}
			}

			private static string GetJsonStr(string filePath)
			{
				var json = string.Empty;
				if (UseEncryption)
				{
					try
					{
						var encrypted = FileHandleManager.ReadFromStream(filePath);
						var result = Security.Instance.DecryptToString(encrypted);
						if (result is {Status: Security.eDecryptStatus.SUCCESS})
						{
							json = result.Value;
							if (json == string.Empty)
								FileHandleManager.DeleteFileAsync(filePath).Forget();
						}
						else
						{
							FileHandleManager.DeleteFileAsync(filePath).Forget();
						}
					}
					catch (Exception)
					{
						FileHandleManager.DeleteFileAsync(filePath).Forget();
					}
				}
				else
				{
					json = FileHandleManager.ReadFromStream(filePath);
					if (json == string.Empty)
						FileHandleManager.DeleteFileAsync(filePath).Forget();
				}

				return json;
			}

			private static T LoadInternal<T>(string path, Func<string, T> onParseData)
			{
				if (!File.Exists(path) || onParseData == null)
					return default;

				try
				{
					var readData = FileHandleManager.ReadFromStream(path);
					if (UseEncryption)
					{
						var result = Security.Instance.DecryptToString(readData);
						if (result is {Status: Security.eDecryptStatus.SUCCESS})
						{
							var value = result.Value;
							if (value == null)
								FileHandleManager.DeleteFileAsync(path).Forget();
							else
								return onParseData.Invoke(value);
						}
						else
						{
							FileHandleManager.DeleteFileAsync(path).Forget();
						}

						return default;
					}
					else
					{
						return onParseData.Invoke(readData);
					}
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...\n{e}");
					FileHandleManager.DeleteFileAsync(path).Forget();
					return default;
				}
			}
#endregion // Load
#region Load (Async)
			private static async UniTask<T> LoadJsonAsync<T>(string path, Action<T> onLoaded) where T : class
			{
				if (!File.Exists(path))
				{
					onLoaded?.Invoke(null);
					return null;
				}

				try
				{
					var jsonStr = await GetJsonStrAsync(path);
					if (string.IsNullOrWhiteSpace(jsonStr))
					{
						C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...Json is Empty");
						onLoaded?.Invoke(null);
						return null;
					}

					var obj = JsonConvert.DeserializeObject<T>(jsonStr);
					onLoaded?.Invoke(obj);
					return obj;
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...\n{e}");
					onLoaded?.Invoke(null);
					return null;
				}
			}

			private static async UniTask<string> GetJsonStrAsync(string filePath)
			{
				var json = string.Empty;
				if (UseEncryption)
				{
					try
					{
						var encrypted = await FileHandleManager.ReadFromStreamAsync(filePath);
						var result = await Security.Instance.DecryptToStringAsync(encrypted);
						if (result is {Status: Security.eDecryptStatus.SUCCESS})
						{
							json = result.Value;
							if (json == string.Empty)
								await FileHandleManager.DeleteFileAsync(filePath);
						}
						else
						{
							await FileHandleManager.DeleteFileAsync(filePath);
						}
					}
					catch (Exception)
					{
						await FileHandleManager.DeleteFileAsync(filePath);
					}
				}
				else
				{
					json = await FileHandleManager.ReadFromStreamAsync(filePath);
					if (json == string.Empty)
						await FileHandleManager.DeleteFileAsync(filePath);
				}

				return json;
			}

			private static async UniTask<T> LoadInternalAsync<T>(string path, Func<string, T> onParseData)
			{
				if (!File.Exists(path) || onParseData == null)
					return default;

				try
				{
					var readData = await FileHandleManager.ReadFromStreamAsync(path);
					if (UseEncryption)
					{
						var result = await Security.Instance.DecryptToStringAsync(readData);
						if (result is {Status: Security.eDecryptStatus.SUCCESS})
						{
							var value = result.Value;
							if (value == null)
								await FileHandleManager.DeleteFileAsync(path);
							else
								return onParseData.Invoke(value);
						}
						else
						{
							await FileHandleManager.DeleteFileAsync(path);
						}

						return default;
					}
					else
					{
						return onParseData.Invoke(readData);
					}
				}
				catch (Exception e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Load Failed...\n{e}");
					await FileHandleManager.DeleteFileAsync(path);
					return default;
				}
			}
#endregion // Load (Async)

			public static string GetPath(string key, eSavePath savePathType)
			{
				if (string.IsNullOrWhiteSpace(key))
					return string.Empty;

				return string.IsNullOrWhiteSpace(_id) ? $"{Path.Combine(GetBaseDir(savePathType), key)}{FileExt}" : $"{Path.Combine(GetBaseDir(savePathType), _id, key)}{FileExt}";
			}

			private static void CreateDir(string path)
			{
				if (string.IsNullOrWhiteSpace(path)) return;

				var dir = Path.GetDirectoryName(path);
				if (string.IsNullOrWhiteSpace(dir)) return;

				Directory.CreateDirectory(dir);
			}
			public static void DeleteLocalSave(eSavePath path)
			{
				var baseDir = GetBaseDir(path);
				if (string.IsNullOrWhiteSpace(baseDir)) return;

				var files = Directory.GetFiles(baseDir);
				foreach (var filePath in files)
				{
					try
					{
						File.Delete(filePath);
					}
					catch (Exception e)
					{
						C2VDebug.LogWarningCategory(LogCategory, $"Delete LocalSave Failed...\n{e}");
					}
				}
			}
#endregion // Private Methods
		}
#endregion // LocalSave Internal

#region LocalSave Wrapped
		public abstract class BaseLocalSave
		{
			protected eSavePath SavePath = eSavePath.TEMP;
			private string _id = string.Empty;

			// Load (Sync)
			public T LoadJson<T>(string key) where T : class
			{
				SetId(_id);
				return LocalSaveInternal.LoadJson<T>(key, SavePath);
			}

			public string LoadString(string key)
			{
				SetId(_id);
				return LocalSaveInternal.LoadString(key, SavePath);
			}

			public int LoadInt(string key)
			{
				SetId(_id);
				return LocalSaveInternal.LoadInt(key, SavePath);
			}

			public float LoadFloat(string key)
			{
				SetId(_id);
				return LocalSaveInternal.LoadFloat(key, SavePath);
			}

			// Load (Async)
			public async UniTask<T> LoadJsonAsync<T>(string key, Action<T> onLoaded = null) where T : class
			{
				SetId(_id);
				return await LocalSaveInternal.LoadJsonAsync(key, SavePath, onLoaded);
			}

			public async UniTask<string> LoadStringAsync(string key, Action<string> onLoaded = null)
			{
				SetId(_id);
				return await LocalSaveInternal.LoadStringAsync(key, SavePath, onLoaded);
			}

			public async UniTask<int> LoadIntAsync(string key, Action<int> onLoaded = null)
			{
				SetId(_id);
				return await LocalSaveInternal.LoadIntAsync(key, SavePath, onLoaded);
			}

			public async UniTask<float> LoadFloatAsync(string key, Action<float> onLoaded = null)
			{
				SetId(_id);
				return await LocalSaveInternal.LoadFloatAsync(key, SavePath, onLoaded);
			}

			// Save (Sync)
			public bool SaveJson<T>(string key, T data) where T : class
			{
				SetId(_id);
				return LocalSaveInternal.Save(key, SavePath, LocalSaveInternal.ToJson(data));
			}

			public bool SaveString(string key, string data)
			{
				SetId(_id);
				return LocalSaveInternal.Save(key, SavePath, data);
			}

			public bool SaveInt(string key, int data)
			{
				SetId(_id);
				return LocalSaveInternal.Save(key, SavePath, Convert.ToString(data));
			}

			public bool SaveFloat(string key, float data)
			{
				SetId(_id);
				return LocalSaveInternal.Save(key, SavePath, Convert.ToString(data, CultureInfo.InvariantCulture));
			}

			// Save (Async)
			public async UniTask SaveJsonAsync<T>(string key, T data) where T : class
			{
				SetId(_id);
				await LocalSaveInternal.SaveAsync(key, SavePath, LocalSaveInternal.ToJson(data));
			}

			public async UniTask SaveStringAsync(string key, string data)
			{
				SetId(_id);
				await LocalSaveInternal.SaveAsync(key, SavePath, data);
			}

			public async UniTask SaveIntAsync(string key, int data)
			{
				SetId(_id);
				await LocalSaveInternal.SaveAsync(key, SavePath, data.ToString());
			}

			public async UniTask SaveFloatAsync(string key, float data)
			{
				SetId(_id);
				await LocalSaveInternal.SaveAsync(key, SavePath, Convert.ToString(data, CultureInfo.InvariantCulture));
			}

			// Delete
			public void Delete(string key)
			{
				SetId(_id);
				LocalSaveInternal.Delete(key, SavePath);
			}

			public async UniTask<bool> DeleteAsync(string key)
			{
				SetId(_id);
				return await LocalSaveInternal.DeleteAsync(key, SavePath);
			}

			public void SetId(string id)
			{
				_id = id ?? string.Empty;
				LocalSaveInternal.SetId(id);
			}

			public void ClearId()
			{
				_id = string.Empty;
				LocalSaveInternal.ClearId();
			}

			public string GetPath(string key)
			{
				SetId(_id);
				return LocalSaveInternal.GetPath(key, SavePath);
			}

			public bool IsExist(string key)
			{
				SetId(_id);
				return LocalSaveInternal.IsExist(key, SavePath);
			}

			public string GetBaseDir() => LocalSaveInternal.GetBaseDir(SavePath);
		}

		public class TempLocalSave : BaseLocalSave
		{
			public TempLocalSave() => SavePath = eSavePath.TEMP;
		}

		public class PersistentLocalSave : BaseLocalSave
		{
			public PersistentLocalSave() => SavePath = eSavePath.PERSISTENT_DATA;
		}
#endregion // LocalSave Wrapped
	}
}
