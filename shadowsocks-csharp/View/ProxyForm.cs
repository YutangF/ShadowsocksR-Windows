﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Properties;

namespace Shadowsocks.View
{
    public partial class ProxyForm : Form
    {
        private ShadowsocksController controller;

        // this is a copy of configuration that we are working on
        private ProxyConfig _modifiedConfiguration;

        public ProxyForm(ShadowsocksController controller)
        {
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            InitializeComponent();

            UpdateTexts();
            this.Icon = Icon.FromHandle(Resources.ssw128.GetHicon());

            this.controller = controller;
            controller.ConfigChanged += controller_ConfigChanged;

            UpdateEnabled();
            LoadCurrentConfiguration();
        }

        private void UpdateTexts()
        {
            UseProxyCheckBox.Text = I18N.GetString("Use Proxy");
            ProxyTypeLabel.Text = I18N.GetString("Proxy Type");
            ProxyAddrLabel.Text = I18N.GetString("Proxy Addr");
            ProxyPortLabel.Text = I18N.GetString("Proxy Port");
            ProxyTimeoutLabel.Text = I18N.GetString("Timeout(Sec)");
            OKButton.Text = I18N.GetString("OK");
            MyCancelButton.Text = I18N.GetString("Cancel");
            this.Text = I18N.GetString("Edit Proxy");
        }

        private void controller_ConfigChanged(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void LoadCurrentConfiguration()
        {
            _modifiedConfiguration = controller.GetConfigurationCopy().proxy;
            UseProxyCheckBox.Checked = _modifiedConfiguration.useProxy;
            ProxyServerTextBox.Text = _modifiedConfiguration.proxyServer;
            ProxyPortTextBox.Text = _modifiedConfiguration.proxyPort.ToString();
            ProxyTimeoutTextBox.Text = _modifiedConfiguration.proxyTimeout.ToString();
            ProxyTypeComboBox.SelectedIndex = _modifiedConfiguration.proxyType;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (UseProxyCheckBox.Checked)
            {
                try
                {
                    var type = ProxyTypeComboBox.SelectedIndex;
                    var proxy = ProxyServerTextBox.Text;
                    var port = int.Parse(ProxyPortTextBox.Text);
                    var timeout = int.Parse(ProxyTimeoutTextBox.Text);
                    Configuration.CheckServer(proxy);
                    Configuration.CheckPort(port);
                    Configuration.CheckTimeout(timeout, ProxyConfig.MaxProxyTimeoutSec);

                    controller.EnableProxy(type, proxy, port);
                }
                catch (FormatException)
                {
                    MessageBox.Show(I18N.GetString("Illegal port number format"));
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                controller.DisableProxy();
            }

            _modifiedConfiguration.useProxy = UseProxyCheckBox.Checked;
            _modifiedConfiguration.proxyType = ProxyTypeComboBox.SelectedIndex;
            _modifiedConfiguration.proxyServer = ProxyServerTextBox.Text;
            var tmpProxyPort = 0;
            int.TryParse(ProxyPortTextBox.Text, out tmpProxyPort);
            _modifiedConfiguration.proxyPort = tmpProxyPort;
            var tmpProxyTimeout = 0;
            int.TryParse(ProxyTimeoutTextBox.Text, out tmpProxyTimeout);
            _modifiedConfiguration.proxyTimeout = tmpProxyTimeout;
            controller.SaveProxyConfig(_modifiedConfiguration);

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProxyForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.ConfigChanged -= controller_ConfigChanged;
        }

        private void UseProxyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            if (UseProxyCheckBox.Checked)
            {
                ProxyServerTextBox.Enabled = true;
                ProxyPortTextBox.Enabled = true;
                ProxyTimeoutTextBox.Enabled = true;
                ProxyTypeComboBox.Enabled = true;
            }
            else
            {
                ProxyServerTextBox.Clear();
                ProxyPortTextBox.Clear();
                ProxyTimeoutTextBox.Clear();
                ProxyServerTextBox.Enabled = false;
                ProxyPortTextBox.Enabled = false;
                ProxyTimeoutTextBox.Enabled = false;
                ProxyTypeComboBox.Enabled = false;
            }
        }
    }
}