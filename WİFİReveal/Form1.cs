using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WİFİReveal
{
    public partial class Form1 : Form
    {
        private const int initialFormWidth = 405;  // Formun başlangıç genişliği
        private const int expandedFormWidth = 609; // QR kodu göstermek için genişletilmiş form genişliği
        public Form1()
        {
            InitializeComponent();
            LoadWifiNames();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Width = initialFormWidth; // Formu başlangıçta dar tut
            pictureBox1.Visible = false;   // QR kodu başlangıçta gizle
        }

        private void LoadWifiNames()
        {
            
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c netsh wlan show profiles";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

           
            var profileNames = Regex.Matches(output, @"All User Profile\s*:\s*(.*)");
            foreach (Match match in profileNames)
            {
                string profileName = match.Groups[1].Value.Trim();
                comboBox1.Items.Add(profileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            string selectedWifi = comboBox1.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedWifi))
            {
                
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c netsh wlan show profile \"{selectedWifi}\" key=clear";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                
                var passwordMatch = Regex.Match(output, @"Key Content\s*:\s*(.*)");
                string password = passwordMatch.Groups[1].Value.Trim();

                
                textBox1.Text = string.IsNullOrEmpty(password) ? "Şifre bulunamadı." : password;
            }
            else
            {
                MessageBox.Show("Lütfen bir Wi-Fi ağı seçin.", "Hata");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedWifi = comboBox1.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedWifi))
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c netsh wlan show profile \"{selectedWifi}\" key=clear";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var passwordMatch = Regex.Match(output, @"Key Content\s*:\s*(.*)");
                string password = passwordMatch.Groups[1].Value.Trim();

                if (!string.IsNullOrEmpty(password))
                {
                    using (var qrGenerator = new QRCoder.QRCodeGenerator())
                    {
                        var qrCodeData = qrGenerator.CreateQrCode(password, QRCoder.QRCodeGenerator.ECCLevel.Q);
                        using (var qrCode = new QRCoder.QRCode(qrCodeData))
                        {
                            pictureBox1.Image = qrCode.GetGraphic(20);
                        }
                    }

                    // Formu genişlet ve QR kodu göster
                    this.Width = expandedFormWidth;
                    pictureBox1.Visible = true;
                }
                else
                {
                    pictureBox1.Image = null;
                    MessageBox.Show("Şifre bulunamadı.", "Hata");
                }
            }
        }
    }
}
