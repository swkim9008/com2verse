// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	CommonPopupDescSynchronizer.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-04 오전 10:23
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Text;
using UnityEngine;

namespace Com2Verse.UIExtension
{
	[RequireComponent(typeof(GUIView))]
	public class CommonPopupDescSynchronizer : MonoBehaviour
	{
		public float UpdateInterval { get; set; } = 0.5f;
		
		private GUIView _targetView;
		private CommonPopupViewModel _viewModel;
		private float _updateTimer;
		private string[] _parameter;
		private string _baseContext;
		private Action<string[]> _parameterSetter = null;

		private void Start()
		{
			_targetView = GetComponent<GUIView>();
			_viewModel = _targetView.ViewModelContainer.GetViewModel<CommonPopupViewModel>();
			UpdateContext();
		}

		public void SetDesc(string context, int parameterCount, Action<string[]> parameterSetter)
		{
			_baseContext = context;
			_parameter = new string[parameterCount];
			_parameterSetter = parameterSetter;
		}

		private void Update()
		{
			_updateTimer += Time.deltaTime;

			if (_updateTimer > UpdateInterval)
			{
				_updateTimer = 0;
				UpdateContext();
			}
		}

		private void UpdateContext()
		{
			UpdateParameter();
			SyncDesc();
		}

		private void UpdateParameter()
		{
			_parameterSetter.Invoke(_parameter);
		}

		private void SyncDesc()
		{
			switch (_parameter.Length)
			{
				case 0:
					_viewModel.Context = _baseContext;
					break;
				case 1:
					_viewModel.Context = ZString.Format(_baseContext, _parameter[0]);
					break;
				case 2:
					_viewModel.Context = ZString.Format(_baseContext, _parameter[0], _parameter[1]);
					break;
				case 3:
					_viewModel.Context = ZString.Format(_baseContext, _parameter[0], _parameter[1], _parameter[2]);
					break;
				case 4:
					_viewModel.Context = ZString.Format(_baseContext, _parameter[0], _parameter[1], _parameter[2], _parameter[3]);
					break;
				case 5:
					_viewModel.Context = ZString.Format(_baseContext, _parameter[0], _parameter[1], _parameter[2], _parameter[3], _parameter[4]);
					break;
				default:
					C2VDebug.LogError("현재 Parameter를 5개까지만 지원합니다. CommonPopupDescSynchronizer.cs에 직접 추가하세요.");
					break;
			}
		}
	}
}
