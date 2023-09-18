// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TextBoardEditViewModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-26 오후 2:33
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Text.RegularExpressions;
using Com2Verse.Extension;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	public class TextBoardEditViewModel : ViewModelBase
	{
		public const string PrefabName = "UI_Popup_TextBoard";
		private const string LengthLimitStringKey = "UI_TextBoard_Message_InputLimit";
		private const int LengthLimitMax = 51;
		private const int LengthLimitMin = 10;
		
		private string _inputText;
		private int _alignment;
		private int _textColor;
		private int _backgroundImageIndex;

		public static readonly Color ColorRed = new Color(0.9372549f, 0.254902f, 0.3647059f);
		public static readonly Color ColorGreen = new Color(0.2352941f, 0.7019608f, 0.4431373f);
		public static readonly Color ColorBlue = new Color(0.145098f, 0.6862745f, 1.0f);
		public static readonly Color ColorBlack = new Color(0.1019608f, 0.09803922f, 0.1058824f);
		
		public CommandHandler OnClickPost { get; set; }
		public CommandHandler OnClickRefresh { get; set; }
		public CommandHandler<int> OnClickAlignment { get; set; }
		public CommandHandler<int> OnClickTextColor { get; set; }
		public GUIView CurrentView { private get; set; }
		public long BoardSequence { private get; set; }
		public bool CanPost
		{
			get
			{
				return LengthLimitMin <= GetLength(_inputText, true) && GetLength(_inputText, false) <= LengthLimitMax;
			}
		}
		
		public string InputText
		{
			get => _inputText;
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_inputText = string.Empty;
					SetProperty(ref _inputText, _inputText);
					InvokePropertyValueChanged(nameof(InputTextTotalByteString), InputTextTotalByteString);
					return;
				}
				
				if (GetLength(value, false) <= LengthLimitMax)
				{
					_inputText = value;
					InvokePropertyValueChanged(nameof(InputTextTotalByteString), InputTextTotalByteString);
					SetProperty(ref _inputText, value);
				}
				else
				{
					SetProperty(ref _inputText, _inputText);
				}

				InvokePropertyValueChanged(nameof(CanPost), CanPost);
			}
		}
		public TMP_InputField InputField { get; set; }

		public void UpdateLength()
		{
			InvokePropertyValueChanged(nameof(InputTextTotalByteString), InputTextTotalByteString);
			InvokePropertyValueChanged(nameof(CanPost), CanPost);
		}
		
		private int GetLength(string value, bool removeWhitespace)
		{
			var info = new System.Globalization.StringInfo(removeWhitespace ? Regex.Replace(value, @"\s+", "") : value);
			int compositionLength = 0;
			if (!InputField.IsUnityNull())
				compositionLength = InputField.compositionLength;
			
			return info.LengthInTextElements + compositionLength;
		}
		
		public string InputTextTotalByteString
		{
			get => string.Format(Localization.Instance.GetString(LengthLimitStringKey), GetLength(_inputText, false), LengthLimitMax);
		}
		
		public int Alignment
		{
			get => _alignment;
			set
			{
				SetProperty(ref _alignment, value);
				InvokePropertyValueChanged(nameof(AlignmentOption), AlignmentOption);
			}
		}

		public Color InputTextColor
		{
			get
			{
				switch (_textColor)
				{
					case 2:
						return ColorRed;
					case 3:
						return ColorGreen;
					case 4:
						return ColorBlue;
					default:
						return ColorBlack;
				}
			}
		}

		public bool BlackColorActivated
		{
			get => _textColor == 1;
		}

		public bool RedColorActivated
		{
			get => _textColor == 2;
		}

		public bool GreenColorActivated
		{
			get => _textColor == 3;
		}

		public bool BlueColorActivated
		{
			get => _textColor == 4;
		}

		public HorizontalAlignmentOptions AlignmentOption
		{
			get
			{
				switch (_alignment)
				{
					case 2:
						return HorizontalAlignmentOptions.Center;
					case 3:
						return HorizontalAlignmentOptions.Right;
					default:
						return HorizontalAlignmentOptions.Left;
				}
			}
		}

		public int TextColor
		{
			get => _textColor;
			set
			{
				SetProperty(ref _textColor, value);
				switch (_textColor)
				{
					case 2:
						InvokePropertyValueChanged(nameof(RedColorActivated), RedColorActivated);
						break;
					case 3:
						InvokePropertyValueChanged(nameof(GreenColorActivated), GreenColorActivated);
						break;
					case 4:
						InvokePropertyValueChanged(nameof(BlueColorActivated), BlueColorActivated);
						break;
					default:
						InvokePropertyValueChanged(nameof(BlackColorActivated), BlackColorActivated);
						break;
				}
				InvokePropertyValueChanged(nameof(InputTextColor), InputTextColor);
			}
		}

		public int BackgroundImageIndex
		{
			get => _backgroundImageIndex;
			set => SetProperty(ref _backgroundImageIndex, value);
		}
		
		public TextBoardEditViewModel()
		{
			OnClickPost = new CommandHandler(OnClickPostProcess);
			OnClickRefresh = new CommandHandler(OnClickRefreshProcess);
			OnClickAlignment = new CommandHandler<int>(OnClickAlignmentProcess);
			OnClickTextColor = new CommandHandler<int>(OnClickTextColorProcess);
		}

		private void OnClickAlignmentProcess(int index)
		{
			Alignment = index;
		}

		private void OnClickTextColorProcess(int index)
		{
			TextColor = index;
		}

		private void OnClickRefreshProcess()
		{
			InputText = string.Empty;
			Alignment = 1;
			TextColor = 1;
			BackgroundImageIndex = 1;
		}
		
		private void OnClickPostProcess()
		{
			Validate().Forget();
		}

		private async UniTask Validate()
		{
			if (await BannedWords.BannedWords.HasBannedWordAsync(_inputText))
			{
				UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Nickname_Toast_Msg_Prohibited"));
			}
			else
			{
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_TextBoard_Post_Popup_Title"), Localization.Instance.GetString("UI_TextBoard_Post_Popup_Warning"), view =>
				{
					if (!CurrentView.IsUnityNull())
						CurrentView!.Hide();

					Commander.Instance.RequestTextBoardUpdate(BoardSequence, _inputText, _alignment, _textColor, _backgroundImageIndex);
				});
			}
		}
	}
}
