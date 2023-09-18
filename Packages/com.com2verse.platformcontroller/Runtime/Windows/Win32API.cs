using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;
using Com2Verse.Logger;

namespace Com2Verse.PlatformControl.Windows
{
	internal static class Win32API
	{
		private static IntPtr _hWnd = IntPtr.Zero;

		public static IntPtr HWnd
		{
			get => _hWnd;
			set => _hWnd = value;
		}
		
#region DEVICE
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hdc"></param>
		/// <param name="lprcClip"></param>
		/// <param name="lpfnEnum"></param>
		/// <param name="dwData"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

		/// <summary>
		/// 
		/// </summary>
		public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref WinRect lprcMonitor, IntPtr dwData);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hmonitor"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		[DllImport("User32.dll", CharSet = CharSet.Unicode)]
		public static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFOEX info);
		
		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
		public class MONITORINFOEX
		{
			public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
			public WinRect rcMonitor = new WinRect();
			public WinRect rcWork = new WinRect();
			public int dwFlags = 0;
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 32)]
			public char[] szDevice = new char[32];
		}
#endregion // DEVICE

#region WINDOW
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern IntPtr GetActiveWindow();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="nCmdShow"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);
		
		/// <summary>
		/// ShowWindow - nCmdShow 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
		/// </summary>
		public enum WindowCommand : int
		{
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
		}
					
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="lpRect"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, out WinRect lpRect);
		
		/// <summary>
		/// 
		/// </summary>
		public struct WinRect
		{
			public int left, top, right, bottom;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="hWndInsertAfter"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="cx"></param>
		/// <param name="cy"></param>
		/// <param name="uFlags"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
		
		/// <summary>
		/// WindowPos - hWndInsertAfter 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
		/// </summary>
		public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
		public static readonly IntPtr HWND_TOP = new IntPtr(0);
		public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		
		/// <summary>
		/// WindowPos - uFlags 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
		/// </summary>
		public enum WindowFlag : uint
		{
			SWP_ASYNCWINDOWPOS = 0x4000,
			SWP_DEFERERASE = 0x2000,
			SWP_DRAWFRAME = 0x0020,
			SWP_FRAMECHANGED = 0x0020,
			SWP_HIDEWINDOW = 0x0080,
			SWP_NOACTIVATE = 0x0010,
			SWP_NOCOPYBITS = 0x0100,
			SWP_NOMOVE = 0x0002,
			SWP_NOOWNERZORDER = 0x0200,
			SWP_NOREDRAW = 0x0008,
			SWP_NOREPOSITION = 0x0200,
			SWP_NOSENDCHANGING = 0x0400,
			SWP_NOSIZE = 0x0001,
			SWP_NOZORDER = 0x0004,
			SWP_SHOWWINDOW = 0x0040
		}
#endregion // WINDOW

#region OPTION
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="nIndex"></param>
		/// <param name="dwNewLong"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
					
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="nIndex"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

		/// <summary>
		/// WindowLong - nIndex 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlonga
		/// </summary>
		public enum OptionOffset : int
		{
			GWL_EXSTYLE = -20,
			GWL_HINSTANCE = -6,
			GWL_ID = -12,
			GWL_STYLE = -16,
			GWL_USERDATA = -21,
			GWL_WNDPROC = -4
		}

		/// <summary>
		/// WindowLong - GWL_STYLE - dwNewLong 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
		/// </summary>
		public enum OptionStyle : uint
		{
			WS_BORDER = 0x00800000,
			WS_CAPTION = 0x00C00000,
			WS_CHILD = 0x40000000,
			WS_CHILDWINDOW = 0x40000000,
			WS_CLIPCHILDREN = 0x02000000,
			WS_CLIPSIBLINGS = 0x04000000,
			WS_DISABLED = 0x08000000,
			WS_DLGFRAME = 0x00400000,
			WS_GROUP = 0x00020000,
			WS_HSCROLL = 0x00100000,
			WS_ICONIC = 0x20000000,
			WS_MAXIMIZE = 0x01000000,
			WS_MAXIMIZEBOX = 0x00010000,
			WS_MINIMIZE = 0x20000000,
			WS_MINIMIZEBOX = 0x00020000,
			WS_OVERLAPPED = 0x00000000,
			WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
			WS_POPUP = 0x80000000,
			WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU),
			WS_SIZEBOX = 0x00040000,
			WS_SYSMENU = 0x00080000,
			WS_TABSTOP = 0x00010000,
			WS_THICKFRAME = 0x00040000,
			WS_TILED = 0x00000000,
			WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
			WS_VISIBLE = 0x10000000,
			WS_VSCROLL = 0x00200000
		}

		/// <summary>
		/// WindowLong - GWL_EXSTYLE - dwNewLong 관련 링크
		/// https://learn.microsoft.com/ko-kr/windows/win32/winmsg/extended-window-styles
		/// </summary>
		public enum OptionExStyle : uint
		{
			WS_EX_ACCEPTFILES = 0x00000010,
			WS_EX_APPWINDOW = 0x00040000,
			WS_EX_CLIENTEDGE = 0x00000200,
			WS_EX_COMPOSITED = 0x02000000,
			WS_EX_CONTEXTHELP = 0x00000400,
			WS_EX_CONTROLPARENT = 0x00010000,
			WS_EX_DLGMODALFRAME = 0x00000001,
			WS_EX_LAYERED = 0x00080000,
			WS_EX_LAYOUTRTL = 0x00400000,
			WS_EX_LEFT = 0x00000000,
			WS_EX_LEFTSCROLLBAR = 0x00004000,
			WS_EX_LTRREADING = 0x00000000,
			WS_EX_MDICHILD = 0x00000040,
			WS_EX_NOACTIVATE = 0x08000000,
			WS_EX_NOINHERITLAYOUT = 0x00100000,
			WS_EX_NOPARENTNOTIFY = 0x00000004,
			WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
			WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
			WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
			WS_EX_RIGHT = 0x00001000,
			WS_EX_RIGHTSCROLLBAR = 0x00000000,
			WS_EX_RTLREADING = 0x00002000,
			WS_EX_STATICEDGE = 0x00020000,
			WS_EX_TOOLWINDOW = 0x00000080,
			WS_EX_TOPMOST = 0x00000008,
			WS_EX_TRANSPARENT = 0x00000020,
			WS_EX_WINDOWEDGE = 0x00000100
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="nIndex"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int nIndex);

		/// <summary>
		/// SystemMetrics - nIndex 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
		/// </summary>
		public enum OptionSystemMetrics : int
		{
			SM_ARRANGE = 56,
			SM_CLEANBOOT = 67,
			SM_CMONITORS = 80,
			SM_CMOUSEBUTTONS = 43,
			SM_CONVERTIBLESLATEMODE = 0x2003,
			SM_CXBORDER = 5,
			SM_CXCURSOR = 13,
			SM_CXDLGFRAME = 7,
			SM_CXDOUBLECLK = 36,
			SM_CXDRAG = 68,
			SM_CXEDGE = 45,
			SM_CXFIXEDFRAME = 7,
			SM_CXFOCUSBORDER = 83,
			SM_CXFRAME = 32,
			SM_CXFULLSCREEN = 16,
			SM_CXHSCROLL = 21,
			SM_CXHTHUMB = 10,
			SM_CXICON = 11,
			SM_CXICONSPACING = 38,
			SM_CXMAXIMIZED = 61,
			SM_CXMAXTRACK = 59,
			SM_CXMENUCHECK = 71,
			SM_CXMENUSIZE = 54,
			SM_CXMIN = 28,
			SM_CXMINIMIZED = 57,
			SM_CXMINSPACING = 47,
			SM_CXMINTRACK = 34,
			SM_CXPADDEDBORDER = 92,
			SM_CXSCREEN = 0,
			SM_CXSIZE = 30,
			SM_CXSIZEFRAME = 32,
			SM_CXSMICON = 49,
			SM_CXSMSIZE = 52,
			SM_CXVIRTUALSCREEN = 78,
			SM_CXVSCROLL = 2,
			SM_CYBORDER = 6,
			SM_CYCAPTION = 4,
			SM_CYCURSOR = 14,
			SM_CYDLGFRAME = 8,
			SM_CYDOUBLECLK = 37,
			SM_CYDRAG = 69,
			SM_CYEDGE = 46,
			SM_CYFIXEDFRAME = 8,
			SM_CYFOCUSBORDER = 84,
			SM_CYFRAME = 33,
			SM_CYFULLSCREEN = 17,
			SM_CYHSCROLL = 3,
			SM_CYICON = 12,
			SM_CYICONSPACING = 39,
			SM_CYKANJIWINDOW = 18,
			SM_CYMAXIMIZED = 62,
			SM_CYMAXTRACK = 60,
			SM_CYMENU = 15,
			SM_CYMENUCHECK = 72,
			SM_CYMENUSIZE = 55,
			SM_CYMIN = 29,
			SM_CYMINIMIZED = 58,
			SM_CYMINSPACING = 48,
			SM_CYMINTRACK = 35,
			SM_CYSCREEN = 1,
			SM_CYSIZE = 31,
			SM_CYSIZEFRAME = 33,
			SM_CYSMCAPTION = 51,
			SM_CYSMICON = 50,
			SM_CYSMSIZE = 53,
			SM_CYVIRTUALSCREEN = 79,
			SM_CYVSCROLL = 20,
			SM_CYVTHUMB = 9,
			SM_DBCSENABLED = 42,
			SM_DEBUG = 22,
			SM_DIGITIZER = 94,
			SM_IMMENABLED = 82,
			SM_MAXIMUMTOUCHES = 95,
			SM_MEDIACENTER = 87,
			SM_MENUDROPALIGNMENT = 40,
			SM_MIDEASTENABLED = 74,
			SM_MOUSEPRESENT = 19,
			SM_MOUSEHORIZONTALWHEELPRESENT = 91,
			SM_MOUSEWHEELPRESENT = 75,
			SM_NETWORK = 63,
			SM_PENWINDOWS = 41,
			SM_REMOTECONTROL = 0x2001,
			SM_REMOTESESSION = 0x1000,
			SM_SAMEDISPLAYFORMAT = 81,
			SM_SECURE = 44,
			SM_SERVERR2 = 89,
			SM_SHOWSOUNDS = 70,
			SM_SHUTTINGDOWN = 0x2000,
			SM_SLOWMACHINE = 73,
			SM_STARTER = 88,
			SM_SWAPBUTTON = 23,
			SM_SYSTEMDOCKED = 0x2004,
			SM_TABLETPC = 86,
			SM_XVIRTUALSCREEN = 76,
			SM_YVIRTUALSCREEN = 77
		}
#endregion // OPTION

#region TRANSPARENT
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="margins"></param>
		/// <returns></returns>
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins margins);
		
		/// <summary>
		/// 
		/// </summary>
		public struct Margins
		{
			public int left, right, top, bottom;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="crKey"></param>
		/// <param name="bAlpha"></param>
		/// <param name="dwFlags"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
				
		/// <summary>
		/// 색상 변환 메서드
		/// </summary>
		/// <param name="R">Red : 0 ~ 255</param>
		/// <param name="G">Green : 0 ~ 255</param>
		/// <param name="B">Blue : 0 ~ 255</param>
		/// <returns></returns>
		public static uint SetColor(int R, int G, int B)
		{
			string r = R.ToString("X2");
			string g = G.ToString("X2");
			string b = B.ToString("X2");
			string color = $"0x00{b}{g}{r}";
			return Convert.ToUInt32(color, 16);
		}
		
		/// <summary>
		/// LayeredWindowAttributes - dwFlags 관련 링크
		/// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setlayeredwindowattributes
		/// </summary>
		public enum TransparentFlag : uint
		{
			LWA_COLORKEY = 0x00000001,
			LWA_ALPHA = 0x00000002
		}
#endregion // TRANSPARENT

#region EVENT
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventMin"></param>
		/// <param name="eventMax"></param>
		/// <param name="hmodWinEventProc"></param>
		/// <param name="lpfnWinEventProc"></param>
		/// <param name="idProcess"></param>
		/// <param name="idThread"></param>
		/// <param name="dwflags"></param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
				
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hWinEventHook"></param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern int UnhookWinEvent(IntPtr hWinEventHook);
				
		/// <summary>
		/// 
		/// </summary>
		public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
		
		/// <summary>
		/// 
		/// </summary>
		public enum WinEventFlags
		{
			WINEVENT_OUTOFCONTEXT = 0x0000,   // Events are ASYNC
			WINEVENT_SKIPOWNTHREAD = 0x0001,  // Don't call back for events on installer's thread
			WINEVENT_SKIPOWNPROCESS = 0x0002, // Don't call back for events on installer's process
			WINEVENT_INCONTEXT = 0x0004,      // Events are SYNC, this causes your dll to be injected into every process
		}

		/// <summary>
		/// 
		/// </summary>
		public enum WinEvent
        {
            EVENT_MIN = 0x00000001,
            EVENT_OBJECT_CREATE = 0x8000, // hwnd ID idChild is created item
            EVENT_OBJECT_DESTROY = 0x8001, // hwnd ID idChild is destroyed item
            EVENT_OBJECT_SHOW = 0x8002, // hwnd ID idChild is shown item
            EVENT_OBJECT_HIDE = 0x8003, // hwnd ID idChild is hidden item
            EVENT_OBJECT_REORDER = 0x8004, // hwnd ID idChild is parent of zordering children
            EVENT_OBJECT_FOCUS = 0x8005, // hwnd ID idChild is focused item
            EVENT_OBJECT_SELECTION = 0x8006, // hwnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex
            EVENT_OBJECT_SELECTIONADD = 0x8007, // hwnd ID idChild is item added
            EVENT_OBJECT_SELECTIONREMOVE = 0x8008, // hwnd ID idChild is item removed
            EVENT_OBJECT_SELECTIONWITHIN = 0x8009, // hwnd ID idChild is parent of changed selected items
            EVENT_OBJECT_STATECHANGE = 0x800A, // hwnd ID idChild is item w/ state change
            EVENT_OBJECT_LOCATIONCHANGE = 0x800B, // hwnd ID idChild is moved/sized item
            EVENT_OBJECT_NAMECHANGE = 0x800C, // hwnd ID idChild is item w/ name change
            EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D, // hwnd ID idChild is item w/ desc change
            EVENT_OBJECT_VALUECHANGE = 0x800E, // hwnd ID idChild is item w/ value change
            EVENT_OBJECT_PARENTCHANGE = 0x800F, // hwnd ID idChild is item w/ new parent
            EVENT_OBJECT_HELPCHANGE = 0x8010, // hwnd ID idChild is item w/ help change
            EVENT_OBJECT_DEFACTIONCHANGE = 0x8011, // hwnd ID idChild is item w/ def action change
            EVENT_OBJECT_ACCELERATORCHANGE = 0x8012, // hwnd ID idChild is item w/ keybd accel change
            EVENT_OBJECT_INVOKED = 0x8013, // hwnd ID idChild is item invoked
            EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014, // hwnd ID idChild is item w? test selection change
            EVENT_OBJECT_CONTENTSCROLLED = 0x8015,
            EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016,
            EVENT_SYSTEM_MOVESIZESTART = 0x000A,
            EVENT_SYSTEM_MOVESIZEEND = 0x000B,  // The movement or resizing of a window has finished. This event is sent by the system, never by servers.
            EVENT_SYSTEM_MINIMIZESTART = 0x0016,    // A window object is about to be minimized.
            EVENT_SYSTEM_MINIMIZEEND = 0x0017,      // A window object is about to be restored.
            EVENT_SYSTEM_END = 0x00FF,
            EVENT_OBJECT_END = 0x80FF,
            EVENT_AIA_START = 0xA000,
            EVENT_AIA_END = 0xAFFF,
        }
#endregion // EVENT

#region PROCESS
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateProcessW(
			string lpApplicationName,
			[In] string lpCommandLine,
			IntPtr procSecAttrs,
			IntPtr threadSecAttrs,
			bool bInheritHandles,
			eProcessCreationFlags dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			ref StartupInfo lpStartupInfo,
			ref ProcessInformation lpProcessInformation
		);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool TerminateProcess(IntPtr processHandle, uint exitCode);
					
		[DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr ShellExecute(IntPtr hwnd, string IpOperation, string IpFile, string IpParameters, string IpDirectory, int nShowCmd);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(eProcessAccessRights access, bool inherit, uint processId);

		[Flags]
		public enum eProcessAccessRights : uint
		{
			PROCESS_CREATE_PROCESS = 0x0080, //  Required to create a process.
			PROCESS_CREATE_THREAD = 0x0002, //  Required to create a thread.
			PROCESS_DUP_HANDLE = 0x0040, // Required to duplicate a handle using DuplicateHandle.
			PROCESS_QUERY_INFORMATION = 0x0400, //  Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
			PROCESS_QUERY_LIMITED_INFORMATION = 0x1000, //  Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP/2000:  This access right is not supported.
			PROCESS_SET_INFORMATION = 0x0200, //    Required to set certain information about a process, such as its priority class (see SetPriorityClass).
			PROCESS_SET_QUOTA = 0x0100, //  Required to set memory limits using SetProcessWorkingSetSize.
			PROCESS_SUSPEND_RESUME = 0x0800, // Required to suspend or resume a process.
			PROCESS_TERMINATE = 0x0001, //  Required to terminate a process using TerminateProcess.
			PROCESS_VM_OPERATION = 0x0008, //   Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
			PROCESS_VM_READ = 0x0010, //    Required to read memory in a process using ReadProcessMemory.
			PROCESS_VM_WRITE = 0x0020, //   Required to write to memory in a process using WriteProcessMemory.
			DELETE = 0x00010000, // Required to delete the object.
			READ_CONTROL = 0x00020000, //   Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
			SYNCHRONIZE = 0x00100000, //    The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
			WRITE_DAC = 0x00040000, //  Required to modify the DACL in the security descriptor for the object.
			WRITE_OWNER = 0x00080000, //    Required to change the owner in the security descriptor for the object.
			STANDARD_RIGHTS_REQUIRED = 0x000f0000,
			PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF //    All possible access rights for a process object.
		}
		
		[Flags]
		public enum eProcessCreationFlags : uint
		{
			NONE = 0,
			CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
			CREATE_DEFAULT_ERROR_MODE = 0x04000000,
			CREATE_NEW_CONSOLE = 0x00000010,
			CREATE_NEW_PROCESS_GROUP = 0x00000200,
			CREATE_NO_WINDOW = 0x08000000,
			CREATE_PROTECTED_PROCESS = 0x00040000,
			CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
			CREATE_SECURE_PROCESS = 0x00400000,
			CREATE_SEPARATE_WOW_VDM = 0x00000800,
			CREATE_SHARED_WOW_VDM = 0x00001000,
			CREATE_SUSPENDED = 0x00000004,
			CREATE_UNICODE_ENVIRONMENT = 0x00000400,
			DEBUG_ONLY_THIS_PROCESS = 0x00000002,
			DEBUG_PROCESS = 0x00000001,
			DETACHED_PROCESS = 0x00000008,
			EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
			INHERIT_PARENT_AFFINITY = 0x00010000
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct ProcessInformation
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public uint dwProcessId;
			public uint dwThreadId;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct StartupInfo
		{
			public uint cb;
			public IntPtr lpReserved;
			public IntPtr lpDesktop;
			public IntPtr lpTitle;
			public uint dwX;
			public uint dwY;
			public uint dwXSize;
			public uint dwYSize;
			public uint dwXCountChars;
			public uint dwYCountChars;
			public uint dwFillAttribute;
			public uint dwFlags;
			public ushort wShowWindow;
			public ushort cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}
#endregion // PROCESS

#region PINVOKE_DEVICE
		private static List<MyMonitor> _monitorList = new();
		private static List<MyDisplayInfo> _displayInfoList = new();

		public static List<MyDisplayInfo> DisplayInfoList => _displayInfoList;
		public static MyDisplayInfo MainDisplay => _displayInfoList[0];
		public static MyDisplayInfo CurrentDisplay => _displayInfoList[CurrentDisplayIndex];
		public static MyDisplayInfo VirtualDisplay = null;
		public static int CurrentDisplayIndex = 0;
		
		public class MyMonitor
		{
			public string Availability { get; set; }
			public string ScreenHeight { get; set; }
			public string ScreenWidth { get; set; }
			public WinRect MonitorArea { get; set; }
			public WinRect WorkArea { get; set; }
		}

		public class MyDisplayInfo
		{
			public int Index;
			public Vector2Int Min;
			public Vector2Int Max;
			public int Width;
			public int Height;

			public MyDisplayInfo(int index, MyMonitor monitor)
			{
				this.Index = index;
				this.Min = new Vector2Int(monitor.WorkArea.left, monitor.WorkArea.top);
				this.Max = new Vector2Int(monitor.WorkArea.left + Convert.ToInt32(monitor.ScreenWidth),
					monitor.WorkArea.top + Convert.ToInt32(monitor.ScreenHeight));
				this.Width = Convert.ToInt32(monitor.ScreenWidth);
				this.Height = Convert.ToInt32(monitor.ScreenHeight);
				
				C2VDebug.LogCategory("PlatformController", $"Device({index}) : {this.Min} {this.Max}");
			}

			public MyDisplayInfo(int left, int top, int right, int bottom)
			{
				this.Index = -1;
				this.Min = new Vector2Int(left, top);
				this.Max = new Vector2Int(right, bottom);
				this.Width = right - left;
				this.Height = bottom - top;
				
				C2VDebug.LogCategory("PlatformController", $"Virtual : {this.Min} {this.Max}");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[MonoPInvokeCallback(typeof(MonitorEnumDelegate))]
		public static void SetDisplayInfo()
		{
			// TODO : 중간에 모니터 세팅을 바꾼다던지 할 때 처리 필요
			int minX = 0, minY = 0, maxX = 0, maxY = 0;
			if (_monitorList.Count == 0)
			{
				MonitorEnumDelegate enumDelegate = EnumDelegate;
				EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, enumDelegate, IntPtr.Zero);
				for (int i = 0; i < _monitorList.Count; i++)
				{
					_displayInfoList.Add(new MyDisplayInfo(i, _monitorList[i]));

					if (minX > _monitorList[i].WorkArea.left) minX = _monitorList[i].WorkArea.left;
					if (minY > _monitorList[i].WorkArea.top) minY = _monitorList[i].WorkArea.top;
					if (maxX < _monitorList[i].WorkArea.left + Convert.ToInt32(_monitorList[i].ScreenWidth))
						maxX = _monitorList[i].WorkArea.left + Convert.ToInt32(_monitorList[i].ScreenWidth);
					if (maxY < _monitorList[i].WorkArea.top + Convert.ToInt32(_monitorList[i].ScreenHeight))
						maxY = _monitorList[i].WorkArea.top + Convert.ToInt32(_monitorList[i].ScreenHeight);
				}
				VirtualDisplay = new MyDisplayInfo(minX, minY, maxX, maxY);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hMonitor"></param>
		/// <param name="hdcMonitor"></param>
		/// <param name="lprcMonitor"></param>
		/// <param name="dwData"></param>
		/// <returns></returns>
		[MonoPInvokeCallback(typeof(MonitorEnumDelegate))]
		private static bool EnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref WinRect lprcMonitor, IntPtr dwData)
		{
			MONITORINFOEX mi = new MONITORINFOEX();
			mi.cbSize = (int)Marshal.SizeOf(mi);
			bool success = GetMonitorInfo(hMonitor, mi);
			if (success)
			{
				MyMonitor di = new MyMonitor();
				di.ScreenWidth = (mi.rcMonitor.right - mi.rcMonitor.left).ToString();
				di.ScreenHeight = (mi.rcMonitor.bottom - mi.rcMonitor.top).ToString();
				di.MonitorArea = mi.rcMonitor;
				di.WorkArea = mi.rcWork;
				di.Availability = mi.dwFlags.ToString();
				_monitorList.Add(di);
			}
			return true;
		}
		
#endregion // PINVOKE_DEVICE

#region PINVOKE_EVENT
		private static Action _minimizeStartAction;
		private static Action _minimizeEndAction;
		private static Action _moveSizeStartAction;
		private static Action _moveSizeEndAction;

		public static event Action MinimizeStartEvent
		{
			add
			{
				_minimizeStartAction -= value;
				_minimizeStartAction += value;
			}
			remove => _minimizeStartAction -= value;
		}
				
		public static event Action MinimizeEndEvent
		{
			add
			{
				_minimizeEndAction -= value;
				_minimizeEndAction += value;
			}
			remove => _minimizeEndAction -= value;
		}
		
		public static event Action MoveSizeStartEvent
		{
			add
			{
				_moveSizeStartAction -= value;
				_moveSizeStartAction += value;
			}
			remove => _moveSizeStartAction -= value;
		}
		
		public static event Action MoveSizeEndEvent
		{
			add
			{
				_moveSizeEndAction -= value;
				_moveSizeEndAction += value;
			}
			remove => _moveSizeEndAction -= value;
		}

		[MonoPInvokeCallback(typeof(WinEventProc))]
		public static void WindowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (hwnd == _hWnd)
			{
				C2VDebug.LogCategory("PlatformController", $"hwnd {hwnd} , EventType : {(WinEvent)eventType}");
				switch ((WinEvent)eventType)
				{
					case WinEvent.EVENT_SYSTEM_MINIMIZESTART:
						_minimizeStartAction?.Invoke();
						break;
					case WinEvent.EVENT_SYSTEM_MINIMIZEEND:
						_minimizeEndAction?.Invoke();
						break;
					case WinEvent.EVENT_SYSTEM_MOVESIZESTART:
						_moveSizeStartAction?.Invoke();
						break;
					case WinEvent.EVENT_SYSTEM_MOVESIZEEND:
						_moveSizeEndAction?.Invoke();
						break;
				}
			}
		}
#endregion // PINVOKE_EVENT
	}
}
