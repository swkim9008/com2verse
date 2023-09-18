/*===============================================================
* Product:		Com2Verse
* File Name:	SingletonHelper.cs
* Developer:	urun4m0r1
* Date:			2022-10-20 11:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Reflection;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse
{
	internal static class SingletonHelper<T> where T : class
	{
		public static string BaseTypeName => typeof(T).BaseType.Name.Replace("`1", "");
		public static string TypeName     => typeof(T).Name;

		public static string GenericTypeName => $"{BaseTypeName}<T>";
		public static string ClassName       => $"{BaseTypeName}<{TypeName}>";

#region Exceptions
		public static InvalidOperationException InstanceNullException            => new($"{ClassName} instance is null.");
		public static InvalidOperationException InstanceNullOrDestroyedException => new($"{ClassName} instance is null or destroyed.");
		public static InvalidOperationException InstanceAlreadyCreatedException  => new($"{ClassName} instance already created.");
#endregion // Exceptions

#region InstanceGeneration
		public static Lazy<T> GenerateThreadSafeLazyInstance() => GenerateThreadSafeLazyInstance(GenerateInstance);

		public static Lazy<T> GenerateThreadSafeLazyInstance(Func<T> valueFactory) => new(valueFactory, SingletonDefine.InstanceSafetyMode);

		public static T GenerateInstance()
		{
			if (SingletonHelper.GetConstructor(typeof(T)).Invoke(null!) is not T instance)
			{
				throw new MissingMethodException($"{ClassName} has a non-public constructor that does not return an instance of {TypeName}.");
			}

			LogInstanceCreated();
			return instance;
		}
#endregion // InstanceGeneration

#region Logging
		public static void LogInstanceAlreadyCreated()
		{
			C2VDebug.LogWarningCategory(GenericTypeName, $"<color=yellow>@ Instance already exists:</color> <{TypeName}>");
		}

		public static void LogInstanceCreated()
		{
			C2VDebug.LogCategory(GenericTypeName, $"<color=red>+ Instance created:</color> <{TypeName}>");
		}

		public static void LogInstanceDestroyed()
		{
			C2VDebug.LogCategory(GenericTypeName, $"<color=blue>- Instance destroyed:</color> <{TypeName}>");
		}
#endregion // Logging

#region Validation
		public static void EnsureApplicationIsPlaying()
		{
#if UNITY_EDITOR
			if (SingletonDefine.AllowEditModeInstanceCreation)
				return;

			if (!Application.isPlaying)
			{
				throw new InvalidOperationException($"{ClassName} cannot be created while the application is not playing or quitting.");
			}
#endif
		}
#endregion // Validation
	}

	internal static class SingletonHelper
	{
		/// <summary>
		/// Get unique non-public parameterless constructor of the specified type.
		/// </summary>
		public static ConstructorInfo GetConstructor(Type type)
		{
			var publicConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			if (publicConstructors.Length > 0)
			{
				if (!SingletonDefine.AllowDefaultPublicConstructor)
				{
					throw new NotSupportedException($"{type.Name} has {publicConstructors.Length} public constructors (or default). No public constructors are allowed.");
				}

				if (publicConstructors.Length != 1)
				{
					throw new NotSupportedException($"{type.Name} has {publicConstructors.Length} public constructors. Only one public constructor is allowed.");
				}

				var publicParameters = publicConstructors[0].GetParameters();
				if (publicParameters.Length > 0)
				{
					throw new NotSupportedException($"{type.Name} has a public constructor with {publicParameters.Length} parameters. No parameters are allowed.");
				}

				return publicConstructors[0];
			}

			var nonPublicConstructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			if (nonPublicConstructors.Length != 1)
			{
				throw new NotSupportedException($"{type.Name} has {nonPublicConstructors.Length} non-public constructors. Only one non-public constructor is allowed.");
			}

			var parameters = nonPublicConstructors[0].GetParameters();
			if (parameters.Length > 0)
			{
				throw new NotSupportedException($"{type.Name} has a non-public constructor with {parameters.Length} parameters. No parameters are allowed.");
			}

			return nonPublicConstructors[0];
		}
	}
}
