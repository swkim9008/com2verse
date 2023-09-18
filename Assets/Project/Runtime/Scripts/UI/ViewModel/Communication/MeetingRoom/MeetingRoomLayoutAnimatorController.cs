/*===============================================================
 * Product:		Com2Verse
 * File Name:	MeetingRoomLayoutAnimatorController.cs
 * Developer:	urun4m0r1
 * Date:		2023-04-17 16:41
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(Animator))]
	public class MeetingRoomLayoutAnimatorController : MonoBehaviour
	{
		[SerializeField] private Animator? _animator;

		private MeetingRoomLayoutViewModel  ViewModel => _viewModel ??= ViewModelManager.Instance.GetOrAdd<MeetingRoomLayoutViewModel>();
		private MeetingRoomLayoutViewModel? _viewModel;

		private readonly Dictionary<string, int> _parameters = new();

		private void Awake()
		{
			if (_animator.IsReferenceNull())
				_animator = GetComponent<Animator>();

			if (_animator.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomLayoutAnimatorController), "Animator is null");
				return;
			}

			var parameters = _animator!.parameters;
			if (parameters == null)
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomLayoutAnimatorController), "Animator has no parameters");
				return;
			}

			foreach (var parameter in parameters)
				_parameters.Add(parameter.name, parameter.nameHash);

			ViewModel.LayoutChanged += OnLayoutChanged;
		}

		private void OnDestroy()
		{
			ViewModel.LayoutChanged -= OnLayoutChanged;
		}

		private void OnLayoutChanged(string propertyName, bool value)
		{
			if (_animator.IsUnityNull())
				return;

			if (!_parameters.TryGetValue(propertyName, out var parameterNameHash))
				return;

			_animator!.SetBool(parameterNameHash, value);
		}
	}
}
