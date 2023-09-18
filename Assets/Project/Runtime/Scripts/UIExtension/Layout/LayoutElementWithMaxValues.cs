/*===============================================================
* Product:		Com2Verse
* File Name:	LayoutElementWithMaxValues.cs
* Developer:	tlghks1009
* Date:			2022-10-28 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UIExtension
{
    [AddComponentMenu("[CVUI]/[CVUI] LayoutElementWithMaxValues")]
    [RequireComponent(typeof(RectTransform))]
    [System.Serializable]
    public class LayoutElementWithMaxValues : LayoutElement
    {
        [SerializeField] private float _maxHeight;
        [SerializeField] private float _maxWidth;

        [SerializeField] private bool _useMaxWidth;
        [SerializeField] private bool _useMaxHeight;

        private bool _ignoreOnGettingPreferredSize;

        public override int layoutPriority
        {
            get => _ignoreOnGettingPreferredSize ? -1 : base.layoutPriority;
            set => base.layoutPriority = value;
        }

        public override float preferredHeight
        {
            get
            {
                if (_useMaxHeight)
                {
                    var defaultIgnoreValue = _ignoreOnGettingPreferredSize;
                    _ignoreOnGettingPreferredSize = true;

                    var baseValue = LayoutUtility.GetPreferredHeight(transform as RectTransform);

                    _ignoreOnGettingPreferredSize = defaultIgnoreValue;

                    return baseValue > _maxHeight ? _maxHeight : baseValue;
                }
                else
                    return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }

        public override float preferredWidth
        {
            get
            {
                if (_useMaxWidth)
                {
                    var defaultIgnoreValue = _ignoreOnGettingPreferredSize;
                    _ignoreOnGettingPreferredSize = true;

                    var baseValue = LayoutUtility.GetPreferredWidth(transform as RectTransform);

                    _ignoreOnGettingPreferredSize = defaultIgnoreValue;

                    return baseValue > _maxWidth ? _maxWidth : baseValue;
                }
                else
                    return base.preferredWidth;
            }
            set => base.preferredWidth = value;
        }
    }
}
