// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TextBoard.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-16 오후 3:46
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Globalization;
using Com2Verse.UIExtension;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	public class TextBoard : MonoBehaviour
	{
		public const int WriteTimeLimitInSeconds = 10 * 60;
		
		public long TargetBoardSeq { get; set; } = -1;
		
		private string _context;
		private string _author;
		private int _alignment;
		private int _textColor;
		private int _backgroundImage;
		private DateTime _lastUpdateTime;
		private Action _updateLengthAction;

		[SerializeField] private TextMeshPro _text;
		[SerializeField] private TextMeshPro _authorText;

		private void OnEnable()
		{
			_text.text = string.Empty;
			_authorText.text = string.Empty;
		}

		private void Update()
		{
			// Composition string length 변경을 위한 주기적 체크
			_updateLengthAction?.Invoke();
		}

		public void SetText(string text, string author, int alignment, int textColor, int backgroundImage, DateTime lastUpdateTime)
		{
			_context = text;
			_author = author;
			_alignment = alignment;
			_textColor = textColor;
			_backgroundImage = backgroundImage;
			_lastUpdateTime = lastUpdateTime;

			_text.text = text;
			_authorText.text = ZString.Format(Localization.Instance.GetString("UI_TextBoard_Nickname"), author);
			_text.horizontalAlignment = alignment switch
			{
				1 => HorizontalAlignmentOptions.Left,
				2 => HorizontalAlignmentOptions.Center,
				3 => HorizontalAlignmentOptions.Right,
				_ => HorizontalAlignmentOptions.Left
			};
			_text.color = GammaCorrection(textColor switch
			{
				1 => TextBoardEditViewModel.ColorBlack,
				2 => TextBoardEditViewModel.ColorRed,
				3 => TextBoardEditViewModel.ColorGreen,
				4 => TextBoardEditViewModel.ColorBlue,
				_ => TextBoardEditViewModel.ColorBlack
			});
		}

		private Color GammaCorrection(Color color)
		{
			return color;
			// return new Color(Mathf.Pow(color.r, 2.2f), Mathf.Pow(color.g, 2.2f), Mathf.Pow(color.b, 2.2f));
		}
		
		public void UpdateTimeString(string[] parameter)
		{
			if (parameter.Length != 2) return;

			var elapsedSeconds = DateTime.Now.Subtract(_lastUpdateTime).TotalSeconds;

			int totalSeconds = Math.Max(0, WriteTimeLimitInSeconds - (int)elapsedSeconds);
			int totalMinutes = totalSeconds / 60;

			parameter[0] = $"{totalMinutes}";
			parameter[1] = $"{totalSeconds - totalMinutes * 60}";
		}

		public void OpenTextBoard()
		{
			var elapsedSeconds = DateTime.Now.Subtract(_lastUpdateTime);
			if (elapsedSeconds.TotalSeconds < WriteTimeLimitInSeconds)
			{
				string baseContext =
					ZString.Format("{0}\n{1}",
					               Localization.Instance.GetString("UI_TextBoard_Notice_Popup_InputTips1"),
					               Localization.Instance.GetString("UI_TextBoard_Notice_Popup_InputTips2"));
				UIManager.Instance.ShowPopupCommon(baseContext, null, null, false, true, view =>
					{
						var descSynchronizer = view.gameObject.AddComponent<CommonPopupDescSynchronizer>();
						descSynchronizer.SetDesc(baseContext, 2, UpdateTimeString);
						view.OnClosedEvent += guiView => { Destroy(descSynchronizer); };
					}
				);
				return;
			}

			UIManager.Instance.CreatePopup(TextBoardEditViewModel.PrefabName, (guiView) =>
			{
				guiView.Show();
				guiView.OnClosedEvent += ClearLengthUpdate;
				var viewModel = guiView.ViewModelContainer.GetViewModel<TextBoardEditViewModel>();
				viewModel.CurrentView = guiView;

				viewModel.InputText = _context;
				viewModel.BoardSequence = TargetBoardSeq;
				viewModel.Alignment = _alignment;
				viewModel.TextColor = _textColor;
				viewModel.BackgroundImageIndex = _backgroundImage;
				_updateLengthAction = viewModel.UpdateLength;
			}).Forget();
		}

		private void ClearLengthUpdate(GUIView view)
		{
			_updateLengthAction = null;
		}
	}
}
