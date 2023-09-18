/*===============================================================
* Product:    Com2Verse
* File Name:  NetworkManager.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Pool;
using WebSocketSharp;

namespace Com2Verse.Network
{
    public sealed class NetworkManager : MonoSingleton<NetworkManager>
    {
        /// <summary>
        /// 디버그용 메시지를 표시할 지 여부
        /// </summary>
        [field: SerializeField] public bool IsVerbose { get; set; } = false;

        [field: SerializeField] private string serverAddress = "127.0.0.1";
        [field: SerializeField] private int serverPort = 11111;

        private long _userID;

        private ClientSocket _socket = null;

        /// <summary>
        /// 소켓 내부적으로 Connect된 상태를 가져온다.
        /// </summary>
        public bool IsConnected => _socket?.IsConnected ?? false;

        private sealed class ReceiveContext
        {
            public Protocols.Channels Channel = 0;
            public int Command = 0;
            public IMessage Message = null;
            public Protocols.ErrorCode ErrorCode = 0;
        }

        private struct WaitingPacketInfo
        {
            public Protocols.Channels Channel;
            public int Command;
            public long ExpiredTime;
            public Action TimeoutCallback;
        }

        private readonly Dictionary<Protocols.Channels, IMessageProcessor> _messageProcessors = new();
        private readonly ConcurrentQueue<ReceiveContext> _receivedMessages = new();
        private readonly ObjectPool<ReceiveContext> _contextPool = new(() => new ReceiveContext(), null, ReturnContextToPool, null, false);
        private readonly object _threadLockObject = new();
        private readonly List<WaitingPacketInfo> _timeoutPacketList = new();
        private static readonly int DefaultTimoutTime = 15000; // 특정 패킷에 걸 수 있는 타임아웃 기본값(ms)

        private int _customTimeoutTime = -1;
        public void SetTimeoutTime(float time) => _customTimeoutTime = (int)(time * 1000);

        private bool _isRequestedDisconnect  = false;
        private bool _isAlreadyAddProcessors = false;
        public  bool TokenExpired { get; set; }

#region Events
        private event Action<string> _onNetworkError;

        /// <summary>
        /// 네트워크 메시지 처리 중 에러가 발생했을 경우 호출되는 이벤트
        /// </summary>
        public event Action<string> OnNetworkError
        {
            add
            {
                _onNetworkError -= value;
                _onNetworkError += value;
            }
            remove => _onNetworkError -= value;
        }

        private event Action _onDisconnected;

        /// <summary>
        /// 어떠한 경로에서든 Disconnect가 이루어지면 호출되는 이벤트
        /// </summary>
        public event Action OnDisconnected
        {
            add
            {
                _onDisconnected -= value;
                _onDisconnected += value;
            }
            remove => _onDisconnected -= value;
        }

        private event Action _socketDisconnected;

        /// <summary>
        /// 소켓 내부적으로 Disconnect되거나 Send과정에서 Error발생하면 호출되는 이벤트
        /// </summary>
        public event Action SocketDisconnected
        {
            add
            {
                _socketDisconnected -= value;
                _socketDisconnected += value;
            }
            remove => _socketDisconnected -= value;
        }

        private event Action _onLogout;

        /// <summary>
        /// 로그아웃이 필요한 상황에서 호출되는 이벤트
        /// </summary>
        public event Action OnLogout
        {
            add
            {
                _onLogout -= value;
                _onLogout += value;
            }
            remove => _onLogout -= value;
        }

        private Action _loginFailed;

        private Action _resendMessage;

        public Action ResendMessage
        {
            get => _resendMessage;
            set => _resendMessage = value;
        }
#endregion Events

        public async UniTask SendIfEncryptActivated(Action onTimeout = null)
        {
            Protocols.ClientConnection.GetEncryptionStatusRequest encryptRequest = new();

            if (_logicalAddress == null)
            {
                Protocols.SourceAddress address = new()
                {
                    ServerType  = Protocols.ServerType.Client,
                    Identifiers = _userID
                };
                _logicalAddress = address.ToByteArray();
            }

            var logicalDestination = Protocols.DestinationLogicalAddress.GetLogicalAddress(Protocols.Channels.ClientConnection);
            _socket!.SendMessage(logicalDestination, (int)Protocols.Channels.ClientConnection, (int)Protocols.ClientConnection.MessageTypes.GetEncryptionStatusRequest, encryptRequest.ToByteArray(),
                                 _logicalAddress);

            var timer = 0f;
            while (!Security.IsActivatedIfEncrypt)
            {
                if (timer > DefaultTimoutTime)
                {
                    onTimeout?.Invoke();
                    return;
                }
                await UniTask.Yield();
                timer += Time.deltaTime * 1000;
            }
        }

        public async UniTask SendPairKeyToServer(Action onTimeout = null)
        {
            if (!Security.IsActivatedEncrypt) return;
            
            var keyIV = Security.CreateNewKeyAndIV();
            Protocols.ClientConnection.ExchangeKeyRequest encryptRequest = new()
            {
                ClientAesKey = ByteString.CopyFrom(keyIV.Key),
                ClientAesIv = ByteString.CopyFrom(keyIV.IV),
            };

            if (_logicalAddress == null)
            {
                Protocols.SourceAddress address = new()
                {
                    ServerType  = Protocols.ServerType.Client,
                    Identifiers = _userID
                };
                _logicalAddress = address.ToByteArray();
            }
            
            var logicalDestination = Protocols.DestinationLogicalAddress.GetLogicalAddress(Protocols.Channels.ClientConnection);
            _socket!.SendRSA(logicalDestination, (int)Protocols.Channels.ClientConnection, (int)Protocols.ClientConnection.MessageTypes.ExchangeKeyRequest, encryptRequest.ToByteArray(), _logicalAddress);

            var timer = 0f;
            while (!Security.IsAESInitialized)
            {
                if (timer > DefaultTimoutTime)
                {
                    onTimeout?.Invoke();
                    return;
                }
                await UniTask.Yield();
                timer += Time.deltaTime * 1000;
            }
        }

        private static void ReturnContextToPool(ReceiveContext context)
        {
            if (context == null) return;
            context.Channel = 0;
            context.Command = 0;
            context.Message = null;
        }

        /// <summary>
        /// 외부에서 어셈블리를 조회하여 MessageProcessor가 담긴 클래스를 등록시켜 줍니다.
        /// </summary>
        /// <param name="types">IMessageProcessor가 담긴 클래스의 모음</param>
        public void RegisterMessageProcessor(IEnumerable<Type> types)
        {
            if (_isAlreadyAddProcessors) return;
            C2VDebug.Log("REGISTER MESSAGE PROCESSOR");
            foreach (var type in types)
            {
                C2VDebug.Log($"-> {type.Name}");
                IMessageProcessor messageProcessor = Activator.CreateInstance(type) as IMessageProcessor;
                messageProcessor.Initialize();
                _messageProcessors.TryAdd(type.GetCustomAttribute<ChannelAttribute>().Channel, messageProcessor);
                C2VDebug.Log($"-> {type.Name}...DONE");
            }

            _isAlreadyAddProcessors = true;
        }

        /// <summary>
        /// 접속할 서버의 주소, 포트를 설정한다. Identity로 쓰일 유저 ID도 설정할 수 있다.
        /// </summary>
        /// <param name="address">접속할 서버 주소</param>
        /// <param name="port">접속할 포트</param>
        /// <param name="userID">Identity 값</param>
        public void SetupServerAddress(string address, int port, long userID = 0)
        {
            _logicalAddress = null;
            serverAddress   = address;
            serverPort      = port;
            _userID         = userID;
        }

        /// <summary>
        /// 퍼블릭 메타버스에 접속하기 위한 준비를 합니다.
        /// </summary>
        /// <param name="onDisconnected">Disconnect될 때의 콜백</param>
        /// <param name="onLoginStart">실제 접속 패킷을 호출하는 콜백</param>
        public void StartCom2VerseConnect(Action onDisconnected, Action onLoginStart)
        {
            TokenExpired   =  false;
            _loginFailed   =  onDisconnected;
            OnDisconnected += _loginFailed;
            C2VDebug.Log($"Initializing network manager : (com2verse) - {serverAddress}:{serverPort}");

            StopSocket();
            C2VDebug.Log("SOCKET CHECK END : (public)");
            _socket = new();
            _socket.OnPacketReceived += OnReceive;
            _socket.OnDisconnected += SocketOnOnDisconnected;
            _socket.Connect(serverAddress, serverPort);
            _isRequestedDisconnect = false;
            C2VDebug.Log($"SOCKET INITIALIZED : (com2verse) - {serverAddress}:{serverPort}");
            onLoginStart?.Invoke();
        }

        public void StartOfficeConnect(Action onLoginStart)
        {
            C2VDebug.Log($"SOCKET INITIALIZED : (office) - {serverAddress}:{serverPort}");
            _isRequestedDisconnect = false;
            onLoginStart?.Invoke();
        }

        private void SocketOnOnDisconnected(CloseEventArgs e)
        {
            _isRequestedDisconnect = true;
            SwitchToMainThreadAction(() => _socketDisconnected?.Invoke()).Forget();
        }

        private async UniTaskVoid SwitchToMainThreadAction(Action action)
        {
            var context = SynchronizationContext.Current;
            await UniTask.SwitchToMainThread();

            action?.Invoke();

            if (context != null)
                await UniTask.SwitchToSynchronizationContext(context);
        }

        /// <summary>
        /// 접속된 서버와의 핑퐁을 시작합니다.
        /// </summary>
        public void StartPingPong()
        {
            C2VDebug.Log("Received login complete");
            _socket.StartPingPong(_userID);
        }

        private void OnReceive(int channel, int command, int errorCode, ArraySegment<byte> receiveData)
        {
            ReceiveContext context;
            lock (_threadLockObject)
                context = _contextPool.Get();
            try
            {
                context.Channel   = (Protocols.Channels)channel;
                context.Command   = command;
                context.ErrorCode = (Protocols.ErrorCode)errorCode;
                if (context.ErrorCode == Protocols.ErrorCode.Success)
                {
                    if (_messageProcessors.TryGetValue(context.Channel, out var message))
                        context.Message = message.Parse(command, receiveData);
                }

                _receivedMessages.Enqueue(context);

                if (IsVerbose)
                    LogMessage(context.Message, context.ErrorCode, context.Channel, context.Command);
            }
            catch (Exception e)
            {
                lock (_threadLockObject)
                    _contextPool.Release(context);
                C2VDebug.LogError($"Error while receiving data: {e.Message}\n{e.StackTrace}");
                if (IsConnected)
                    _onNetworkError?.Invoke(e.Message);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            while (_receivedMessages.TryDequeue(out var context))
            {
                try
                {
                    var channel = context.Channel;
                    CheckTimeoutPacket(channel, context.Command);
                    if (context.ErrorCode == Protocols.ErrorCode.Success)
                        _messageProcessors[channel].Process(channel, context.Command, context.Message);
                    else
                        _messageProcessors[channel].ErrorProcess(channel, context.Command, context.ErrorCode);
                    lock (_threadLockObject)
                        _contextPool.Release(context);
                }
                catch (Exception e)
                {
                    lock (_threadLockObject)
                        _contextPool.Release(context);
                    C2VDebug.LogError($"Error while Receiving Packet: {e.Message}\n{e.StackTrace}");
                    if (IsConnected)
                        _onNetworkError?.Invoke(e.Message);
                }
                if (!IsConnected)
                    break;
            }

            if (_isRequestedDisconnect)
            {
                _isRequestedDisconnect = false;
                Disconnect(false);
            }

            PollingCheckTimoutInUpdate(); // 모든 패킷 처리가 끝난 후 체크
        }

        public void Send(IMessage message, Protocols.Notification.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.Notification, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }
        
        public void Send(IMessage message, Protocols.CommonLogic.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.CommonLogic, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        public void Send(IMessage message, Protocols.OfficeMessenger.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.OfficeMessenger, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// GameLogic 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.GameLogic.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.GameLogic, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// GameMechanic 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.GameMechanic.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.GameMechanic, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// WorldState 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.WorldState.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.WorldState, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// Controller 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.Controller.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.Controller, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// InternalCycle 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.InternalCycle.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.InternalCycle, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// Communication 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.Communication.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.Communication, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// OfficeMeeting 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.OfficeMeeting.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.OfficeMeeting, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// ObjectCondition 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.ObjectCondition.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.ObjectCondition, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        /// <summary>
        /// Chat 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.Chat.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.Chat, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }
        /// <summary>
        /// Mice 채널의 메시지를 보냅니다. timeout을 설정하여 응답에 대한 타임아웃 처리를 추가할 수 있습니다.
        /// </summary>
        /// <param name="message">메시지 페이로드(Protobuf)</param>
        /// <param name="type">메시지 타입</param>
        /// <param name="waitingChannel">기다릴 응답에 대한 채널</param>
        /// <param name="waitingCommand">기다릴 응답에 대한 커맨드</param>
        /// <param name="timeout">기다릴 응답에 대한 타임아웃</param>
        /// <param name="timeoutAction">타임아웃이 발생되었을 때의 콜백</param>
        public void Send(IMessage message, Protocols.Mice.MessageTypes type, Protocols.Channels waitingChannel = 0, int waitingCommand = -1, int timeout = -1, Action timeoutAction = null)
        {
            Send(message, Protocols.Channels.Mice, type, waitingChannel, waitingCommand, timeout, timeoutAction);
        }

        private byte[]       _logicalAddress;
        private void Send<T>(IMessage message, Protocols.Channels channel, T type, Protocols.Channels waitingChannel, int waitingCommand, int timeout, Action timeoutAction) where T : unmanaged, Enum
        {
            if (_logicalAddress == null)
            {
                Protocols.SourceAddress address = new()
                {
                    ServerType = Protocols.ServerType.Client,
                    Identifiers = _userID
                };
                _logicalAddress = address.ToByteArray();
            }
            var logicalDestination = Protocols.DestinationLogicalAddress.GetLogicalAddress(channel);
            var buffer = message.ToByteArray();
            
            bool result = _socket?.SendMessage(logicalDestination,
                                               (int)channel,
                                               type.CastInt(),
                                               buffer,
                                               _logicalAddress) ?? false;

            if (IsVerbose)
            {
                var errorCode = result ? Protocols.ErrorCode.Success : Protocols.ErrorCode.None;
                LogMessage(message, errorCode, channel, type, logicalDestination);
            }

            if (result)
            {
                if (waitingCommand != -1)
                {
                    if (timeout == -1)
                        timeout = _customTimeoutTime == -1 ? DefaultTimoutTime : _customTimeoutTime;
                    WaitingPacketInfo info = new WaitingPacketInfo
                    {
                        Channel         = waitingChannel,
                        Command         = waitingCommand,
                        ExpiredTime     = MetaverseWatch.Time + timeout,
                        TimeoutCallback = timeoutAction,
                    };
                    _timeoutPacketList.Add(info);
                }
            }
            else
                C2VDebug.LogWarning("Cannot send message. It was disconnected.");
        }

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough, StackTraceIgnore]
        private static void LogMessage<T>(IMessage message, Protocols.ErrorCode errorCode, Protocols.Channels channel, T command, byte[] destinationAddress = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message?.GetType().ToString());
            AppendInfo("Result", errorCode.ToString());
            AppendInfo("Channel", channel.ToString());
            AppendInfo("Command", command?.ToString());
            AppendArrayInfo("Destination", destinationAddress);
            C2VDebug.LogCategory(nameof(NetworkManager), sb.ToString());

            void AppendArrayInfo(string title, IEnumerable<byte> array)
            {
                sb.Append(title);
                sb.Append(": ");
                if (array != null)
                    foreach (var x in array)
                        sb.Append(x.ToString()).Append('.');
                sb.AppendLine();
            }

            void AppendInfo(string title, string info)
            {
                sb.Append(title);
                sb.Append(": ");
                sb.Append(info);
                sb.AppendLine();
            }
        }

#region Timout Check
        /// <summary>
        /// 등록된 응답리스트를 순회하면서 타임아웃 체크를 합니다. IMessageProcessor에서 호출됩니다.
        /// </summary>
        /// <param name="channel">응답이 온 채널</param>
        /// <param name="command">응답이 온 커맨드</param>
        public void CheckTimeoutPacket(Protocols.Channels channel, int command)
        {
            var index = _timeoutPacketList.FindIndex((info) => info.Channel == channel && info.Command == command);
            if (index < 0) return;
            _timeoutPacketList.RemoveAt(index);
        }

        private void PollingCheckTimoutInUpdate()
        {
            for (int i = _timeoutPacketList.Count - 1; i >= 0; --i)
            {
                if (TokenExpired)
                    _timeoutPacketList.RemoveAt(i);
                else if (_timeoutPacketList[i].ExpiredTime < MetaverseWatch.Time)
                {
                    _timeoutPacketList[i].TimeoutCallback?.Invoke();
                    _timeoutPacketList.RemoveAt(i);
                }
            }
        }
#endregion Timout Check

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Disconnect 처리를 합니다. 내부적으로 모든 멤버들과 소켓을 정리합니다.
        /// </summary>
        /// <param name="withLogout">로그아웃을 먼저 보낼지 여부</param>
        public void Disconnect(bool withLogout)
        {
            if (withLogout)
            {
                C2VDebug.LogError("Disconnecting connection: Logout");
                _onLogout?.Invoke();
            }
            else
                C2VDebug.LogError("Disconnecting connection: Connection lost");

            CleanMemberVariables();
            StopSocket();
            _onDisconnected?.Invoke();
        }

        public int GetRtt() => _socket?.AverageRTT ?? 0;

        private void CleanMemberVariables()
        {
            _isAlreadyAddProcessors = false;
            _messageProcessors.Clear();
            _receivedMessages.Clear();
            _contextPool.Clear();
        }

        private void CleanEvents()
        {
            _socketDisconnected = null;
            _onDisconnected     = null;
            _onNetworkError     = null;
            _onLogout           = null;
        }

        protected override void OnDestroyInvoked()         => Destroy();
        protected override void OnApplicationQuitInvoked() => Destroy();

        private void Destroy()
        {
            CleanMemberVariables();
            StopSocket();
            CleanEvents();
        }

        private void StopSocket()
        {
            if (_socket != null)
            {
                _socket.OnPacketReceived -= OnReceive;
                _socket.OnDisconnected -= SocketOnOnDisconnected;
                _socket.StopPingPong();
                _socket.Stop();
                _socket = null;
            }
        }
    }
}
