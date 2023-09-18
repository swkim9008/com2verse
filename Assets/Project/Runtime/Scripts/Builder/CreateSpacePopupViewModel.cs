/*===============================================================
* Product:		Com2Verse
* File Name:	CreateSpacePopupViewModel.cs
* Developer:	yangsehoon
* Date:			2023-03-06 13:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Builder;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
	public sealed class CreateSpacePopupViewModel : ViewModelBase
	{
		public CommandHandler CommandCreate { get; }
		public CommandHandler<int> CommandSelectSize { get; }

		private SpaceManager.eSpaceSize _selectedSize;

		public CreateSpacePopupViewModel()
		{
			CommandCreate = new CommandHandler(OnClickCreate);
			CommandSelectSize = new CommandHandler<int>(OnSelectSize);
		}

		private void OnClickCreate()
		{
			SpaceManager.Instance.CreateSpace(_selectedSize);

			BuilderBaseViewModel.Show();
		}

		private void OnSelectSize(int index)
		{
			_selectedSize = (SpaceManager.eSpaceSize)index;
		}
	}
}
