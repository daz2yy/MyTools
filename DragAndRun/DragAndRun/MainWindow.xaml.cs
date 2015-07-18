using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DragAndRun
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_Drag(object sender, DragEventArgs e)
        {
            string msg = "Drop";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                System.Array array = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            }
            if (msg.Contains(".lua"))
            {
                msg = "This is a lua file:" + msg;
            }
            else if (msg.Contains(".png") || msg.Contains(".jpg"))
            {
                msg = "This is a image file:" + msg;
            }

            //Process myProcess = new Process();
            //string fileName = @"D:\PngEncode.exe";
            //string para = @"-i " + msg + @" -o " + msg;
            //ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(fileName, para);
            //myProcess.StartInfo = myProcessStartInfo;
            //myProcess.Start();
            //while (!myProcess.HasExited)
            //{
            //   myProcess.WaitForExit();
            //}

            MessageBox.Show("finish encodeing");
        }
    }
}
