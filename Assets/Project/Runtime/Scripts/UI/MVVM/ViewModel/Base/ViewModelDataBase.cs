/*===============================================================
* Product:		Com2Verse
* File Name:	ViewModelData.cs
* Developer:	tlghks1009
* Date:			2022-09-26 10:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
    public abstract class DataModel
    {
        public ViewModelBase ViewModel { get; set; }

        public T GetViewModel<T>() where T : ViewModelBase => ViewModel as T;
    }

    public abstract class ViewModelDataBase<TDataModel> : ViewModelBase where TDataModel : DataModel, new()
    {
        private TDataModel _dataModel;
        private ViewModelBase _viewModel;

        protected TDataModel Model
        {
            get
            {
                if (_dataModel == null)
                {
                    _dataModel = new TDataModel
                    {
                        ViewModel = this,
                    };
                }
                return _dataModel;
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();

            _dataModel = null;
        }
    }
}
