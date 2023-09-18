using System.Net;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Protocols.GameLogic;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public async UniTask RequestBoardWrite(string text, string objectId)
		{
			var enrollRequest= new Components.EnrollRequest();
			enrollRequest.Message = text;
			enrollRequest.ObjectId = objectId.ToString();

			var response = await Api.TodayBoard.PostTodayBoardEnroll(enrollRequest);
			if (response == null)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				C2VDebug.LogWarning($"오늘의 한마디 글 등록에 실패하였습니다. response is null");
				return;
			}

			if (response.StatusCode == HttpStatusCode.OK && response.Value?.Code == Components.OfficeHttpResultCode.Success)
			{
				UIManager.Instance.UseHandlerAction();
			}
			else
			{
				UIManager.Instance.SendToastMessage("UI_Office_Voicemessage_Today_Error", 3f, UIManager.eToastMessageType.WARNING);
				
				if(response.Value == null)
					C2VDebug.LogWarning($"오늘의 한마디 글 등록에 실패하였습니다. StatusCode : {response.StatusCode} , response.Value is null");
				else
					C2VDebug.LogWarning($"오늘의 한마디 글 등록에 실패하였습니다. StatusCode : {response.StatusCode}, response.Value.Code : {response.Value.Code}");	
			}
		}

		public void RequestAIBotString()
		{
			if (long.TryParse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, out long mapId))
			{
				NetworkManager.Instance.Send(new ReadAIBotRequest()
				{
					MapId = mapId
				}, Protocols.GameLogic.MessageTypes.ReadAiBotRequest);
			}
		}
	}
}
