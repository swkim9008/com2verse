/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesExceptionHelper.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.AssetSystem
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AddressableExceptionAttribute : Attribute
    {
        public Type ExceptionType { get; set; }

        public AddressableExceptionAttribute(Type exceptionType)
        {
            this.ExceptionType = exceptionType;
        }
    }


    public static class C2VAddressablesExceptionHelper
    {
        public static bool TryGetExceptionType(Type exceptionType, out eC2VAddressablesErrorCode currentErrorCode)
        {
            foreach (var fieldInfo in typeof(eC2VAddressablesErrorCode).GetFields())
            {
                if (Attribute.GetCustomAttribute(fieldInfo, typeof(AddressableExceptionAttribute)) is not
                    AddressableExceptionAttribute exceptionAttribute)
                {
                    continue;
                }

                if (exceptionAttribute.ExceptionType != exceptionType)
                {
                    continue;
                }

                currentErrorCode = (eC2VAddressablesErrorCode) fieldInfo.GetValue(null);

                return true;
            }

            currentErrorCode = default;
            return false;
        }
    }
}