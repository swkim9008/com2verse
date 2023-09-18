/*===============================================================
* Product:		Com2Verse
* File Name:	CompilerDefinition.cs
* Developer:	urun4m0r1
* Date:			2022-04-15 15:32
* History:		
* Documents:	https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// 이 파일은 C# 9.0 문법 { init; } 을 사용하기 위해 별도로 선언이 필요한 파일입니다.
	/// 임의로 삭제시 Mono 버전에 따라 { init; } 문법을 사용하는 코드가 작동하지 않을 가능성이 있습니다.
	/// 해당 파일의 네임스페이스를 이동해서는 안됩니다.
	/// </summary>
	static partial class IsExternalInit { }
}
