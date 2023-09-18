/*===============================================================
* Product:		Com2Verse
* File Name:	EasyTagProcessor.cs
* Developer:	sprite
* Date:			2023-07-25 18:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Network
{
    public interface IUpdateTag<TTag>
    {
        void UpdateTag(TTag tag);
    }

    public abstract class EasyTagProcessor : BaseTagProcessor
    {
        protected void SetDelegates<TTag>()
            => this.SetDelegates(typeof(TTag).Name, (json, mapObject) => this.ParseAndDispatch<TTag>(json, mapObject));

        protected void ParseAndDispatch<TTag>(string json, BaseMapObject mapObject)
        {
            TTag tag;
            bool result;
            try
            {
                tag = JsonUtility.FromJson<TTag>(json);
                result = true;
            }
            catch
            {
                tag = default;
                result = false;
            }

            if (!result) return;

            var components = mapObject.GetComponents<IUpdateTag<TTag>>();
            for (int i = 0, cnt = components?.Length ?? 0; i < cnt; i++)
            {
                components[i]?.UpdateTag(tag);
            }
        }
    }

    public class EasyTagProcessor<TTag0> : EasyTagProcessor
    {
        public override void Initialize()
        {
            this.SetDelegates<TTag0>();
        }
    }

    public class EasyTagProcessor<TTag0, TTag1> : EasyTagProcessor
    {
        public override void Initialize()
        {
            this.SetDelegates<TTag0>();
            this.SetDelegates<TTag1>();
        }
    }

    public class EasyTagProcessor<TTag0, TTag1, TTag2> : EasyTagProcessor
    {
        public override void Initialize()
        {
            this.SetDelegates<TTag0>();
            this.SetDelegates<TTag1>();
            this.SetDelegates<TTag2>();
        }
    }
}

