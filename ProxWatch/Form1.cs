using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace ProxWatch
{
   public partial class Form1 : Form
   {
      private readonly NotifyIcon _trayIcon;
      private readonly string _proxy;

      public Form1()
      {
         InitializeComponent();

         // Use the assembly GUID as the name of the mutex which we use to detect if an application instance is already running
         bool createdNew = false;
         string mutexName = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID.ToString();
         using (new System.Threading.Mutex(false, mutexName, out createdNew))
         {
            if (!createdNew)
            {
               // Only allow one instance of the application to run at any one time.
               MessageBox.Show("Only one instance of ProxWatch can be running.", "ProxWatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }
         }

         try
         {
            var proxyDetails = WebRequest.GetSystemWebProxy();
            _proxy = proxyDetails.GetProxy(new Uri("http://example.com")).ToString();

            // Create a simple tray menu with only one item.
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add($"Proxy: {_proxy}", OnClickProxyMenuItem);
            trayMenu.MenuItems.Add($"Set 'HTTP_PROXY/HTTPS_PROXY' Environment Variables", OnClickSetEnvVariablesMenuItem);
            trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "ProxWatch";
            _trayIcon.Icon = new Icon(SystemIcons.Information, 40, 40);

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenu = trayMenu;
            _trayIcon.Visible = true;
         }
         catch (Exception ex)
         {
            MessageBox.Show($"Failed to start ProxWatch: {ex.Message}", "ProxWatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
         
      }

      protected override void OnLoad(EventArgs e)
      {
         Visible = false; // Hide form window.
         ShowInTaskbar = false; // Remove from taskbar.

         base.OnLoad(e);
      }

      private void OnExit(object sender, EventArgs e)
      {
         _trayIcon.Visible = false;
         Application.Exit();
      }

      private void OnClickProxyMenuItem(object sender, EventArgs e)
      {
         Clipboard.SetText(_proxy);
         _trayIcon.ShowBalloonTip(2000, "ProxWatch", "Proxy address copied to clipboard.", ToolTipIcon.Info);
      }

      private void OnClickSetEnvVariablesMenuItem(object sender, EventArgs e)
      {
         Environment.SetEnvironmentVariable("HTTP_PROXY", _proxy, EnvironmentVariableTarget.Machine);
         Environment.SetEnvironmentVariable("HTTPS_PROXY", _proxy, EnvironmentVariableTarget.Machine);
         _trayIcon.ShowBalloonTip(2000, "ProxWatch", "HTTP_PROXY/HTTPS_PROXY environment variables updated.", ToolTipIcon.Info);
      }


      protected override void Dispose(bool isDisposing)
      {
         if (isDisposing)
         {
            // Release the icon resource.
            _trayIcon.Icon.Dispose();
            _trayIcon.Dispose();
         }

         base.Dispose(isDisposing);
      }
   }
}