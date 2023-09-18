/*===============================================================
* Product:		Com2Verse
* File Name:	INestedViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-31 15:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;

namespace Com2Verse.UI
{
	public interface INestedViewModel
	{
		IList<ViewModel> NestedViewModels { get; }
	}
}
