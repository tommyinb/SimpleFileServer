using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace SimpleFileServer
{
    public partial class Form1 : Form
    {
        private BasicServer server = null;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            var arguments = Environment.GetCommandLineArgs();
            port = arguments.Length > 1 ? int.Parse(arguments[1]) : defaultPort;
            directory = arguments.Length > 2 ? arguments[2] : ".";

            notifyIcon.Text += ": " + port;

            try
            {
                server = new BasicServer(port, directory);
            }
            catch (Exception ex)
            {
                countTimeLabel.Text = "Error";
                countTimeLabel.ForeColor = Color.Red;

                if (ex is HttpListenerException && ((HttpListenerException)ex).NativeErrorCode == 5)
                {
                    MessageBox.Show("Require Administrator Right\r\n" + ex.ToString());
                }
                else
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            stopwatch = Stopwatch.StartNew();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (server != null)
            {
                server.Dispose();
            }
        }

        private Stopwatch stopwatch = null;
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            if (server != null)
            {
                countTimeLabel.Text = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            }

            if (stopwatch != null)
            {
                currTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (server != null)
            {
                switch (WindowState)
                {
                    case FormWindowState.Normal:
                    case FormWindowState.Maximized:
                        ShowInTaskbar = true;
                        notifyIcon.Visible = false;
                        break;

                    case FormWindowState.Minimized:
                    default:
                        ShowInTaskbar = false;
                        notifyIcon.Visible = true;
                        break;
                }
            }
        }
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private int port;
        private const int defaultPort = 8892;
        private string directory;
        private void countTimeLabel_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                Process.Start("http://localhost:" + port + "/");

                var directoryPath = Path.GetFullPath(directory);
                Process.Start(directoryPath);
            }
        }
    }
}
