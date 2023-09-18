#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey.cs
* Developer:	jehyun
* Date:			2022-07-13 19:46
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Com2Verse.AudioRecord;
using Com2Verse.Avatar;
using Com2Verse.Chat;
using Com2Verse.Contents;
using Com2Verse.Data;
using Com2Verse.Deeplink;
using Com2Verse.Extension;
using Com2Verse.HttpHelper;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Notification;
using Com2Verse.Option;
using Com2Verse.Pathfinder;
using Com2Verse.PlayerControl.LocalMode;
using Com2Verse.Project.Animation;
using Com2Verse.SoundSystem;
using Com2Verse.Tutorial;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using Localization = Com2Verse.UI.Localization;
using Object = UnityEngine.Object;
using Security = Com2Verse.Utils.Security;
using Util = Com2Verse.Utils.Util;
using Com2Verse.Mice;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{
#region Log
		[MetaverseCheat("Cheat/Log/Log")] [HelpText("message")]
		private static void Log(string message)
		{
			C2VDebug.Log(message);
		}

		[MetaverseCheat("Cheat/Log/LogCategory")] [HelpText("category", "message")]
		private static void LogCategory(string category, string message)
		{
			C2VDebug.LogCategory(category, message);
		}

		[MetaverseCheat("Cheat/Log/LogMethod")] [HelpText("category", "message", "caller")]
		private static void LogMethod(string category, string message, [CallerMemberName] string caller = null)
		{
			C2VDebug.LogMethod(category, message, caller);
		}

		[MetaverseCheat("Cheat/Log/AddIgnoredCategory")] [HelpText("category")]
		private static void AddIgnoredCategory(string category)
		{
			category ??= string.Empty;
			LogManager.AddIgnoredCategory(category);
		}

		[MetaverseCheat("Cheat/Log/RemoveIgnoredCategory")] [HelpText("category")]
		private static void RemoveIgnoredCategory(string category)
		{
			category ??= string.Empty;
			LogManager.RemoveIgnoredCategory(category);
		}
#endregion // Log

#region Command Cheat
		[MetaverseCheat("Cheat/CommandCheat/Command")] [HelpText("command", "param1", "param2", "param3", "param4")]
		private static void CommandCheat(string command, string param1, string param2, string param3, string param4)
		{
			try
			{
				param1 = string.IsNullOrWhiteSpace(param1) ? null : param1;
				param2 = string.IsNullOrWhiteSpace(param2) ? null : param2;
				param3 = string.IsNullOrWhiteSpace(param3) ? null : param3;
				param4 = string.IsNullOrWhiteSpace(param4) ? null : param4;
				Commander.Instance.CheatCommand(command, param1, param2, param3, param4);
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
			}
		}
#endregion

#region Character
		[MetaverseCheat("Cheat/Network/EnableNetworkLogger")] [HelpText("0 or 1")]
		private static void EnableNetworkLogger(string isOn = "1")
		{
			NetworkManager.Instance.IsVerbose = isOn == "1";
		}

		[MetaverseCheat("Cheat/Network/EnableAvatarCreate")] [HelpText("0 or 1")]
		private static void EnableAvatarCreate(string isOn = "0")
		{
			MapController.Instance.IsEnableAvatarCreate = isOn == "1";
		}

		[MetaverseCheat("Cheat/Network/EnableObjectCountLog")] [HelpText("0 or 1")]
		private static void EnableObjectCountLog(string isOn = "1")
		{
			MapController.Instance.IsEnableObjectCountLog = isOn == "1";
		}

		[MetaverseCheat("Cheat/Network/EnableObjectCountLimit")] [HelpText("0 or 1")]
		private static void EnableObjectCountLimit(string countString = "1000")
		{
			MapController.Instance.ObjectCountLimit = int.TryParse(countString, out var count) ? count : 0;
		}

		[MetaverseCheat("Cheat/Network/CS-Log")] [HelpText("0 or 1")]
		private static void EnableCSLog(string isOn = "1")
		{
			MapController.Instance.IsCSLogEnable = isOn == "1";
		}

		[MetaverseCheat("Cheat/Network/SessionIdleTimeout")] [HelpText("0 or 1")]
		private static void EnableSessionIdleTimeout(string isOn = "0")
		{
			User.Instance.ForcedDisableCheckSession = isOn == "0";
		}

		[MetaverseCheat("Cheat/Character/ChangeSpeed")] [HelpText("walk speed")]
		private static void ChangeWalkSpeed(string walk)
		{
			try
			{
				var walkSpeed     = Mathf.Max(Convert.ToSingle(walk), 0f);
				var mapController = MapController.Instance;
				var localMode     = Util.GetOrAddComponent<LocalMode>(mapController.gameObject);
				localMode.ForceLoadCurrentData();
				localMode.MovementData.Speed    = walkSpeed;
				localMode.SetMovementData();
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
			}
		}

		[MetaverseCheat("Cheat/Character/EnableAvatarHudViewModel")] [HelpText("0 = disable, 1 = enable")]
		private static void EnableAvatarHudViewModel(string value)
		{
			var viewModel = ViewModelManager.InstanceOrNull?.Get<ActiveObjectManagerViewModel>();
			if (viewModel == null) return;

			if (value      == "0") viewModel.Enable = false;
			else if (value == "1") viewModel.Enable = true;
		}

		[MetaverseCheat("Cheat/Character/ServerPathFinding")]
		private static void EnableClientPathFinding()
		{
			ClientPathFinding.Instance.enabled = false;
		}

		[MetaverseCheat("Cheat/Character/DebugAddCharacter")] [HelpText("Number of characters to add")]
		private static void DebugAddCharacter(string numOfCharacter, string radius = "5")
		{
			Vector3 pivot = !User.Instance.CharacterObject.IsUnityNull() ? User.Instance.CharacterObject.transform.position : Vector3.zero;

			var numberOfCharacterInt = Convert.ToInt32(numOfCharacter);

			for (int i = 0; i < numberOfCharacterInt; ++i)
			{
				DebugUtils.DebugAddCharacter(UnityEngine.Random.Range(0, int.MaxValue), onCompleted: objectId =>
				{
					var objectTransform = MapController.Instance[objectId].transform;
					objectTransform.position = pivot + MathUtil.RandomPositionOnCircle(Convert.ToSingle(radius));
				}).Forget();
			}
		}

		[MetaverseCheat("Cheat/Character/RemoveAvatar")] [HelpText("Avatar remove by nickname")]
		private static void RemoveAvatar(string nickname)
		{
			Commander.Instance.CheatAvatarRemove(nickname);
		}

		[MetaverseCheat("Cheat/Character/AnimationSound")] [HelpText("AnimationSound option")]
		private static void AnimationSound(string soundOtherAvatarVolume = "0.7", string soundOtherAvatarCount = "10")
		{
			var soundOtherAvatarVolumeFloat = Convert.ToSingle(soundOtherAvatarVolume);
			var soundOtherAvatarCountFloat  = Convert.ToInt32(soundOtherAvatarCount);

			AnimationSoundController.SoundOtherAvatarVolume = soundOtherAvatarVolumeFloat;
			AnimationSoundController.SoundOtherAvatarCount  = soundOtherAvatarCountFloat;
		}

		[MetaverseCheat("Cheat/Character/AnimationSoundVolume")] [HelpText("AnimationSound volume")]
		private static void AnimationSoundVolume(string walkSoundVolume = "0.8", string runSoundVolume = "1", string jumpSoundVolume = "1", string landSoundVolume = "1")
		{
			var walkSoundVolumeFloat = Convert.ToSingle(walkSoundVolume);
			var runSoundVolumeFloat  = Convert.ToSingle(runSoundVolume);
			var jumpSoundVolumeFloat = Convert.ToSingle(jumpSoundVolume);
			var landSoundVolumeFloat = Convert.ToSingle(landSoundVolume);

			AnimationSoundController.WalkSoundVolume = walkSoundVolumeFloat;
			AnimationSoundController.RunSoundVolume  = runSoundVolumeFloat;
			AnimationSoundController.JumpSoundVolume = jumpSoundVolumeFloat;
			AnimationSoundController.LandSoundVolume = landSoundVolumeFloat;
		}

		[MetaverseCheat("Cheat/Character/Customize/PrintCustomizeItemListInCloset")]
		private static void PrintCustomizeItemListInCloset()
		{
			var currentAvatar = AvatarMediator.InstanceOrNull?.AvatarCloset.CurrentAvatar;
			if (currentAvatar == null) return;
			currentAvatar.PrintCustomizeItemList();
		}
#endregion Character

#region Npc
		[MetaverseCheat("Cheat/Npc/SetData")]
		private static void SetData(string npcSpeechSpeed = "0.1", string npcSpeechSpeedSkip = "5")
		{
			// TODO: 테이블 데이터 값들 추가
			var npcSpeechSpeedFloat     = Convert.ToSingle(npcSpeechSpeed);
			var npcSpeechSpeedSkipFloat = Convert.ToInt32(npcSpeechSpeedSkip);

			NpcManager.Instance.SetData(npcSpeechSpeedFloat, npcSpeechSpeedSkipFloat);
		}

		[MetaverseCheat("Cheat/Npc/SetCameraData")]
		private static void SetCameraData(string cameraDistance = "4.5", string forceRotateYawThreshold = "20", string pitchThreshold = "20", string heightRatio = "0.33", string distanceRatio = "0.5", string inCameraBlendTime = "2", string outCameraBlendTime = "2")
		{
			var cameraDistanceFloat          = Convert.ToSingle(cameraDistance);
			var forceRotateYawThresholdFloat = Convert.ToSingle(forceRotateYawThreshold);
			var pitchThresholdFloat          = Convert.ToSingle(pitchThreshold);
			var heightRatioFloat             = Convert.ToSingle(heightRatio);
			var distanceRatioFloat           = Convert.ToSingle(distanceRatio);
			var inCameraBlendTimeFloat       = Convert.ToSingle(inCameraBlendTime);
			var outCameraBlendTimeFloat      = Convert.ToSingle(outCameraBlendTime);

			NpcManager.Instance.SetData(cameraDistanceFloat, forceRotateYawThresholdFloat, pitchThresholdFloat, heightRatioFloat, distanceRatioFloat, inCameraBlendTimeFloat, outCameraBlendTimeFloat);
		}
#endregion Npc

#region WebView
		[MetaverseCheat("Cheat/WebView/Show")]
		private static void WebViewShow(string url, string width, string height)
		{
			if (string.IsNullOrWhiteSpace(url)) return;

			int.TryParse(width, out var w);
			int.TryParse(height, out var h);

			if (w == 0) w = 1300;
			if (h == 0) h = 800;

			UIManager.Instance.ShowPopupWebView(false, new Vector2(w, h), url);
		}
		[MetaverseCheat("Cheat/WebView/Naver")]
		private static void WebViewNaver()
		{
			UIManager.Instance.ShowPopupWebView(true, new Vector2(1300, 800), "https://www.naver.com");
		}

		[MetaverseCheat("Cheat/WebView/Youtube")]
		private static void WebViewYoutube()
		{
			UIManager.Instance.ShowPopupWebView(true, new Vector2(1300, 800), "https://www.youtube.com");
		}

		[MetaverseCheat("Cheat/WebView/pdf")]
		private static void WebViewPdf()
		{
			string URL = "https://nss-seoul.s3.ap-northeast-2.amazonaws.com/nss/temp/BehaviorDesignerDocumentation.pdf";
			var screenSize = new Vector2(1700, 1000);	//GetScreenSize();
			UIManager.Instance.ShowPopupWebView(false,
			                                    screenSize,	//new Vector2(1300, 800),
			                                    URL,
												downloadProgressAction:(eventArgs =>
												{
													if (eventArgs.Type == ProgressChangeType.Finished)
													{
														var text = Data.Localization.eKey.MICE_UI_SessionHall_FileDownload_Msg_Success.ToLocalizationString();

                                                        UIManager.Instance.ShowPopupCommon(text, ()=> //$"DUMMY : PDF 파일 다운로드가 완료되었습니다.", () =>
														{
															OpenInFileBrowser.Open(eventArgs.FilePath);
														});
														//File.Move(eventArgs.FilePath, someOtherLocation);
													}
													else if (eventArgs.Type == ProgressChangeType.Updated)
													{
														C2VDebug.Log($">>>>>>>>>> {eventArgs.Progress}");
													}
												}));
		}


		[MetaverseCheat("Cheat/WebView/Theoplayer")]
		private static void WebViewTheoplayer()
		{
			UIManager.Instance.ShowPopupWebView(true, new Vector2(1300, 800),
				"https://www.theoplayer.com/test-your-stream-hls-dash-hesp?hsCtaTracking=0e523328-fba2-4bf1-8bc8-22cf9732fc3b%7Cb1033600-0811-4353-9b14-0e8f90bd6d62");
		}

		[MetaverseCheat("Cheat/WebView/MiroTest")]
		private static void WebViewMiroTest(string width = "1300", string height = "800", string offsetX = "15", string offsetY = "90", string boardId = "uXjVMHEpaLc=")
		{
			int.TryParse(width, out var w);
			int.TryParse(height, out var h);
			int.TryParse(offsetX, out var ox);
			int.TryParse(offsetY, out var oy);

			if (w == 0) w = 1300;
			if (h == 0) h = 800;

			WhiteBoardWebView.Show(boardId, new (w, h), new (ox, oy));
		}
		[MetaverseCheat("Cheat/WebView/WhiteBoard/Capture")]
		private static async void WhiteBoardCapture(string width = "1300", string height = "800", string offsetX = "15", string offsetY = "90", string boardId = "uXjVMHEpaLc=")
		{
			int.TryParse(width, out var w);
			int.TryParse(height, out var h);
			int.TryParse(offsetX, out var ox);
			int.TryParse(offsetY, out var oy);

			if (w == 0) w = 1300;
			if (h == 0) h = 800;

			string miroHtmlFormat = WhiteBoardWebView.MiroHtmlViewFormat;
			var htmlStr = string.Format(miroHtmlFormat, Convert.ToString(w - ox), Convert.ToString(h - oy), boardId);
			var webView = Web.CreateWebView();
			await webView.Init(1300, 800);
			webView.LoadHtml(htmlStr);
			await webView.WaitForNextPageLoadToFinish();
			var material = webView.CreateMaterial();
			SaveImage(material.mainTexture, "vuplexCapture.png");
		}

		private static void SaveImage(Texture t, string path)
		{
			RenderTexture rt = new RenderTexture(t.width, t.height, 0);
			Graphics.Blit(t, rt);
			Texture2D t2d = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
			t2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			File.WriteAllBytes(path, t2d.EncodeToPNG());
		}

		[MetaverseCheat("Cheat/WebView/WhiteBoard/Show")]
		private static void WhiteBoardShow()
		{
		}
#endregion WebView

#region Notification
		[MetaverseCheat("Cheat/Notification/Notification MaintainedTime")] [HelpText("time")]
		private static void SetNotificationMaintainedTime(string time = "5")
		{
			NotificationManager.NotificationHoldingTime = Convert.ToInt32(time);
		}

		[MetaverseCheat("Cheat/Notification/Send Notification")] [HelpText("count", "intervalMilliseconds")]
		private static void SetNotificationMaintainedTime(string count = "1", string intervalMilliseconds = "1000")
		{
			NotificationManager.Instance.TestNotification(Convert.ToInt32(count), Convert.ToInt32(intervalMilliseconds)).Forget();
		}
		
		[MetaverseCheat("Cheat/Notification/NotificationCreateRequest")] [HelpText("type 0 ~ 2")]
		private static void NotificationCreateRequest(string type = "0")
		{
			Commander.Instance.RequestNotificationCreate(Convert.ToInt32(type));
		}
#endregion

#region ToastPopup
		[MetaverseCheat("Cheat/ToastPopup/NormalToastPopup")]
		private static void ShowNormalToastPopup()
		{
			UIManager.Instance.SendToastMessage("테스트 입니다.");
		}

		[MetaverseCheat("Cheat/ToastPopup/WarningToastPopup")]
		private static void ShowWarningToastPopup()
		{
			UIManager.Instance.SendToastMessage("테스트 입니다.", 3f, UIManager.eToastMessageType.WARNING);
		}
#endregion

#region LOGIN_CHEAT
		[MetaverseCheat("Cheat/LoginCheat/Com2VerseLoginWithTokenExpires")] [HelpText("CheatId", "Init : 0 (1 hour)", "Init : 0 (1 day)")]
		private static void Com2VerseLoginWithTokenExpires(string cheatId, string accessExpires, string refreshExpires)
		{
			LoginManager.Instance.RequestCom2VerseLoginCheat(cheatId, Convert.ToInt32(accessExpires), Convert.ToInt32(refreshExpires));
		}

		[MetaverseCheat("Cheat/LoginCheat/SetCheatEngagement")] [HelpText("world / office / mice", "param")]
		private static void SetCheatEngagement(string type, string param)
		{
			DeeplinkParser.SetDeeplinkParamAsync(type, param);
		}

		[MetaverseCheat("Cheat/LoginCheat/RequestServiceLogin")]
		private static void RequestServiceLogin()
		{
			LoginManager.Instance.RequestOrganizationChart();
		}
#endregion

#region Quality Settings
		[MetaverseCheat("Cheat/QualitySettings/EmptyQualityLevelEnabled")] [HelpText("true or false")]
		private static void EmptyQualityLevelEnabled(string enabled = "true")
		{
			try
			{
				var graphicsOption = OptionController.Instance.GetOption<GraphicsOption>();
				if (graphicsOption != null)
					graphicsOption.EmptyQualityLevelEnabled = Convert.ToBoolean(enabled.Trim().ToLower());
			}
			catch (Exception)
			{
				C2VDebug.LogWarning($"The parameter value is incorrect. {enabled}");
			}
		}
#endregion

#region Security
		[MetaverseCheat("Cheat/Security/Encrypt Test")] [HelpText("Plain Text")]
		private static async void SecurityEncryptTest(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return;
			var base64Str = await Security.Instance.EncryptAesAsync(text);
			C2VDebug.Log($"Encrypt : {text} => {base64Str}");
		}

		[MetaverseCheat("Cheat/Security/Decrypt Test")] [HelpText("Base64 Encoded Text")]
		private static async void SecurityDecryptText(string base64Str)
		{
			if (string.IsNullOrWhiteSpace(base64Str)) return;
			var text = await Security.Instance.DecryptToStringAsync(base64Str);
			C2VDebug.Log($"Decrypt : {base64Str} => {text}");
		}

		[MetaverseCheat("Cheat/Security/Encrypt Test (File)")] [HelpText("Plain Text")]
		private static async void SecurityEncryptFileTest(string plainFilePath)
		{
			if (string.IsNullOrWhiteSpace(plainFilePath) || !File.Exists(plainFilePath)) return;

			var text = await File.ReadAllBytesAsync(plainFilePath)!;
			var sw = new Stopwatch();

			C2VDebug.Log($"Encrypt File {text.Length} bytes");

			sw.Start();
			var base64Str = await Security.Instance.EncryptAesAsync(text);
			sw.Stop();

			var saveFilePath = GetFilePath(plainFilePath, "encrypted");
			await File.WriteAllTextAsync(saveFilePath, base64Str)!;
			C2VDebug.Log($"Encrypted File {base64Str.Length} bytes. Elapsed = {sw.Elapsed:g}");
			C2VDebug.Log($"Encrypt File Save = {saveFilePath}");
		}

		[MetaverseCheat("Cheat/Security/Decrypt Test (File)")]
		private static async void SecurityDecryptFileText(string encryptedFilePath)
		{
			if (string.IsNullOrWhiteSpace(encryptedFilePath) || !File.Exists(encryptedFilePath)) return;

			var base64Str = await File.ReadAllTextAsync(encryptedFilePath)!;
			var sw = new Stopwatch();

			C2VDebug.Log($"Decrypt File {base64Str.Length} bytes");

			sw.Start();
			var result = await Security.Instance.DecryptToBytesAsync(base64Str);
			sw.Stop();

			if (result.Status == Security.eDecryptStatus.SUCCESS)
			{
				var text = result.Value;
				var saveFilePath = GetFilePath(encryptedFilePath, "plain");
				await File.WriteAllBytesAsync(saveFilePath, text)!;
				C2VDebug.Log($"Decrypted File {text.Length} bytes. Elapsed = {sw.Elapsed:g}");
				C2VDebug.Log($"Decrypt File Saved = {saveFilePath}");
			} else {
				C2VDebug.LogWarning($"Decrypted fail = {result.Status}");
			}
		}

		private static string GetFilePath(string filePath, string suffix, string ext = "txt")
		{
			var dir = Path.GetDirectoryName(filePath);
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			return Path.Combine(dir, $"{fileName}_{suffix}.{ext}");
		}
#endregion // Security

#region Local Save
		private static readonly string LocalSaveName = "LOCALSAVE_TEST";

		[Serializable]
		class LocalSaveTest
		{
			public string Name;
			public int[]  Values;

			public void Print()
			{
				C2VDebug.Log($"Name = {Name}");
				for (int i = 0; i < Values.Length; ++i)
					C2VDebug.Log($"  [{i}] = {Convert.ToString(Values[i])}");
			}
		}

		[MetaverseCheat("Cheat/LocalSave/SaveTest")]
		private static void LocalSaveSaveTest(string name)
		{
			var arrayCnt = UnityEngine.Random.Range(5, 20);
			var values   = new int[arrayCnt];
			for (int i = 0; i < arrayCnt; ++i)
				values[i] = UnityEngine.Random.Range(0, 100);
			var data = new LocalSaveTest
			{
				Name   = name,
				Values = values,
			};
			data.Print();

			LocalSave.Temp.SaveJson(LocalSaveName, data);
		}

		[MetaverseCheat("Cheat/LocalSave/LoadTest")]
		private static void LocalSaveLoadTest()
		{
			var data = LocalSave.Temp.LoadJson<LocalSaveTest>(LocalSaveName);
			if (data != null)
				data.Print();
			else
				C2VDebug.LogWarning($"save data load failed");
		}

		[MetaverseCheat("Cheat/LocalSave/DeleteTest")]
		private static void LocalSaveDeleteTest()
		{
			LocalSave.Temp.Delete(LocalSaveName);
		}

#if UNITY_EDITOR
		[MetaverseCheat("Cheat/LocalSave/Open LocalSave Folder")]
		private static void LocalSaveOpenFolder()
		{
			EditorUtility.RevealInFinder(LocalSave.Temp.GetBaseDir());
		}
#endif     // UNITY_EDITOR
#endregion // Local Save

#region Texture Cache
		// private static string _texUrl4165k = "https://picsum.photos/200";
		// private static string _texUrl2790k = "https://picsum.photos/300";
		// private static string _texUrl2612k = "https://picsum.photos/400";
		// private static string _texUrl2058k = "https://picsum.photos/500";
		// private static string _texUrl2057k = "https://picsum.photos/600";
		// private static string _texUrl1804k = "https://picsum.photos/700";

		[MetaverseCheat("Cheat/Texture Cache/Run Test")]
		private static void TextureCacheRunTest()
		{
			// var _testUrls = new string[]
			// {
			// 	_texUrl4165k,
			// 	_texUrl2790k,
			// 	_texUrl2612k,
			// 	_texUrl2058k,
			// 	_texUrl2057k,
			// 	_texUrl1804k,
			// };

			var testUrls = new[]
			{
				"https://file-examples.com/storage/fea582e6406477bb69e8a67/2017/10/file_example_PNG_1MB.png",
				"https://file-examples.com/storage/fea582e6406477bb69e8a67/2017/10/file_example_PNG_2100kB.png",
				"https://file-examples.com/storage/fea582e6406477bb69e8a67/2017/10/file_example_PNG_3MB.png",
			};

			foreach (var testUrl in testUrls)
				TextureCache.Instance.GetOrDownloadTextureAsync(testUrl).Forget();
		}
#endregion // Texture Cache

#region Login Server List
		[MetaverseCheat("Cheat/LoginServerList/Refresh ServerList")]
		private static async void LoginServerListRefreshServerList()
		{
			await LoginServerList.RefreshServerListAsync();

			PrintLoginServerList();
		}

		[MetaverseCheat("Cheat/LoginServerList/Load From File")]
		private static void LoginServerListLoadFromFile()
		{
			LoginServerList.Load(async success =>
			{
				if (success)
					PrintLoginServerList();
				else
					await LoginServerList.RefreshServerListAsync();
			});
		}

		[MetaverseCheat("Cheat/LoginServerList/Load Config.json")]
		private static void LoginServerListLoadConfigJson()
		{
			var selectServerPanel = Object.FindObjectOfType<SelectServerPanel>();
			if (selectServerPanel.IsUnityNull()) return;
			selectServerPanel.LoadConfigJson(true);
		}

		private static void PrintLoginServerList()
		{
			foreach (var (key, value) in LoginServerList.ServerInfoMap)
				C2VDebug.Log($"[{key}] Public = {value.PublicIP}");
		}
#endregion // Login Server List

#region TTS
		[MetaverseCheat("Cheat/TTS/Voice")] [HelpText("Ment", "name")]
		private static void SttVoice(string ment, string name)
		{
			TTS.TTSSecretary.TTS(ment, DirectoryUtil.GetTempPath("TTS", "Audio"), $"{name}.mp3", () => { PlayTTS(name).Forget(); });
		}

		private static async UniTask PlayTTS(string name)
		{
			var path = DirectoryUtil.GetTempPath("TTS", "Audio", $"{name}.mp3");
			var www  = new WWW(path);
			await UniTask.WaitUntil(() => www != null);
			AudioClip                            clip     = www.GetAudioClip();
			GameObject                           audioObj = new GameObject();
			Sound.MetaverseAudioSource audio    = null;

			audio = audioObj.gameObject.GetComponent<Sound.MetaverseAudioSource>();
			if (audio == null)
				audio = Sound.MetaverseAudioSource.CreateNew(audioObj.gameObject);

			audio.SetClip(clip);
			audio.Play();
		}
#endregion

#region LogOnOff
		[MetaverseCheat("Cheat/LogOnOff/request")]
		private static void LogOnOffUserInfoRequest()
		{
			UserState.Info.Instance.SendLogOnOffUserInfoRequest(new long[] { 1, 2, 3, 4, 5 });
		}
#endregion

#region Tutorial
		[MetaverseCheat("Cheat/Tutorial/TutorialMents_GourpId")] [HelpText("groupId = 100001")]
		private static void TutorialMents(string groupId = "100001")
		{
			var convertgroupId = Convert.ToInt32(groupId);
			TutorialManager.Instance.TutorialPlay((eTutorialGroup)convertgroupId).Forget();

			var _cheatWindow = GameObject.Find("UI_Cheat(Clone)");
			if (null != _cheatWindow)
			{
				_cheatWindow.SetActive(false);
			}
		}

		[MetaverseCheat("Cheat/Tutorial/TutorialMents_Clear")]
		private static void TutorialClear()
		{
			TutorialManager.Instance.TutorialClear();
		}
#endregion

#region Disconnected
		[MetaverseCheat("Cheat/Disconnect/ForceDisconnect")]
		private static void ForceDisconnect()
		{
			LoginManager.Instance.Logout();
		}
#endregion

#region Builder

#if UNITY_EDITOR
		[MenuItem("Com2Verse/Builder Test/Enter _F12")]
#endif
		[MetaverseCheat("Cheat/Builder/Enter")]
		private static void BuilderEnter()
		{
			LoadingManager.Instance.ChangeScene<SceneBuilder>();
		}
#endregion

#region Option
		[MetaverseCheat("Cheat/Option/HotkeyChange")]
		private static void HotkeyChange()
		{
			UIManager.Instance.ShowRebindPopup();
		}

		[MetaverseCheat("Cheat/Option/LocalizationStringCheck")]
		private static void LocalizationStringCheck()
		{
			Localization.Instance.ChangeLanguage(Localization.eLanguage.UNKNOWN);
		}

		[MetaverseCheat("Cheat/Option/ResetOptions")]
		private static void ResetOptions()
		{
			OptionController.Instance.RemoveOptionData();
		}
#endregion

#region Announcement
		[MetaverseCheat("Cheat/Announcement/Test")]
		private static void OnAnnouncement()
		{
			NoticeManager.Instance.SendNotice("공지테스트공지테스트공지테스트공지테스트공지테스트공지테스트", NoticeManager.eNoticeType.EVERYTHING);
		}
#endregion

#region Graphy
		[MetaverseCheat("Cheat/Graphy/Toggle OnOff")]
		private static void ToggleGraphy()
		{
			var graphy = GameObject.Find("[Graphy]");
			if (graphy.IsUnityNull()) return;

			var graphyCanvas = graphy.GetComponent<Canvas>();
			if (graphyCanvas.IsReferenceNull()) return;

			graphyCanvas.enabled = !graphyCanvas.enabled;
		}
#endregion // Graphy

#region HttpHelper
		[MetaverseCheat("Cheat/HttpHelper/PapagoTest")] [HelpText("Message", "Source Language", "Target Language", "Client ID", "Client Secret")]
		private static async UniTask Translate(string message      = "Hello World", string sourceLang = "en", string targetLang = "ko", string clientId = "nd2uMOepW6SLmdGlsJPE",
		                                       string clientSecret = "gVNnj7hCh6")
		{
			var papagoTranslateUrl = "https://openapi.naver.com/v1/papago/n2mt";
			var postParams         = $"source={sourceLang}&target={targetLang}&text={message}";
			var requestMessage = HttpRequestBuilder.Generate(new HttpRequestMessageInfo
			{
				RequestMethod = Client.eRequestType.POST,
				Url           = papagoTranslateUrl,
				Content       = new StringContent(postParams),
				Headers = new (string, string)[]
				{
					("X-Naver-Client-Id", clientId),
					("X-Naver-Client-Secret", clientSecret),
				},
				ContentType = "application/x-www-form-urlencoded",
			});

			var responseString = await Client.Message.RequestStringAsync(requestMessage);
			var response       = JsonUtility.FromJson<JsonPapagoResponse>(responseString.Value);
			var translated     = response.message.result.translatedText;
			C2VDebug.Log($"[{sourceLang} -> {targetLang}] : {message} -> {translated}");
		}

		[Serializable]
		private class JsonPapagoResponse
		{
			public JsonPapagoMessageResponse message;
		}

		[Serializable]
		private class JsonPapagoMessageResponse
		{
			public JsonPapagoResultResponse result;
		}

		[Serializable]
		private class JsonPapagoResultResponse
		{
			public string translatedText;
		}
#endregion // HttpHelper

#region AudioRecord
		[MetaverseCheat("Cheat/AudioRecord/LoccalSave")][HelpText("filename = testfile")]
		private static void AudioRecordLocalSave(string filename = "testfile")
		{
			AudioRecordManager.Instance.LocalSave(filename);
		}
#endregion
		
#region Sound
		[MetaverseCheat("Cheat/Sound/Snapshot Transition")]
		private static void SnapshotTransition(string index = "0", string duration = "0")
		{
			SoundManager.SnapshotTransition((eAudioSnapshot)int.Parse(index), float.Parse(duration));
		}


		[MetaverseCheat("Cheat/Sound/BGM Transition")]
		private static void BGMTransition(string index = "0", string fadeDuration = "2", string fadeInDelay = "0")
		{
			SoundManager.PlayBGM((eSoundIndex)int.Parse(index), float.Parse(fadeDuration), float.Parse(fadeInDelay));
		}
#endregion Sound


#region RedDot
        [MetaverseCheat("Cheat/Mice/RedDot")]
        private static void TestReddot(string index = "0")
        {
			Mice.RedDotManager.TryCreateInstance();
        }

        [MetaverseCheat("Cheat/Mice/RedDotTigger")]
        private static void TestReddotTrigger(string index = "0")
        {
			Mice.RedDotManager.SetTrigger(Mice.RedDotManager.RedDotData.TriggerKey.ShowNotice);
        }
        #endregion RedDot



        #region Chatting
        [MetaverseCheat("Cheat/Chat/Disconnect")]
		private static void DisconnectWebsocket()
		{
			ChatManager.Instance.Disconnect();
		}

		[MetaverseCheat("Cheat/Chat/Connect")]
		private static void ConnectWebsocket()
		{
			ChatManager.Instance.Connect();
		}

		[MetaverseCheat("Cheat/Chat/SetStateChangeTime")]
		private static void SetStateChangeTime(string time = "5")
		{
			ChatManager.Instance.TableSetting.StateChangeTime = int.Parse(time);
		}
#endregion

#region Error
		[MetaverseCheat("Cheat/ErrorMessage/ShowCommonErrorMessage")]
		private static void ShowCommonErrorMessage()
		{
			NetworkUIManager.Instance.ShowCommonErrorMessage();
		}

		[MetaverseCheat("Cheat/ErrorMessage/ShowProtocolErrorMessage")]
		private static void ShowProtocolErrorMessage(string errorCode)
		{
			var target = (Protocols.ErrorCode)Enum.Parse(typeof(Protocols.ErrorCode), errorCode);
			NetworkUIManager.Instance.ShowProtocolErrorMessage(target);
		}

		[MetaverseCheat("Cheat/ErrorMessage/ShowWebApiErrorMessage")]
		private static void ShowWebApiErrorMessage(string errorCode)
		{
			var target = (WebApi.Service.Components.OfficeHttpResultCode)Enum.Parse(typeof(WebApi.Service.Components.OfficeHttpResultCode), errorCode);
			NetworkUIManager.Instance.ShowWebApiErrorMessage(target);
		}
#endregion

#region Banned Words
		[MetaverseCheat("Cheat/BannedWords")] [HelpText("", "", "", "Usage : all, name, sentence")]
		private static async void CheckBannedWords(string word, string lang = BannedWords.BannedWords.All, string countryCode = BannedWords.BannedWords.All, string usage = BannedWords.BannedWords.All)
		{
			BannedWords.BannedWords.SetLanguageCode(lang);
			BannedWords.BannedWords.SetCountryCode(countryCode);
			BannedWords.BannedWords.SetUsage(usage);

			var hasBannedWord = await BannedWords.BannedWords.HasBannedWordAsync(word);
			C2VDebug.Log($"금칙어 필터링 됨? {Convert.ToString(hasBannedWord)}");
		}
#endregion // Banned Words
		
#region Security
		[MetaverseCheat("Cheat/Security/PrintAES")]
		private static async void CheckBannedWords()
		{
			Network.Security.ShowKeys();
		}
#endregion // Security
	}

}
#endif
