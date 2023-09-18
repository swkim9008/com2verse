/*===============================================================
* Product:    Com2Verse
* File Name:  MetaverseWatch.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Diagnostics;

namespace Com2Verse.Network
{
    public sealed class MetaverseWatch
    {
        /// <summary>
        /// TimerUpdate에 의해 갱신이 되는 시간. TimerUpdate를 어떤 주기로 호출하느냐에 따라 달라질 수 있다. 
        /// </summary>
        public static long Time;
    
        private static readonly DateTime SOriginTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static long _startTime = (long)(DateTime.UtcNow - SOriginTime).TotalMilliseconds;
        private static readonly Stopwatch Watch = new Stopwatch();
        
        /// <summary>
        /// 호출 즉시 정확한 시간을 가져오는 값.
        /// </summary>
        /// <returns>TimeStamp 시간</returns>
        public static long Realtime
        {
            get
            {
                if (!Watch.IsRunning)
                    Watch.Start();
                return _startTime + Watch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// 서버로부터의 현재 시간을 받아 MetaverseWatch의 기준 시간을 정하고, 시계를 다시 시작한다. 
        /// </summary>
        /// <param name="time">서버의 현재 시간(timestamp)</param>
        public static void SetServerTime(long time)
        {
            _startTime = time;
            Watch.Restart();
            TimerUpdate();
        }
    
        /// <summary>
        /// Time 변수에 현재 시간인 Realtime을 갱신한다.
        /// </summary>
        public static void TimerUpdate()
        {
            Time = Realtime;
        }

        /// <summary>
        /// 클라이언트 로컬 시간을 가져옵니다.(DateTime)
        /// </summary>
        public static DateTime NowDateTime => SOriginTime.AddMilliseconds(Realtime).ToLocalTime();
    }
}