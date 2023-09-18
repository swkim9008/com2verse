/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Cheat.cs
* Developer:	haminjeong
* Date:			2022-06-27 12:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;

namespace Com2Verse.Network
{
	public sealed partial class Commander 
	{
#region Avatar
		public void CheatAvatarRemove(string username)
		{
			if (!User.Instance.Standby) return;
			if (User.Instance.CurrentUserData!.UserName!.Equals(username))
			{
				C2VDebug.LogErrorCategory("Cheat", "Cannot remove avatar, because it's mine");
				return;
			}
			
			Protocols.Controller.CheatNotify cheatNotify = new()
			{
				Command = "deleteavatar",
				Param1  = username,
			};
			C2VDebug.Log($"send cheat {cheatNotify.Command} {cheatNotify.Param1}");
			NetworkManager.Instance.Send(cheatNotify, Protocols.Controller.MessageTypes.CheatNotify);
		}
#endregion // Avatar

#region Command
		public void CheatCommand(string command, params string[] param)
		{
			if (!User.Instance.Standby) return;
			Protocols.Controller.CheatNotify cheatNotify = new()
			{
				Command = command,
				Param1 = param[0],
			};
			cheatNotify.Param2 ??= param[1];
			cheatNotify.Param3 ??= param[2];
			cheatNotify.Param4 ??= param[3];
			C2VDebug.Log($"send cheat {cheatNotify.Command} {cheatNotify.Param1} {cheatNotify.Param2} {cheatNotify.Param3} {cheatNotify.Param4}");
			NetworkManager.Instance.Send(cheatNotify, Protocols.Controller.MessageTypes.CheatNotify);
		}
#endregion // Command
	}
}
