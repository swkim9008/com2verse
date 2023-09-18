/*===============================================================
* Product:		Com2Verse
* File Name:	BaseModule.cs
* Developer:	urun4m0r1
* Date:			2022-04-07 19:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	public abstract class BaseModule : IModule, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseModule"/> class.
		/// </summary>
		/// <param name="initialState">Initial value for <see cref="IsRunning"/>.</param>
		/// <param name="notifyAlways">Set to <c>true</c> will raise the <see cref="StateChanged"/> event
		/// every time when you access the <see cref="IsRunning"/> property setter.</param>
		protected BaseModule(bool initialState = false, bool notifyAlways = false)
		{
			_isRunning    = initialState;
			_notifyAlways = notifyAlways;
		}

		private readonly bool _notifyAlways;

		private bool _isRunning;

		public virtual bool IsRunning
		{
			get => _isRunning;
			set
			{
				if (_notifyAlways || _isRunning != value)
				{
					ApplyState(value);
				}
			}
		}

		protected virtual void ApplyState(bool value)
		{
			RaiseStateChanged(value);
		}

		protected void RaiseStateChanged(bool value)
		{
			_isRunning = value;
			StateChanged?.Invoke(value);
		}

		public event Action<bool>? StateChanged;

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				IsRunning = false;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
