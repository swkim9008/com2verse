/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableCellViewModelBase.cs
* Developer:	eugene9721
* Date:			2023-01-11 17:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	/// <summary>
	/// RecyclableCellViewModel을 베이스로 프로젝트에서 사용할 CellViewModel을 만들기 위한 클래스입니다.
	/// </summary>
	public abstract class RecyclableCellViewModelBase : RecyclableCellViewModel, ILocalizationUI
	{
		public virtual void OnLanguageChanged() { }

		public override void OnInitialize()
		{
			base.OnInitialize();

			(this as ILocalizationUI).InitializeLocalization();
		}


		public override void OnRelease()
		{
			base.OnRelease();

			(this as ILocalizationUI).ReleaseLocalization();
		}
	}
}
