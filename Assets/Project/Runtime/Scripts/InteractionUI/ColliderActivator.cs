// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ColliderActivator.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-08 오전 10:44
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using UnityEngine;

namespace Com2Verse.Interaction
{
	public class ColliderActivator : MonoBehaviour
	{
		public Collider TargetCollider { get; set; }
		private GameObject _targetCharacter;

		private void Start()
		{
			_targetCharacter = User.Instance.CharacterObject.gameObject;
		}

		private void OnDisable()
		{
			Restore();
		}

		private void OnTriggerExit(Collider other)
		{
			if (ReferenceEquals(other.gameObject, _targetCharacter))
			{
				Restore();
			}
		}

		private void Restore()
		{
			TargetCollider.isTrigger = false;
			Destroy(this);
		}
	}
}
