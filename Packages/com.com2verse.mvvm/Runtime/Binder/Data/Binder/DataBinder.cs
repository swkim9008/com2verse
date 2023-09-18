/*===============================================================
* Product:    Com2Verse
* File Name:  DataBinder.cs
* Developer:  tlghks1009
* Date:       2022-03-04 11:00
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] Data Binder")]
    public class DataBinder : Binder
    {
        public override void Bind()
        {
            base.Bind();

            switch (_bindingMode)
            {
                case eBindingMode.TWO_WAY:
                {
                    OneWayToSource();
                    OneWayToTarget();
                }
                    break;
                case eBindingMode.ONE_WAY_TO_SOURCE:
                    OneWayToSource();
                    break;
                case eBindingMode.ONE_TIME:
                    OneTimeToTarget();
                    break;
                case eBindingMode.ONE_WAY_TO_TARGET:
                    OneWayToTarget();
                    break;
            }
        }

        protected override void OneTimeToTarget()
        {
            base.OneTimeToTarget();

            UpdateTargetProp();
        }


        protected override void OneWayToTarget()
        {
            base.OneWayToTarget();

            Subscribe(eBindingMode.ONE_WAY_TO_TARGET);

            UpdateTargetProp();
        }

        protected override void OneWayToSource()
        {
            base.OneWayToSource();

            Subscribe(eBindingMode.ONE_WAY_TO_SOURCE);

            if (string.IsNullOrEmpty(EventPropertyName))
            {
                UpdateSourceProp();
            }
            else
            {
                if (_syncOnAwake)
                {
                    UpdateSourceProp();
                }
            }
        }
    }
}
