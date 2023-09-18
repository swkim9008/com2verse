/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtension_Enum.cs
* Developer:	klizzard
* Date:			2023-07-31 17:59
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/
using Com2Verse.Data;

namespace Com2Verse.Mice
{
    public static partial class EnumExtensions
    {
        public static string ToLocalizationString(this MiceWebClient.eMicePackageType type)
        {
            switch (type)
            {
                case MiceWebClient.eMicePackageType.ENTRANCE_ALL_IN_EVENT:
                    return Data.Localization.eKey.MICE_UI_Mobile_MP_MyTicket_Ticket_FreePass.ToLocalizationString();
                case MiceWebClient.eMicePackageType.ENTRANCE_ALL_IN_PROGRAM:
                    return Data.Localization.eKey.MICE_UI_Mobile_MP_MyTicket_Ticket_ProgramPass.ToLocalizationString();
                case MiceWebClient.eMicePackageType.ENTRANCE_TO_SESSION:
                    return Data.Localization.eKey.MICE_UI_Mobile_MP_MyTicket_Ticket_Special3.ToLocalizationString();
            }

            return type.ToString();
        }
        public static string ToShortWord(this eNetErrorSourceType type)
        {
            switch (type)
            {
                case eNetErrorSourceType.WORLD:
                    return "W";
                case eNetErrorSourceType.OFFICE:
                    return "O";
                case eNetErrorSourceType.MICE:
                    return "M";
                case eNetErrorSourceType.MICE_WEB:
                    return "MW";
                case eNetErrorSourceType.HIVE:
                    return "HV";
                case eNetErrorSourceType.HTTP:
                    return "HT";
            }
            return type.ToString();
        }
    }
}