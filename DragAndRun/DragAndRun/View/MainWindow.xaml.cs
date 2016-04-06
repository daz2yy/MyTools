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
using System.Threading;
using DragAndRun.Utils;
using DragAndRun.ViewModule;
using DragAndRun.View;

namespace DragAndRun
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Config> _ListViewItems;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = PackageAllVM.Instance;
            //初始在打整包界面
            GlobalVM.Instance.Type = GlobalVM.PackType.Pack_All;
            GlobalVM.Instance.EncodeAndroid = true;
            PackageAllVM.Instance.AllPackageAndroid = true;
            PackageAllVM.Instance.AllPackageEncodeAll = true;
        }

        private int _lastSelectedIndex;
        private void TabItem_MouseLeftButtonDown_PackAll(object sender, MouseButtonEventArgs e)
        {
            if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_All)
            {
                return;
            }
            this.DataContext = PackageAllVM.Instance;
            GlobalVM.Instance.Type = GlobalVM.PackType.Pack_All;
            GlobalVM.Instance.EncodeAndroid = PackageAllVM.Instance.AllPackageAndroid;
            PackageAllVM.Instance.AllDescription = @"拖动需要加密的文件夹, 或者是文件所在的文件夹到这里(所有路径不要有空格):";
        }

        private List<string> _configFiles;
        private void TabItem_MouseLeftButtonDown_PackSub(object sender, MouseButtonEventArgs e)
        {
            if (GlobalVM.Instance.Type == GlobalVM.PackType.Pack_Sub)
            {
                return;
            }
            this.DataContext = PackageSubVM.Instance;
            GlobalVM.Instance.Type = GlobalVM.PackType.Pack_Sub;

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

        private void loadData(string configName)
        {
            PackageSubVM.Instance.loadData(configName);
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
            System.Array array = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            PackageAllVM.Instance.handleDropItems(array);
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
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
            //open setting 
            Window dialog = new PackageSetting();
            dialog.ShowDialog();
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
                PackageSubVM.Instance.saveData();
                this._lastSelectedIndex = _ListViewItems.IndexOf(configItem);
                this.loadData(_configFiles[this._lastSelectedIndex]);
            }
        }
        // todo rename
        private void comItemName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
        }

        // public api
        // 检查是否选择了项目
        public bool checkProjectSelect()
        {
            bool bCheckSelected = false;
            foreach (Config item in _ListViewItems)
            {
                if (item.IsChecked == true)
                {
                    bCheckSelected = true;
                    break;
                }
            }
            return bCheckSelected;
        }

    }
}
