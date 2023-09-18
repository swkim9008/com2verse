/*===============================================================
* Product:		Com2Verse
* File Name:	ViewModelBase.cs
* Developer:	tlghks1009
* Date:			2023-01-05 15:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
    public abstract class ViewModelBase : ViewModel, ILocalizationUI
    {
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


        public virtual void OnLanguageChanged() { }
    }
}
