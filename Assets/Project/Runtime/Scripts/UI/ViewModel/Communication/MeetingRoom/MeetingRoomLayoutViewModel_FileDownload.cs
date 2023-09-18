/*===============================================================
 * Product:		Com2Verse
 * File Name:	MeetingRoomLayoutViewModel_FileDownload.cs
 * Developer:	seaman2000
 * Date:		2023-06-07 11:47
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Com2Verse.Communication;
using Com2Verse.Mice;
using Com2Verse.Notification;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public sealed partial class MeetingRoomLayoutViewModel
	{
        [UsedImplicitly] public CommandHandler<bool> SetDataDownload { get; private set; } = default;
        [UsedImplicitly] public CommandHandler ToggleDataDownload { get; private set; } = default;

        private bool _isDataDownload;

		private partial void Initialize()
		{
            SetDataDownload = new CommandHandler<bool>(value => IsDataDownload = value);
            ToggleDataDownload = new CommandHandler(() => IsDataDownload ^= true);
		}

        public bool IsDataDownload
        {
            get => _isDataDownload;
            set
            {
                if (!UpdateProperty(ref _isDataDownload, value)) return;

                if (value)
                {
                    ShowFileDownloadPopup().Forget();
                }
            }
        }

        private async UniTask ShowFileDownloadPopup()
        {
            var view = await MiceUIConferenceDataDownloadViewModel.ShowPopup(null, (view) => {
                IsDataDownload = false;
            });

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIConferenceDataDownloadViewModel), out var viewModel))
            {
                var dataDonwloadViewModel = viewModel as MiceUIConferenceDataDownloadViewModel;
                dataDonwloadViewModel?.SyncData(view);
            }
        }
    }
}
