/*===============================================================
* Product:		Com2Verse
* File Name:	SystemVolume.cs
* Developer:	urun4m0r1
* Date:			2022-05-24 19:31
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Threading;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication
{
	public abstract class SystemVolume : BaseVolume, IRefreshable
	{
		protected SystemVolume(float maxVolume) : base(maxVolume) { }

		public bool IsDeviceExist { get; set; }

		public int RefreshInterval { get; set; } = Define.Device.RefreshInterval;

		private bool _useAutoRefresh;

		public bool UseAutoRefresh
		{
			get => _useAutoRefresh;
			set
			{
				_useAutoRefresh = value;
				if (value) StartUpdate();
				else StopUpdate();
			}
		}

		private CancellationTokenSource? _tokenSource;

		public void StartUpdate()
		{
			StopUpdate();
			_tokenSource ??= new CancellationTokenSource();
			Refresh(_tokenSource).Forget();
		}

		public void StopUpdate()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}

		public async UniTask Refresh(CancellationTokenSource? tokenSource)
		{
			await UniTask.Defer(UpdateSystemVolume).RunOnMainThread(tokenSource);
		}

		private async UniTask UpdateSystemVolume()
		{
			do
			{
				if (IsDeviceExist)
				{
					var level     = GetSystemLevel();
					var isAudible = GetSystemAudible();

					Level     = level;
					IsAudible = isAudible;
				}
			}
			while (await UniTaskHelper.Delay(RefreshInterval, _tokenSource));
		}

		protected override void ApplyLevel(float value)
		{
			if (IsDeviceExist)
			{
				SetSystemLevel(value);
				RaiseLevelChanged(value);
			}
		}

		protected override void ApplyAudible(bool value)
		{
			if (IsDeviceExist)
			{
				SetSystemAudible(value);
				RaiseAudibleChanged(value);
			}
		}

		protected abstract float GetSystemLevel();
		protected abstract bool  GetSystemAudible();
		protected abstract void  SetSystemLevel(float  value);
		protected abstract void  SetSystemAudible(bool value);

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				StopUpdate();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
