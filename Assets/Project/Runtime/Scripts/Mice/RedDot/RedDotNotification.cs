/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotNotification.cs
* Developer:	seaman2000
* Date:			2023-08-01 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System;

namespace Com2Verse
{
    public interface IRedDotNotification
    {
        Mice.RedDotManager.RedDotData RedDotData { get; }

        public void UpdateRedDot(bool value);
    }

    public sealed class RedDotNotification : MonoBehaviour, IRedDotNotification
    {
        [SerializeField] private string redDotKey;

        [SerializeField] private GameObject[] goActives;

        public Mice.RedDotManager.RedDotData RedDotData { get; private set; }

        private void Awake()
        {
            // convert enum key by string
            if (!Enum.TryParse(redDotKey, out Mice.RedDotManager.RedDotData.Key toEnum))
                return;

            Mice.RedDotManager.Instance.Register(this);
            RedDotData = Mice.RedDotManager.Instance.GetDataRow(toEnum);
        }

        private void OnDestroy()
        {
            Mice.RedDotManager.Instance.Unregister(this);
        }

        public void UpdateRedDot(bool value)
        {
            if (goActives == null) return;

            foreach (var entry in goActives)
            {
                entry.SetActive(value);
            }
        }

        private void Start()
        {
            Mice.RedDotManager.Instance.StartWithMarking(this, true);
        }
    }
}