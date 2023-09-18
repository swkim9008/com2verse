// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SpaceSelectionViewModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오전 10:59
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse
{
	public sealed class SpaceSelectionViewModel : ViewModelBase
	{
		public Collection<BuilderSelectionCardViewModel> BuilderSpaceCollection { get; set; } = new Collection<BuilderSelectionCardViewModel>();
		public CommandHandler CommandCreate { get; }
		public GameObject SelectionView { get; set; }
		public string SpaceCountText => $"공간 수 : {_spaceCount} / 20"; // (TODO) Temp string
		private int _spaceCount;

		public SpaceSelectionViewModel()
		{
			CommandCreate = new CommandHandler(OnClickCreate);

			string builderDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C2VBuilder");
			if (!System.IO.Directory.Exists(builderDataPath))
			{
				System.IO.Directory.CreateDirectory(builderDataPath);
			}

			var files = System.IO.Directory.EnumerateFiles(builderDataPath);
			foreach (var file in files)
			{
				if (System.IO.Path.GetExtension(file).Equals(BuilderManager.TempDataExtension))
				{
					_spaceCount++;
					BuilderSpaceCollection.AddItem(new BuilderSelectionCardViewModel()
					{
						Path = file,
						SpaceName = System.IO.Path.GetFileNameWithoutExtension(file),
						ParentViewModel = this
					});
				}
			}
			
			InvokePropertyValueChanged(nameof(SpaceCountText), SpaceCountText);
		}

		private void OnClickCreate()
		{
			SelectionView.GetComponent<UIView>().Hide();
			UIManager.Instance.CreatePopup("UI_Popup_CreateSpace", view => { view.Show(); }).Forget();
		}
	}
}
