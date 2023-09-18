/*===============================================================
* Product:		Com2Verse
* File Name:	ViewModelUsageAttribute.cs
* Developer:	urun4m0r1
* Date:			2022-11-21 12:16
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Utils
{
	/// <summary>
	/// 프로퍼티가 어느 ViewModel 에서 사용되는지 Annotation 해주기 위한 어트리뷰트.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ViewModelUsageAttribute : Attribute
	{
		// ReSharper disable once UnusedParameter.Local
		public ViewModelUsageAttribute(string _) { }
	}
}
