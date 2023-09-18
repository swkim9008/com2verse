/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_ToastPopup.cs
* Developer:	tlghks1009
* Date:			2022-08-25 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;

namespace Com2Verse.UI
{
    public partial class UIManager
    {
        public enum eToastMessageType
        {
            NORMAL,
            WARNING,
        }

        private Timer _timer = new();

        public void SendToastMessage(string message, float seconds = 3f, eToastMessageType toastMessageType = eToastMessageType.NORMAL)
        {
            HideToastPopup();

            ShowToastPopup(message, seconds, toastMessageType);
        }

        private bool SceneLoadingValidator()
        {
            return SceneManager.InstanceOrNull?.CurrentScene.SceneState is eSceneState.LOADED;
        }

        private void ShowToastPopup(string message, float seconds, eToastMessageType toastMessageType)
        {
            _timer.Set(this, seconds, (timer) =>
            {
                timer.Reset();

                HideToastPopup();
            }, SceneLoadingValidator);

            var toastPopup = GetSystemView(eSystemViewType.TOAST_POPUP);

            if (toastPopup.IsUnityNull()) return;
            
            toastPopup!.Show();


            var toastMessageViewModel = toastPopup.ViewModelContainer!.GetViewModel<ToastMessageViewModel>();

            toastMessageViewModel.Message = message;

            toastMessageViewModel.IsVisibleNormal  = toastMessageType == eToastMessageType.NORMAL;
            toastMessageViewModel.IsVisibleWarning = toastMessageType == eToastMessageType.WARNING;
        }


        public void HideToastPopup()
        {
            _timer?.Reset();

            var toastPopup = GetSystemView(eSystemViewType.TOAST_POPUP);

            if (toastPopup.IsUnityNull()) return;

            toastPopup.Hide();
        }
    }
}
