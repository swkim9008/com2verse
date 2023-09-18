/*===============================================================
* Product:		Com2Verse
* File Name:	BaseMyPadAction.cs
* Developer:	tlghks1009
* Date:			2022-09-28 11:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.InputSystem;

/*
 * MyPad ID 테이블 -
 * https://docs.google.com/spreadsheets/d/13yOmgejXRR_WMS3wUP1LGFVs-2Hiq3ml3eQWMFJQDmI/edit#gid=53286131
 * 사용법 -
 * https://jira.com2us.com/wiki/pages/viewpage.action?pageId=300549812
*/

namespace Com2Verse.UI
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MyPadElementAttribute : Attribute
	{
		public string Identifier { get; }

		public MyPadElementAttribute(string identifier)
		{
			Identifier = identifier;
		}
	}

	public abstract class BaseMyPadAction
	{
		protected string Id;
		protected bool   IsActivation = false;

		protected abstract void DoAction();

		public void ActionInvoke(string id, bool isActivation)
		{
			Id           = id;
			IsActivation = isActivation;
			DoAction();
		}

		protected void SetCustomInfo(GUIView view)
		{
			UIStackManager.Instance.SetCustomInfoByObject(view.gameObject, Id, IsActivation);
		}
	}
}
