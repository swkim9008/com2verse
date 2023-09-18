/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSyncData.cs
* Developer:	sprite
* Date:			2023-04-25 19:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public interface IMiceSyncData<TData>
    {
        List<TData> Cache { get; set; }
        UniTask Sync();

        TData UpdateData(TData cached, TData fromServer) => fromServer;

        void OnAdd(TData cached) { }
        void OnUpdate(TData cached) { }
        void OnRemove(TData cached) { }

        void OnDataChanged() { }

        public List<TData> UpdateCache(IList<TData> sourceData, Func<TData, TData, bool> comparer)
        {
            var lastCache = this.Cache;
            var cache = this.Cache;

            bool changed = false;

            if (cache == null || cache.Count == 0)
            {
                cache = sourceData.ToList();

                for (int i = 0, cnt = cache.Count; i < cnt; i++)
                {
                    this.OnAdd(cache[i]);
                }

                changed = true;
            }
            else
            {
#region 비교자 초기화
                IEqualityComparer<TData> equalityComparer = EqualityComparer<TData>.Default;

                if (comparer == null)
                {
                    comparer = (a, b) => equalityComparer.Equals(a, b);
                }
                else
                {
                    equalityComparer = new EqualityComparerFromMethod<TData>(comparer);
                }
#endregion // 비교자 초기화

                IEnumerable<TData> Maintenance()
                {
                    var cache = this.Cache;

                    for (int i = 0, cnt = sourceData?.Count ?? 0; i < cnt; i++)
                    {
                        var fromServer = sourceData[i];

                        var cached = cache.FirstOrDefault(e => comparer(e, fromServer));

                        if (cached != null)
                        {
                            var updated = this.UpdateData(cached, fromServer);
                            this.OnUpdate(updated);

                            changed = true;

                            yield return updated;
                        }
                        else
                        {
                            this.OnAdd(fromServer);

                            changed = true;

                            yield return fromServer;
                        }
                    }
                }

                cache = Maintenance().ToList();

#region 서버에서 삭제된 객체 삭제하기
                // 여기서는 lastCache가 null 일 수 없음.
                TData item;
                for (int i = 0, cnt = lastCache.Count; i < cnt; i++)
                {
                    item = lastCache[i];
                    if (cache.Any(e => equalityComparer.Equals(item, e))) continue;

                    this.OnRemove(item);
                    changed = true;
                }
#endregion  // 서버에서 삭제된 객체 삭제하기
            }

            this.Cache = cache;

            if (changed)
            {
                this.OnDataChanged();
            }

            return cache;
        }
    }

    internal class EqualityComparerFromMethod<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> _comparer;

        public EqualityComparerFromMethod(Func<T, T, bool> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(T a, T b) => _comparer(a, b);
        public int GetHashCode(T a) => a.GetHashCode();
    }

    public static partial class IMiceSyncDataExtensions
    {
        public static List<TData> UpdateCache<TData>(this IMiceSyncData<TData> targetData, IList<TData> sourceData) => targetData.UpdateCache(sourceData, null);
        public static List<TData> UpdateCache<TData>(this IMiceSyncData<TData> targetData, IList<TData> sourceData, Func<TData, TData, bool> comparer) => targetData.UpdateCache(sourceData, comparer);
    }
}
