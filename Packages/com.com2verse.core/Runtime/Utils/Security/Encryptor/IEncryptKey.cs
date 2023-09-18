/*===============================================================
* Product:		Com2Verse
* File Name:	IEncryptKey.cs
* Developer:	jhkim
* Date:			2023-08-11 13:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Utils
{
	/// <summary>
	/// 암호화 키에 대한 생성/조회/삭제 작업에 대한 인터페이스
	/// </summary>
	/// <typeparam name="T">암호화 오브젝트 타입</typeparam>
	public interface IEncryptKey<T>
	{
		/// <summary>
		/// 암호화 객체를 가져오거나 새로 생성합니다.
		/// </summary>
		/// <returns>암호화 객체</returns>
		public T GetOrCreate();
		/// <summary>
		/// 암호화 키를 새로 생성합니다.
		/// </summary>
		/// <returns>암호화 객체</returns>
		public T CreateKey();
		/// <summary>
		/// 기존 암호화 키를 폐기하고 다시 생성합니다.
		/// </summary>
		/// <returns>암호화 객체</returns>
		public T RecreateKey();
		/// <summary>
		/// 암호화 키를 삭제합니다.
		/// </summary>
		public void DeleteKey();
		/// <summary>
		/// 암호화 키를 불러옵니다.
		/// </summary>
		/// <returns>암호화 객체</returns>
		public T LoadKey();
		/// <summary>
		/// 암호화 키가 존재하는지 확인 합니다.
		/// </summary>
		/// <returns>성공 / 실패</returns>
		public bool IsKeyExist();
	}
}
