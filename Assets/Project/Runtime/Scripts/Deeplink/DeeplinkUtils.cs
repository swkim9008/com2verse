/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkUtils.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Net;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;

namespace Com2Verse.Deeplink
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ServiceAttribute : Attribute
	{
		public eServiceType ServiceType { get; }

		public ServiceAttribute(eServiceType type) => ServiceType = type;
	}

	public class EngagementInfo
	{
		public eServiceType            ServiceType;
		public Components.DeeplinkType DeeplinkType;
		public string                  Param;
		public bool                    IsCheat;

		public bool IsValid() => !string.IsNullOrEmpty(Param) && DeeplinkType != Components.DeeplinkType.None;
	}

	public abstract class DeeplinkBase
	{
		protected Action FinishInvokeAction;

		protected void ShowWebApiErrorMessage(Components.OfficeHttpResultCode code)
		{
			NetworkUIManager.Instance.ShowWebApiErrorMessage(code);
			FinishInvokeAction?.Invoke();
		}

		protected void ShowHttpErrorMessage(HttpStatusCode code)
		{
			NetworkUIManager.Instance.ShowHttpErrorMessage(code);
			FinishInvokeAction?.Invoke();
		}
	}

	public abstract class DeeplinkBaseController : DeeplinkBase
	{
		// protected Dictionary<Components.DeeplinkType, DeeplinkBaseTarget> TargetList = new();
		protected Dictionary<int, DeeplinkBaseTarget> TargetList = new();

		public string Name;

		public abstract void Initialize();

		public void Invoke(EngagementInfo info, Action finishInvoke)
		{
			C2VDebug.LogCategory("Deeplink", $"Invoke {Name}");
			FinishInvokeAction = finishInvoke;
			TryCheckParam(info);
		}

		protected abstract void TryCheckParam(EngagementInfo info);

		protected T LoadTarget<T>() where T : DeeplinkBaseTarget
		{
			C2VDebug.LogCategory("Deeplink", $"Add Target : {typeof(T).Name} => {Name}");
			var newTarget = Activator.CreateInstance(typeof(T)) as T;
			return newTarget;
		}
	}

	public abstract class DeeplinkBaseTarget : DeeplinkBase
	{
		public abstract void InvokeAsync(string param, Action finishInvoke);
	}
}
