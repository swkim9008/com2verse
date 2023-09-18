/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIViewSessionList.cs
* Developer:	wlemon
* Date:			2023-04-10 11:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.UIExtension;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
    public sealed class MiceUIViewSessionList : MonoBehaviour
	{
        public MiceUIViewSessionListSpeaker PfMiceUIViewSessionListSpeaker;

        private List<MiceUIViewSessionListSpeaker> _items = new List<MiceUIViewSessionListSpeaker>();




        public void UpdateSpeaker(MiceSessionInfo sessionInfo)
		{
            int speakerCount = sessionInfo.Speakers.Count;
            while (_items.Count > speakerCount) 
            {
                GameObject.Destroy(_items[0].gameObject);
                _items.RemoveAt(0); 
            }

            for (int loop = 0, max = speakerCount; loop < max; ++loop)
            {
                MiceUIViewSessionListSpeaker speaker = null;

                if (_items.Count <= loop)
                {
                    var go = GameObject.Instantiate(this.PfMiceUIViewSessionListSpeaker.gameObject, this.transform);
                    speaker = go.GetComponent<MiceUIViewSessionListSpeaker>();
                    _items.Add(speaker);
                }
                _items[loop].SetData(sessionInfo.Speakers[loop]);
            }
        }
    }
}

