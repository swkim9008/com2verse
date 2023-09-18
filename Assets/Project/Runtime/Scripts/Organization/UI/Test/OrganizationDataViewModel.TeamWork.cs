/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.TeamWork.cs
* Developer:	jhkim
* Date:			2022-10-11 11:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Com2Verse.Logger;
using Com2Verse.Network;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Protocols.GameLogic;
using UnityEngine;

namespace Com2Verse.UI
{
	// TeamWork
	public partial class OrganizationDataViewModel
	{
		private static readonly string ParamGroupId = "GroupId";
		private static readonly string ParamEmployeeNoList = "EmployeeNoList";
		private static readonly string ParamGroupName = "GroupName";
		private static readonly string ParamGroupDescription = "GroupDescription";
		private static readonly string ParamEmployeeNo = "EmployeeNo";
		private static readonly string ParamReply = "reply";

		private static readonly float _requestListHeightMax = 300;
#region Variables
		public Collection<OrganizationDataTeamWorkListViewModel> _buttons = new();
		private string _requestName;

		// Request
		private float _requestListHeight;
		private string _requestPreview;
		public Collection<OrganizationDataTeamWorkRequestFieldListViewModel> _requestFieldItems = new();
		public CommandHandler SendRequest { get; private set; }

		// Response
		private string _responseMessage;
		public CommandHandler CopyToClipboard { get; private set; }
		public CommandHandler ClearResponse { get; private set; }
		private DataInjector<IMessage> _injector;
#endregion // Variables

#region Properties
		public Collection<OrganizationDataTeamWorkListViewModel> Buttons
		{
			get => _buttons;
			set
			{
				_buttons = value;
				InvokePropertyValueChanged(nameof(Buttons), value);
			}
		}

		public string RequestName
		{
			get => _requestName;
			set
			{
				_requestName = value;
				InvokePropertyValueChanged(nameof(RequestName), value);
			}
		}

		public float RequestListHeight
		{
			get => _requestListHeight;
			set
			{
				_requestListHeight = MathF.Min(_requestListHeightMax, value);
				InvokePropertyValueChanged(nameof(RequestListHeight), RequestListHeight);
			}
		}
		public string RequestPreview
		{
			get => _requestPreview;
			set
			{
				_requestPreview = value;
				InvokePropertyValueChanged(nameof(RequestPreview), value);
			}
		}
		public Collection<OrganizationDataTeamWorkRequestFieldListViewModel> RequestFieldItems
		{
			get => _requestFieldItems;
			set
			{
				_requestFieldItems = value;
				InvokePropertyValueChanged(nameof(RequestFieldItems), value);
			}
		}

		public string ResponseMessage
		{
			get => _responseMessage;
			set
			{
				_responseMessage = value;
				InvokePropertyValueChanged(nameof(ResponseMessage), value);
			}
		}
#endregion // Properties

#region Data
		private struct RequestInfo
		{
			public string Title;
			public Type RequestType;
			public MessageTypes MessageTypes;
			public RequestParam[] RequestParams;
		}

		private struct RequestParam
		{
			public eRequestType Type;
			public string ParamName { get; private set; }
			public string Value;

			public static RequestParam[] Empty => Array.Empty<RequestParam>();
			private static RequestParam NewParam(string paramName) => new RequestParam {ParamName = ToPropertyName(paramName)};

			private static string ToPropertyName(string name)
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < name.Length; ++i)
				{
					if (i == 0 && IsLower(name[i]))
						sb.Append((char)(name[i] - 'a' + 'A'));
					else
						sb.Append(name[i]);
				}

				return sb.ToString();

				bool IsLower(char c) => c >= 'a' && c <= 'z';
			}
			public static RequestParam NewIntParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.INT;
				return param;
			}

			public static RequestParam NewIntArrayParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.ARRAY_INT;
				return param;
			}
			public static RequestParam NewLongParam(string paramName)
			{
				var param = NewLongParam(paramName);
				param.Type = eRequestType.LONG;
				return param;
			}

			public static RequestParam NewLongArrayParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.ARRAY_LONG;
				return param;
			}

			public static RequestParam NewStringParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.STRING;
				return param;
			}

			public static RequestParam NewStringArrayParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.ARRAY_STRING;
				return param;
			}
			public static RequestParam NewBooleanParam(string paramName)
			{
				var param = NewParam(paramName);
				param.Type = eRequestType.BOOLEAN;
				return param;
			}
			public static RequestParam[] MakeParams(params RequestParam[] parameters) => parameters;
		}

		private enum eRequestType
		{
			INT,
			LONG,
			STRING,
			ARRAY_INT,
			ARRAY_LONG,
			ARRAY_STRING,
			BOOLEAN,
		}

		private RequestInfo[] _requestInfos = new RequestInfo[]
		{
			new RequestInfo
			{
				Title = "그룹 목록 조회/(MessageTypes.GroupListRequest)",
				RequestType = typeof(GroupListRequest),
				MessageTypes = MessageTypes.GroupListRequest,
				RequestParams = RequestParam.Empty, // TODO : 파라미터 전달 (0 - 전체 조회, ID - 해당 그룹 조회)
			},
			// new RequestInfo
			// {
			// 	Title = "Group Item Request",
			// 	RequestType = typeof(GroupItemRequest),
			// 	MessageTypes = MessageTypes.GroupItemRequest,
			// 	RequestParams = RequestParam.MakeParams(RequestParam.NewIntParam(ParamGroupId)),
			// },
			new RequestInfo
			{
				Title = "그룹 생성/(MessageTypes.GroupCreateRequest)",
				RequestType = typeof(GroupCreateRequest),
				MessageTypes = MessageTypes.GroupCreateRequest,
				RequestParams = RequestParam.MakeParams(RequestParam.NewStringArrayParam(ParamEmployeeNoList)),
			},
			new RequestInfo
			{
				Title = "그룹 편집/(MessageTypes.GroupEditRequest)",
				RequestType = typeof(GroupEditRequest),
				MessageTypes = MessageTypes.GroupEditRequest,
				RequestParams = RequestParam.MakeParams(
					RequestParam.NewIntParam(ParamGroupId),
					RequestParam.NewStringParam(ParamGroupName),
					RequestParam.NewStringParam(ParamGroupDescription)
				),
			},
			new RequestInfo
			{
				Title = "관리자 지정/(MessageTypes.GroupSetManagerRequest)",
				RequestType = typeof(GroupSetManagerRequest),
				MessageTypes = MessageTypes.GroupSetManagerRequest,
				RequestParams = RequestParam.MakeParams(
					RequestParam.NewIntParam(ParamGroupId),
					RequestParam.NewIntParam(ParamEmployeeNo)
				),
			},
			new RequestInfo
			{
				Title = "멤버 초대/(MessageTypes.GroupInviteMemberRequest)",
				RequestType = typeof(GroupInviteMemberRequest),
				MessageTypes = MessageTypes.GroupInviteMemberRequest,
				RequestParams = RequestParam.MakeParams(
					RequestParam.NewIntParam(ParamGroupId),
					RequestParam.NewStringArrayParam(ParamEmployeeNoList)
				),
			},
			new RequestInfo
			{
				Title = "멤버 내보내기/(MessageTypes.GroupDismissMemberRequest)",
				RequestType = typeof(GroupDismissMemberRequest),
				MessageTypes = MessageTypes.GroupDismissMemberRequest,
				RequestParams = RequestParam.MakeParams(
					RequestParam.NewIntParam(ParamGroupId),
					RequestParam.NewStringArrayParam(ParamEmployeeNoList)
				),
			},
			new RequestInfo
			{
				Title = "그룹 나가기/(MessageTypes.GroupOutRequest)",
				RequestType = typeof(GroupOutRequest),
				MessageTypes = MessageTypes.GroupOutRequest,
				RequestParams = RequestParam.MakeParams(RequestParam.NewIntParam(ParamGroupId)),
			},
			new RequestInfo
			{
				Title = "고정 그룹 지정/(MessageTypes.GroupSetPrimaryRequest)",
				RequestType = typeof(GroupSetPrimaryRequest),
				MessageTypes = MessageTypes.GroupSetPrimaryRequest,
				RequestParams = RequestParam.MakeParams(RequestParam.NewIntParam(ParamGroupId)),
			},
			new RequestInfo
			{
				Title = "그룹 삭제/(MessageTypes.GroupDeleteRequest)",
				RequestType = typeof(GroupDeleteRequest),
				MessageTypes = MessageTypes.GroupDeleteRequest,
				RequestParams = RequestParam.MakeParams(RequestParam.NewIntParam(ParamGroupId)),
			},
			new RequestInfo
			{
				Title = "그룹 초대 수락/(MessageTypes.GroupParticipateRequest)",
				RequestType = typeof(GroupParticipateRequest),
				MessageTypes = MessageTypes.GroupParticipateRequest,
				RequestParams = RequestParam.MakeParams(
					RequestParam.NewIntParam(ParamGroupId),
					RequestParam.NewBooleanParam(ParamReply)
				),
			},
		};
#endregion // Data

#region Initialize
		private void InitTeamWork()
		{
			SendRequest = new CommandHandler(OnSendRequest);
			InitButtons();
			InitResponse();
		}

		private void InitButtons()
		{
			Buttons.Reset();
			foreach (var info in _requestInfos)
				AddTeamWorkMenuButton(info.Title, () => SetTeamworkInfoView(info));
		}

		private void InitResponse()
		{
			CopyToClipboard = new CommandHandler(OnCopyToClipboard);
			ClearResponse = new CommandHandler(OnClearResponse);
		}
#endregion // Initialize

#region Binding Events
		private void OnSendRequest()
		{
			C2VDebug.LogWarning($"OnSendRequest - {_injector.MessageTypes.ToString()}\n{_injector.Value}");
			NetworkManager.Instance.Send(_injector.Value, _injector.MessageTypes);
		}

		private void OnCopyToClipboard()
		{
			GUIUtility.systemCopyBuffer = ResponseMessage;
			C2VDebug.Log($"복사되었습니다\n{ResponseMessage}");
		}

		private void OnClearResponse()
		{
			ResponseMessage = string.Empty;
		}
#endregion // Binding Events

		private void AddTeamWorkMenuButton(string label, Action onClick) => Buttons.AddItem(new OrganizationDataTeamWorkListViewModel(onClick) {Label = GetButtonLabel(label)});

		private string GetButtonLabel(string label) => label.Contains("/") ? label.Split("/")[0] : label;

#region Set View
		private void SetTeamworkInfoView(RequestInfo info)
		{
			RequestName = info.Title;

			_injector = new DataInjector<IMessage>(info.RequestType, info.MessageTypes);
			RefreshPreview();

			RequestFieldItems.Reset();
			foreach (var param in info.RequestParams)
			{
				switch (param.Type)
				{
					case eRequestType.INT:
						AddRequestDataParam(param.ParamName, value => SetInt(param.ParamName, value));
						break;
					case eRequestType.LONG:
						AddRequestDataParam(param.ParamName, value => SetLong(param.ParamName, value));
						break;
					case eRequestType.BOOLEAN:
						AddRequestDataParam(param.ParamName, value => SetBoolean(param.ParamName, value));
						break;
					case eRequestType.STRING:
						AddRequestDataParam(param.ParamName, value => SetString(param.ParamName, value));
						break;
					case eRequestType.ARRAY_INT:
						AddRequestArrayParam(param.ParamName, (action, list, index) => OnIntArrayValueChanged(param.ParamName, action, list, index));
						break;
					case eRequestType.ARRAY_LONG:
						AddRequestArrayParam(param.ParamName, (action, list, index) => OnLongArrayValueChanged(param.ParamName, action, list, index));
						break;
					case eRequestType.ARRAY_STRING:
						AddRequestArrayParam(param.ParamName, (action, list, index) => OnStringArrayValueChanged(param.ParamName, action, list, index));
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			void SetInt(string paramName, string value)
			{
				_injector.SetInt(paramName, value);
				RefreshPreview();
			}

			void SetLong(string paramName, string value)
			{
				_injector.SetLong(paramName, value);
				RefreshPreview();
			}

			void SetBoolean(string paramName, string value)
			{
				_injector.SetBoolean(paramName, value);
				RefreshPreview();
			}
			void SetString(string paramName, string value)
			{
				_injector.SetString(paramName, value);
				RefreshPreview();
			}

			void OnIntArrayValueChanged(string paramName, eNotifyCollectionChangedAction action, IList list, int index)
			{
				var result = list.OfType<OrganizationDataTeamWorkArrayElementListViewModel>().Select(viewModel =>
				{
					if (int.TryParse(viewModel.Value, out var n))
						return n;
					return default;
				}).ToList();
				_injector.SetArrayInt(paramName, result);
				RefreshPreview();
			}

			void OnLongArrayValueChanged(string paramName, eNotifyCollectionChangedAction action, IList list, int index)
			{
				var result = list.OfType<OrganizationDataTeamWorkArrayElementListViewModel>().Select(viewModel =>
				{
					if (long.TryParse(viewModel.Value, out var l))
						return l;
					return default;
				}).ToList();
				_injector.SetArrayLong(paramName, result);
				RefreshPreview();
			}

			void OnStringArrayValueChanged(string paramName, eNotifyCollectionChangedAction action, IList list, int index)
			{
				var result = list.OfType<OrganizationDataTeamWorkArrayElementListViewModel>().Select(viewModel => viewModel.Value).ToList();
				_injector.SetArrayString(paramName, result);
				RefreshPreview();
			}

			void RefreshPreview()
			{
				RequestPreview = _injector.Value.ToString();
			}
		}

		private void AddRequestDataParam(string name, Action<string> onValueChanged) => RequestFieldItems.AddItem(new OrganizationDataTeamWorkRequestFieldListViewModel
		{
			Name = name,
			IsArrayParam = false,
			OnValueChanged = onValueChanged,
		});

		private void AddRequestArrayParam(string name, Action<eNotifyCollectionChangedAction, IList, int> onListChanged) => RequestFieldItems.AddItem(new OrganizationDataTeamWorkRequestFieldListViewModel
		{
			Name = name,
			IsArrayParam = true,
			OnListChanged = onListChanged,
		});
#endregion // Set View

#region Data Injector
		private class DataInjector<T> where T : class
		{
			private Type _type;
			private T _value;
			private MessageTypes _messageTypeses;
			public T Value => _value;
			public MessageTypes MessageTypes => _messageTypeses;
			public DataInjector(Type type, MessageTypes messageTypes)
			{
				_type = type;
				_value = Activator.CreateInstance(type) as T;
				_messageTypeses = messageTypes;
			}

			public void SetInt(string paramName, string value)
			{
				if (int.TryParse(value, out var n))
					SetValue(paramName, n);
			}

			public void SetLong(string paramName, string value)
			{
				if (long.TryParse(value, out var l))
					SetValue(paramName, l);
			}

			public void SetBoolean(string paramName, string value)
			{
				if (bool.TryParse(value, out var b))
					SetValue(paramName, b);
			}
			public void SetString(string paramName, string value) => SetValue(paramName, value);

			public void SetArrayInt(string paramName, IList<int> values) => SetArrayInt(paramName, values.ToArray());
			public void SetArrayInt(string paramName, string[] values)
			{
				var result = values.Select(value => int.TryParse(value, out var n) ? n : default).ToArray();
				SetArrayInt(paramName, result);
				// SetValue(paramName, result);
			}
			public void SetArrayInt(string paramName, int[] values) => SetArray(paramName, values);

			public void SetArrayLong(string paramName, IList<long> values) => SetArrayLong(paramName, values.ToArray());
			public void SetArrayLong(string paramName, string[] values)
			{
				var result = values.Select(value => long.TryParse(value, out var l) ? l : default).ToArray();
				SetArrayLong(paramName, result);
			}
			public void SetArrayLong(string paramName, long[] values) => SetArray(paramName, values);

			public void SetArrayString(string paramName, IList<string> values) => SetArrayString(paramName, values.ToArray());
			public void SetArrayString(string paramName, string[] values) => SetArray(paramName, values);
			private void SetValue(string paramName, object value) => GetProperty(paramName).SetValue(_value, value);
			private void SetArray<TValue>(string paramName, TValue[] array)
			{
				var repeatedField = GetProperty(paramName).GetValue(_value) as RepeatedField<TValue>;
				repeatedField.Clear();
				repeatedField.AddRange(array);
			}
			private PropertyInfo GetProperty(string name) => _type.GetProperty(name);
		}
#endregion // Data Injector

#region Debug
		private void OnResponse(IMessage response)
		{
			ResponseMessage = JsonConvert.SerializeObject(response, Formatting.Indented);
		}
#endregion // Debug
	}
}
