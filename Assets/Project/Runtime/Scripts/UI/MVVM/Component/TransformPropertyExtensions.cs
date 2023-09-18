/*===============================================================
* Product:		Com2Verse
* File Name:	TransformPropertyExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-18 19:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse
{
	public sealed class TransformPropertyExtensions : MonoBehaviour
	{
		[HideInInspector] public UnityEvent<Transform> _onAwake;

		private void Awake()
		{
			_onAwake?.Invoke(Transform);
		}

		public Transform Transform
		{
			get => this.transform;
			set { } 
		}

		public bool SetAsLastSibling
		{
			get => false;
			set
			{
				if (value && !gameObject.IsReferenceNull())
					transform.SetAsLastSibling();
			}
		}
	}
}
