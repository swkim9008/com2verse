/*===============================================================
* Product:    Com2Verse
* File Name:  ButtonClickSound.cs
* Developer:  yangsehoon
* Date:       2022-04-08 13:10
* History:    
* Documents:  Sound import postprocessor
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace Com2Verse.Sound
{
    [RequireComponent(typeof(IPointerClickHandler))]
    public class ButtonClickSound : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AssetReference _audioFile;
        private UnityEngine.Events.UnityAction _clickAction;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            SoundManager.Instance.PlayUISound(_audioFile);
        }
    }
}
