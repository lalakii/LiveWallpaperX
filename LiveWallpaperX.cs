// SPDX-License-Identifier: GPL-2.0-only
// Copyright (C) 2026 lalaki.
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

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
        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(string section, string key, string defaultValue, IntPtr str, uint size, string filePath);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const int VIDEO_HOTKEY_ID = 1;
        private const int SETUP_HOTKEY_ID = 2;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;
        private const long WS_POPUP = 0x80000000L;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const string APP_NAME = "LiveWallpaperX";
        private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new(-4);
        private static readonly Lazy<string> mLocation = new(() => System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly Lazy<string> mConfig = new(() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), $"{APP_NAME}.ini"));
        private static readonly Lazy<string> mPidFile = new(() => Path.Combine(Path.GetDirectoryName(mConfig.Value), $"{APP_NAME}.pid"));
        private static readonly Lazy<LiveWallpaperX> mWallpaperWindow = new(() => new LiveWallpaperX(ValueTypes.Wallpaper));
        private static readonly Lazy<Language.LanguagePack> mLanguagePack = new(() => Language.GetLanguagePack(mLangName));
        private static readonly Lazy<LiveWallpaperX> mSettingsWindow = new(() => new LiveWallpaperX(ValueTypes.Settings));
        private static readonly Lazy<ManualResetEvent> mResetEvent = new(() => new ManualResetEvent(false));
        private readonly Lazy<MediaElement> mMedia = new(() => new MediaElement()
        {
            IsMuted = true,
            IsHitTestVisible = false,
            LoadedBehavior = MediaState.Manual,
            Stretch = System.Windows.Media.Stretch.Fill,
            UnloadedBehavior = MediaState.Manual
        });
        private static IntPtr mWorker = IntPtr.Zero;
        private static FileStream? mPidLock;
        private static string? mLangName;
        private readonly byte mValueTypes;
        private bool mPlaying = true;
        private static class ValueTypes
        {
            public const byte Wallpaper = 0;
            public const byte Settings = 1;
            public const string Video = "video";
            public const string Pause = "pause";
            public const string Lang = "lang";
        }
        public LiveWallpaperX(byte valueTypes)
        {
            mValueTypes = valueTypes;
            var languagePack = mLanguagePack.Value;
            Icon = Icon.ExtractAssociatedIcon(mLocation.Value);
            switch (valueTypes)
            {
                case ValueTypes.Wallpaper:
                    FormBorderStyle = FormBorderStyle.None;
                    Bounds = Screen.PrimaryScreen.Bounds;
                    ShowInTaskbar = false;
                    Text = languagePack.Title;
                    Opacity = 0;
                    BackColor = Color.White;
                    AutoSize = false;
                    DoubleBuffered = true;
                    SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
                    WindowState = FormWindowState.Maximized;
                    var media = mMedia.Value;
                    media.MediaEnded += Media_Ended;
                    var grid = new Grid();
                    var children = grid.Children;
                    children.Add(media);
                    children.Add(new Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 70)),
                        IsHitTestVisible = false
                    });
                    Controls.Add(new ElementHost
                    {
                        Dock = DockStyle.Fill,
                        Child = grid
                    });
                    Shown += WallpaperForm_Shown;
                    break;
                case ValueTypes.Settings:
                    Text = languagePack.SetupTitle;
                    try
                    {
                        Text += $" | Ver: {FileVersionInfo.GetVersionInfo(mLocation.Value).ProductVersion}";
                    }
                    catch
                    {
                    }

                    AutoSize = false;
                    MaximizeBox = false;
                    TopMost = true;
                    StartPosition = FormStartPosition.CenterScreen;
                    Height = 320;
                    var menuStrip = new MenuStrip();
                    var menuItem = new ToolStripMenuItem(languagePack.Menu);
                    var chooseVideoItem = new ToolStripMenuItem(languagePack.SelectVideo);
                    var autoStartItem = new ToolStripMenuItem(languagePack.AutoRun)
                    {
                        CheckOnClick = true,
                        Checked = CheckAutoRun()
                    };
                    var exitItem = new ToolStripMenuItem(languagePack.ForceExit);
                    var resetItem = new ToolStripMenuItem(languagePack.Reset);
                    menuItem.DropDownItems.Add(chooseVideoItem);
                    menuItem.DropDownItems.Add(autoStartItem);
                    menuItem.DropDownItems.Add(new ToolStripSeparator());
                    menuItem.DropDownItems.Add(exitItem);
                    menuItem.DropDownItems.Add(new ToolStripSeparator());
                    menuItem.DropDownItems.Add(resetItem);
                    menuStrip.Items.Add(menuItem);
                    var linkBtn = new ToolStripButton() { Text = languagePack.Source };
                    menuStrip.Items.Add(linkBtn);
                    MainMenuStrip = menuStrip;
                    Controls.Add(menuStrip);
                    var help = new System.Windows.Forms.Label()
                    {
                        Text = languagePack.Help,
                        AutoSize = false,
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 12.5f),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    Controls.Add(help);
                    Width = TextRenderer.MeasureText(languagePack.Help, help.Font).Width * 5 / 4;
                    linkBtn.Click += LinkBtn_Click;
                    exitItem.Click += ExitItem_Click;
                    resetItem.Click += Reset_Click;
                    chooseVideoItem.Click += ChooseItem_Click;
                    autoStartItem.CheckedChanged += AutoStart_Changed;
                    break;
            }
        }

        [STAThread]
        public static void Main()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var pidFile = mPidFile.Value;
            try
            {
                mPidLock = new FileStream(pidFile, FileMode.Create, FileAccess.ReadWrite);
            }
            catch
            {
            }

            if (mPidLock == null)
            {
                try
                {
                    mPidLock = new FileStream(pidFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new BinaryReader(mPidLock);
                    Process.GetProcessById(reader.ReadInt32()).Kill();
                }
                catch
                {
                }

                return;
            }

            try
            {
                var bw = new BinaryWriter(mPidLock);
                bw.Write(Process.GetCurrentProcess().Id);
                bw.Flush();
            }
            catch
            {
            }

            mLangName = GetValue(ValueTypes.Lang);
            var path = GetValue(ValueTypes.Video);
            Uri? videoUrl = null;
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
                return;
            }

            ThreadPool.QueueUserWorkItem(BeforeCreateWorker, 0);
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

            if (bool.TryParse(GetValue(ValueTypes.Pause), out bool pause))
            {
                wallpaperWindow.mPlaying = !pause;
            }

            Application.Run(wallpaperWindow);
        }

        private void BeginWallpaperHandling(object obj)
        {
            if (mResetEvent.Value.WaitOne(20 * 1000))
            {
                Invoke(SetParent, Handle, mWorker);
            }
            else
            {
                MessageBox.Show("This system is not compatible.");
                Quit();
            }
        }

        private void WallpaperForm_Shown(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(BeginWallpaperHandling, sender);
            if (!RegisterHotKey(Handle, VIDEO_HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)Keys.P))
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

        private void Media_Ended(object sender, System.Windows.RoutedEventArgs e)
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

        private void ChooseItem_Click(object sender, EventArgs e)
        {
            using var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = $"{mLanguagePack.Value.VideoFiles}|*.mp4;*.avi;*.wmv"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (!SetValue(ValueTypes.Video, dlg.FileName))
            {
                MessageBox.Show("Failed to save configuration.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ReleasePid();
            Application.Restart();
            Quit();
        }

        private static void ReleasePid()
        {
            var pidLock = mPidLock;
            if (pidLock != null)
            {
                try
                {
                    pidLock.Dispose();
                }
                catch
                {
                }
            }
        }

        private static void Quit()
        {
            Environment.Exit(0);
            Environment.FailFast(string.Empty);
        }

        private void LinkBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start($"https://github.com/lalakii/{APP_NAME}");
            }
            catch
            {
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            ReleasePid();
            SetRun(false);
            SafeDelete(mConfig.Value);
            SafeDelete(mPidFile.Value);
            Quit();
        }

        private static void SafeDelete(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {

                }
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
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
                Quit();
            }
        }

        private void AutoStart_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem cbx)
            {
                SetRun(cbx.Checked);
            }
        }

        private static void BeforeCreateWorker(object _)
        {
            EnumWindows(EnumWindowsProcFunc, IntPtr.Zero);
        }

        private static void BeginCreateWorker(object hWndObj)
        {
            if (hWndObj is IntPtr hWnd)
            {
                int limit = 10;
                while (limit-- > 0)
                {
                    var hWorker = FindWindowEx(hWnd, IntPtr.Zero, "WorkerW", IntPtr.Zero);
                    if (hWorker != IntPtr.Zero)
                    {
                        mWorker = hWorker;
                        break;
                    }

                    SendMessageTimeout(hWnd, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out IntPtr r);
                    SendMessageTimeout(hWnd, 0x052C, new IntPtr(0xD), IntPtr.Zero, 0, 1000, out r);
                    SendMessageTimeout(hWnd, 0x052C, new IntPtr(0xD), new IntPtr(0x1), 0, 1000, out r);
                    Thread.Sleep(1000);
                }
            }

            mResetEvent.Value.Set();
        }

        private static bool EnumWindowsProcFunc(IntPtr hWnd, IntPtr lParam)
        {
            if (FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero) != IntPtr.Zero)
            {
                ThreadPool.QueueUserWorkItem(BeginCreateWorker, hWnd);
                return false;
            }

            return true;
        }

        private static bool SetValue(string key, string value)
        {
            return WritePrivateProfileString(APP_NAME, key, value, mConfig.Value);
        }

        private static string GetValue(string key)
        {
            string value = string.Empty;
            if (File.Exists(mConfig.Value))
            {
                uint size;
                var chars = new char[4096];
                var handle = GCHandle.Alloc(chars, GCHandleType.Pinned);
                if ((size = GetPrivateProfileString(APP_NAME, key, string.Empty, handle.AddrOfPinnedObject(), (uint)chars.Length, mConfig.Value)) > 0)
                {
                    value = new string(chars, 0, (int)size);
                }

                handle.Free();
            }

            return value;
        }

        private static void SetRun(bool auto)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RUN_KEY);
                if (auto)
                {
                    key.SetValue(APP_NAME, string.Format("\"{0}\"", mLocation.Value), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(APP_NAME);
                }
            }
            catch
            {
            }
        }

        private static bool CheckAutoRun()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY);
            var path = key.GetValue(APP_NAME);
            if (path is string appPath && Path.GetFullPath(appPath.Trim('\"')).Equals(mLocation.Value))
            {
                return true;
            }

            return false;
        }

        private void SettingsWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is LiveWallpaperX window)
            {
                window.Hide();
                e.Cancel = true;
            }
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
                var cp = base.CreateParams;
                if (mValueTypes == ValueTypes.Settings)
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
                    case VIDEO_HOTKEY_ID:
                        var media = mMedia.Value;
                        if (media != null)
                        {
                            SetValue(ValueTypes.Pause, mPlaying.ToString());
                            mPlaying = !mPlaying;
                            if (mPlaying)
                            {
                                media.Play();
                            }
                            else
                            {
                                media.Pause();
                            }
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
    }
}
