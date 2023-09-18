/*===============================================================
* Product:		Com2Verse
* File Name:	MiceService_Operator.cs
* Developer:	wlemon
* Date:			2023-06-08 15:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.Avatar;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Mice
{
	public sealed partial class MiceService
	{
		private static readonly eMiceCameraJigKey[] OperatorCameraSequenceKeys = new[]
			{ eMiceCameraJigKey.MICE_HALL_ALL_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_LEFT_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_RIGHT_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_AUDIENCE_VIEW, eMiceCameraJigKey.MICE_HALL_WIDE_VIEW };
		private const int OperatorCameraSequenceTransitionTime = 5;

		private CancellationTokenSource _ctsOperatorCameraSequence   = default;
		private int                     _operatorCameraSequenceIndex = 0;
		public bool IsPlayingOperatorCameraSequence { get; private set; }
		
		private CancellationTokenSource _ctsOperatorQuestionerSequence = default;
		private AvatarController        _questionerAvatar              = default;
		public  bool                    IsPlayingOperatorQuestionerSequence { get; private set; }

		public void StartOperatorCameraSequence()
		{
			StopOperatorCameraSequence();
			_ctsOperatorCameraSequence      = new CancellationTokenSource();
			IsPlayingOperatorCameraSequence = true;
			Run(_ctsOperatorCameraSequence.Token).Forget();

			async UniTask Run(CancellationToken token)
			{
				while (!token.IsCancellationRequested)
				{
					CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, OperatorCameraSequenceKeys[_operatorCameraSequenceIndex].ToString());
					_operatorCameraSequenceIndex++;
					if (_operatorCameraSequenceIndex >= OperatorCameraSequenceKeys.Length)
					{
						_operatorCameraSequenceIndex = 0;
					}

					await UniTask.Delay(OperatorCameraSequenceTransitionTime * 1000, true, PlayerLoopTiming.Update, token);
				}
			}
		}

		public void StopOperatorCameraSequence()
		{
			if (_ctsOperatorCameraSequence != null)
			{
				_ctsOperatorCameraSequence.Cancel();
			}
			_ctsOperatorCameraSequence      = null;
			IsPlayingOperatorCameraSequence = false;
		}

		public void StartOperatorQuestionerSequence()
		{
			StopOperatorQuestionerSequence();
			_ctsOperatorQuestionerSequence      = new CancellationTokenSource();
			IsPlayingOperatorQuestionerSequence = true;

			Run(_ctsOperatorQuestionerSequence.Token).Forget();

			async UniTask Run(CancellationToken token)
			{
				var seatController = MiceSeatController.Instance;
				var seatTransform  = seatController.FindMySeat();

				var questionerPosition = seatTransform.position;
				var questionerRotation = seatTransform.rotation;

				//TODO: 질문자 정보 받아오기
				var avatarInfo = AvatarInfo.CreateTestInfo(true);
				_questionerAvatar                    = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.WORLD, questionerPosition, (int)Define.eLayer.CHARACTER);
				_questionerAvatar.transform.rotation = questionerRotation;
				_questionerAvatar.gameObject.SetActive(true);

				CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, eMiceCameraJigKey.MICE_HALL_QUESTIONER_VIEW.ToString());
			}
		}

		public void StopOperatorQuestionerSequence()
		{
			if (_ctsOperatorCameraSequence != null)
			{
				_ctsOperatorCameraSequence.Cancel();
			}
			if (!_questionerAvatar.IsUnityNull())
			{
				GameObject.Destroy(_questionerAvatar.gameObject);
				_questionerAvatar = null;
			}
			IsPlayingOperatorQuestionerSequence = false;
		}
	}
}
