/*===============================================================
* Product:		Com2Verse
* File Name:	BinderIconDrawer.cs
* Developer:	tlghks1009
* Date:			2023-05-24 10:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.UI;
using Com2VerseEditor.UI;
using UnityEditor;

namespace Com2verseEditor.UI
{
	public class BinderDrawer : HierarchyIconController.HierarchyIconDrawer
	{
		private Binder[]  _binders = null;
		private Texture _errorTexture;

		public BinderDrawer(Texture defaultTexture, Texture errorTexture) : base(defaultTexture)
		{
			_errorTexture = errorTexture;
		}

		public override bool TryInitialize(int instanceId)
		{
			InstanceId = instanceId;

			var objInHierarchy = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

			if (objInHierarchy.IsUnityNull())
			{
				return false;
			}

			AssemblyUtils.RefreshAssembly();
			AssemblyUtils.PrepareViewModelTypes();

			_binders = objInHierarchy.GetComponents<Binder>();

			return _binders.Length != 0;
		}

		public override bool TryDrawHierarchyIcon(Rect selectedRect)
		{
			var texture = DefaultTexture;
			foreach (var binder in _binders)
			{
				if (binder is DataBinder)
				{
					if (!ValidateBinder(binder))
					{
						texture = _errorTexture;
					}
				}
			}

			GUI.DrawTexture(selectedRect, texture);
			return true;
		}

		private bool ValidateBinder(Binder binder)
		{
			var serializedObject    = new SerializedObject(binder);
			var bindingModeProperty = serializedObject.FindProperty("_bindingMode");
			var targetPropertyOwner = serializedObject.FindProperty("_targetPath.propertyOwner");
			var targetProperty      = serializedObject.FindProperty("_targetPath.property");
			var sourcePropertyOwner = serializedObject.FindProperty("_sourcePath.propertyOwner");
			var sourceProperty      = serializedObject.FindProperty("_sourcePath.property");

			return BinderEditor.ValidateType(bindingModeProperty, targetPropertyOwner, sourcePropertyOwner, targetProperty, sourceProperty, binder);
		}
	}
}
