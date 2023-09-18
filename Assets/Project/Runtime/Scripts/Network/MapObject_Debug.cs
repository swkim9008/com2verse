/*===============================================================
* Product:		Com2Verse
* File Name:	MapObject_Debug.cs
* Developer:	eugene9721
* Date:			2022-07-29 12:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.CameraSystem;
using Com2Verse.Chat;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if ENABLE_CHEATING
namespace Com2Verse.Network
{
	public partial class MapObject
	{
		[ContextMenu("ChangeCameraTarget")]
		public void ChangeCameraTarget()
		{
			CameraManager.Instance.ChangeTarget(transform);
		}

		[ContextMenu("CreateSpeechBubble")]
		public void CreateSpeechBubble()
		{
			const ChatCoreBase.eMessageType type = ChatCoreBase.eMessageType.AREA;

			const string message = "품었기 위하여 이상이 보이는 아니다. 남는 아름답고 싸인 부패뿐이다. 봄날의 않는 품었기 보라.";

			ChatManager.Instance.CreateSpeechBubble(this, message, type);
		}

#if UNITY_EDITOR
		private GUIStyle _style = new();

		[Tooltip("'PlayGesture' ContextMenu를 통해 제스처를 재생합니다.")]
		[SerializeField] private AnimationClip _gestureClip;

		private void AwakeOnDebugEditor()
		{
			_style.normal.textColor =  Color.blue;
		}

		private void OnDrawGizmosSelected()
		{
			Handles.Label(transform.position + Vector3.up * 2, $"Serial {ObjectID}\nOwner {OwnerID}\nDistance {_distance}\nCell({CellIndex.x},{CellIndex.y})", _style);
		}

		[ContextMenu("PlayGesture")]
		public void PlayGestureTest()
		{
			if (this is not ActiveObject activeObject) return;
			if (_gestureClip.IsUnityNull()) return;
			if (activeObject.AnimatorController.IsUnityNull()) return;

			activeObject.AnimatorController!.PlayGestureAsync(_gestureClip!).Forget();
		}
#endif // UNITY_EDITOR
	}
}
#endif // ENABLE_CHEATING
