using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseTracker
{
    public partial class frmMain : Form
    {
        #region [ vars ]

        double Mileage { get; set; }
        TimeSpan TotalTime { get; set; }
        TimeSpan MovingTime { get; set; }
        string MileageText
        {
            get
            {
                if (Mileage < 100)
                    return string.Format("{0:0.00} cm", Mileage);
                else if (Mileage < 100000)
                    return string.Format("{0:0.00} m", Mileage/100);
                else
                    return string.Format("{0:0.00} km", Mileage / 100000);
            }
        }

        double x1;
        double y1;
        double Px2CmX { get; set; }
        double Px2CmY { get; set; }
        bool ForceClosing { get; set; }

        #endregion

        #region [ config ]

        Lazy<string> filepath = new Lazy<string>(() => {
            string startuppath = Application.StartupPath;
            string filename = ConfigurationManager.AppSettings["filename"];
            string fullpath = Path.Combine(startuppath, filename);

            return fullpath;
        });
        Lazy<int> timerPositionMS = new Lazy<int>(() => int.Parse(ConfigurationManager.AppSettings["timerPositionMS"]));
        Lazy<int> timerSaveMS = new Lazy<int>(() => int.Parse(ConfigurationManager.AppSettings["timerSaveMS"]));

        string FilePath { get { return filepath.Value; } }
        int TimerPositionMS { get { return timerPositionMS.Value; } }
        int TimerSaveMS { get { return timerSaveMS.Value; } }
        bool AutoStartup { get { return ConfigurationManager.AppSettings["autoStartup"] == "1"; } }

        #endregion

        public frmMain()
        {
            InitializeComponent();

            ShowInTaskbar = false;

            //auto add to startup
            try
            {
                string AppName = "MouseTracker.1.0";

                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (AutoStartup)
                    rk.SetValue(AppName, Application.ExecutablePath.ToString());
                else
                    rk.DeleteValue(AppName, false);
            }
            catch
            {
            }

            //initial values
            x1 = Cursor.Position.X;
            y1 = Cursor.Position.Y;

            Mileage = 0;
            TotalTime = new TimeSpan(1);
            MovingTime = new TimeSpan(1);

            using (var g = this.CreateGraphics())
            {
                Px2CmX = 2.54 / g.DpiX;
                Px2CmY = 2.54 / g.DpiY;
            }

            //load from file
            try
            {
                string[] s = File.ReadAllLines(FilePath);
                Mileage = double.Parse(s[0]);
                TotalTime = new TimeSpan((long)(double.Parse(s[1]) * 10000)); //ms to 100 ns
                MovingTime = new TimeSpan((long)(double.Parse(s[2]) * 10000)); //ms to 100 ns
            }
            catch
            {
            }

            //timers
            timerPosition.Interval = TimerPositionMS;
            timerPosition.Enabled = true;
            timerSave.Interval = TimerSaveMS;
            timerSave.Enabled = true;

            //fill labels
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            try
            {
                lblTotal.Text = MileageText;
                lblTotalTime.Text = TotalTime.ToString(@"dd\.hh\:mm\:ss");
                lblMovingTime.Text = MovingTime.ToString(@"dd\.hh\:mm\:ss");
                lblSpeed.Text = string.Format("{0:0.00 m/s}", Mileage / (100 * MovingTime.TotalSeconds));
            }
            catch
            {
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ForceClosing)
                return;
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void timerPosition_Tick(object sender, EventArgs e)
        {
            int x2 = Cursor.Position.X;
            int y2 = Cursor.Position.Y;
            double dx = (x2 - x1) * Px2CmX;
            double dy = (y2 - y1) * Px2CmY;
            Mileage += Math.Sqrt(dx * dx + dy * dy);
            TotalTime = TotalTime.Add(new TimeSpan(TimerPositionMS * 10000)); //ms to 100 ns
            if (x1 != x2 || y1 != y2)
                MovingTime = MovingTime.Add(new TimeSpan(TimerPositionMS * 10000)); //ms to 100 ns
            x1 = x2;
            y1 = y2;

            sysTray.Text = MileageText;
            UpdateLabels();
        }

        private void timerSave_Tick(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(FilePath, string.Format("{0}\n{1}\n{2}", Mileage, TotalTime.TotalMilliseconds, MovingTime.TotalMilliseconds));
            }
            catch
            {
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ForceClosing = true;
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateLabels();
            Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void sysTray_DoubleClick(object sender, EventArgs e)
        {
            UpdateLabels();
            Show();
        }
    }
}
