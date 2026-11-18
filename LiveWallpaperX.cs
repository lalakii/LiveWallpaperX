// SPDX-License-Identifier: GPL-2.0-only
// Copyright (C) 2026 lalaki.
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace LiveWallpaperX.Project
{
    [System.ComponentModel.DesignerCategory("")]
    public class LiveWallpaperX : Form
    {
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll", EntryPoint = "FindWindowExW", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string className, IntPtr windowName);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder str, uint size, string filePath);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const int HOTKEY_ID = 1;
        private const int SETUP_HOTKEY_ID = 2;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint GW_HWNDNEXT‌ = 2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const long WS_POPUP = 0x80000000L;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "LiveWallpaperX";
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);
        private static readonly Lazy<string> mLocation = new Lazy<string>(() => Assembly.GetExecutingAssembly().Location);
        private static readonly Lazy<string> mConfig = new Lazy<string>(() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), $"{AppName}.ini"));
        private static readonly Lazy<LiveWallpaperX> mWallpaperWindow = new Lazy<LiveWallpaperX>(() => new LiveWallpaperX(WindowType.Wallpaper));
        private static readonly Lazy<Timer> mTimer = new Lazy<Timer>(() => new Timer() { Interval = 3000 });
        private static readonly Lazy<Language.LanguagePack> mLanguagePack = new Lazy<Language.LanguagePack>(() => Language.GetLanguagePack(mLangName));
        private static readonly Lazy<LiveWallpaperX> mSettingsWindow = new Lazy<LiveWallpaperX>(() => new LiveWallpaperX(WindowType.Settings));
        private readonly Lazy<MediaElement> mMedia = new Lazy<MediaElement>(() => new MediaElement()
        {
            IsMuted = true,
            IsHitTestVisible = false,
            LoadedBehavior = MediaState.Manual,
            Stretch = Stretch.Fill,
            UnloadedBehavior = MediaState.Manual
        });
        private static IntPtr mDefview = IntPtr.Zero;
        private static IntPtr mParent = IntPtr.Zero;
        private static FileStream mPidLock;
        private readonly WindowType mType;
        private bool mPlaying = true;
        private static string mLangName = null;
        public enum WindowType : byte
        {
            Wallpaper, Settings
        }
        public LiveWallpaperX(WindowType type)
        {
            mType = type;
            var languagePack = mLanguagePack.Value;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(mLocation.Value);
            switch (type)
            {
                case WindowType.Wallpaper:
                    FormBorderStyle = FormBorderStyle.None;
                    ShowInTaskbar = false;
                    Text = languagePack.Title;
                    Opacity = 0;
                    BackColor = System.Drawing.Color.White;
                    AutoSize = false;
                    DoubleBuffered = true;
                    Bounds = Screen.PrimaryScreen.Bounds;
                    SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
                    WindowState = FormWindowState.Minimized;
                    var media = mMedia.Value;
                    media.MediaEnded += Media_MediaEnded;
                    var grid = new Grid();
                    var children = grid.Children;
                    children.Add(media);
                    children.Add(new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 70)),
                        IsHitTestVisible = false
                    });
                    Controls.Add(new ElementHost
                    {
                        Dock = DockStyle.Fill,
                        Child = grid
                    });
                    SetParent(Handle, mParent);
                    Shown += WallpaperForm_Shown;
                    break;
                case WindowType.Settings:
                    Text = languagePack.SetupTitle;
                    AutoSize = true;
                    StartPosition = FormStartPosition.CenterScreen;
                    MaximizeBox = false;
                    TopMost = true;
                    Height = 320;
                    var menuStrip = new MenuStrip();
                    var menuItem = new ToolStripMenuItem(languagePack.Menu);
                    var selectVideoItem = new ToolStripMenuItem(languagePack.SelectVideo);
                    var autoStartItem = new ToolStripMenuItem(languagePack.AutoRun)
                    {
                        CheckOnClick = true,
                        Checked = CheckAutoRun()
                    };
                    var exitItem = new ToolStripMenuItem(languagePack.ForceExit);
                    menuItem.DropDownItems.Add(selectVideoItem);
                    menuItem.DropDownItems.Add(autoStartItem);
                    menuItem.DropDownItems.Add(new ToolStripSeparator());
                    menuItem.DropDownItems.Add(exitItem);
                    menuStrip.Items.Add(menuItem);
                    var link = new ToolStripButton() { Text = languagePack.Source };
                    menuStrip.Items.Add(link);
                    MainMenuStrip = menuStrip;
                    Controls.Add(menuStrip);
                    var help = new System.Windows.Forms.Label()
                    {
                        Text = languagePack.Help,
                        AutoSize = false,
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("Segoe UI", 12.5f),
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                    };
                    Controls.Add(help);
                    Width = TextRenderer.MeasureText(languagePack.Help, help.Font).Width * 5 / 4;
                    selectVideoItem.Click += Btn_Click;
                    exitItem.Click += ExitBtn_Click;
                    autoStartItem.CheckedChanged += AutoCbx_Checked;
                    link.Click += Link_Click;
                    break;
            }
        }

        private void WallpaperForm_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            var timer = mTimer.Value;
            timer.Tick += Timer_Tick;
            timer.Start();
            Timer_Tick(sender, e);
            if (!RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)Keys.P))
            {
                SetValue("RegisterHotKey Ctrl + Alt + P", "failed");
            }

            if (!RegisterHotKey(Handle, SETUP_HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)Keys.O))
            {
                SetValue("RegisterHotKey Ctrl + Alt + O", "failed");
            }

            if (!mPlaying)
            {
                mMedia.Value.Pause();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (GetWindow(mDefview, GW_HWNDNEXT‌) != Handle)
            {
                SetWindowPos(Handle, mDefview, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }

        private void Media_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            var media = mMedia.Value;
            try
            {
                media.Position = TimeSpan.Zero;
                media.Play();
            }
            catch
            {
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            using (var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = $"{mLanguagePack.Value.VideoFiles}|*.mp4;*.avi;*.wmv"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (!SetValue("video", dlg.FileName))
                {
                    MessageBox.Show("Failed to save configuration.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (mPidLock != null)
                {
                    try
                    {
                        mPidLock.Dispose();
                    }
                    catch
                    {
                    }
                }

                Application.Restart();
                Environment.Exit(0);
            }
        }

        private void Link_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start($"https://github.com/lalakii/{AppName}");
            }
            catch
            {
            }
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Hide();
            }
            catch
            {
            }
            finally
            {
                Environment.Exit(0);
                Environment.FailFast(string.Empty);
            }
        }

        private void AutoCbx_Checked(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem cbx)
            {
                SetRun(cbx.Checked);
            }
        }


        [STAThread]
        public static int Main()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var configIni = mConfig.Value;
            var pidFile = Path.Combine(Path.GetDirectoryName(configIni), $"{AppName}.pid");
            try
            {
                mPidLock = new FileStream(pidFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            }
            catch
            {
            }

            if (mPidLock == null)
            {
                int pid = -1;
                try
                {
                    mPidLock = new FileStream(pidFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (var reader = new StreamReader(mPidLock))
                    {
                        if (int.TryParse(reader.ReadLine(), out pid))
                        {
                            var proc = Process.GetProcessById(pid);
                            proc?.Kill();
                        }
                    }

                    return 0;
                }
                catch
                {
                }

                return -pid;
            }

            try
            {
                var sw = new StreamWriter(mPidLock);
                sw.WriteLine(Process.GetCurrentProcess().Id.ToString());
                sw.Flush();
            }
            catch
            {
            }

            mLangName = GetValue("lang");
            Uri videoUrl = null;
            var path = GetValue("video");
            if (File.Exists(path))
            {
                try
                {
                    videoUrl = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                }
                catch
                {
                }
            }

            if (videoUrl == null)
            {
                Application.Run(mSettingsWindow.Value);
                return 0;
            }

            EnumWindows(EnumWindowsProcFunc, IntPtr.Zero);
            if (mDefview == IntPtr.Zero)
            {
                return -1;
            }

            var hWorker = FindWindowEx(mParent, IntPtr.Zero, "WorkerW", IntPtr.Zero);
            if (hWorker == IntPtr.Zero)
            {
                SendMessageTimeout(mParent, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out IntPtr r);
                SendMessageTimeout(mParent, 0x052C, new IntPtr(0xD), IntPtr.Zero, 0, 1000, out r);
                SendMessageTimeout(mParent, 0x052C, new IntPtr(0xD), new IntPtr(0x1), 0, 1000, out r);
            }

            var wallpaperWindow = mWallpaperWindow.Value;
            var media = wallpaperWindow.mMedia.Value;
            try
            {
                media.Source = videoUrl;
                media.Play();
            }
            catch
            {
            }

            if (GetValue("pause").IndexOf("true", StringComparison.OrdinalIgnoreCase) > -1)
            {
                wallpaperWindow.mPlaying = false;
            }

            Application.Run(wallpaperWindow);
            return 0;
        }

        private static bool EnumWindowsProcFunc(IntPtr hWnd, IntPtr lParam)
        {
            IntPtr hDefview = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            if (hDefview != IntPtr.Zero)
            {
                mDefview = hDefview;
                mParent = hWnd;
                return false;
            }

            return true;
        }

        private static bool SetValue(string key, string value)
        {
            return WritePrivateProfileString(string.Empty, key, value, mConfig.Value);
        }

        private static string GetValue(string key)
        {
            if (File.Exists(mConfig.Value))
            {
                var buffer = new StringBuilder(4096);
                if (GetPrivateProfileString(string.Empty, key, string.Empty, buffer, (uint)buffer.Capacity, mConfig.Value) > 0)
                {
                    return buffer.ToString();
                }
            }

            return string.Empty;
        }

        private static void SetRun(bool auto)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (auto)
                {
                    key.SetValue(AppName, string.Format("\"{0}\"", mLocation.Value), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(AppName);
                }
            }
        }

        private static bool CheckAutoRun()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey))
            {
                var path = key.GetValue(AppName);
                if (path is string appPath)
                {
                    if (Path.GetFullPath(appPath.Trim('\"')).Equals(mLocation.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (mType == WindowType.Settings)
                {
                    return cp;
                }

                cp.ExStyle |= (int)WS_EX_LAYERED;
                cp.ExStyle |= (int)WS_EX_TRANSPARENT;
                cp.Style &= (int)~(uint)WS_POPUP;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case HOTKEY_ID:
                        var media = mMedia.Value;
                        if (media != null)
                        {
                            mPlaying = !mPlaying;
                            if (mPlaying)
                            {
                                media.Play();
                            }
                            else
                            {
                                media.Pause();
                            }

                            SetValue("pause", (!mPlaying).ToString());
                        }

                        break;
                    case SETUP_HOTKEY_ID:
                        var settingsWindow = mSettingsWindow.Value;
                        settingsWindow.FormClosing -= SettingsWindow_FormClosing;
                        settingsWindow.FormClosing += SettingsWindow_FormClosing;
                        if (settingsWindow.Visible)
                        {
                            settingsWindow.Hide();
                        }
                        else
                        {
                            settingsWindow.Show();
                        }

                        break;
                }
            }

            base.WndProc(ref m);
        }

        private void SettingsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is LiveWallpaperX window)
            {
                window.Hide();
                e.Cancel = true;
            }
        }
    }
}