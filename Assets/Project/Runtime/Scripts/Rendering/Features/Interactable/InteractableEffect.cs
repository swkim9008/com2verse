/*===============================================================
* Product:		Com2Verse
* File Name:	InteractableEffect.cs
* Developer:	ljk
* Date:			2022-07-28 11:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Rendering.Interactable
{
	public abstract class InteractableEffect : MonoBehaviour
	{
		protected bool _isInit = false;
		protected bool _render = false;
		public virtual void Init()
		{
			_isInit = true;
			_render = true;
		}

		public virtual void SetRenderState(bool render)
		{
			_render = render;
		}

		public abstract void Evaluate(float time);

		public abstract void Draw();
	}
}
