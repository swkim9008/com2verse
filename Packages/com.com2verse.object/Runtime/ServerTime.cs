/*===============================================================
* Product:    Com2Verse
* File Name:  ServerTime.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.Network
{
    public static class ServerTime
    {
        private static long _delta;
        private static readonly int s_Delay = 100;
        public static readonly int SyncRate = 20;                  // 1초당 서버로부터 업데이트 되는 횟수
        public static readonly int SyncInterval = 1000 / SyncRate; // 서버로부터 업데이트 되는 주기(ms)
        public static readonly float SyncDeltaTime = 1f / SyncRate;

        /// <summary>
        /// 서버의 현재 시간과 RTT값을 동기화
        /// </summary>
        /// <param name="t">서버의 현재 시간</param>
        /// <param name="rtt">RTT 값</param>
        public static void SetServerTime(long t, int rtt)
        {
            MetaverseWatch.SetServerTime(t);
            int networkLatency = rtt / 2;
            C2VDebug.Log($"Time server sent: {t}");
            C2VDebug.Log($"Network latency: {networkLatency}");
            t += networkLatency;
            _delta = t - MetaverseWatch.Time;
            C2VDebug.Log($"Current client time: {MetaverseWatch.Time} server time: {t} Calculated Latency {Time - MetaverseWatch.Time}");
        }

        /// <summary>
        /// 임의의 딜레이(100ms)를 포함한 서버의 현재 시간 추정치
        /// </summary>
        public static long Time => MetaverseWatch.Time + _delta - s_Delay;

        /// <summary>
        /// 보간에 사용되는 델타 타임을 구한다.
        /// </summary>
        /// <param name="deltaThisFrame">델타 타임에 기준이 되는 시간 값</param>
        /// <returns>보간에 사용될 최종 값</returns>
        public static float DeltaTime(float deltaThisFrame)
        {
            return deltaThisFrame * MapController.Instance.MultiplyOfDeltaTime;
        }
    }
}
