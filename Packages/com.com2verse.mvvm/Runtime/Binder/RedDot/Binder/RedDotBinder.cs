/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotBinder.cs
* Developer:	NGSG
* Date:			2023-04-18 11:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.UI
{
	public sealed class RedDotBinder : Binder
	{
		public string _badgeType = null;

		public override void Bind()
		{
			base.Bind();
		
			Execute();
		}

		public override void Unbind()
		{
			// 레드닷에 바인딩되어 있는 컬렉션 아이템을 삭제한다
			RedDotViewModel vm = SourceOwnerOfOneWayTarget as RedDotViewModel;
			if (vm != null)
			{
				CollectionItem ci = RedDotManager.Instance.FindCollectionItem(vm);
				if(ci != null)
					RedDotManager.Instance.RemoveCollectionItem(ci);
			} 
			
			base.Unbind();
		}
		
		public override void Execute() => OneWayToTarget();

		protected override void OneWayToTarget()
		{
			base.OneWayToTarget();

			Subscribe(eBindingMode.ONE_WAY_TO_TARGET);

			UpdateTargetProp();
		}
	}
}
