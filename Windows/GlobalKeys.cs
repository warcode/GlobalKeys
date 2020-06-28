using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace warcode.GlobalKeys.Windows
{
	public delegate System.IntPtr WndProcDelegate(System.IntPtr hWnd, uint msg, System.IntPtr wParam, System.IntPtr lParam);

	public class GlobalKeys : MonoBehaviour
	{

		private static int WM_HOTKEY = 0x0312;

		//public System.IntPtr interactionWindow;
		private System.IntPtr _foregroundWindowPtr;
		private WndProcDelegate _wndProcDelegate;
		private System.IntPtr _newWndProcPtr;
		private System.IntPtr _oldWndProcPtr;

		private int _currentId = 0;
		private Dictionary<Vector2Int, GlobalKeyEvent> _events = new Dictionary<Vector2Int, GlobalKeyEvent>();
		private List<KeyRegistration> _keyRegistrations = new List<KeyRegistration>();
		
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern System.IntPtr GetForegroundWindow();
		
		[DllImport("user32.dll")]
		static extern System.IntPtr SetWindowLongPtr(System.IntPtr hWnd, int nIndex, System.IntPtr dwNewLong);
		
		[DllImport("user32.dll")]
		static extern System.IntPtr CallWindowProc(System.IntPtr lpPrevWndFunc, System.IntPtr hWnd, uint Msg, System.IntPtr wParam, System.IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(System.IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(System.IntPtr hWnd, int id);

		// Use this for initialization
		void Start () {
			Debug.Log ( "Installing global keyboard Hook" );
			_foregroundWindowPtr = GetForegroundWindow();
			_wndProcDelegate = new WndProcDelegate(wndProc);
			_newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
			_oldWndProcPtr = SetWindowLongPtr(_foregroundWindowPtr, -4, _newWndProcPtr);
		}
		
		// Update is called once per frame
		void Update () {
			foreach (var registration in _keyRegistrations.Where(x => !x.Registered))
			{
        registration.Registered = InternalRegister(registration.Modifier, registration.Key, registration.Listener);
			}
			_keyRegistrations.RemoveAll(x => x.Registered);
		}

		void OnDisable ()
		{
			_events.Clear();

			Debug.Log ( "Unregistering all global keys" );
			for (int i = 0; i <= _currentId; i++)
			{
				UnregisterHotKey(_foregroundWindowPtr, i);
				Debug.Log(i);
			}
			Debug.Log ( "Uninstalling global keyboard Hook" );
			SetWindowLongPtr(_foregroundWindowPtr, -4, _oldWndProcPtr);
			_foregroundWindowPtr = System.IntPtr.Zero;
			_oldWndProcPtr = System.IntPtr.Zero;
			_newWndProcPtr = System.IntPtr.Zero;
			_wndProcDelegate = null;
		}

		public bool Register(KeyModifier modifier, Keys key, UnityAction<KeyModifier, Keys> listener)
		{
			_keyRegistrations.Add(new KeyRegistration(modifier, key, listener));
			return true;
		}

		private bool InternalRegister(KeyModifier modifier, Keys key, UnityAction<KeyModifier, Keys> listener)
		{
			var registered = RegisterHotKey(_foregroundWindowPtr, _currentId, (uint)modifier, (uint)key);
			Debug.Log(registered);
			if (registered)
			{
				var eventKey = new Vector2Int((int)modifier, (int)key);
				if(!_events.ContainsKey(eventKey))
				{
					_events.Add(eventKey, new GlobalKeyEvent());
				}
				_events[eventKey].AddListener(listener);
				_currentId++;
			}

			return registered;
		}

		System.IntPtr wndProc(System.IntPtr hWnd, uint msg, System.IntPtr wParam, System.IntPtr lParam)
		{
			if (msg == WM_HOTKEY)
			{
				Keys key = (Keys)(((int)lParam >> 16) & 0xFFFF);
				KeyModifier modifier = (KeyModifier)((int)lParam & 0xFFFF);
				//int id = wParam.ToInt32();

				var eventKey = new Vector2Int((int)modifier, (int)key);
				_events[eventKey].Invoke(modifier, key);
			}

			return CallWindowProc(_oldWndProcPtr, hWnd, msg, wParam, lParam);
		}
	}
}