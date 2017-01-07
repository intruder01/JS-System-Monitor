using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;
using Microsoft.Win32;

namespace JS_System_Monitor
{
	public partial class Form1 : Form
	{
		private string cfgFilename;  //last open config file
		private int winSizeWidth, winSizeHeight, winLocX, winLocY;  //window pos and size

		NotifyIcon hddNotifyIcon;
		Icon readIcon;
		Icon writeIcon;
		Icon idleIcon;
		Thread workerThread;

		public Form1()
		{
			InitializeComponent();

			readSettings(); //get registry settings

			////set window position
			//this.Width = winSizeWidth;
			//this.Height = winSizeHeight;
			//this.Location = new Point(winLocX, winLocY);

			//load icons
			readIcon = new Icon(".\\resources\\HardDrive_13_Read_48x48.ico");
			writeIcon = new Icon(".\\resources\\HardDrive_13_Write_48x48.ico");
			idleIcon = new Icon(".\\resources\\HardDrive_13_Idle_48x48.ico");
			hddNotifyIcon = new NotifyIcon();
			hddNotifyIcon.Icon = idleIcon;
			hddNotifyIcon.Visible = true;

			//context menu
			ContextMenu contextMenu = new ContextMenu();
			MenuItem progNameMenuItem = new MenuItem("System Monitor");
			MenuItem quitMenuItem = new MenuItem("Quit");
			contextMenu.MenuItems.Add(progNameMenuItem);
			contextMenu.MenuItems.Add(quitMenuItem);
			hddNotifyIcon.ContextMenu = contextMenu;
			// wire up quit item to close application
			quitMenuItem.Click += QuitMenuItem_Click;
			this.ShowInTaskbar = false;

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//set window position
			this.Width = winSizeWidth;
			this.Height = winSizeHeight;
			this.Location = new Point(winLocX, winLocY);

			//Start worker thread
			workerThread = new Thread(new ThreadStart(HddActivityThread));
			workerThread.Start();
		}

		private void QuitMenuItem_Click(object sender, EventArgs e)
		{
			workerThread.Abort();
			hddNotifyIcon.Dispose();
			saveSettings();
			this.Close();
		}

		public void HddActivityThread()
		{
			ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");

			try
			{

				//Main loop where  polling hapens
				while (true)
				{
					ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
					foreach (ManagementObject obj in driveDataClassCollection)
					{
						//Instances button in WMI
						if (obj["Name"].ToString() == "_Total")
						{
							if (Convert.ToUInt64(obj["DiskReadBytesPersec"]) > 0)
							{
								//Show busy icon
								hddNotifyIcon.Icon = readIcon;
							}
							else if (Convert.ToUInt64(obj["DiskWriteBytesPersec"]) > 0)
							{
								//Show busy icon
								hddNotifyIcon.Icon = writeIcon;
							}
							else
							{
								//Show idle icon
								hddNotifyIcon.Icon = idleIcon;
							}
						}
					}

					Thread.Sleep(100);
				}
			}
			catch (ThreadAbortException)
			{
				driveDataClass.Dispose();
			}
		}


		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			workerThread.Abort();
			hddNotifyIcon.Dispose();
			saveSettings();
		}



		private void saveSettings()
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\JS System Monitor");
			key.SetValue("cfgFilename", cfgFilename, RegistryValueKind.String);
			key.SetValue("winSizeWidth", this.Size.Width.ToString(), RegistryValueKind.String);
			key.SetValue("winSizeHeight", this.Size.Height.ToString(), RegistryValueKind.String);
			key.SetValue("winLocX", this.Location.X.ToString(), RegistryValueKind.String);
			key.SetValue("winLocY", this.Location.Y.ToString(), RegistryValueKind.String);


		}
		private void readSettings()
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\JS System Monitor");
			cfgFilename = Convert.ToString(key.GetValue("cfgFilename", ""));
			winSizeWidth = Convert.ToInt32(key.GetValue("winSizeWidth", "200"));
			winSizeHeight = Convert.ToInt32(key.GetValue("winSizeHeight", "280"));
			winLocX = Convert.ToInt32(key.GetValue("winLocX", "100"));
			winLocY = Convert.ToInt32(key.GetValue("winLocY", "100"));
		}
	}
}
