/*===============================================================
* Product:		Com2Verse
* File Name:	UnityToggleEditor.cs
* Developer:	tlghks1009
* Date:			2022-05-24 16:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(Toggle))]
	public sealed class UnityToggleEditor : ToggleEditor
	{
		protected override void OnEnable()
		{
			base.OnEnable();

			var unityButtonComponent = target as Toggle;
			var buttonObject = unityButtonComponent.gameObject;

			var propertyElements = unityButtonComponent.CopySerializedProperties();

			DestroyImmediate(unityButtonComponent);

			var newButtonComponent = buttonObject.AddComponent<MetaverseToggle>();
			newButtonComponent.PasteSerializedProperties(propertyElements);
		}
	}
}
