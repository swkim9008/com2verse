/*===============================================================
* Product:		Com2Verse
* File Name:	WebSocketHelperUI.cs
* Developer:	jhkim
* Date:			2023-05-01 15:18
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.Chat;
using Com2Verse.Network;
using UnityEditor;
using Com2VerseEditor.UGC.UIToolkitExtension;
using Com2VerseEditor.UGC.UIToolkitExtension.Containers;
using Com2VerseEditor.UGC.UIToolkitExtension.Controls;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;
using WebSocketSharp;

namespace Com2Verse
{
	public class WebSocketHelperUI : EditorWindowEx
	{
#region Variables
		private WebSocketHelperUIModel _model;
		private int _selectedIdx = -1;

		private Dictionary<WebSocketClient.eWebSocketState, StyleColor> _stateColorMap = new Dictionary<WebSocketClient.eWebSocketState, StyleColor>()
		{
			{WebSocketClient.eWebSocketState.NEW, new StyleColor(Color.gray)},
			{WebSocketClient.eWebSocketState.CONNECTING, new StyleColor(Color.blue)},
			{WebSocketClient.eWebSocketState.OPEN, new StyleColor(Color.green)},
			{WebSocketClient.eWebSocketState.CLOSING, new StyleColor(Color.red)},
			{WebSocketClient.eWebSocketState.CLOSED, new StyleColor(Color.gray)},
		};
#endregion // Variables

#region UI Toolkit
		[MenuItem("Com2Verse/Tools/웹 소켓 테스트", priority = 0)]
		public static void Open()
		{
			var window = GetWindow<WebSocketHelperUI>();
			window.SetConfig(window, new Vector2Int(700, 1000), "웹 소켓 테스트");
		}
		public override string MetaGuid => "16cc89494b2a903498831514601fba91";
		public override string ModelGuid => "53d0e07b3c3f51e4396b5273fbe7cde9";

		protected override void OnStart(VisualElement root)
		{
			base.OnStart(root);
			if (metaData != null && metaData.modelObject is WebSocketHelperUIModel model)
			{
				_model = model;
				_model.ClearTemporaryField();
			}
			InitUI();
		}

		protected override void OnDraw(VisualElement root)
		{
			base.OnDraw(root);
		}

		protected override void OnClear(VisualElement root)
		{
			base.OnClear(root);
			_model.DisposeAll();
		}

		private void OnDisable()
		{
			_model.DisposeAll();
		}
#endregion // UI Toolkit

#region Initialization
		private void InitUI()
		{
			InitConnections();
			InitConnectionInfo();
			InitSendMessage();
			InitReceiveMessage();
			InitChatTest();
		}

		private void InitConnections()
		{
			var connections = rootVisualElement.Q<ListViewEx>("listConnections");
			connections.selectionType = SelectionType.Single;
			connections.showAddRemoveFooter = true;
			connections.makeItem = MakeItem;
			connections.bindItem = BindItem;
			connections.itemsAdded += OnItemAdded;
			connections.itemsRemoved += OnItemRemoved;
			connections.onSelectedIndicesChange += OnItemIdxChanged;

			VisualElement MakeItem() => InstantiateFromUxml("ConnectionItem");
			void BindItem(VisualElement element, int i)
			{
				if (_model.TryGetConnectionInfo(i, out var info))
				{
					var label = element.Q<LabelEx>("label");
					var url = element.Q<LabelEx>("url");
					var state = element.Q<LabelEx>("state");
					label.text = info.Label;
					url.text = info.Url;
					url.tooltip = info.Url;
					state.text = info.Client == null ? "NONE" : info.Client.State.ToString();
					state.style.color = info.Client == null ? _stateColorMap[WebSocketClient.eWebSocketState.CLOSED] : _stateColorMap[info.Client.State];
				}
			}

			void OnItemAdded(IEnumerable<int> indices)
			{
				var idx = indices.First();
				_model.AddConnectionInfos(new WebSocketHelperUIModel.ConnectionInfo());
				SelectItem(idx);
				RefreshConnectionInfoAsync().Forget();
			}

			void OnItemRemoved(IEnumerable<int> indices)
			{
				var idx = indices.First();
				_model.RemoveConnectionInfos(idx);
			}
			void OnItemIdxChanged(IEnumerable<int> indices)
			{
				var idx = -1;
				if (indices.Any()) idx = indices.First();
				if (idx == -1) idx = _model.GetLastConnectionInfoIdx();

				SelectItem(idx);
			}

			RefreshConnectionInfoAsync().Forget();
		}

		private async UniTask RefreshConnectionInfoAsync()
		{
			var connections = rootVisualElement.Q<ListViewEx>("listConnections");
			await InvokeOnMainThreadAsync(() => connections.itemsSource = _model.ConnectionInfos.ToArray());
		}
		private void InitConnectionInfo()
		{
			SetButtonOnClick("btnSave", OnSave);
			SetButtonOnClick("btnConnect", OnConnect);

			void OnSave(Button btn)
			{
				if (_model.SaveConnectionInfo(_selectedIdx))
					RefreshConnectionInfoAsync().Forget();
			}

			void OnConnect(Button btn)
			{
				OnSave(null);
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
				{
					if (info.Client == null)
					{
						var client = new WebSocketClient();
						client.OnStateChanged += OnStateChanged;
						client.OnEventReceived += OnEventReceived;
						_model.SetClient(_selectedIdx, client);
						client.Connect(info.Url);
					}
					else
					{
						if (info.Client.IsConnected)
						{
							info.Client.Stop();
						}
						else
						{
							info.Client.SetUrl(info.Url);
							info.Client.Connect(info.Url);
						}
					}
				}

				void OnStateChanged(WebSocketClient.eWebSocketState state)
				{
					if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
					{
						_model.Refresh(info);
						RefreshConnectionInfoAsync().Forget();
					}
				}

				void OnEventReceived(EventArgs e)
				{
					switch (e)
					{
						case CloseEventArgs closeEvent:
							break;
						case ErrorEventArgs errorEvent:
							break;
						case MessageEventArgs messageEvent:
						{
							if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
							{
								info.AddReceiveMessage(messageEvent.Data);
								RefreshReceiveMessageAsync().Forget();
							}
						}
							break;
						default:
							break;
					}
				}
			}
		}
#endregion // Initialization

#region Connect Items
		private void SelectItem(int idx)
		{
			if (idx == -1) return;

			_selectedIdx = idx;
			if (_model.TryGetConnectionInfo(idx, out var info))
			{
				_model.Refresh(info);
				RefreshSendMessage();
				RefreshReceiveMessageAsync().Forget();
			}
		}
#endregion // Connect Items

#region Send
		private void InitSendMessage()
		{
			var sendItems = rootVisualElement.Q<ListViewEx>("sendMessages");
			sendItems.makeItem = () => InstantiateFromUxml("SendMessage");
			sendItems.bindItem = (element, i) =>
			{
				var message = element.Q<Label>("message");
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
					message.text = info.GetSendMessage(i);
			};
			RefreshSendMessage();

			var textSendMessage = rootVisualElement.Q<TextFieldEx>("textSendMessage");
			textSendMessage.RegisterCallback<KeyDownEvent>(OnTextSendMessageKeyDown);

			var btnSend = rootVisualElement.Q<ButtonEx>("btnSend");
			btnSend.clickable = new Clickable(OnSend);

			var btnClearSendMessages = rootVisualElement.Q<ButtonEx>("btnClearSendMessages");
			btnClearSendMessages.clickable = new Clickable(OnClearSendMessages);

			void OnTextSendMessageKeyDown(KeyDownEvent evt)
			{
				if (evt.keyCode == KeyCode.Return)
					OnSend();
			}

			void OnSend()
			{
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
				{
					if (info.Client == null) return;

					if (info.Client.State == WebSocketClient.eWebSocketState.OPEN)
					{
						info.Client.Send(_model.SendText);
						info.AddSendMessage(_model.SendText);
						_model.ClearSendText();
						RefreshSendMessage();
						FocusSendText();
					}
				}

				void FocusSendText()
				{
					var textSendMessage = rootVisualElement.Q<TextFieldEx>("textSendMessage");
					textSendMessage.Focus();
				}
			}

			void OnClearSendMessages()
			{
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
				{
					info.ClearSendMessages();
					RefreshSendMessage();
				}
			}
		}

		private void RefreshSendMessage()
		{
			var sendItems = rootVisualElement.Q<ListViewEx>("sendMessages");
			if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
				sendItems.itemsSource = info.SendMessages.ToArray();
		}
#endregion // Send

#region Receive
		private void InitReceiveMessage()
		{
			var receiveItems = rootVisualElement.Q<ListViewEx>("receiveMessages");
			receiveItems.makeItem = () => InstantiateFromUxml("ReceiveMessage");
			receiveItems.bindItem = async (element, i) =>
			{
				var label = element.Q<Label>("message");
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
					await InvokeOnMainThreadAsync(() => label.text = info.GetReceiveMessage(i));
			};
			RefreshReceiveMessageAsync().Forget();

			var btnClearReceiveMessages = rootVisualElement.Q<ButtonEx>("btnClearReceiveMessages");
			btnClearReceiveMessages.clickable = new Clickable(OnClearReceiveMessages);

			void OnClearReceiveMessages()
			{
				if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
				{
					info.ClearReceiveMessages();
					RefreshReceiveMessageAsync().Forget();
				}
			}
		}

		private async UniTask RefreshReceiveMessageAsync()
		{
			var receiveItems = rootVisualElement.Q<ListViewEx>("receiveMessages");
			if (_model.TryGetConnectionInfo(_selectedIdx, out var info))
			{
				await InvokeOnMainThreadAsync(() =>
				{
					if (info.ReceiveMessages != null)
						receiveItems.itemsSource = info.ReceiveMessages.ToArray();
				});
			}
		}
#endregion // Receive

#region Chat Test
		private void InitChatTest()
		{
			InitChatInput();
			InitChatButtons();
			InitChatChannels();
			InitChatMessages();
			RefreshChatUI();
		}

		void InitChatInput()
		{
			var textChatMessage = rootVisualElement.Q<TextField>("textChatMessage");
			textChatMessage.RegisterCallback<KeyDownEvent>(OnTextSendMessageKeyDown);

			void OnTextSendMessageKeyDown(KeyDownEvent evt)
			{
				if (evt.keyCode == KeyCode.Return)
				{
					SendChatMessage();
					FocusChatInput();
				}
			}
		}
		void InitChatButtons()
		{
			SetButtonOnClick("btnChatConnect", OnChatConnect);
			SetButtonOnClick("btnChatReset", OnChatReset);
			SetButtonOnClick("btnChatSend", OnSendMessage);
			SetButtonOnClick("btnChatMoveArea", OnMoveArea);
			SetButtonOnClick("btnChatClear", OnClearAllChat);

			void OnChatConnect(Button btn)
			{
				if (_model.ChatClient == null)
				{
					_model.ChatClient                 =  new ChatCoreCom2Verse();
					_model.ChatClient.SetServerUrl(_model.ChatServerAddr, _model.ChatUserId, _model.ChatDeviceId, _model.ChatAppId);
					// _model.ChatClient.OnStateChanged  += OnStateChanged;
					// _model.ChatClient.OnEventReceived += OnEventReceived;
					_model.ChatClient.ConnectChatServerV1(long.Parse(_model.ChatUserId));
				}
				else
				{
					if (_model.ChatClient.IsOpen)
					{
						_model.ChatClient.DisconnectChatServer();
					}
					else
					{
						_model.ChatClient.SetServerUrl(_model.ChatServerAddr, _model.ChatUserId, _model.ChatDeviceId, _model.ChatAppId);
						_model.ChatClient.ConnectChatServerV1(long.Parse(_model.ChatUserId));
					}
				}

				void OnStateChanged(WebSocketClient.eWebSocketState state)
				{
					RefreshChatConnectionInfo();
				}

				void OnEventReceived(EventArgs e)
				{
					switch (e)
					{
						case MessageEventArgs _:
							RefreshChatItems();
							break;
						default:
							break;
					}
				}
				void RefreshChatConnectionInfo()
				{
					InvokeOnMainThreadAsync(() =>
					{
						var chatState = rootVisualElement.Q<LabelEx>("chatState");
						chatState.text = _model.ChatClient?.State.ToString() ?? "NONE";
						chatState.style.color = _model.ChatClient == null ? _stateColorMap[WebSocketClient.eWebSocketState.CLOSED] : _stateColorMap[_model.ChatClient.State];
						RefreshChatUI();
					}).Forget();
				}
			}

			void OnChatReset(Button btn) => ChatReset();
			void OnSendMessage(Button btn) => SendChatMessage();
			void OnMoveArea(Button btn) => MoveChatArea();
			void OnClearAllChat(Button btn) => ClearAllChat();
		}

		void InitChatChannels()
		{
			var channels = rootVisualElement.Q<ListView>("listChatChannels");
			channels.makeItem = MakeChannelItem;
			channels.bindItem = (element, idx) =>
			{
				var channelNames = GetChannelNames();
				var btn = element as Button;
				var selected = channelNames[idx] == _model.ChatClient.CurrentArea;
				btn.text = channelNames[idx];
				btn.style.unityFontStyleAndWeight = selected ? FontStyle.Bold : FontStyle.Normal;
				btn.clickable = new Clickable(() =>
				{
					_model.ChatClient?.AreaMove(btn.text);
					_model.ChatArea = btn.text;
					RefreshChatItems();
				});
			};

			VisualElement MakeChannelItem() => new Button();
		}
		void InitChatMessages()
		{
			var messages = rootVisualElement.Q<ListView>("listChatMessages");
			messages.makeItem = () => InstantiateFromUxml("ChatMessage");
			messages.bindItem = BindMessage;
			RefreshChatItems();

			void BindMessage(VisualElement element, int idx)
			{
				var item = messages.itemsSource[idx] as ChatApi.SendMessageResponsePayload;
				var messageTime = element.Q<LabelEx>("messageTime");
				var messageId = element.Q<LabelEx>("messageId");
				var sendUserId = element.Q<LabelEx>("sendUserId");
				var type = element.Q<LabelEx>("type");
				var message = element.Q<LabelEx>("message");

				messageTime.text = Convert.ToString(item.MessageTime);
				messageId.text = Convert.ToString(item.MessageId);
				sendUserId.text = item.SendUserId;
				if (item.Comments.Length > 0)
				{
					type.text = ChatApi.GetCommentsType(item.Comments[0].Type).ToString();
					message.text = item.Comments[0].Text;
				}
			}
		}

		void RefreshChatItems()
		{
			InvokeOnMainThreadAsync(() =>
			{
				var channels = rootVisualElement.Q<ListView>("listChatChannels");
				var messages = rootVisualElement.Q<ListView>("listChatMessages");
				channels.itemsSource = GetChannelNames();
				messages.itemsSource = _model.ChatClient?.CurrentAreaMessages.ToArray();
			}).Forget();
		}

		string[] GetChannelNames()
		{
			var messages = _model.ChatClient?.ChatMessages;
			return messages?.Count == 0 ? Array.Empty<string>() : messages?.Select(kvp => kvp.Key).ToArray();
		}

		private void ChatReset()
		{
			_model.ChatClient?.Dispose();
		}
		private void SendChatMessage()
		{
			if (_model.ChatArea != _model.ChatClient?.CurrentArea)
				_model.ChatClient?.AreaMove(_model.ChatArea);

			_model.ChatClient.RequestSendAreaMessage(_model.ChatMessage, "editorUser");
		}

		private void FocusChatInput()
		{
			var textChatMessage = rootVisualElement.Q<TextField>("textChatMessage");
			textChatMessage.Focus();
		}
		private void MoveChatArea()
		{
			_model.ChatClient?.AreaMove(_model.ChatArea);
		}

		private void ClearAllChat()
		{
			_model.ChatArea = string.Empty;
			_model.ChatClient?.ClearAllChat();
			RefreshChatItems();
		}
		void RefreshChatUI()
		{
			var chatMenu = rootVisualElement.Q<VisualElement>("layoutChatMenu");
			switch (_model.ChatClient?.State)
			{
				case WebSocketClient.eWebSocketState.NEW:
				case WebSocketClient.eWebSocketState.CONNECTING:
				case WebSocketClient.eWebSocketState.OPEN:
					chatMenu.style.display = DisplayStyle.Flex;
					break;
				case WebSocketClient.eWebSocketState.CLOSING:
				case WebSocketClient.eWebSocketState.CLOSED:
				case null:
				default:
					chatMenu.style.display = DisplayStyle.None;
					break;
			}
		}
#endregion // Chat Test

#region Util
		private VisualElement InstantiateFromUxml(string name)
		{
			var item = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Packages/com.com2verse.websockethelper/Editor/WebSocketHelperUI/{name}.uxml");
			return item != null ? item.Instantiate() : null;
		}

		private async UniTask InvokeOnMainThreadAsync(Action onAction)
		{
			if (SynchronizationContext.Current == null)
				await InvokeActionAsync();
			else
			{
				await using (UniTask.ReturnToCurrentSynchronizationContext())
				{
					await InvokeActionAsync();
				}
			}

			async UniTask InvokeActionAsync()
			{
				await UniTask.SwitchToMainThread();
				onAction?.Invoke();
			}
		}

		private void SetButtonOnClick(string name, Action<Button> onClick)
		{
			var btn = rootVisualElement.Q<Button>(name);
			if (btn == null) return;
			btn.clickable = new Clickable(() => onClick?.Invoke(btn));
		}
#endregion // Util
	}
}
