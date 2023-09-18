/*===============================================================
* Product:		Com2Verse
* File Name:	SerializedPropertyListExtenstion.cs
* Developer:	tlghks1009
* Date:			2022-05-13 15:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	public static class UnityObjectExtension
	{
		public static List<SerializedProperty> CopySerializedProperties(this Object sourceObject)
		{
			var source = new SerializedObject(sourceObject);
			var propertyElements = new List<SerializedProperty>();

			SerializedProperty propIterator = source.GetIterator();
			if (propIterator.NextVisible(true))
			{
				while (propIterator.NextVisible(true))
				{
					if (propIterator == null)
						continue;

					propertyElements.Add(source.FindProperty(propIterator.name));
				}
			}

			return propertyElements;
		}

		public static void PasteSerializedProperties( this Object destObject, List<SerializedProperty> sourcePropertyElements )
		{
			SerializedObject dest = new SerializedObject(destObject);

			foreach (var sourceSerializedProperty in sourcePropertyElements)
			{
				if (sourceSerializedProperty == null)
					continue;

				var destElement = dest.FindProperty(sourceSerializedProperty.name);
				if (destElement != null)
				{
					dest.CopyFromSerializedProperty(sourceSerializedProperty);
				}
			}
			dest.ApplyModifiedProperties();
		}
	}
}
