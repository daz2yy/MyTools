using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            string fileExt = System.IO.Path.GetExtension(msg);
            string inputPath = fileExt == "" ? msg : System.IO.Path.GetDirectoryName(msg);
            if (fileExt == "lua")
            {
                msg = "This is a lua file:" + msg;
            }
            else if (fileExt == "png" || fileExt == ".jpg")
            {
                msg = "This is a image file:" + msg;
            }

            if (this.encodeALLBtn.IsChecked == true)
            {
                
            }
            else if (this.encodeLuaBtn.IsChecked == true)
            {

            }

            string outputPath = System.IO.Path.GetDirectoryName(msg);
            string path = this.inputText.ToString();
            if (path != "")
            {
                outputPath = path;
                if (!System.IO.Directory.Exists(path))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    catch
                    {
                        MessageBox.Show("创建文件夹失败, 请检查文件夹路径是否正确.");
                        return;
                    }
                }
            }
            Process myProcess = new Process();
            string fileName = @"D:\NeedToCleanUp\test.bat";
            string para = @"-i " + msg + @" -o " + msg;
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(fileName, para);
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
            while (!myProcess.HasExited)
            {
                myProcess.WaitForExit();
            }

            MessageBox.Show("finish");
        }
    }
}
