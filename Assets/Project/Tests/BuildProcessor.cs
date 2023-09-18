/*===============================================================
* Product:		Com2Verse
* File Name:	BuildProcessor.cs
* Developer:	urun4m0r1
* Date:			2022-08-18 13:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using NUnit.Framework;

namespace Com2VerseTests
{
	/// <summary>
	/// 빌드 전처리기 Validation을 통과하기 위한 테스트 클래스.<br/><br/>
	/// "UNITY_INCLUDE_TESTS" 전처리기가 포함되지 않는 이상 해당 어셈블리는 컴파일되지 않는다.<br/><br/>
	/// 해당 파일이 존재하지 않을 경우 빌드시 다음 에러가 발생한다.<br/>
	/// Assembly for Assembly Definition File 'xxx.asmdef' will not be compiled, because it has no scripts associated with it.
	/// </summary>
	public static class BuildProcessor
	{
		public static void ValidateAssembly()
		{
			Assert.Pass();
		}
	}
}
