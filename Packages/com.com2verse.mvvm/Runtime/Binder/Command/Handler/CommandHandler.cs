/*===============================================================
* Product:    Com2Verse
* File Name:  CommandHandler.cs
* Developer:  tlghks1009
* Date:       2022-03-22 18:57
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public sealed class CommandHandler : ICommand
	{
		private Action _onFunc;
		private Func<bool> _onCanInvokedEventHandler;

		public CommandHandler() { }

		public CommandHandler(Action onEventHandler, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc += onEventHandler;

			_onCanInvokedEventHandler += onCanInvokedEventHandler;
		}

		public void Register(Action onFunc, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc -= onFunc;
			_onFunc += onFunc;

			_onCanInvokedEventHandler -= onCanInvokedEventHandler;
			_onCanInvokedEventHandler += onCanInvokedEventHandler;
		}

		public void Unregister(Action onFunc, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc -= onFunc;

			_onCanInvokedEventHandler -= onCanInvokedEventHandler;
		}

		public void Unregister()
		{
			_onFunc = null;

			_onCanInvokedEventHandler = null;
		}


		public void Invoke([CanBeNull] object additionalData)
		{
			if (_onCanInvokedEventHandler != null)
			{
				if (!_onCanInvokedEventHandler())
					return;
			}

			_onFunc?.Invoke();
		}
	}


	public sealed class CommandHandler<T> : ICommand
	{
		private Action<T> _onFunc;
		private Func<bool> _onCanInvokedEventHandler;

		public CommandHandler() { }

		public CommandHandler(Action<T> onEventHandler, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc += onEventHandler;
			_onCanInvokedEventHandler += onCanInvokedEventHandler;
		}

		public void Register(Action<T> onFunc, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc -= onFunc;
			_onFunc += onFunc;

			_onCanInvokedEventHandler -= onCanInvokedEventHandler;
			_onCanInvokedEventHandler += onCanInvokedEventHandler;
		}

		public void Unregister(Action<T> onFunc, Func<bool> onCanInvokedEventHandler = null)
		{
			_onFunc -= onFunc;

			_onCanInvokedEventHandler -= onCanInvokedEventHandler;
		}

		public void Unregister()
		{
			_onFunc = null;

			_onCanInvokedEventHandler = null;
		}


		public void Invoke(object additionalData)
		{
			if (_onCanInvokedEventHandler != null)
			{
				if (!_onCanInvokedEventHandler())
					return;
			}

			_onFunc?.Invoke((T) additionalData);
		}
	}
}
