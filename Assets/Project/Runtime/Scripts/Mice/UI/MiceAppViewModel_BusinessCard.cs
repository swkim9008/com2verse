/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAppViewModel_BusinessCard.cs
* Developer:	klizzard
* Date:			2023-07-17 15:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
    public partial class MiceAppViewModel //BusinessCard
    {
        #region Variables
        private MiceBusinessCardBookViewModel _miceBusinessCardBookViewModel;
        #endregion
        
        #region Properties

        public bool IsBusinessCardViewOn => ViewMode is eViewMode.BUSINESS_MYCARD or eViewMode.BUSINESS_CARDLIST;

        public MiceBusinessCardBookViewModel MiceBusinessCardBookViewModel
        {
            get => _miceBusinessCardBookViewModel;
            set => SetProperty(ref _miceBusinessCardBookViewModel, value);
        }

        #endregion

        partial void InitBusinessCardView()
        {
            MiceBusinessCardBookViewModel = new MiceBusinessCardBookViewModel();
            NestedViewModels.Add(MiceBusinessCardBookViewModel);
        }
        
        partial void InvokeBusinessCardView()
        {
            InvokePropertyValueChanged(nameof(IsBusinessCardViewOn), IsBusinessCardViewOn);
            if (IsBusinessCardViewOn) MiceBusinessCardBookViewModel.OnInitialize();
        }
    }
}