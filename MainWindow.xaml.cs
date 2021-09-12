using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace StopWatch
{
    public partial class MainWindow : Window
    {
        const string SavedTimeFile = "SavedTime.dat";
        const string LogFile = "log.txt";

        TimeSpan _elapsedOffset = new System.TimeSpan(0, 0, 0);
        System.Diagnostics.Stopwatch _elapsedStopwatch = new System.Diagnostics.Stopwatch();
        DispatcherTimer UIUpdateTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            UIUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            UIUpdateTimer.Tick += timer_Tick;
            UIUpdateTimer.Start();

            LoadTime();
            MoveToBottonRight();
        }

        void MoveToBottonRight()
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
        string GetTimeString(TimeSpan ts)
        {
            int h = (int)Math.Floor(ts.TotalHours);
            return h.ToString() + ts.ToString(@"\:mm\:ss");
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = GetTimeString(_elapsedOffset.Add(_elapsedStopwatch.Elapsed));
        }

        private void lblTime_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_elapsedStopwatch.IsRunning)
            {
                _elapsedStopwatch.Stop();
                UpdateOffset();
                SaveTime();
                Log("Stop:" + GetTimeString(_elapsedOffset.Add(_elapsedStopwatch.Elapsed)));
            }
            else
            {
                _elapsedStopwatch.Start();
                Log("Start:" + GetTimeString(_elapsedOffset.Add(_elapsedStopwatch.Elapsed)));
            }
            this.DragMove();
        }

        void UpdateOffset()
        {
            _elapsedOffset = _elapsedOffset.Add(_elapsedStopwatch.Elapsed);
            _elapsedStopwatch.Reset();
        }

        void SaveTime()
        {
            UpdateOffset();

            WriteToBinaryFile(SavedTimeFile, _elapsedOffset);
        }

        void LoadTime()
        {
            if(File.Exists(SavedTimeFile))
            _elapsedOffset = ReadFromBinaryFile<TimeSpan>(SavedTimeFile);
        }

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTime();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        void Log(string txt)
        {
            File.AppendAllText(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + txt + Environment.NewLine);
        }



        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log("*** Mark:" + GetTimeString(_elapsedOffset.Add(_elapsedStopwatch.Elapsed)));
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveTime();
            Close();
        }
    }
}
