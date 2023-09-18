using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("Board")]
	public class BoardViewModel : ViewModelBase
	{
		private string _inputText;
		private readonly string _lengthLimitStringKey = "UI_MessageBoard_Message_InputLimit";
		private bool _showMessageBoard;

		public DailyResetRedDotSource BoardWriteRedDotSource { get; private set; }
		public DailyResetRedDotSource BoardReadRedDotSource { get; private set; }
		
		public string InputText
		{
			get => _inputText;
			set
			{
				if (value.Length <= BoardManager.LengthLimit)
				{
					_inputText = value;
					InvokePropertyValueChanged(nameof(InputTextTotalByteString), InputTextTotalByteString);
				}

				InvokePropertyValueChanged(nameof(InputText), _inputText);
				InvokePropertyValueChanged(nameof(CanSubmit), CanSubmit);
			}
		}

		private AnimationPropertyExtensions _animationPropertyExtensions;

		public AnimationPropertyExtensions AnimationPropertyExtensions
		{
			get => _animationPropertyExtensions;
			set => SetProperty(ref _animationPropertyExtensions, value);
		}

		public string InputTextTotalByteString
		{
			get => string.Format(Localization.Instance.GetString(_lengthLimitStringKey), _inputText.Length, BoardManager.LengthLimit);
		}

		public string TodayTopic
		{
			get => BoardManager.Instance.TodayTopic;
		}

		public bool ShowMessageBoard
		{
			get => _showMessageBoard;
			set
			{
				_showMessageBoard = value;
				InvokePropertyValueChanged(nameof(ShowMessageBoard), _showMessageBoard);
			}
		}

		public bool CanSubmit
		{
			get => !string.IsNullOrWhiteSpace(_inputText);
		}

		public bool ShowBoardWriteRedDot
		{
			get => BoardWriteRedDotSource.Enabled();
		}

		public bool ShowBoardReadRedDot
		{
			get => !BoardWriteRedDotSource.Enabled() && BoardReadRedDotSource.Enabled();
		}

		public bool ShowNormalBoard
		{
			get => !BoardWriteRedDotSource.Enabled() && !BoardReadRedDotSource.Enabled();
		}
		
		public CommandHandler Command_WriteToBoard { get; }
		public string ObjectId { get; set; }

		public BoardViewModel()
		{
			Command_WriteToBoard = new CommandHandler(OnClick);
			BoardReadRedDotSource = new(string.Concat(nameof(BoardViewModel), "Read", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
			BoardWriteRedDotSource = new(string.Concat(nameof(BoardViewModel), "Write", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

			UpdateRedDot();
		}
		
		private void OnClick()
		{
			Commander.Instance.RequestBoardWrite(_inputText, ObjectId).Forget();
			BoardWriteRedDotSource.Deactivate();
			UpdateRedDot();
		}

		public override void OnInitialize()
		{
			base.OnInitialize();

			InputText = string.Empty;
		}

		public void OnRead()
		{
			BoardReadRedDotSource.Deactivate();
			UpdateRedDot();
		}

		private void UpdateRedDot()
		{
			InvokePropertyValueChanged(nameof(ShowBoardWriteRedDot), ShowBoardWriteRedDot);
			InvokePropertyValueChanged(nameof(ShowBoardReadRedDot), ShowBoardReadRedDot);
			InvokePropertyValueChanged(nameof(ShowNormalBoard), ShowNormalBoard);
		}

		public override void OnLanguageChanged()
		{
			InvokePropertyValueChanged(nameof(InputTextTotalByteString), InputTextTotalByteString);
			InvokePropertyValueChanged(nameof(TodayTopic), TodayTopic);
		}
	}
}
