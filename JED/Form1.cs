using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace JED
{
    public partial class Jed : Form
    {
        private double[] cpuArray = new double[60]; // массив со значениями нагрузки проца. передаётся для визуализации данных графика
        private double[] ramArray = new double[60]; // массив со значениями занятой ОЗУ. передаётся для визуализации данных графика
        private void getPerformanceCounters() // метод для получения загрузки проца и передачи данных в массив для визуализации в графике
        {
            var cpuPerfCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total"); /* PerformanceCounter - счётчик всяких данных.
                                                                                                                 * через него можно посмотреть загрузку проца и инфы по дискам,
                                                                                                                 * но озу нельзя почему-то*/

            while (true)
            {
                cpuArray[cpuArray.Length - 1] = Math.Round(cpuPerfCounter.NextValue(), 0); // заносим значения в массив, обрабатываем

                Array.Copy(cpuArray, 1, cpuArray, 0, cpuArray.Length - 1);

                if (chart1.IsHandleCreated) /* это условие проверяет, создан ли какой-либо элемент управления(присвоен ли ему свой базовый декриптор),
                                             * если да, то выполняет действие(делегат) в отдельном потоке
                                             это чудо позволяет каждое отдельное взаимодействие с элементом интерфейса программы выполнять по отдельности
                                             и не захломлять один поток. Пример: отрисовка формы происходит в одном потоке, а изменение текста в другом
                                            (благодаря этому подходу прога не лагает каждую секунду, когда мы делаем thread(time) sleep и всё летает по красоте*/
                        

                {
                    this.Invoke((MethodInvoker) delegate { UpdateCpuChart(); });
                }
                Thread.Sleep(1000);
            }
        }

        private void UpdateCpuChart() // метод, отвечающий за добавление данных по процу в график. 
        {
            chart1.ChartAreas[0].AxisY.Maximum = 100; // y наиб = 100%
            chart1.Series["CPU"].Points.Clear(); // очищаем нанесённые точки(чтобы график был непрерывным)

            for (int i = 0; i < cpuArray.Length - 1; ++i)
            {
                chart1.Series["CPU"].Points.AddY(cpuArray[i]); // отмечаем точки
            }
        }

        private void getRAM() // метод для получения озу и передачи данных в массив для визуализации в графике
        {
            double A_RAM = Math.Round(new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1073741824.0, 2); // сколько свободно ОЗУ на ПК
            double T_RAM = ((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1073741824) + 1); // сколько доступно физически ОЗУ на ПК
            double F_RAM = T_RAM - A_RAM; // сколько занято ОЗУ на ПК

            while (true)
            {
                ramArray[ramArray.Length - 1] = F_RAM; // заносим данные(занятое ОЗУ) в массив и обрабатываем по аналогии с процом. Также добавляем асинхронность 

                Array.Copy(ramArray, 1, ramArray, 0, ramArray.Length - 1);

                if (chart2.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { UpdateRAMChart(); });
                }
                Thread.Sleep(1000);
            }
        }
        private void UpdateRAMChart() // метод, отвечающий за добавление данных по ОЗУ в график. 
        {
            chart2.ChartAreas[0].AxisY.Maximum = (int)((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1073741824) + 1); // у наиб = сколько доступно физически ОЗУ
            chart2.Series["RAM"].Points.Clear(); // очищаем нанесённые точки(чтобы график был непрерывным)

            for (int i = 0; i < ramArray.Length - 1; ++i)
            {
                chart2.Series["RAM"].Points.AddY(ramArray[i]); // отмечаем точки
            }
        }


        private void label1_Click(object Sender, EventArgs e) // когда жмём на текст с нагрузкой проца, включается график с ЦП и выключается график с ОЗУ
        {
            chart1.Visible = true;
            chart2.Visible = false;
        }
        public Jed() // самый главный класс формы(нашей проги). инициализируем всё и запускаем отдельные потоки с процессом сбора данных по процу и ОЗУ
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen; // прога при запуске встаёт ровно по центру экрана
            Thread cpuThread = new Thread(new ThreadStart(this.getPerformanceCounters));
            cpuThread.IsBackground = true;
            cpuThread.Start();
            Thread cpuRAMThread = new Thread(new ThreadStart(this.getRAM));
            cpuRAMThread.IsBackground = true;
            cpuRAMThread.Start();
        }
               
        private string getRAMCounter() // метод для передачи инфы по ОЗУ для текстового поля
        {
            double A_RAM = Math.Round(new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1073741824.0, 2);
            ulong T_RAM = ((new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1073741824) + 1);
            return "Используется: " + (T_RAM - A_RAM) + "ГБ / " + T_RAM + "ГБ";
        }
        private string getDriveInfo() // метод для передачи инфы по дискам и памяти для текстового поля
        {
            var drives = DriveInfo.GetDrives();
            string fin_res = "";
            foreach (DriveInfo info in drives)
            {
                fin_res+="Накопитель " + info.Name + "\nСвободно: " + info.TotalFreeSpace/1073741824 + "ГБ / " + info.TotalSize/1073741824 + " ГБ\nФормат диска: " + info.DriveFormat + "\n\n";
            }
            return fin_res;
        }

        private void PC_DATA() // объединяющий метод, который обновляет текстовую инфу
        {
            for (; ; )
            {
                if (label2.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < cpuArray.Length - 1; ++i)
                        {
                            label2.Text = "Нагрузка: " + (int)cpuArray[i] + "%";
                        }
                    });

                    if (label4.IsHandleCreated)
                    {
                        this.Invoke((MethodInvoker)delegate { label4.Text = getRAMCounter(); });
                    }
                    if (label6.IsHandleCreated)
                    {
                        this.Invoke((MethodInvoker)delegate { label6.Text = getDriveInfo(); });
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e) // когда жмём на плюсик. временная заглушка
        {
            MessageBox.Show("В данный момент недоступно");
        }

        private void Jed_Load(object sender, EventArgs e) /* метод, который срабатывает после полной загрузки и отрисовки формы(проги). тут мы 
                                                           * запускаем наш "объединяющий метод" */
        {
            Thread GET_INFO = new Thread(new ThreadStart(this.PC_DATA));
            GET_INFO.IsBackground = true;
            GET_INFO.Start();
            
        }

        private void label3_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            chart2.Visible = true;
        }
    }
}
