/*===============================================================
* Product:    Com2Verse
* File Name:  DrawIfAttribute.cs
* Developer:  hyj
* Date:       2022-04-28 14:03
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.Utils
{
	/// <summary>
	/// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
	/// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public sealed class DrawIfAttribute : PropertyAttribute
	{
		public string ComparedPropertyName { get; private set; }
		public object ComparedValue { get; private set; }
		public eDisablingType DisablingType { get; private set; }
		public bool InvertCondition { get; set; }

		public enum eDisablingType
		{
			READ_ONLY = 2,
			DONT_DRAW = 3
		}

		/// <summary>
		/// Only draws the field only if a condition is met. Supports enum and bool.
		/// </summary>
		/// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
		/// <param name="comparedValue">The value the property is being compared to.</param>
		/// <param name="disablingType">The type of disabling that should happen if the condition is NOT met. Defaulted to eDisablingType.DONTDRAW.</param>
		/// <param name="invertCondition">Inverts the condition.</param>
		public DrawIfAttribute(string comparedPropertyName, object comparedValue, eDisablingType disablingType = eDisablingType.DONT_DRAW, bool invertCondition = false)
		{
			ComparedPropertyName = comparedPropertyName;
			ComparedValue= comparedValue;
			DisablingType = disablingType;
			InvertCondition = invertCondition;
		}
	}
}
