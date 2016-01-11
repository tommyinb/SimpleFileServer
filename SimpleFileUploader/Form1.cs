using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SimpleFileUploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void localFileTextBox_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(localFileTextBox.Text))
            {
                uploadButton.Enabled = true;

                var fileNamePattern = @"/([^/]+)\.([^/]+)$";
                if (Regex.IsMatch(serverAddressTextBox.Text, fileNamePattern))
                {
                    var fileName = Path.GetFileName(localFileTextBox.Text);
                    serverAddressTextBox.Text = Regex.Replace(serverAddressTextBox.Text, fileNamePattern, "/" + fileName);
                }
            }
            else
            {
                uploadButton.Enabled = false;
            }
        }

        private OpenFileDialog fileDialog = new OpenFileDialog { Filter = "All File|*.*" };
        private void localFileButton_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            localFileTextBox.Text = fileDialog.FileName;
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            var bytes = File.ReadAllBytes(localFileTextBox.Text);

            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.UploadData(serverAddressTextBox.Text, bytes);

                    MessageBox.Show("Upload Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Upload Error\r\n" + ex.ToString());
                }
            }
        }
    }
}
