/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesPathGenerator.cs
* Developer:	tlghks1009
* Date:			2023-03-03 15:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.IO;
using System.Text.RegularExpressions;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesPathGenerator
	{
		public string GenerateFromAssetPath(eProjectRootPath projectRootPath, string rootPath, string assetPath)
		{
			var regex = new Regex($"^{GetProjectRootPath(projectRootPath)}/.*{rootPath}/(?<relative_path>.+?)$", RegexOptions.RightToLeft);

			var matches = regex.Matches(assetPath!);

			return matches.Count == 0 ? string.Empty : matches[^1].Groups["relative_path"].Value;
		}


		public string GeneratePathByAddressType(eAddressType addressType, string addressablePath)
		{
			switch (addressType)
			{
				case eAddressType.ADDRESSABLE_PATH:
					return addressablePath;

				case eAddressType.ADDRESSABLE_PATH_WITHOUT_EXTENSIONS:
					return Path.ChangeExtension(addressablePath, null);

				case eAddressType.ASSET_NAME:
					return Path.GetFileName(addressablePath);

				case eAddressType.ASSET_NAME_WITHOUT_EXTENSIONS:
					return Path.GetFileNameWithoutExtension(addressablePath);
			}

			return string.Empty;
		}


		private string GetProjectRootPath(eProjectRootPath projectRootPath)
		{
			return projectRootPath switch
			{
				eProjectRootPath.ASSETS   => "Assets",
				eProjectRootPath.PACKAGES => "Packages",
				_                         => string.Empty
			};
		}
	}
}
