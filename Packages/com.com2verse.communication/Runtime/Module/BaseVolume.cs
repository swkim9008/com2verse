/*===============================================================
* Product:		Com2Verse
* File Name:	BaseVolume.cs
* Developer:	urun4m0r1
* Date:			2022-04-05 20:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Com2Verse.Communication
{
	public abstract class BaseVolume : IVolume, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseVolume"/> class.
		/// </summary>
		/// <param name="maxLevel">Max level info for normalization.</param>
		/// <param name="initialAudible">Initial value for <see cref="IsAudible"/>.</param>
		/// <param name="initialLevel">Initial value for <see cref="Level"/>.</param>
		/// <param name="notifyAlways">Set to <c>true</c> will raise the <see cref="AudibleChanged"/> or <see cref="LevelChanged"/> event
		/// every time when you access the <see cref="IsAudible"/> or <see cref="Level"/> property setter.</param>
		protected BaseVolume(float maxLevel, bool initialAudible = false, float initialLevel = 0f, bool notifyAlways = false)
		{
			MaxLevel      = maxLevel;
			_isAudible    = initialAudible;
			_level        = initialLevel;
			_notifyAlways = notifyAlways;
		}

		protected readonly float MaxLevel;

		protected float ScaledLevel => Level * MaxLevel;

		private readonly bool _notifyAlways;

		private float _level;

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public virtual float Level
		{
			get => _level;
			set
			{
				if (_notifyAlways || _level != value)
				{
					ApplyLevel(value);
				}
			}
		}

		private bool _isAudible;

		public virtual bool IsAudible
		{
			get => _isAudible;
			set
			{
				if (_notifyAlways || _isAudible != value)
				{
					ApplyAudible(value);
				}
			}
		}

		protected virtual void ApplyLevel(float value)
		{
			RaiseLevelChanged(value);
		}

		protected virtual void ApplyAudible(bool value)
		{
			RaiseAudibleChanged(value);
		}

		protected void RaiseLevelChanged(float value)
		{
			_level = value;
			LevelChanged?.Invoke(value);
		}

		protected void RaiseAudibleChanged(bool value)
		{
			_isAudible = value;
			AudibleChanged?.Invoke(value);
		}

		public event Action<float>? LevelChanged;
		public event Action<bool>?  AudibleChanged;

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
				Level     = 0f;
				IsAudible = false;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
