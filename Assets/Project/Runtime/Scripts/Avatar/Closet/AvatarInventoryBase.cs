/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarInventoryBase.cs
* Developer:	eugene9721
* Date:			2023-04-19 12:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Logger;

namespace Com2Verse.Avatar
{
	/// <summary>
	/// 기본 아바타의 외형을 변경시 선택 가능한 아이템의 ID리스트를 가져오기 위한 클래스
	/// </summary>
	public abstract class AvatarInventoryBase
	{
		private readonly List<int> _faceItemList      = new();
		private readonly List<int> _bodyShapeItemList = new();
		private readonly List<int> _fashionItemList   = new();

		public abstract bool Initialize();

		/// <summary>
		/// FIXME: 8월 QA빌드용 임시처리
		/// 아바타 선택씬 진입시 AvatarManager의 테이블 데이터를 받아 저장
		/// </summary>
		/// <returns> 테이블 데이터가 로드되지 않은 경우 false </returns>
		protected bool InitializeByTable()
		{
			if (AvatarTable.TableAvatarCreateFaceItem == null || AvatarTable.TableAvatarCreateFaceItem.Datas == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarTable.TableFaceItem is null");
				return false;
			}

			if (AvatarTable.TableBodyShapeItem == null || AvatarTable.TableBodyShapeItem.Datas == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarTable.TableBodyItem is null");
				return false;
			}

			if (AvatarTable.TableAvatarCreateFashionItem == null || AvatarTable.TableAvatarCreateFashionItem.Datas == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarTable.TableAvatarCreateFashionItem is null");
				return false;
			}

			Clear();
			foreach (var faceKey in AvatarTable.TableAvatarCreateFaceItem.Datas)
			{
				if (AvatarTable.TableExcludedCreateFaceItem is { Datas: { } })
				{
					var isExcluded = false;
					foreach (var excludedCreateFaceItem in AvatarTable.TableExcludedCreateFaceItem.Datas)
					{
						if (excludedCreateFaceItem.AvatarCreateFaceItemId == faceKey.AvatarCreateFaceItemId)
							isExcluded = true;
					}

					if (isExcluded)
						continue;
				}

				_faceItemList.Add(faceKey.AvatarCreateFaceItemId);
			}

			foreach (var bodyKey in AvatarTable.TableBodyShapeItem.Datas.Keys)
				_bodyShapeItemList.Add(bodyKey);

			foreach (var fashionItem in AvatarTable.TableAvatarCreateFashionItem.Datas)
			{
				if (AvatarTable.TableExcludedCreateFashionItem is { Datas: { } })
				{
					var isExcluded = false;
					foreach (var excludedCreateFaceItem in AvatarTable.TableExcludedCreateFashionItem.Datas)
					{
						if (excludedCreateFaceItem.AvatarCreateFashionItemId == fashionItem.AvatarCreateFashionItemId)
							isExcluded = true;
					}

					if (isExcluded)
						continue;
				}

				_fashionItemList.Add(fashionItem.AvatarCreateFashionItemId);
			}

			return true;
		}

		/// <summary>
		/// 아바타 선택씬 퇴장시 데이터 클리어
		/// </summary>
		public void Clear()
		{
			_faceItemList.Clear();
			_bodyShapeItemList.Clear();
			_fashionItemList.Clear();
		}

		public List<int> GetFaceItemList() => _faceItemList;

		public List<int> GetBodyShapeItemList() => _bodyShapeItemList;

		public List<int> GetFashionItemList() => _fashionItemList;
	}
}
