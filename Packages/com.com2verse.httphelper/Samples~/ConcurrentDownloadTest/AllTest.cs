/*===============================================================
* Product:		Com2Verse
* File Name:	AllTest.cs
* Developer:	jhkim
* Date:			2023-02-07 12:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Com2Verse.HttpHelper.Sample
{
	public sealed class AllTest : MonoBehaviour
	{
		[SerializeField] private TestItem _template;
		[SerializeField] private int _testCount;
		[SerializeField] private TMP_Text _activeDownload;

		private TestItem[] _tests;
		private int _activeDownloadCnt = 0;
		private void Awake()
		{
			_template.gameObject.SetActive(false);
			_tests = new TestItem[_testCount];
			for (int i = 0; i < _testCount; ++i)
			{
				_tests[i] = Instantiate(_template, _template.transform.parent);
				_tests[i].SetNumber(i + 1);
				_tests[i].SetOnChangeState(OnChangeState);
				_tests[i].gameObject.SetActive(true);
			}
		}

		private void OnChangeState(TestItem.eState state)
		{
			switch (state)
			{
				case TestItem.eState.NONE:
					break;
				case TestItem.eState.DOWNLOAD_START:
					_activeDownloadCnt++;
					break;
				case TestItem.eState.REQUEST:
					break;
				case TestItem.eState.DOWNLOADING:
					break;
				case TestItem.eState.COMPLETE:
					if (_activeDownloadCnt - 1 < 0) break;
					_activeDownloadCnt--;
					break;
				case TestItem.eState.CANCEL:
					if (_activeDownloadCnt - 1 < 0) break;
					_activeDownloadCnt--;
					break;
				default:
					break;
			}
			RefreshUI();
		}

		private void RefreshUI()
		{
			_activeDownload.text = $"ACTIVE DOWNLOADS = {Convert.ToString(_activeDownloadCnt)}, CONCURRENT REQUEST = {Convert.ToString(Client.Debug.ConcurrentRequest)}";
		}
		public void RunAll()
		{
			foreach (var test in _tests)
				test.Run();
		}

		public void CancelAll()
		{
			foreach (var test in _tests)
				test.Cancel();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(AllTest))]
	public sealed class AllTESTEditor : Editor
	{
		private AllTest _script;

		private void Awake()
		{
			_script = target as AllTest;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("RUN ALL"))
				_script.RunAll();
			if (GUILayout.Button("CANCEL ALL"))
				_script.CancelAll();
		}
	}
#endif // UNITY_EDITOR
}
