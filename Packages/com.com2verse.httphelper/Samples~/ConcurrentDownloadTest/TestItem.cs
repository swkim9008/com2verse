/*===============================================================
* Product:		Com2Verse
* File Name:	TestItem.cs
* Developer:	jhkim
* Date:			2023-02-06 12:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

namespace Com2Verse.HttpHelper.Sample
{
	public sealed class TestItem : MonoBehaviour
	{
		[SerializeField] private Image _bg;
		[SerializeField] private Color[] _bgColors;
		[SerializeField] private Image _progress;
		[SerializeField] private TMP_Text _text;
		[SerializeField] private TMP_Text _number;

		public enum eState
		{
			NONE,
			REQUEST,
			DOWNLOADING,
			COMPLETE,
			CANCEL,
			DOWNLOAD_START,
		}

		private string _testUrl = "https://github.com/yourkin/fileupload-fastapi/raw/a85a697cab2f887780b3278059a0dd52847d80f3/tests/data/test-5mb.bin"; //"http://ipv4.download.thinkbroadband.com/50MB.zip";
		private IRequestHandler _requestHandler;
		public Action<eState> _onChangeState;
		private void Awake()
		{
			SetState(eState.NONE);
		}

		public void SetNumber(int no)
		{
			_number.text = $"#{no}";
		}

		public void SetOnChangeState(Action<eState> onChangeState) => _onChangeState = onChangeState;
		public async void Run()
		{
			_text.text = "REQUEST";
			long ts = 0;
			var callbacks = new Callbacks
			{
				OnDownloadStart = () =>
				{
					_text.text = "START";
					SetState(eState.DOWNLOAD_START);
				},
				OnDownloadProgress = (read, totalRead, totalSize) =>
				{
					var p = totalRead / (float) totalSize;
					_progress.fillAmount = p;
					// _text.text = $"{(p * 100).ToString("00.00")}%\n[{totalRead} / {totalSize}]\n{read}bytes";
					_text.text = $"{(p * 100).ToString("00.00")}%\n{read}bytes";
					ts = totalSize;
					SetState(eState.DOWNLOADING);
				},
				OnComplete = (stream, totalSize) =>
				{
					_text.text = $"COMPLETE\n[{ts}]";
					SetState(eState.COMPLETE);
				},
				OnFinally = () =>
				{
					C2VDebug.Log("DOWNLOAD COMPLETE...");
				},
			};

			_requestHandler = await Client.Request.CreateRequestWithCallbackAsync(HttpRequestBuilder.Generate(new HttpRequestMessageInfo
			{
				RequestMethod = Client.eRequestType.GET,
				Url = _testUrl,
			}), callbacks);
			SetState(eState.REQUEST);
			await _requestHandler.SendAsync();
		}

		public void Cancel()
		{
			_text.text = "CANCEL";
			_requestHandler?.Cancel();
			SetState(eState.CANCEL);
		}

		private void SetState(eState state)
		{
			var idx = (int) state;
			if (idx < 0 || idx > _bgColors.Length) return;
			_bg.color = _bgColors[idx];
			_onChangeState?.Invoke(state);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(TestItem))]
	public class TestItemEditor : Editor
	{
		private TestItem _target;

		private void Awake()
		{
			_target = target as TestItem;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("RUN"))
				_target.Run();
			if (GUILayout.Button("CANCEL"))
				_target.Cancel();
		}
	}
#endif // UNITY_EDITOR
}
