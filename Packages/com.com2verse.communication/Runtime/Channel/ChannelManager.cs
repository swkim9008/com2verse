/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 16:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Communication.MediaSdk;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Communication
{
	public sealed class ChannelManager : Singleton<ChannelManager>, IDisposable
	{
		public event Action<IChannel>? ChannelAdded;
		public event Action<IChannel>? ChannelRemoved;

		public event Action<IChannel, ICommunicationUser>? ChannelUserAdded;
		public event Action<IChannel, ICommunicationUser>? ChannelUserRemoved;

		public event Action<IChannel, IViewModelUser>? ViewModelUserAdded;
		public event Action<IChannel, IViewModelUser>? ViewModelUserRemoved;

		public event Action<IChannel, ICommunicationUser?, ICommunicationUser?>? HostChanged;
		public event Action<IChannel, ILocalUser?, ILocalUser?>?                 SelfChanged;

		private CancellationTokenSource? _engineCts;

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private ChannelManager() { }

		public void Dispose()
		{
			ClearAllChannels();
			UniTaskHelper.InvokeOnMainThread(DisposeEngine, _engineCts).Forget();
		}

#region EngineInitialize
		public UnityRTCConfig? UnityRtcSettings { get; private set; }

		public async UniTask TryInitializeEngineAsync()
		{
			TryInitializeEngine();

			_engineCts ??= new CancellationTokenSource();
			await UniTaskHelper.DelayFrame(1, _engineCts, PlayerLoopTiming.LastPostLateUpdate);
		}

		public async UniTask DisposeEngineAsync()
		{
			DisposeEngine();

			await UniTaskHelper.DelayFrame(1, _engineCts, PlayerLoopTiming.LastPostLateUpdate);
		}

		private void TryInitializeEngine()
		{
#if DISABLE_WEBRTC
			return;
#endif // DISABLE_WEBRTC

			if (UnityRtcSettings == null)
			{
				UnityRtcSettings = Resources.Load<UnityRTCConfig>("Communication/UnityRTCConfig");
			}

			if (!UnityRTCEngine.IsInitialized)
			{
				UnityRTCEngine.Initialize(UnityRtcSettings);
			}

			if (!UnityRTCEngine.IsRunning)
			{
				UnityRTCEngine.Start(CoroutineManager.Instance);
			}
		}

		private static void DisposeEngine()
		{
#if DISABLE_WEBRTC
			return;
#endif // DISABLE_WEBRTC

			if (UnityRTCEngine.IsRunning)
			{
				UnityRTCEngine.Stop();
			}

			if (UnityRTCEngine.IsInitialized)
			{
				UnityRTCEngine.Dispose();
			}
		}
#endregion // EngineInitialize

#region ChannelLifecycle
		private async UniTask ConnectChannel(IChannel channel)
		{
			_engineCts ??= new CancellationTokenSource();
			await UniTask.Defer(TryInitializeEngineAsync).RunOnMainThread(_engineCts);

			channel.Connector.Connect();
		}

		private void DisconnectChannel(IChannel? channel)
		{
			(channel as IDisposable)?.Dispose();
		}

		public void NotifyChannelDisposed(IChannel? channel)
		{
			if (channel == null) return;

			var channelId = channel.Info.ChannelId;
			_joiningChannels.Remove(channelId);

			channel.UserJoin    -= InvokeChannelUserAdded;
			channel.UserLeft    -= InvokeChannelUserRemoved;
			channel.HostChanged -= InvokeHostChanged;
			channel.SelfChanged -= InvokeSelfChanged;
			ChannelRemoved?.Invoke(channel);
		}

		public void NotifyChannelCreated(IChannel? channel)
		{
			if (channel == null) return;

			channel.UserJoin    += InvokeChannelUserAdded;
			channel.UserLeft    += InvokeChannelUserRemoved;
			channel.HostChanged += InvokeHostChanged;
			channel.SelfChanged += InvokeSelfChanged;
			ChannelAdded?.Invoke(channel);
		}

		private void InvokeChannelUserAdded(IChannel channel, ICommunicationUser user)
		{
			ChannelUserAdded?.Invoke(channel, user);

			if (user is IViewModelUser viewModelUser)
				ViewModelUserAdded?.Invoke(channel, viewModelUser);
		}

		private void InvokeChannelUserRemoved(IChannel channel, ICommunicationUser user)
		{
			ChannelUserRemoved?.Invoke(channel, user);

			if (user is IViewModelUser viewModelUser)
				ViewModelUserRemoved?.Invoke(channel, viewModelUser);
		}

		private void InvokeHostChanged(IChannel channel, ICommunicationUser? oldHost, ICommunicationUser? newHost)
		{
			HostChanged?.Invoke(channel, oldHost, newHost);
		}

		private void InvokeSelfChanged(IChannel channel, ILocalUser? oldSelf, ILocalUser? newSelf)
		{
			SelfChanged?.Invoke(channel, oldSelf, newSelf);
		}
#endregion // ChannelLifecycle

#region ChannelControllers
		public IReadOnlyDictionary<string, IChannel> WaitingChannels => _waitingChannels;
		public IReadOnlyDictionary<string, IChannel> JoiningChannels => _joiningChannels;

		private readonly Dictionary<string, IChannel> _waitingChannels = new();
		private readonly Dictionary<string, IChannel> _joiningChannels = new();

		private readonly Dictionary<string, IChannel> _leavingChannels = new();

		public void AddChannel(IChannel channel)
		{
			if (_waitingChannels.ContainsKey(channel.Info.ChannelId)) return;
			if (_joiningChannels.ContainsKey(channel.Info.ChannelId)) return;

			_waitingChannels.Add(channel.Info.ChannelId, channel);
		}

		public IChannel? JoinChannel(string channelId)
		{
			if (_joiningChannels.TryGetValue(channelId, out var channel))
			{
				return channel!;
			}

			if (!_waitingChannels.TryGetValue(channelId, out channel))
			{
				return null;
			}

			_waitingChannels.Remove(channelId);

			_joiningChannels.Add(channelId, channel);
			ConnectChannel(channel!).Forget();
			return channel;
		}

		public void RemoveChannel(string? channelId)
		{
			if (string.IsNullOrEmpty(channelId!)) return;

			if (_waitingChannels.TryGetValue(channelId, out var channel))
			{
				(channel as IDisposable)?.Dispose();
				_waitingChannels.Remove(channelId);
			}

			if (_joiningChannels.TryGetValue(channelId, out channel))
			{
				DisconnectChannel(channel);
			}
		}

		public void JoinAllChannels()
		{
			if (_waitingChannels.Count == 0)
				return;

			foreach (var channel in _waitingChannels.Values)
			{
				_joiningChannels.Add(channel.Info.ChannelId, channel);
				ConnectChannel(channel).Forget();
			}

			_waitingChannels.Clear();
		}

		public void LeaveAllChannels()
		{
			MigrateItems(_joiningChannels, _leavingChannels);

			foreach (var channel in _leavingChannels.Values)
			{
				DisconnectChannel(channel);
			}
		}

		private static void MigrateItems<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
		{
			target.Clear();

			foreach (var channel in source)
				target.Add(channel.Key, channel.Value);

			source.Clear();
		}

		public void ClearAllChannels()
		{
			LeaveAllChannels();

			foreach (var channel in _waitingChannels.Values)
			{
				(channel as IDisposable)?.Dispose();
			}

			_waitingChannels.Clear();
		}

#if ENABLE_CHEATING
		public Cheat.DummyChannelDecorator? DebugChannel { get; private set; }

		public void AddDebugChannel(User user, string channelId, string loginToken)
		{
			if (_waitingChannels.ContainsKey(channelId)) return;
			if (_joiningChannels.ContainsKey(channelId)) return;

			DebugChannel = MediaChannelFactory.CreateDebugInstance(user, channelId, loginToken);
			_waitingChannels.Add(channelId, DebugChannel);
		}

		public void RemoveDebugChannel()
		{
			if (DebugChannel == null) return;

			RemoveChannel(DebugChannel.Info.ChannelId);
			DebugChannel = null;
		}
#endif     // ENABLE_CHEATING
#endregion // ChannelControllers

#region Utils
		public IEnumerable<IChannel> GetAllChannels()
		{
			foreach (var channel in WaitingChannels.Values)
			{
				yield return channel;
			}

			foreach (var channel in JoiningChannels.Values)
			{
				yield return channel;
			}
		}

		public IEnumerable<ICommunicationUser> GetAllUsers()
		{
			foreach (var channel in JoiningChannels.Values)
			{
				foreach (var user in channel.ConnectedUsers.Values)
				{
					yield return user;
				}
			}
		}

		public IEnumerable<IViewModelUser> GetViewModelUsers()
		{
			foreach (var user in GetAllUsers())
			{
				if (user is IViewModelUser viewModelUser)
				{
					yield return viewModelUser;
				}
			}
		}
#endregion // Utils
	}
}
