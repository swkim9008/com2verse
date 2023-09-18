// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderSelectionCardViewModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-13 오후 3:08
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.IO;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse
{
	public class BuilderSelectionCardViewModel : ViewModelBase
	{
		public string Path { get; set; }
		public string SpaceName { get; set; }
		public CommandHandler CommandLoadSpace { get; }
		public CommandHandler CommandDeleteSpace { get; }

		public SpaceSelectionViewModel ParentViewModel { get; set; }

		public BuilderSelectionCardViewModel()
		{
			CommandLoadSpace = new CommandHandler(OnClickLoad);
			CommandDeleteSpace = new CommandHandler(OnClickDelete);
		}

		private void OnClickDelete()
		{
			UIManager.Instance.ShowPopupYesNo("걍고", "공간을 삭제하시겠습니까? 다시는 되돌릴 수 없습니다.", view =>
			{
				System.IO.File.Delete(Path);
				ParentViewModel.BuilderSpaceCollection.RemoveItem(this);
			});
		}

		private async void OnClickLoad()
		{
			using (var reader = File.OpenText(Path))
			{
				string mapData = reader.ReadToEnd();
				await Builder.BuilderSerializer.DeserializeSpace(SpaceName, mapData);

				GameObject.Find("UI_BuilderSelection").GetComponent<UIView>().Hide();
				BuilderBaseViewModel.Show();
			}
		}
	}
}
