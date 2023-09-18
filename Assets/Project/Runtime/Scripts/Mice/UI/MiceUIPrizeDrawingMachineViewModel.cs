/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIPrizeDrawingMachineViewModel.cs
* Developer:	sprite
* Date:			2023-07-11 17:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Com2Verse.UI;
using System;

namespace Com2Verse.Mice
{
	public abstract class MiceUIPrizeDrawingMachineViewModel : MiceViewModel
	{
        public enum ResultButtonType
        {
            XButton,
            CloseButton,
        }

        public ResultButtonType PopupResultButtonType { get; private set; } = ResultButtonType.XButton;

        public CommandHandler CloseButton { get; private set; }

        public MiceUIPrizeDrawingMachineViewModel()
        {
            this.InvokePropertyValueChanged(nameof(PopupResultButtonType), PopupResultButtonType);

            this.CloseButton = new(() => this.PopupResultButtonType = ResultButtonType.CloseButton);
        }

    }
}
