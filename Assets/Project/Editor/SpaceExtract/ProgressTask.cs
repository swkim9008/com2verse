/*===============================================================
* Product:		Com2Verse
* File Name:	ProgressTask.cs
* Developer:	jhkim
* Date:			2023-05-15 17:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Com2VerseEditor.SpaceExtract
{
	public static class ProgressTask
	{
		private static bool _isRunning = false;

		public static async UniTask ExecuteAsync(params ProgressTaskInfo[] taskInfos)
		{
			if (_isRunning)
			{
				C2VDebug.LogWarning($"ProgressTask already running");
				return;
			}

			_isRunning = true;

			var idx = 0;
			var total = taskInfos.Length;

			foreach (var taskInfo in taskInfos)
			{
				try
				{
					await ExecuteOnMainThreadAsync(() => DisplayProgress(taskInfo, idx, total));
					if (taskInfo.Job != null)
						await taskInfo.Job.Invoke();
					idx++;
				}
				catch (Exception e)
				{
					C2VDebug.LogWarning($"ProgressTask Error\n{e}");
					break;
				}
			}

			await ExecuteOnMainThreadAsync(() =>
			{
				EditorUtility.ClearProgressBar();
				_isRunning = false;
			});
		}

		private static void DisplayProgress(ProgressTaskInfo taskInfo, int idx, int count) => DisplayProgress(taskInfo.Title, taskInfo.Info, idx, count);
		private static void DisplayProgress(string title, string info, int idx, int count)
		{
			var pct = idx / (float) count;
			EditorUtility.DisplayProgressBar($"[{idx + 1}/{count}] {title}", info, pct);
		}

		private static async UniTask ExecuteOnMainThreadAsync(Action onAction)
		{
			await using (UniTask.ReturnToCurrentSynchronizationContext())
			{
				UniTask.SwitchToMainThread();
				onAction?.Invoke();
			}
		}

		public struct ProgressTaskInfo
		{
			public string Title;
			public string Info;
			public Func<UniTask> Job;
		}
	}
}
