using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace JED
{
    public partial class Jed : Form
    {
        
        public Jed()
        {
            InitializeComponent();
        }
          
        public string getCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            dynamic firstValue = cpuCounter.NextValue();
            Thread.Sleep(1000); 
            dynamic secondValue = cpuCounter.NextValue();
            return "Нагрузка: " + (int)secondValue + "%";

        }
        public string getRAMCounter()
        {
            double A_RAM = Math.Round(new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1073741824.0, 2);
            ulong T_RAM = ((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1073741824) + 1);
            return "Используется: " + (T_RAM - A_RAM) + "ГБ / " + T_RAM + "ГБ";
        }
        public string getDriveInfo()
        {
            var drives = DriveInfo.GetDrives();
            string fin_res = "";
            foreach (DriveInfo info in drives)
            {
                fin_res+="Накопитель " + info.Name + "\nСвободно: " + info.TotalFreeSpace/1073741824 + "ГБ / " + info.TotalSize/1073741824 + " ГБ\nФормат диска: " + info.DriveFormat + "\n\n";
            }
            return fin_res;
        }

        private async void Jed_Load(object sender, EventArgs e)
        {
            await PC_DATA();
        }
        private async Task PC_DATA()
        {
           for (; ; )
           {
                label2.Text = await Task.Run(() => getCPUCounter());
                label4.Text = await Task.Run(() => getRAMCounter());
                label6.Text = await Task.Run(() => getDriveInfo());
           }
        }
        
    }
}
