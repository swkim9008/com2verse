/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-17 13:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CommunicationViewModel : ViewModelBase, IDisposable
	{
		public CommunicationViewModel()
		{
			CommunicationManager.Instance.CommunicationTypeChanged    += OnCommunicationTypeChanged;
			CommunicationManager.Instance.CommunicationTypeChangedInt += OnCommunicationTypeChangedInt;

			OnCommunicationTypeChanged(CommunicationManager.Instance.CommunicationType);
			OnCommunicationTypeChangedInt(CommunicationManager.Instance.CommunicationTypeInt);
		}

		private void OnCommunicationTypeChanged(eCommunicationType communicationType)
		{
			InvokePropertyValueChanged(nameof(CommunicationType), communicationType);
		}

		private void OnCommunicationTypeChangedInt(int communicationTypeInt)
		{
			InvokePropertyValueChanged(nameof(CommunicationTypeInt), communicationTypeInt);
		}

		public eCommunicationType CommunicationType    => CommunicationManager.Instance.CommunicationType;
		public int                CommunicationTypeInt => CommunicationManager.Instance.CommunicationTypeInt;

		public void Dispose()
		{
			var communicationManager = CommunicationManager.InstanceOrNull;
			if (communicationManager != null)
			{
				communicationManager.CommunicationTypeChanged    -= OnCommunicationTypeChanged;
				communicationManager.CommunicationTypeChangedInt -= OnCommunicationTypeChangedInt;
			}
		}
	}
}
