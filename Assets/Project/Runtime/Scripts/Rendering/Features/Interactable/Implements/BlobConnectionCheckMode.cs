/*===============================================================
* Product:		Com2Verse
* File Name:	BlobConnectionCheckMode.cs
* Developer:	ljk
* Date:			2023-07-28 13:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.Rendering.Interactable;
using Random = UnityEngine.Random;

namespace Com2Verse
{
	public sealed class BlobDummy : MonoBehaviour
	{
		public float MoveRadius = 0;
		public float RunningStack = 0;
		public Vector3 Center;
		public BlobConnection MainBC;
		private ConnectionNode node;
		public float connectionRate = 0;
		public float RunningSpeed = 0;
		
		private void Update()
		{
			if(MainBC == null)
				return;
			if (node == null)
			{
				node = MainBC.Connect(transform);
				return;
			}
			
			RunningStack += Time.deltaTime*RunningSpeed;
			transform.position = Center+ new Vector3( Mathf.Cos(RunningStack)*MoveRadius , 0 ,Mathf.Sin(RunningStack)*MoveRadius  );

			connectionRate = Mathf.Clamp01(Mathf.Sin(RunningStack)*2);
			node.SetWeight(connectionRate);
		}
	}
	public sealed class BlobConnectionCheckMode : MonoBehaviour
	{
		public BlobConnection targetBlob;
		
		private void OnEnable()
		{
			if(targetBlob == null)
				return;
			targetBlob.transform.position = Vector3.zero;

			for (int i = 0; i < 5; i++)
			{
				GameObject bd = new GameObject("bdc");
				BlobDummy bdd = bd.AddComponent<BlobDummy>();
				bdd.Center = transform.position;
				bdd.MoveRadius = Random.Range(1, 5f);
				bdd.RunningSpeed = Random.Range(0.01f, 0.3f);
				bdd.MainBC = targetBlob;
				bdd.RunningStack = Random.Range(0, Mathf.PI);
			}
		}
	}
}
