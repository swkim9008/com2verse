/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtensions_ViewModelContainer.cs
* Developer:	sprite
* Date:			2023-04-25 12:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using System;

namespace Com2Verse.Mice
{
    public static partial class ViewModelContainerExntensions
    {
        public static ViewModelContainer UniqueAddViewModel<TViewModel>(this ViewModelContainer container, Func<TViewModel> viewModelGetter)
            where TViewModel : ViewModel
        {
            if (viewModelGetter != null && container != null && !container.TryGetViewModel(typeof(TViewModel), out var _))
            {
                container.AddViewModel(viewModelGetter());
            }

            return container;
        }
    }
}
