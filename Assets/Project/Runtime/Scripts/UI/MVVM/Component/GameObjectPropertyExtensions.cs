/*===============================================================
* Product:		Com2Verse
* File Name:	GameObjectPropertyExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-13 10:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] GameObjectPropertyExtensions")]
	public class GameObjectPropertyExtensions : MonoBehaviour
	{
		public bool ActiveState
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public bool ActiveStateReverse
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(!value); 
		}

		public bool ActiveOn
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(true);
		}

		public bool ActiveOff
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(false);
		}

		public GameObject GameObject
		{
			get => this.gameObject;
			set { }
		}
	}
}
