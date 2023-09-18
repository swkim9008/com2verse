/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUISessionQuestionListItemViewModel.cs
* Developer:	sprite
* Date:			2023-05-17 16:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Net;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUISessionQuestionListItemViewModel : MiceViewModel
    {
        private MiceSessionQuestionInfo.Item _data;

        public MiceUISessionQuestionListItemViewModel(MiceSessionQuestionInfo.Item data)
        {
            _data = data;
        }
    }
}
