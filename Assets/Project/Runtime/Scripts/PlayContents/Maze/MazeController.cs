/*===============================================================
* Product:		Com2Verse
* File Name:	MazeController.cs
* Developer:	haminjeong
* Date:			2023-05-26 12:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Contents
{
	public sealed class MazeController : IPlayController
	{
		public enum ePlayState
		{
			NONE       = -1,
			INITIALIZE = 0,
			READY,
			PLAYING,
			PAUSE,
			VALID_CHECK,
			RESULT,
			CLEARING,
		}

		private ePlayState _playState = ePlayState.NONE;

		public ePlayState PlayState => _playState;
		
		[Serializable]
		public class RankInfo
		{
			public string   GUID;
			public int      Rank;
			public int      TotalParticipant;
			public int      RankRate;
			public int      MazeID;
			public string   Name;
			public DateTime StartTime;
			public DateTime EndTime;
			public TimeSpan ResultTime;

			public RankInfo() { }

			public RankInfo(string guid)
			{
				GUID = guid;
			}

			public string GetResultTimeString() => ResultTime.ToString(@"hh\:mm\:ss\.ff");
		}

		private RankInfo    _bestRecord;
		private RankInfo    _currentRecord;

		public PlayContentsManager.eContentsType ContentsType => PlayContentsManager.eContentsType.MAZE;

		private static readonly string MazeUIPrefab            = "UI_Maze";
		private static readonly string MazeCompletePopupPrefab = "UI_Maze_Complete_Ranking_Popup";
		private static readonly string GoalFxTransformPath     = "Renderer/fx_W_Maze_goal_01_text";

		private MazeViewModel       _mazeViewModel;
		private GUIView             _mazeView;

		private List<long> _currentStartGates;
		private List<long> _currentGoalGates;

		public bool IsStartGate(BaseMapObject obj) => !obj.IsUnityNull() && (_currentStartGates?.Contains(obj!.ObjectTypeId) ?? false);
		public bool IsGoalGate(BaseMapObject  obj) => !obj.IsUnityNull() && (_currentGoalGates?.Contains(obj!.ObjectTypeId)  ?? false);

		private void SetGates(long start1, long start2, long goal1, long goal2)
		{
			_currentStartGates?.Clear();
			_currentGoalGates?.Clear();
			_currentStartGates?.Add(start1);
			_currentStartGates?.Add(start2);
			_currentGoalGates?.Add(goal1);
			_currentGoalGates?.Add(goal2);

			_currentGoalGates?.ForEach((id) =>
			{
				var mapObject = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObject.IsUnityNull()) return;
				mapObject!.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(true);
				mapObject.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Play(true);
			});
			_currentStartGates?.ForEach((id) =>
			{
				var mapObject = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObject.IsUnityNull()) return;
				mapObject!.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(true);
				mapObject.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
			});
		}

		public void Initialize()
		{
			SetState(ePlayState.INITIALIZE);
			_currentStartGates                       =  new();
			_currentGoalGates                        =  new();
			MapController.Instance.OnMapObjectCreate += OnObjectCreated;
			MapController.Instance.OnMapObjectRemove += OnObjectRemoved;
		}

		private void OnObjectCreated(Protocols.ObjectState state, BaseMapObject mapObject)
		{
			if (mapObject.IsUnityNull()) return;

			_currentGoalGates?.ForEach((id) =>
			{
				var mapObj = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObj.IsUnityNull()) return;
				mapObj!.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(true);
				mapObj.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Play(true);
			});
			_currentStartGates?.ForEach((id) =>
			{
				var mapObj = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObj.IsUnityNull()) return;
				mapObj!.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(true);
				mapObj.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
			});
		}

		private void OnObjectRemoved(BaseMapObject mapObject)
		{
			if (mapObject.IsUnityNull()) return;

			_currentGoalGates?.ForEach((id) =>
			{
				if (mapObject!.ObjectTypeId != id) return;
				mapObject.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
				mapObject.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(false);
			});
			_currentStartGates?.ForEach((id) =>
			{
				if (mapObject!.ObjectTypeId != id) return;
				mapObject.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
				mapObject.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(false);
			});
		}

		public void SetGates(TriggerInEventParameter parameter)
		{
			if (parameter == null || parameter.ParentMapObject.IsUnityNull()) return;
			var  startGateObj1 = parameter.ParentMapObject;
			long goalGateObj1ID, goalGateObj2ID;
			var  startGateObj2ID = goalGateObj1ID = goalGateObj2ID = 0;
			var idString = InteractionManager.Instance.GetInteractionValue(parameter.ParentMapObject!.InteractionValues,
			                                                               parameter.TriggerIndex,
			                                                               parameter.CallbackIndex,
			                                                               0);
			if (long.TryParse(idString, out var startID))
				startGateObj2ID = startID;
			idString = InteractionManager.Instance.GetInteractionValue(parameter.ParentMapObject.InteractionValues,
			                                                           parameter.TriggerIndex,
			                                                           parameter.CallbackIndex,
			                                                           1);
			if (long.TryParse(idString, out var goalID1))
				goalGateObj1ID = goalID1;
			idString = InteractionManager.Instance.GetInteractionValue(parameter.ParentMapObject.InteractionValues,
			                                                           parameter.TriggerIndex,
			                                                           parameter.CallbackIndex,
			                                                           2);
			if (long.TryParse(idString, out var goalID2))
				goalGateObj2ID = goalID2;

			if (startGateObj2ID == 0 || goalGateObj1ID == 0 || goalGateObj2ID == 0) return;
			
			SetGates(startGateObj1!.ObjectTypeId, startGateObj2ID, goalGateObj1ID, goalGateObj2ID);
			_currentRecord = new RankInfo(Guid.NewGuid().ToString());
			SetState(ePlayState.READY);
		}
		
		public void PlayStart()
		{
			if (_playState != ePlayState.READY) return;

			UIManager.Instance.CreatePopup(MazeUIPrefab, (guiView) =>
			{
				_mazeView = guiView;
				_mazeView.Show();
				_mazeViewModel = guiView.ViewModelContainer.GetViewModel<MazeViewModel>();
			}).Forget();
			SetState(ePlayState.PLAYING);
			Commander.Instance.StartMaze(_currentRecord!.GUID, MetaverseWatch.Realtime);
		}
		
		private void SetState(ePlayState state, bool executeStateAction = true)
		{
			_playState = state;
			if (!executeStateAction) return;
			switch (PlayState)
			{
				case ePlayState.READY:
					break;
				case ePlayState.PLAYING:
					if (_currentRecord == null) return;
					_currentRecord.StartTime = MetaverseWatch.NowDateTime;
					PlayerController.Instance.SetEnableClickMove(false);
					break;
				case ePlayState.RESULT:
				{
					if (_currentRecord == null) return;
					PlayerController.Instance.SetEnableClickMove(true);
					_currentRecord.EndTime    = MetaverseWatch.NowDateTime;
					_currentRecord.ResultTime = _currentRecord.EndTime - _currentRecord.StartTime;
					break;
				}
			}
		}

		public void OnUpdateLoop() { }

		public void SetPause(bool isPause)
		{
			if (_playState != ePlayState.PAUSE && _playState != ePlayState.PLAYING) return;
			if (isPause)
				SetState(ePlayState.PAUSE);
			else
				SetState(ePlayState.PLAYING, false);
		}

		public bool PlayEnd()
		{
			if (_playState != ePlayState.PLAYING) return false;
			SetState(ePlayState.VALID_CHECK);
			return true;
		}

		public void RestorePlaying()
		{
			if (_playState != ePlayState.VALID_CHECK) return;
			SetState(ePlayState.PLAYING, false);
		}

		public void GoToResult()
		{
			if (_currentRecord == null) return;
			SetState(ePlayState.RESULT);
			UIManager.Instance.ShowWaitingResponsePopup();
			Commander.Instance.ResultMaze(_currentRecord!.GUID, MetaverseWatch.Realtime, () =>
			{
				UIManager.Instance.HideWaitingResponsePopup();
				PlayContentsManager.Instance.ContentsEnd();
				PlayerController.Instance.ForceForwardMove(1f);
				UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Maze_Error_Popup_Desc"));
			});
		}

		public void OnResultResponse(Protocols.GameLogic.ExitMazeResposne response)
		{
			UIManager.Instance.HideWaitingResponsePopup();
			_bestRecord                  = new RankInfo
			{
				TotalParticipant = response.TotalUserCount,
				Rank             = response.Ranking,
				RankRate         = response.RankingRate,
				ResultTime       = new TimeSpan(0,
				                                response.BestElapsed.Hour,
				                                response.BestElapsed.Minute,
				                                response.BestElapsed.Second,
				                                response.BestElapsed.CentiSecond * 10),
			};
			UIManager.Instance.CreatePopup(MazeCompletePopupPrefab, (guiView) =>
			{
				guiView.Show();
				var viewModel = guiView.ViewModelContainer.GetViewModel<MazeResultViewModel>();
				viewModel.CurrentView = guiView;

				if (_currentRecord != null)
				{
					viewModel.IsNewRecord      = Mathf.Abs((float)_bestRecord.ResultTime.TotalMilliseconds - (float)_currentRecord!.ResultTime.TotalMilliseconds) < 20;
					viewModel.RecordTimeString = viewModel.IsNewRecord ? _bestRecord!.GetResultTimeString() : _currentRecord.GetResultTimeString();
				}
				viewModel.BestTimeString = Localization.Instance.GetString("UI_Maze_Escape_Popup_RankRecord", _bestRecord!.GetResultTimeString());
				viewModel.RankingText    = Localization.Instance.GetString("UI_Maze_Escape_Popup_Rank",       _bestRecord.TotalParticipant, _bestRecord.Rank, _bestRecord.RankRate);
				PlayContentsManager.Instance.ContentsEnd();
				PlayerController.Instance.ForceForwardMove(1f);
			}).Forget();
		}

		public void Clear()
		{
			SetState(ePlayState.CLEARING);

			_currentGoalGates?.ForEach((id) =>
			{
				var mapObject = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObject.IsUnityNull()) return;
				mapObject!.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
				mapObject.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(false);
			});
			_currentStartGates?.ForEach((id) =>
			{
				var mapObject = MapController.Instance.GetObjectByBaseObjectID(id);
				if (mapObject.IsUnityNull()) return;
				mapObject!.transform.Find(GoalFxTransformPath)!.GetComponent<ParticleSystem>()!.Stop(true);
				mapObject.transform.Find(GoalFxTransformPath)!.gameObject.SetActive(false);
			});

			MapController.Instance.OnMapObjectCreate -= OnObjectCreated;
			MapController.Instance.OnMapObjectRemove -= OnObjectRemoved;
			PlayerController.Instance.SetEnableClickMove(true);

			_currentGoalGates?.Clear();
			_currentGoalGates = null;
			_currentStartGates?.Clear();
			_currentStartGates   = null;
			_bestRecord          = null;
			_currentRecord       = null;
			if (!_mazeView.IsUnityNull())
				_mazeView!.Hide();
			_mazeView      = null;
			_mazeViewModel = null;
		}
	}
}
