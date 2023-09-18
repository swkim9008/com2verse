/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkWorldController.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Deeplink
{
	[UsedImplicitly]
	[Service(eServiceType.WORLD)]
	public sealed class DeeplinkWorldController : DeeplinkBaseController
	{
		public override void Initialize()
		{
			Name = nameof(DeeplinkWorldController);
		}

		protected override void TryCheckParam(EngagementInfo info)
		{
			FinishInvokeAction?.Invoke();
		}
	}
}
