/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtensions_Collection.cs
* Developer:	sprite
* Date:			2023-04-20 21:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.UI;

namespace Com2Verse.Mice
{
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// 컨테이너(<paramref name="container"/>)의 요소(<typeparamref name="T"/>)들을 컬렉션(<paramref name="collection"/>)에 추가한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="container"></param>
        /// <param name="append">false 일 경우 컬렉션을 비우고 추가한다(=default)</param>
        /// <returns>컬렉션(<paramref name="collection"/>) 갯수 변화량</returns>
        public static int AppendFrom<T>(this Collection<T> collection, IEnumerable<T> container, bool append = false)
            where T : ViewModel
        {
            var lastCount = collection.CollectionCount;

            if (!append) collection.Reset();

            if (container == null) return 0 - lastCount;

            foreach (var item in container)
            {
                collection.AddItem(item);
            }

            return collection.CollectionCount - lastCount;
        }

        /// <summary>
        /// 컬랙션(<paramref name="collection"/>) 용량(<paramref name="capacity"/>) 설정.
        /// <para>*주의*</para>
        /// <para><see cref="Collection&lt;T&gt;"/> [T=<typeparamref name="T"/>] 의</para>
        /// <para>내부 컨테이너(<see cref="List&lt;T&gt;"/> [T=<typeparamref name="T"/>])를 재 생성한다.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static bool SetCapacity<T>(this Collection<T> collection, int capacity)
            where T : ViewModel
        {
            var field = collection.GetType().GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field == null) return false;

            field.SetValue(collection, new List<T>(capacity));

            return true;
        }

        /// <summary>
        /// 컨테이너(<paramref name="container"/>)의 요소(<typeparamref name="T"/>)들을 컬렉션(<paramref name="collection"/>)에 넣는다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="collection"></param>
        /// <param name="append">false 일 경우 컬렉션을 비우고 추가한다(=default)</param>
        /// <returns>컬렉션(<paramref name="collection"/>) 갯수 변화량</returns>
        public static int Into<T>(this IEnumerable<T> container, Collection<T> collection, bool append = false)
             where T : ViewModel
            => collection.AppendFrom(container, append);


        private static System.Random rand = new System.Random();

        /// <summary>
        /// 컨테이너(<paramref name="container"/>)의 요소(<typeparamref name="T"/>)들 중 하나를 무작위로 반환한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static T RandomPickOne<T>(this IEnumerable<T> container)
            => container.ElementAt(rand.Next(container.Count()));
    }
}
