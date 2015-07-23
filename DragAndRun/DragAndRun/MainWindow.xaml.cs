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
using DragAndRun.FileFilter;

namespace DragAndRun
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CFileFilter _Filter = new CFileFilter();
        public MainWindow()
        {
            InitializeComponent();
            this.packAllGrid.Visibility = System.Windows.Visibility.Visible;
            this.packSubGrid.Visibility = System.Windows.Visibility.Hidden;
//             _Filter.setWindowHandler(this);
//             _Filter.setVersion("0.7.26942.20150716");
//             _Filter.setResourcePath(@"D:\Project\D\Resource");
//             _Filter.setOutPutPath(@"D:\1temp\history");
//             _Filter.setHistoryPath(@"D:\1temp\history");
//             _Filter.setOutHistoryPath(@"D:\1temp\history");
//             _Filter.setOutDifferentPath(@"D:\1temp\history");
//             _Filter.setOutResourcePath(@"D:\1temp\history\source");
//             _Filter.genCurVersionXML();

//             _Filter.setVersion("0.7.26942.20150722");
//             _Filter.setResourcePath(@"E:\Dispatch\0.7.26843.20150716Culture\Resource");
//             _Filter.setOutPutPath(@"D:\1temp\history");
//             _Filter.setHistoryPath(@"D:\1temp\history");
//             _Filter.setOutHistoryPath(@"D:\1temp\history");
//             _Filter.setOutDifferentPath(@"D:\1temp\history");
//             _Filter.setOutResourcePath(@"D:\1temp\history\source");
//             _Filter.genCurVersionXML();
            this.comOutputPath.Text = SaveData.GetInstance().getData("szCompleteOutputPath");
            this.comEncodeFilePath.Text = SaveData.GetInstance().getData("szEncodeFilePath");
        }

        private void packAllButton_Click(object sender, RoutedEventArgs e)
        {
            this.packAllGrid.Visibility = System.Windows.Visibility.Visible;
            this.packSubGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        private void packSubButton_Click(object sender, RoutedEventArgs e)
        {
            this.packAllGrid.Visibility = System.Windows.Visibility.Hidden;
            this.packSubGrid.Visibility = System.Windows.Visibility.Visible;
            this.comResourcePath.Text   = SaveData.GetInstance().getData("szResourcePath");
            this.comOutPath.Text = SaveData.GetInstance().getData("szOutPath");
            this.comHistoryPath.Text = SaveData.GetInstance().getData("szHistoryPath");
            this.comHistoryOutPath.Text = SaveData.GetInstance().getData("szHistoryOutPath");
            this.comDifferentOutPath.Text = SaveData.GetInstance().getData("szDifferentOutPath");
            this.comResourceOutPath.Text = SaveData.GetInstance().getData("szResourceOutPath");
            this.comFilter.Text = SaveData.GetInstance().getData("szFilter");
        }
        private void ListBox_Drag(object sender, DragEventArgs e)
        {
            string msg = "Drop";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                System.Array array = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            }
            string inputPath = msg;

            string fileExt = System.IO.Path.GetExtension(msg);
            string outputPath = fileExt == "" ? msg : System.IO.Path.GetDirectoryName(msg);
            string path = this.comOutputPath.Text;
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
            SaveData.GetInstance().setData("szCompleteOutputPath", this.comOutputPath.Text);
            SaveData.GetInstance().setData("szEncodeFilePath", this.comEncodeFilePath.Text);

            bool bEncodeAll = this.encodeALLBtn.IsChecked == true;
            bool bEncodeLua = this.encodeLuaBtn.IsChecked == true;
            bool bEncodeImage = this.encodeImageBtn.IsChecked == true;
            this.comContentText.Text = @"拖动需要加密的文件夹, 或者是文件所在的文件夹到这里(所有路径不要有空格):";
            if (bEncodeAll)
            {
                encodeLua(inputPath, outputPath);
                encodeImage(inputPath, outputPath);
            }
            else if (bEncodeLua)
            {
                encodeLua(inputPath, outputPath);
            }
            else if (bEncodeImage)
            {
                encodeImage(inputPath, outputPath);
            }
            MessageBox.Show("完成加密!");
        }

        //加密lua
        public void encodeLua(string inputPath, string outputPath)
        {
            this.comContentText.Text = this.comContentText.Text + "\n加密Lua: ";
            string executeFilePath;
            string param;
            if (this.encodAndroidBtn.IsChecked == true)
            {
                bool bMoveFile = false;
                string oldPath = "";
                if (System.IO.Path.GetExtension(inputPath) != "")
                {
                    if (!System.IO.Directory.Exists(inputPath + "_temp"))
                    {
                        System.IO.Directory.CreateDirectory(inputPath + "_temp");
                    }
                    System.IO.File.Move(inputPath, inputPath + @"_temp\" + System.IO.Path.GetFileName(inputPath));
                    oldPath = inputPath;
                    inputPath = inputPath + "_temp";
                    bMoveFile = true;
                }
                executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                executeFilePath = executeFilePath + @"\encodeTools\luaToJit\compile_scripts.bat";
                param = "-i \"" + inputPath + "\" -o \"" + outputPath + "\" -m files -jit";
                executeFile(executeFilePath, param);
                if (bMoveFile)
                {
                    System.IO.Directory.Delete(oldPath + "_temp", true);
                }
            }
            else
            {
                executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                executeFilePath = executeFilePath + @"\encodeTools\PngEncode.exe";
                param = "-i \"" + inputPath + "\" -o \"" + outputPath + "\" -m files -jit";
                executeFile(executeFilePath, param);
            }
        }

        //加密图片
        public void encodeImage(string inputPath, string outputPath)
        {
            this.comContentText.Text = this.comContentText.Text + "\n加密图片: ";
            string executeFilePath;
            string param;
            executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
            executeFilePath = executeFilePath + @"\encodeTools\PngEncode.exe";
            param = "-i " + "\"" + inputPath + "\"";
            executeFile(executeFilePath, param);
        }

        void executeFile(string filePath, string param)
        {
            if (this.comEncodeFilePath.Text != "")
            {
                filePath = this.comEncodeFilePath.Text;
            }
            //filePath = "C:\\Program Files (x86)\\神曲世界打包\\encodeTools\\luaToJit\\compile_scripts.bat";
            this.comContentText.Text = this.comContentText.Text + "\n执行文件: " + filePath;
            this.comContentText.Text = this.comContentText.Text + "\n执行参数: " + param;
            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("\"" + filePath + "\"", param);
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
            while (!myProcess.HasExited)
            {
                myProcess.WaitForExit();
            }

            // filePath = @"C:\Program Files (x86)\神曲世界打包\encodeTools\luaToJit\compile_scripts.bat";
            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.FileName = System.IO.Path.GetFileName(filePath);
            //psi.WorkingDirectory = System.IO.Path.GetDirectoryName(filePath);
            //psi.Arguments = param;
            //Process.Start(psi);
        }

        private void GenButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.comVersionPath.Text == "" || this.comResourcePath.Text == "" || this.comOutPath.Text == "")
            {
                MessageBox.Show("还有信息没填完整!");
                return;
            }
            this.Description.Text = "";
            _Filter.setVersion(this.comVersionPath.Text);
            _Filter.setResourcePath(this.comResourcePath.Text);
            _Filter.setOutPutPath(this.comOutPath.Text);
            _Filter.setHistoryPath(this.comHistoryPath.Text);
            _Filter.setOutHistoryPath(this.comHistoryOutPath.Text);
            _Filter.setOutDifferentPath(this.comDifferentOutPath.Text);
            _Filter.setOutResourcePath(this.comResourceOutPath.Text);
            _Filter.setFileFilter(this.comFilter.Text);
            _Filter.setWindowHandler(this);

            //save data
            SaveData.GetInstance().setData("szResourcePath", this.comResourcePath.Text);
            SaveData.GetInstance().setData("szOutPath", this.comOutPath.Text);
            SaveData.GetInstance().setData("szHistoryPath", this.comHistoryPath.Text);
            SaveData.GetInstance().setData("szHistoryOutPath", this.comHistoryOutPath.Text);
            SaveData.GetInstance().setData("szDifferentOutPath", this.comDifferentOutPath.Text);
            SaveData.GetInstance().setData("szResourceOutPath", this.comResourceOutPath.Text);
            SaveData.GetInstance().setData("szFilter", this.comFilter.Text);
            //run
            _Filter.genCurVersionXML();
            MessageBox.Show("打包完成!");
        }

        private void comOutPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.comHistoryPath.Text == "")
            {
                this.comHistoryPath.Text = this.comOutPath.Text + @"\history";
                this.comHistoryOutPath.Text = this.comHistoryPath.Text;
            }
            if (this.comDifferentOutPath.Text == "")
            {
                this.comDifferentOutPath.Text = this.comOutPath.Text + @"\edition";
            }
            if (this.comResourceOutPath.Text == "")
            {
                this.comResourceOutPath.Text = this.comOutPath.Text + @"\source";
            }
        }

        public void updateDescription(string str)
        {
            this.Description.Text = this.Description.Text + str;
            this.Description.ScrollToEnd();
        }

    }
}
