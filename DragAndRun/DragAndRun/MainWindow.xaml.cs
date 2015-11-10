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
using System.Threading;
using DragAndRun.Utils;

namespace DragAndRun
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CFileFilter _Filter = new CFileFilter();
        List<Config> _ListViewItems;
        public Thread _logicThread;
        public MainWindow()
        {
            InitializeComponent();
            //FtpHelper ftp = new FtpHelper("120.131.1.236", "root", "#1MzjpMr]63w");
            //ftp.UpLoad("");
            //ftp.webUpload();
        }

        public enum PackType
        {
            Pack_All,
            Pack_Sub,
        }
        public PackType _nType = PackType.Pack_All;
        private string oldOutPutPath;
        private int _lastSelectedIndex;
        private void TabItem_MouseLeftButtonDown_PackAll(object sender, MouseButtonEventArgs e)
        {
            _nType = PackType.Pack_All;
        }

        private List<string> _configFiles;
        private void TabItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _nType = PackType.Pack_Sub;
            _lastSelectedIndex = -1;
            _configFiles = new List<string>();
            DirectoryInfo TheFolder = new DirectoryInfo("PackageConfig/");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                if (NextFile.Extension == ".xml")
                {
                    _configFiles.Add(System.IO.Path.GetFileNameWithoutExtension(NextFile.Name));   
                }
            }

            if (_configFiles.Count > 0)
            {
                SaveData.GetInstance().loadConfig(_configFiles[0]);
                this.loadData(_configFiles[0]);
            }
            else
            {
                _ListViewItems = new List<Config>();
                _ListViewItems.Add(new Config() { Name = "默认", IsChecked = true });
                _configFiles.Add("默认");
                this.comConfigList.ItemsSource = _ListViewItems;
                SaveData.GetInstance().addConfig("默认");
            }
            this.comConfigList.SelectedIndex = 0;
        }

        private void saveData()
        {
            SaveData.GetInstance().setData("szResourcePath", this.comResourcePath.Text);
            SaveData.GetInstance().setData("szOutPath", this.comOutPath.Text);
            SaveData.GetInstance().setData("szHistoryPath", this.comHistoryPath.Text);
            SaveData.GetInstance().setData("szHistoryOutPath", this.comHistoryOutPath.Text);
            SaveData.GetInstance().setData("szDifferentOutPath", this.comDifferentOutPath.Text);
            SaveData.GetInstance().setData("szResourceOutPath", this.comResourceOutPath.Text);
            SaveData.GetInstance().setData("szFilter", this.comFilter.Text);
            SaveData.GetInstance().setData("szPackageURL", this.comPackageURL.Text);
            SaveData.GetInstance().setData("szUploadOutPath", this.comOutZipPath.Text);
            SaveData.GetInstance().setData("szUseSVN", this.comUseSVN.IsChecked.ToString());
            SaveData.GetInstance().setData("szVersionPath", this.comSVNPath.Text);
            SaveData.GetInstance().setData("szBeginVersion", this.comBeginVersion.Text);
            SaveData.GetInstance().setData("szEndVersion", this.comEndVersion.Text);
            SaveData.GetInstance().setData("szEncodeType", this.subEncodIOSBtn.IsChecked.ToString());
        }

        private void loadData(string configName)
        {
            SaveData.GetInstance().loadConfig(configName);
            this.comResourcePath.Text = SaveData.GetInstance().getData("szResourcePath");
            this.comOutPath.Text = SaveData.GetInstance().getData("szOutPath");
            this.comHistoryPath.Text = SaveData.GetInstance().getData("szHistoryPath");
            this.comHistoryOutPath.Text = SaveData.GetInstance().getData("szHistoryOutPath");
            this.comDifferentOutPath.Text = SaveData.GetInstance().getData("szDifferentOutPath");
            this.comResourceOutPath.Text = SaveData.GetInstance().getData("szResourceOutPath");
            this.comFilter.Text = SaveData.GetInstance().getData("szFilter");
            this.comPackageURL.Text = SaveData.GetInstance().getData("szPackageURL");
            this.comOutZipPath.Text = SaveData.GetInstance().getData("szUploadOutPath");
            this.comUseSVN.IsChecked = SaveData.GetInstance().getData("szUseSVN") == "True";
            this.comSVNPath.Text = SaveData.GetInstance().getData("szVersionPath");
            this.comBeginVersion.Text = SaveData.GetInstance().getData("szBeginVersion");
            this.comEndVersion.Text = SaveData.GetInstance().getData("szEndVersion");
            this.subEncodIOSBtn.IsChecked = SaveData.GetInstance().getData("szEncodeType") == "True";
            oldOutPutPath = this.comOutPath.Text;
            _ListViewItems = new List<Config>();
            for (int i = 0; i < _configFiles.Count; i++)
            {
                _ListViewItems.Add(new Config() { Name = _configFiles[i], IsChecked = false });
            }
            this.comConfigList.ItemsSource = _ListViewItems;
            this.comConfigList.Items.Refresh();
            this.comConfigList.SelectedIndex = this._lastSelectedIndex;
        }

        private void ListBox_Drag(object sender, DragEventArgs e)
        {
            string msg = "";
            System.Array array = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            comLoadingPanel.Visibility = System.Windows.Visibility.Visible;
            for (int i = 0; i < array.Length; i++)
            {
                msg = array.GetValue(i).ToString();
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

                bool bEncodeAll = this.encodeALLBtn.IsChecked == true;
                bool bEncodeLua = this.encodeLuaBtn.IsChecked == true;
                bool bEncodeImage = this.encodeImageBtn.IsChecked == true;
                this.updateDescription(@"拖动需要加密的文件夹, 或者是文件所在的文件夹到这里(所有路径不要有空格):");
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
            }
            MessageBox.Show("完成加密!");
            comLoadingPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        //加密lua
        public void encodeLua(string inputPath, string outputPath)
        {
            this.updateDescription("加密Lua: ");
            if (System.IO.Directory.Exists(inputPath))
            {
                string executeFilePath;
                string param;
                if ((this.comPackAll.IsSelected == true && this.encodAndroidBtn.IsChecked == true) ||
                    (this.comPackSub.IsSelected == true && this.subEncodAndroidBtn.IsChecked == true))
                {
                    bool bMoveFile = false;
                    string oldPath = "";
                    if (System.IO.Path.GetExtension(inputPath) != "")
                    {
                        if (!System.IO.Directory.Exists(inputPath + "_temp"))
                        {
                            System.IO.Directory.CreateDirectory(inputPath + "_temp");
                        }
                        else
                        {
                            System.IO.Directory.Delete(inputPath + "_temp", true);
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
                    executeFilePath = executeFilePath + @"\encodeTools\LuaEncode.exe";
                    param = "-i \"" + inputPath + "\" -o \"" + outputPath;
                    executeFile(executeFilePath, param);
                }
            }
            else
            {
                this.updateDescription("加密Lua路径不存在!");
            }
        }

        //加密图片
        public void encodeImage(string inputPath, string outputPath)
        {
            this.updateDescription("加密图片: ");
            if (System.IO.Directory.Exists(inputPath))
            {
                string executeFilePath;
                string param;
                executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                executeFilePath = executeFilePath + @"\encodeTools\PngEncode.exe";
                param = "-i " + "\"" + inputPath + "\"" + " -o " + "\"" + outputPath + "\"";
                executeFile(executeFilePath, param);
            }
            else
            {
                this.updateDescription("加密图片路径不存在!");
            }
        }

        void executeFile(string filePath, string param)
        {
            if (this.comEncodeFilePath.Text != "")
            {
                filePath = this.comEncodeFilePath.Text;
            }
            //filePath = "C:\\Program Files (x86)\\神曲世界打包\\encodeTools\\luaToJit\\compile_scripts.bat";
            this.updateDescription("执行文件: " + filePath);
            this.updateDescription("执行参数: " + param);
            this.updateDescription("输出: " + param);
            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("\"" + filePath + "\"", param);
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.Start();
            while (!myProcess.HasExited)
            {
                this.updateDescription(myProcess.StandardOutput.ReadToEnd());
                myProcess.WaitForExit();
            }
            myProcess.Close();
        }

        private void GenButton_Click(object sender, RoutedEventArgs e)
        {
            //_Filter.setResourcePath(this.comResourcePath.Text);
            //_Filter.getSVNDifferFiles();
            //return;
            if (this.comVersionPath.Text == "" || this.comResourcePath.Text == "" || this.comOutPath.Text == "")
            {
                MessageBox.Show("还有信息没填完整!");
                return;
            }
            if (this.comUseSVN.IsChecked == true && (this.comBeginVersion.Text == "" || this.comEndVersion.Text == ""))
            {
                MessageBox.Show("还有信息没填完整!");
                return;   
            }
            this.Description.Text = "";
            _Filter.szVersion = this.comVersionPath.Text;
            _Filter.szResourcePath = this.comResourcePath.Text;
            _Filter.szOutputPath = this.comOutPath.Text;
            _Filter.szHistoryPath = this.comHistoryPath.Text;
            _Filter.szHistoryOutPath = this.comHistoryOutPath.Text;
            _Filter.szDifferentOutPath = this.comDifferentOutPath.Text;
            _Filter.szResourceOutPath = this.comResourceOutPath.Text;
            _Filter.szUploadZipOutPath = this.comOutZipPath.Text;
            _Filter.setFileFilter(this.comFilter.Text);
            _Filter.window = this;
            _Filter.bUseSVN = this.comUseSVN.IsChecked == true;
            _Filter.nBeginVersion = this.comBeginVersion.Text;
            _Filter.nEndVersion = this.comEndVersion.Text;
            _Filter.szSVNPath = this.comSVNPath.Text;

            //save data
            this.saveData();
            //run
            this.comLoadingPanel.Visibility = System.Windows.Visibility.Visible;
            _logicThread = new Thread(delegate()
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    _Filter.genCurVersionXML();
                    string preStr = "Android分包";
                    if ((this.comPackAll.IsSelected == true && this.encodIOSBtn.IsChecked == true) ||
                        (this.comPackSub.IsSelected == true && this.subEncodIOSBtn.IsChecked == true))
                    {
                        preStr = "IOS分包";
                    }
                    MessageBox.Show(preStr + "打包完成.");
                    comLoadingPanel.Visibility = System.Windows.Visibility.Hidden;
                    this._logicThread.Abort();
                }));
            });
            _logicThread.IsBackground = true;
            _logicThread.Start();
        }

        private void comOutPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.comHistoryPath.Text == "")
            {
                this.comHistoryPath.Text = this.comOutPath.Text + @"\history";
                this.comHistoryOutPath.Text = this.comHistoryPath.Text;
            }
            else
            {
                this.comHistoryPath.Text = this.comHistoryPath.Text.Replace(this.oldOutPutPath, textBox.Text);
                this.comHistoryOutPath.Text = this.comHistoryOutPath.Text.Replace(this.oldOutPutPath, textBox.Text);
            }
            if (this.comDifferentOutPath.Text == "")
            {
                this.comDifferentOutPath.Text = this.comOutPath.Text + @"\edition";
            }
            else
            {
                this.comDifferentOutPath.Text = this.comDifferentOutPath.Text.Replace(this.oldOutPutPath, textBox.Text);
            }
            if (this.comResourceOutPath.Text == "")
            {
                this.comResourceOutPath.Text = this.comOutPath.Text + @"\source";
            }
            else
            {
                this.comResourceOutPath.Text = this.comResourceOutPath.Text.Replace(this.oldOutPutPath, textBox.Text);
            }
            this.oldOutPutPath = this.comOutPath.Text;
        }

        public void updateDescription(string str)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                if (this._nType == PackType.Pack_All)
                {
                    this.comContentText.Text = this.comContentText.Text + "\n" + str;
                }
                else if (this._nType == PackType.Pack_Sub)
                {
                    this.Description.Text = this.Description.Text + str;
                    this.Description.ScrollToEnd();
                }
            }));
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string path = "";
            switch (btn.Name)
            {
                case "comOpenResourcePath":
                    path = this.comResourcePath.Text;
                    break;
                case "comOpenOutPath":
                    path = this.comOutPath.Text;
                    break;
                case "comOpenHistoryPath":
                    path = this.comHistoryPath.Text;
                    break;
                case "comOpenHistoryOutPath":
                    path = this.comHistoryOutPath.Text;
                    break;
                case "comOpenDifferentOutPath":
                    path = this.comDifferentOutPath.Text;
                    break;
                case "comOpenResourceOutPath":
                    path = this.comResourceOutPath.Text;
                    break;
            }
            Process.Start("explorer.exe", path);
        }

        public class Config
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }
        private void comAddConfig_Click(object sender, RoutedEventArgs e)
        {
            if (this.comNewConfigName.Text == "")
            {
                MessageBox.Show("请在按钮前的输入框内填写新配置的名字.");
                return;
            }
            for (int i = 0; i < _configFiles.Count; i++)
            {
                if (_configFiles[i] == this.comNewConfigName.Text)
                {
                    MessageBox.Show("重复的配置名");
                    return;
                }
            }
            _ListViewItems.Add(new Config() { Name = this.comNewConfigName.Text, IsChecked = true });
            this.comConfigList.Items.Refresh();
            SaveData.GetInstance().addConfig(this.comNewConfigName.Text);
            _configFiles.Add(this.comNewConfigName.Text);
            this.comNewConfigName.Text = "";
            this.comConfigList.SelectedIndex = _ListViewItems.Count - 1;
        }

        private void conDeleteConfig_Click(object sender, RoutedEventArgs e)
        {
            foreach (Config item in this.comConfigList.SelectedItems)
            {
                int index = _ListViewItems.IndexOf((Config)item);
                _ListViewItems.Remove((Config)item);
                System.IO.File.Delete("PackageConfig/" + item.Name + ".xml");
                _configFiles.Remove(item.Name);
            }
            this.comConfigList.Items.Refresh();
            if (_configFiles.Count > 0)
            {
                this.loadData(_configFiles[0]);
            }
            this.comConfigList.SelectedIndex = 0;
        }

        private void comItemName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
        }

        private void comConfigList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._lastSelectedIndex == -1 && _configFiles.Count > 0)
            {
                    this._lastSelectedIndex = 0;
                    this.loadData(_configFiles[this._lastSelectedIndex]);
            }
            if (this.comConfigList.SelectedItems.Count > 0)
            {
                Config configItem = (Config)this.comConfigList.SelectedItems[0];
                if (this._lastSelectedIndex == _ListViewItems.IndexOf(configItem))
                {
                    return;
                }
                this.saveData();
                this._lastSelectedIndex = _ListViewItems.IndexOf(configItem);
                this.loadData(_configFiles[this._lastSelectedIndex]);
            }
        }

    }
}
