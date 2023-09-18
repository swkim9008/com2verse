/*===============================================================
* Product:		Com2Verse
* File Name:	DataHelper.cs
* Developer:	klizzard
* Date:			2023-04-05 13:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Text;
using Com2Verse.QrCode.Schemas;
using JetBrains.Annotations;

namespace Com2Verse.QrCode.Helpers
{
    public static class DataHelper
    {
        private const string NewLine = "\r\n";
        private const string Separator = ";";
        private const string Header = "BEGIN:VCARD\r\nVERSION:3.0";
        private const string Name = "N:";
        private const string OrganizationName = "ORG:";
        private const string PhotoPrefix = "PHOTO;ENCODING=BASE64;JPEG:";
        private const string PhonePrefix = "TEL:";
        private const string EmailPrefix = "EMAIL:";
        private const string Footer = "END:VCARD";

        public static string GetData([NotNull] VCardSchema vCard)
        {
            StringBuilder fw = new StringBuilder();
            fw.Append(Header);
            fw.Append(NewLine);

            //Full Name
            if (!string.IsNullOrEmpty(vCard.FirstName) || !string.IsNullOrEmpty(vCard.LastName))
            {
                //N:{LastName};{FirstName};{MiddleName};{Prefix};{Suffix}
                fw.Append(Name);
                fw.Append(vCard.FirstName); //First Name
                fw.Append(Separator);
                fw.Append(vCard.LastName);  //Last Name
                fw.Append(Separator);
                fw.Append(vCard.MiddleName); //Middle Name
                fw.Append(Separator);
                //Prefix
                fw.Append(Separator);
                //Suffix
                fw.Append(NewLine);
            }

            //Organization name
            if (!string.IsNullOrEmpty(vCard.Organization))
            {
                fw.Append(OrganizationName);
                fw.Append(vCard.Organization);
                fw.Append(NewLine);
            }

            //Email
            if (!string.IsNullOrEmpty(vCard.Email))
            {
                fw.Append(EmailPrefix);
                fw.Append(vCard.Email);
                fw.Append(NewLine);
            }

            //Phone
            if (!string.IsNullOrEmpty(vCard.PhoneNumber))
            {
                fw.Append(PhonePrefix);
                fw.Append(vCard.PhoneNumber);
                fw.Append(NewLine);
            }

            //Photo
            if (!string.IsNullOrEmpty(vCard.Photo))
            {
                fw.Append(PhotoPrefix);
                fw.Append(ImageHelper.ConvertImageURLToBase64(vCard.Photo));
                fw.Append(NewLine);
                fw.Append(NewLine);
            }

            fw.Append(Footer);

            return fw.ToString();
        }
    }
}
