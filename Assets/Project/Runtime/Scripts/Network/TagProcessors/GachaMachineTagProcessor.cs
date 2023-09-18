/*===============================================================
* Product:		Com2Verse
* File Name:	GachaMachineTagProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-13 16:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Network
{
	[TagObjectType(eObjectType.GACHA_MACHINE)]
	public sealed class GachaMachineTagProcessor : BaseTagProcessor
	{
		public override void Initialize()
		{
            this.SetDelegates
			(
				typeof(GachaMachineTag).Name, (value, mapObject) =>
				{
					var prizeDrawingMachine = mapObject.GetComponent<GachaMachineObject>();
					if (prizeDrawingMachine.IsReferenceNull()) return;

					prizeDrawingMachine.InitTagValueFromJson(value);
				}
			);
		}
	}
}
