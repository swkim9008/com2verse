/*===============================================================
* Product:		Com2Verse
* File Name:	NpcPrevDialogueViewModel.cs
* Developer:	eugene9721
* Date:			2023-09-05 14:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("Npc")]
	public sealed class NpcPrevDialogueViewModel : ViewModelBase
	{
		private bool   _isNpcMessage;
		private string _message = string.Empty;
		private string _name    = string.Empty;

		[UsedImplicitly]
		public bool IsNpcMessage
		{
			get => _isNpcMessage;
			set => SetProperty(ref _isNpcMessage, value);
		}

		[UsedImplicitly]
		public string Message
		{
			get => _message;
			set => SetProperty(ref _message, value);
		}

		[UsedImplicitly]
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}
	}
}
