/*===============================================================
* Product:    Com2Verse
* File Name:  Util.cs
* Developer:  hyj
* Date:       2022-05-02 12:38
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Text.RegularExpressions;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Utils
{
	public static class Util
	{
		private const string AsciiPattern = @"[\u0000-\u007F]";

		[NotNull]
		public static T GetOrAddComponent<T>([NotNull] GameObject go) where T : UnityEngine.Component
		{
			bool hasComponent            = go.TryGetComponent(out T component);
			if (!hasComponent) component = go.AddComponent<T>();
			return component!;
		}

		public static void UndoDontDestroyOnLoad([NotNull] GameObject go)
		{
			go.transform.SetParent(null);
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
		}

		public static void ChangeLayersRecursively([NotNull] Transform trans, int layer)
		{
			trans!.gameObject.layer = layer;
			foreach (Transform child in trans)
			{
				if (child.IsReferenceNull()) continue;

				child!.gameObject.layer = layer;
			}
		}

		/// <summary>
		/// ascii 1글자당 1, 그 외 1글자당 2의길이를 구하기 위한 메서드
		/// 현재 C# 제공 인코딩 방식에서 위와 같은 규칙을 가진 것은 없는것으로 판단되어 직접 구현
		/// </summary>
		/// <param name="value">길이를 계산할 스트링</param>
		/// <returns>계산된 길이</returns>
		public static int GetAsciiLength([CanBeNull] string value)
		{
			if (string.IsNullOrEmpty(value!))
				return 0;

			var stringLength = value.Length;
			var asciiMatches = Regex.Matches(value, AsciiPattern);
			var asciiLength  = asciiMatches.Count;

			return asciiLength + (stringLength - asciiLength) * 2;
		}

		public static int StringToBitmaskWithComma(string value)
		{
			var result = 0;
			try
			{
				var split = value.Split(',');
				foreach (string s in split)
					if (int.TryParse(s, out int i))
						result |= Mathf.FloorToInt(Mathf.Pow(2, i - 1));
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
				return 0;
			}

			return result;
		}

		public static Vector3 StringToVector3(string stringVector)
		{
			try
			{
				if (stringVector.StartsWith("(") && stringVector.EndsWith(")"))
					stringVector = stringVector.Substring(1, stringVector.Length - 2);

				var stringArray = stringVector.Split(',');

				var x = stringArray.Length >= 1 && stringArray[0] != string.Empty ? float.Parse(stringArray[0]) : 0;
				var y = stringArray.Length >= 2 && stringArray[1] != string.Empty ? float.Parse(stringArray[1]) : 0;
				var z = stringArray.Length >= 3 && stringArray[2] != string.Empty ? float.Parse(stringArray[2]) : 0;

				return new Vector3(x, y, z);
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
				return Vector3.zero;
			}
		}

		public static Vector2 StringToVector2(string stringVector)
		{
			try
			{
				if (stringVector.StartsWith("(") && stringVector.EndsWith(")"))
					stringVector = stringVector.Substring(1, stringVector.Length - 2);

				string[] stringArray = stringVector.Split(',');

				float x = stringArray.Length >= 1 && stringArray[0] != string.Empty ? float.Parse(stringArray[0]) : 0;
				float y = stringArray.Length >= 2 && stringArray[1] != string.Empty ? float.Parse(stringArray[1]) : 0;

				return new Vector2(x, y);
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
				return Vector2.zero;
			}
		}
	}
}
