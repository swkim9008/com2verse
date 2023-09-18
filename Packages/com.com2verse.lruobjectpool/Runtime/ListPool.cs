/*===============================================================
* Product:		Com2Verse
* File Name:	ListPool.cs
* Developer:	NGSG
* Date:			2023-05-31 11:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.LruObjectPool
{
    public class ListPoolItem<T>
    {
        public float _lastUsedSeconds;
        public List<List<T>> _pool;

        public ListPoolItem(int initSize)
        {
            _lastUsedSeconds = DateTime.Now.Millisecond;
            _pool = new List<List<T>>(initSize);
        }

        // public void DoSelfCheck(float nowSeconds)
        // {
        //     if (_pool.Count > 0)
        //     {
        //         float durationNotUse = nowSeconds - _lastUsedSeconds;
        //         if (durationNotUse > 5 * 60)
        //         {
        //             _pool.Clear();
        //         }
        //     }
        // }

        public void Use(int add)
        {
            _lastUsedSeconds = DateTime.Now.Millisecond;
        }
    }
    
	public static class ListPool<T>
    {
        private const int MAX_POOL_CAPACITY = 16384;
        public static int maxPoolSize = 4;
        static ListPoolItem<T> _poolItem;

        static ListPool()
        {
            _poolItem = new ListPoolItem<T>(maxPoolSize);
        }

        public static void Warmup(int count, int size)
        {
            lock (_poolItem)
            {
                var tmp = new List<T>[count];
                for (int i = 0; i < count; i++)
                    tmp[i] = Claim(size);
                for (int i = 0; i < count; i++)
                    Release(ref tmp[i]);
            }
        }
        
        public static List<T> Claim(int capacity = 0)
        {
            lock(_poolItem)
            {
                _poolItem.Use(1);

                List<List<T>> pool = _poolItem._pool;

                int curPoolSize = pool.Count;
                if (curPoolSize > 0)
                {
                    List<T> objList = pool[curPoolSize - 1];
                    pool.RemoveAt(curPoolSize - 1);
                    return objList;
                }
                else
                {
                    if(capacity > 0)
                        return new List<T>(capacity);
                    else
                        return new List<T>();
                }
            }
        }

        public static void Release(ref List<T> objList)
        {
            if (objList == null) 
                return;

            lock (_poolItem)
            {
                _poolItem.Use(-1);
                List<List<T>> pool = _poolItem._pool;
                List<T> objList2 = objList;
                objList = null;
    
                if (pool.Contains(objList2))
                {
                    UnityEngine.Debug.LogError(string.Concat("ListPool Release list already in pool ", objList2.GetType().ToString()));
                    return;
                }

                objList2.Clear();

                // 맥스 용량 체크
                // if (objList2.Capacity > MAX_POOL_CAPACITY)
                // {
                //     objList2.TrimExcess();
                //     UnityEngine.Debug.LogError("harmless recycle list capacity " + objList2.Capacity);
                // }

                int curPoolSize = pool.Count;
                if (curPoolSize >= maxPoolSize)
                {
                    int removeIndex = -1;
                    int maxCapacity = objList2.Capacity;
                    for (int i = 0; i < curPoolSize; ++i)
                    {
                        List<T> curList = pool[i];
                        if (curList.Capacity > maxCapacity)
                        {
                            removeIndex = i;
                            maxCapacity = curList.Capacity;
                        }
                    }

                    if (removeIndex >= 0)
                    {
                        pool[removeIndex] = objList2;
                    }
                    return;
                }
                pool.Add(objList2);
            }
        }
	}
}
