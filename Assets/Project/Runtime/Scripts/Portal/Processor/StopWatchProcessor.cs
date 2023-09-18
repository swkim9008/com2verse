/*===============================================================
* Product:		Com2Verse
* File Name:	StopWatchProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-30 17:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.STOP_WATCH)]
	public sealed class StopWatchProcessor : BaseLogicTypeProcessor
	{
		private static readonly string StopwatchUIPrefab = "UI_Popup_Stopwatch";
		
		private long               _timerUpdateTime;
		private StopwatchViewModel _viewModel;
		private GUIView            _guiView;
		private bool               _isPlaying = false;

		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			if (_guiView.IsUnityNull())
			{
				UIManager.Instance.CreatePopup(StopwatchUIPrefab, (guiView) =>
				{
					_guiView   = guiView.Show();
					_viewModel = guiView.ViewModelContainer.GetViewModel<StopwatchViewModel>();
					UpdateLoop().Forget();
				}).Forget();
			}
			else
			{
				_guiView!.Show();
				UpdateLoop().Forget();
			}
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			_isPlaying = false;
			_guiView!.Hide();
		}

		private async UniTaskVoid UpdateLoop()
		{
			if (_viewModel == null) return;
			_viewModel.StartTime   = MetaverseWatch.NowDateTime;
			_viewModel.TimerString = "00:00:00.00";
			_isPlaying             = true;
			while (_isPlaying)
			{
				_viewModel.UpdateTimerString();
				await UniTask.Delay(10, true);
			}
		}
	}
}
