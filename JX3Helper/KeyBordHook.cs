using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JX3Helper
{
    public class KeyBordHook
    {
        private double _ManyTimeout = 200.0;
        private Dictionary<Keys, KeyPressInfo> ditKeyPressInfo = new Dictionary<Keys, KeyPressInfo>();
        private static IntPtr hKeyboardHook = IntPtr.Zero;
        private bool IsDownAlt;
        private bool IsDownControl;
        private bool IsDownShift;
        public bool IsHooked;
        private HookProc KeyboardHookProcedure;
        private const int KEYEVENTF_KEYDOWN = 1;
        private const int KEYEVENTF_KEYUP = 2;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 260;
        private const int WM_SYSKEYUP = 0x105;

        public event KeyEventHandler OnKeyDownEvent;

        public event KeyManyEventHandler OnKeyManyClick;

        public event KeyPressEventHandler OnKeyPressEvent;

        public event KeyEventHandler OnKeyUpEvent;

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);
        ~KeyBordHook()
        {
            this.Stop();
        }

        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] pbKeyState);
        private KeyEventArgs getKeyEventArg(Keys key)
        {
            if (this.IsDownControl)
            {
                key = Keys.Control | key;
            }
            if (this.IsDownShift)
            {
                key = Keys.Shift | key;
            }
            if (this.IsDownAlt)
            {
                key = Keys.Alt | key;
            }
            return new KeyEventArgs(key);
        }

        private KeyPressInfo GetKeyPressInfo(Keys key, bool NullToDefault)
        {
            KeyPressInfo info = null;
            if (this.ditKeyPressInfo.ContainsKey(key))
            {
                return this.ditKeyPressInfo[key];
            }
            if (NullToDefault)
            {
                info = new KeyPressInfo();
                this.ditKeyPressInfo.Add(key, info);
            }
            return info;
        }

        [DllImport("kernel32.dll")]
        private static extern int GetModuleHandle(string lpModuleName);
        public static bool IsAltKeys(Keys key)
        {
            switch (key)
            {
                case Keys.LMenu:
                case Keys.RMenu:
                    return true;
            }
            return false;
        }

        public static bool IsCtrlKeys(Keys key)
        {
            switch (key)
            {
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return true;
            }
            return false;
        }

        public bool IsPressHold(Keys key)
        {
            if (this.ditKeyPressInfo.ContainsKey(key))
            {
                return this.ditKeyPressInfo[key].PressHoldState;
            }
            return true;
        }

        public static bool IsShiftKeys(Keys key)
        {
            switch (key)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return true;
            }
            return false;
        }

        [DllImport("User32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if ((nCode < 0) || (((this.OnKeyDownEvent == null) && (this.OnKeyUpEvent == null)) && (this.OnKeyPressEvent == null)))
            {
                goto Label_0219;
            }
            KeyboardHookStruct struct2 = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            Keys vkCode = (Keys)struct2.vkCode;
            if ((wParam != 0x100) && (wParam != 260))
            {
                goto Label_0143;
            }
            if (IsCtrlKeys(vkCode))
            {
                this.IsDownControl = true;
                vkCode = Keys.Control;
            }
            else if (IsAltKeys(vkCode))
            {
                this.IsDownAlt = true;
                vkCode = Keys.Alt;
            }
            else if (IsShiftKeys(vkCode))
            {
                this.IsDownShift = true;
                vkCode = Keys.Shift;
            }
            if (this.OnKeyDownEvent != null)
            {
                this.OnKeyDownEvent(this, this.getKeyEventArg(vkCode));
            }
            KeyPressInfo keyPressInfo = this.GetKeyPressInfo(vkCode, false);
            if (keyPressInfo == null)
            {
                keyPressInfo = new KeyPressInfo();
                this.ditKeyPressInfo.Add(vkCode, keyPressInfo);
            }
            else
            {
                if (this.OnKeyManyClick != null)
                {
                    TimeSpan span = (TimeSpan)(DateTime.Now - keyPressInfo.LastPressTime);
                    if (span.TotalMilliseconds <= this.ManyTimeout)
                    {
                        if (keyPressInfo.PressHoldState)
                        {
                            keyPressInfo.KeyPressTimes++;
                            this.OnKeyManyClick(this, new KeyManyEventArgs(vkCode, keyPressInfo.KeyPressTimes));
                        }
                        goto Label_013C;
                    }
                }
                keyPressInfo.KeyPressTimes = 1;
                keyPressInfo.LastPressTime = DateTime.Now;
            }
        Label_013C:
            keyPressInfo.PressHoldState = false;
        Label_0143:
            if ((wParam == 0x101) || (wParam == 0x105))
            {
                if (IsCtrlKeys(vkCode))
                {
                    this.IsDownControl = false;
                    vkCode = Keys.Control;
                }
                else if (IsAltKeys(vkCode))
                {
                    this.IsDownAlt = false;
                    vkCode = Keys.Alt;
                }
                else if (IsShiftKeys(vkCode))
                {
                    this.IsDownShift = false;
                    vkCode = Keys.Shift;
                }
                if (this.OnKeyUpEvent != null)
                {
                    this.OnKeyUpEvent(this, this.getKeyEventArg(vkCode));
                }
                this.SetPressHold(vkCode, true);
            }
            if ((this.OnKeyPressEvent != null) && (wParam == 0x100))
            {
                byte[] pbKeyState = new byte[0x100];
                GetKeyboardState(pbKeyState);
                byte[] lpwTransKey = new byte[2];
                if (ToAscii(struct2.vkCode, struct2.scanCode, pbKeyState, lpwTransKey, struct2.flags) == 1)
                {
                    KeyPressEventArgs e = new KeyPressEventArgs((char)lpwTransKey[0]);
                    this.OnKeyPressEvent(this, e);
                }
            }
        Label_0219:
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        public static void KeyDown(Keys key)
        {
            keybd_event((byte)key, 0, 1, 0);
        }

        public static void KeyUp(Keys key)
        {
            keybd_event((byte)key, 0, 2, 0);
        }

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        private void SetPressHold(Keys key, bool val)
        {
            if (this.ditKeyPressInfo.ContainsKey(key))
            {
                this.ditKeyPressInfo[key].PressHoldState = val;
            }
            else
            {
                KeyPressInfo info = new KeyPressInfo
                {
                    PressHoldState = val
                };
                this.ditKeyPressInfo.Add(key, info);
            }
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        public bool Start()
        {
            bool flag;
            try
            {
                this.Stop();
                using (Process process = Process.GetCurrentProcess())
                {
                    using (ProcessModule module = process.MainModule)
                    {
                        this.KeyboardHookProcedure = new HookProc(this.KeyboardHookProc);
                        hKeyboardHook = SetWindowsHookEx(13, this.KeyboardHookProcedure, (IntPtr)GetModuleHandle(module.ModuleName), 0);
                    }
                    if (hKeyboardHook.ToInt32() != 0)
                    {
                        this.IsHooked = true;
                        return true;
                    }
                    flag = false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                flag = false;
            }
            return flag;
        }

        public bool Stop()
        {
            this.IsDownAlt = this.IsDownControl = this.IsDownShift = false;
            return (!this.IsHooked || UnhookWindowsHookEx(hKeyboardHook));
        }

        [DllImport("user32.dll")]
        private static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private static extern bool UnhookWindowsHookEx(IntPtr idHook);

        public double ManyTimeout
        {
            get
            {
                return this._ManyTimeout;
            }
            set
            {
                this._ManyTimeout = value;
            }
        }

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public class KeyPressInfo
        {
            public KeyPressInfo()
            {
                this.KeyPressTimes = 1;
                this.LastPressTime = DateTime.Now;
                this.PressHoldState = false;
            }

            public int KeyPressTimes { get; set; }

            public DateTime LastPressTime { get; set; }

            public bool PressHoldState { get; set; }
        }
    }
}
