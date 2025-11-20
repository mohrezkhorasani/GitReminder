using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms; 

internal class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONWARNING = 0x00000030;

    private static HashSet<int> vsProcessIds = new HashSet<int>();
    private static NotifyIcon trayIcon;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        trayIcon = new NotifyIcon
        {
            Icon = CreateGreenCircleIcon(),  
            Text = "Git Reminder Active - هرگز فراموش نکن push کنی!",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };

        trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Exit());
        trayIcon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) trayIcon.ShowBalloonTip(1000, "گیت ریمایندر", "در حال نظارت بر Visual Studio...", ToolTipIcon.Info); };

        // حلقه اصلی نظارت
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                var current = GetVsProcessIds();    

                if (vsProcessIds.Count > 0 && current.Count == 0)
                {
                    Thread.Sleep(2000);
                    if (GetVsProcessIds().Count == 0)
                    {
                        MessageBoxW(IntPtr.Zero,
                            "پوش کردی رو گیت؟ اگه نکردی یادت نره 😄",
                            "⛔⛔❌❌یادآوری گیت❌❌⛔⛔",
                            MB_OK | MB_ICONWARNING);
                    }
                }

                vsProcessIds = current;
                Thread.Sleep(1000);
            }
        }).Start();

        Application.Run();
    }

    private static Icon CreateGreenCircleIcon()
    {
        Bitmap bmp = new Bitmap(32, 32);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.FillEllipse(Brushes.LimeGreen, 2, 2, 28, 28);
            g.DrawEllipse(Pens.DarkGreen, 2, 2, 28, 28);

            using (Font f = new Font("Segoe UI", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString("G", f, Brushes.White, 16, 16, sf);
            }
        }
        return Icon.FromHandle(bmp.GetHicon());
    }
    // gereftane VS
    private static HashSet<int> GetVsProcessIds()
    {
        var ids = new HashSet<int>();
        string[] names = { "devenv", "Code" };

        foreach (var name in names)
        {
            try
            {
                foreach (var p in Process.GetProcessesByName(name))
                    ids.Add(p.Id);
            }
            catch { }
        }
        return ids;
    }
}