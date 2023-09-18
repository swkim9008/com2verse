/*===============================================================
 * Product:		Com2Verse
 * File Name:	CommunicationUserDecorator.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-24 13:41
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="ICommunicationUser"/>
	/// <summary>
	/// <see cref="ICommunicationUser" />의 Decorator패턴 구현체입니다.
	/// <br/><see cref="Dispose"/> 메서드는 <see cref="DecoratedUser"/>의 <see cref="Dispose"/> 메서드를 호출합니다.
	/// </summary>
	public abstract class CommunicationUserDecorator : ICommunicationUser, IDisposable
	{
		public ChannelInfo ChannelInfo => DecoratedUser.ChannelInfo;
		public User        User        => DecoratedUser.User;
		public eUserRole   Role        => DecoratedUser.Role;

		public string GetDebugInfo() => DecoratedUser.GetDebugInfo();
		public string GetInfoText()  => DecoratedUser.GetInfoText();

		protected ICommunicationUser DecoratedUser { get; }

		protected CommunicationUserDecorator(ICommunicationUser user)
		{
			DecoratedUser = user;
		}

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
				(DecoratedUser as IDisposable)?.Dispose();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
