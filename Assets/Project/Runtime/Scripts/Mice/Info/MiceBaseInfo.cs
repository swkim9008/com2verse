/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBaseInfo.cs
* Developer:	ikyoung
* Date:			2023-04-14 17:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
	public enum SyncState
	{
		None,
		Synced,
		UnSynced,
	}

	public abstract class MiceBaseInfo
	{
		public SyncState CurrentSyncState { get; protected set;}
		public long SyncVersion { get; protected set; }

		public void SetSyncState(SyncState syncState, long version)
		{
			CurrentSyncState = syncState;
			SyncVersion = version;
		}

		public bool IsVersion(long checkVersion)
		{
			return SyncVersion == checkVersion;
		}

		public virtual void OnAdd(MiceBaseInfo infoHolder)
		{
		}
		public virtual void OnUpdate(MiceBaseInfo infoHolder)
		{
		}
		
		public virtual void OnRemove(MiceBaseInfo infoHolder)
		{
		}

		protected virtual void RemoveUnsyncedInfos()
		{
		}

#if UNITY_EDITOR || ENV_DEV
        /// <summary>
        /// 더미 데이터를 생성해야 하는지 여부를 판단한다
		/// <para>(에디터 상에서 로그인하지 않았고 데이터가 비어있는 경우, 더미 데이터 생성 가능(기능 테스트용))</para>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected static bool NeedToCreateDummyData(IList list)
			=> string.IsNullOrEmpty(Network.User.Instance.CurrentUserData.AccessToken) && (list == null || list.Count == 0);
#endif
	}
}
