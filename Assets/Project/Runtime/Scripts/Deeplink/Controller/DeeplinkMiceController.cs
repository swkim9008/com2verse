/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkMiceController.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Deeplink
{
	[UsedImplicitly]
	[Service(eServiceType.MICE)]
	public sealed class DeeplinkMiceController : DeeplinkBaseController
	{
		public override void Initialize()
		{
			Name = nameof(DeeplinkMiceController);
		}

		protected override void TryCheckParam(EngagementInfo info)
		{
			FinishInvokeAction?.Invoke();
		}
	}
}
