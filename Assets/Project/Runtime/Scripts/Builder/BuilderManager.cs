// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-15 오전 10:39
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Builder;
using UnityEngine;

namespace Com2Verse
{
	public class BuilderManager : DestroyableMonoSingleton<BuilderManager>
	{
		public const string TempDataExtension = ".c2vbuilder";
		private BuilderManager() { }

		private void Update()
		{
			CheckAction();
		}

		private void CheckAction()
		{
			if (Input.GetKey(KeyCode.LeftControl))
			{
				if (Input.GetKeyDown(KeyCode.Z))
				{
					ActionSystem.Instance.Undo();
				}
				else if (Input.GetKeyDown(KeyCode.Y))
				{
					ActionSystem.Instance.Redo();
				}
			}
		}
	}
}
