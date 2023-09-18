/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObject_Tag.cs
* Developer:	haminjeong
* Date:			2023-07-26 20:01
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Extension;

namespace Com2Verse.Network
{
	public partial class ActiveObject
	{
		private static readonly string CurrentUseObjectIDKey = "CurInteractionUseObjects";

		private readonly List<long> _prevUseObjectIDList = new();
		private readonly List<long> _useObjectIDList     = new();
		
		protected override void TagChanged()
		{
			TagChangedProcess(CurrentUseObjectIDKey);
			RefreshConferenceObjectType();
		}

		private void TagChangedProcess(string tagKey)
		{
			var tagValue = GetStringFromTags(tagKey);
			if (string.IsNullOrEmpty(tagValue))
			{
				_prevUseObjectIDList!.ForEach((objectID) =>
				{
					var targetObject = MapController.Instance.GetStaticObjectByID(objectID);
					if (targetObject.IsUnityNull()) return;
					TagProcessorManager.Instance.UpdateUseObjectProcess(ObjectUtil.GetObjectType(targetObject!.ObjectTypeId), this, false);
				});
				_prevUseObjectIDList.Clear();
				_useObjectIDList!.Clear();
				return;
			}

			string[] split = tagValue.Split(",");
			if (split == null) return;
			_useObjectIDList!.Clear();
			for (int i = 0; i < split.Length; ++i)
			{
				if (long.TryParse(split[i], out var objectID))
					_useObjectIDList.Add(objectID);
			}

			_prevUseObjectIDList!.ForEach((objectID) =>
			{
				if (_useObjectIDList.Contains(objectID)) return;
				var targetObject = MapController.Instance.GetStaticObjectByID(objectID);
				if (targetObject.IsUnityNull()) return;
				TagProcessorManager.Instance.UpdateUseObjectProcess(ObjectUtil.GetObjectType(targetObject!.ObjectTypeId), this, false);
			});
			_useObjectIDList!.ForEach((objectID) =>
			{
				if (_prevUseObjectIDList.Contains(objectID)) return;
				var targetObject = MapController.Instance.GetStaticObjectByID(objectID);
				if (targetObject.IsUnityNull()) return;
				TagProcessorManager.Instance.UpdateUseObjectProcess(ObjectUtil.GetObjectType(targetObject!.ObjectTypeId), this, true);
			});

			_prevUseObjectIDList.Clear();
			_prevUseObjectIDList.AddRange(_useObjectIDList);
		}
	}
}
