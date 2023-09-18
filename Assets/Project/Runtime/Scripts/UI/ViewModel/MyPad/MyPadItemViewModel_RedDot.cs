/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadItemViewModel_RedDot.cs
* Developer:	klizzard
* Date:			2023-08-09 13:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
    public sealed partial class MyPadItemViewModel : IRedDotNotification
    {
        public Mice.RedDotManager.RedDotData RedDotData { get; private set; }

        partial void RegisterRedDot()
        {
            RedDotData = Id switch
            {
                "com.com2verse.convention" =>
                    Mice.RedDotManager.Instance.GetDataRow(Mice.RedDotManager.RedDotData.Key.MiceApp),
                _ => null
            };

            if (RedDotData != null)
            {
                Mice.RedDotManager.Instance.Register(this);
                Mice.RedDotManager.Instance.StartWithMarking(this, true);
            }
            else
            {
                UpdateRedDot(false);
            }
        }

        partial void UnregisterRedDot()
        {
            if (RedDotData != null)
            {
                Mice.RedDotManager.InstanceOrNull?.Unregister(this);
            }
        }
        
        public void UpdateRedDot(bool value)
        {
            NotifyCount = value ? Mice.RedDotManager.Instance.CheckWithCounting(RedDotData).ToString() : "0";
        }
    }
}