#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Managers
{
    public class RuntimeMonitorSwitcher : MonoBehaviour
    {
        // ===== Win32 interop =====
        private delegate bool MonitorEnumProc(IntPtr _hMonitor, IntPtr _hdc, ref Rect _lprcMonitor, IntPtr _dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice; // \\.\DISPLAY1 etc
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString; // Human name
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
        }

        [DllImport("user32.dll")] static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
        [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();

        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;

        public class MonitorInfo
        {
            public string DevicePath;     // \\.\DISPLAY1
            public string FriendlyName;   // e.g., DELL U2720Q
            public RectInt PixelRect;     // monitor bounds in pixels
        }

        public List<MonitorInfo> Monitors { get; private set; } = new();

        [Tooltip("Enable Ctrl+Alt+Arrow to cycle monitors at runtime.")]
        public bool enableHotkeys = true;

        [Tooltip("Force borderless FullscreenWindow when switching (recommended).")]
        public bool forceBorderlessOnSwitch = true;

        int currentIndex = -1;

        void Awake()
        {
            RefreshMonitorList();
            // Pick the monitor that currently contains the window (best guess = primary / 0)
            currentIndex = Mathf.Clamp(GetPrimaryMonitorIndex(), 0, Monitors.Count - 1);
        }

        void Update()
        {
            if (!enableHotkeys) return;

            bool next = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow);
            bool prev = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftArrow);

            if (next && Monitors.Count > 0)
            {
                currentIndex = (currentIndex + 1) % Monitors.Count;
                MoveToMonitor(currentIndex);
            }
            else if (prev && Monitors.Count > 0)
            {
                currentIndex = (currentIndex - 1 + Monitors.Count) % Monitors.Count;
                MoveToMonitor(currentIndex);
            }
        }

        public void RefreshMonitorList()
        {
            Monitors.Clear();

            EnumDisplayMonitors(
                IntPtr.Zero, IntPtr.Zero,
                (IntPtr _hMon, IntPtr _hdc, ref Rect _r, IntPtr _data) =>
                {
                    var mi = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };
                    if (!GetMonitorInfo(_hMon, ref mi)) return true;

                    string friendly = mi.szDevice;
                    var dd = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
                    if (EnumDisplayDevices(mi.szDevice, 0, ref dd, 0) && !string.IsNullOrWhiteSpace(dd.DeviceString))
                        friendly = dd.DeviceString;

                    var rect = new RectInt(
                        mi.rcMonitor.left,
                        mi.rcMonitor.top,
                        mi.rcMonitor.right - mi.rcMonitor.left,
                        mi.rcMonitor.bottom - mi.rcMonitor.top
                    );

                    Monitors.Add(new MonitorInfo
                    {
                        DevicePath = mi.szDevice,
                        FriendlyName = friendly,
                        PixelRect = rect
                    });

                    return true;
                },
                IntPtr.Zero
            );
        }

        public void MoveToMonitor(int index)
        {
            if (Monitors.Count == 0)
            {
                Debug.LogWarning("No monitors detected.");
                return;
            }
            if (index < 0 || index >= Monitors.Count)
            {
                Debug.LogWarning($"Monitor index {index} out of range (0..{Monitors.Count - 1}).");
                return;
            }

            var m = Monitors[index];

            // Borderless is reliable for moving across adapters.
            if (forceBorderlessOnSwitch)
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            // Match monitor resolution; avoids scaling blur in borderless.
            if (Screen.currentResolution.width != m.PixelRect.width || Screen.currentResolution.height != m.PixelRect.height
                                                                    || Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                Screen.SetResolution(m.PixelRect.width, m.PixelRect.height, Screen.fullScreenMode);
            }

            var hwnd = GetPlayerWindowHandle();
            if (hwnd == IntPtr.Zero)
            {
                Debug.LogWarning("Could not get Unity player window handle.");
                return;
            }

            SetWindowPos(hwnd, IntPtr.Zero, m.PixelRect.x, m.PixelRect.y, m.PixelRect.width, m.PixelRect.height,
                SWP_NOZORDER | SWP_NOACTIVATE);

            currentIndex = index;
            Debug.Log($"Moved to monitor {index} ({m.FriendlyName}) {m.PixelRect}");
        }

        public int GetPrimaryMonitorIndex()
        {
            // DISPLAY_DEVICE.StateFlags bit 0x4 indicates primary; map it back to our list.
            const int DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4;

            for (int i = 0; i < Monitors.Count; i++)
            {
                var dd = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
                if (EnumDisplayDevices(Monitors[i].DevicePath, 0, ref dd, 0))
                {
                    if ((dd.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0)
                        return i;
                }
            }
            return 0; // fallback
        }

        // Tries to get the player window handle robustly
        private static IntPtr GetPlayerWindowHandle()
        {
            // In a built player, GetActiveWindow usually returns our HWND.
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero) hwnd = GetForegroundWindow();
            return hwnd;
        }
    }
}
#endif
