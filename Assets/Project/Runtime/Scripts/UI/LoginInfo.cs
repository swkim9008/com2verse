/*===============================================================
* Product:		Com2Verse
* File Name:	UILoginView.cs
* Developer:	tlghks1009
* Date:			2022-05-10 14:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class LoginInfo : MonoBehaviour
	{
		[SerializeField] private string _authFrontend;
		[SerializeField] private string _publicBackend;
		[SerializeField] private string _privateBackend;
		[SerializeField] private string _generalBackend;
		[SerializeField] private string _portBackend;

		public string AuthFrontend
		{
			get => _authFrontend;
			set => _authFrontend = value;
		}
		
		public string PublicBackend
		{
			get => _publicBackend;
			set => _publicBackend = value;
		}
		
		public string PrivateBackend
		{
			get => _privateBackend;
			set => _privateBackend = value;
		}
		
		public string GeneralBackend
		{
			get => _generalBackend;
			set => _generalBackend = value;
		}
		
		public string PortBackend
		{
			get => _portBackend;
			set => _portBackend = value;
		}
	}
}
