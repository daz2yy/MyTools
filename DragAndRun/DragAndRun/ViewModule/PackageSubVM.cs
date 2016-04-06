using DragAndRun.Utils;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DragAndRun.Module;
using DragAndRun.View;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;

namespace DragAndRun.ViewModule
{
    class PackageSubVM : INotifyPropertyChanged
    {
        public PackageSubVM()
        {
            this.CDNList = new ObservableCollection<string>();
            this.CDNList.Add("无");
            this.CDNList.Add("腾讯CDN");
            this.CDNList.Add("蓝汛CDN");
        }
        #region  数据绑定
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private static PackageSubVM _instance;
        public static PackageSubVM Instance 
        {
            get
            {
                if (_instance == null)
                    _instance = new PackageSubVM();
                return _instance;
            }
        }

        private Visibility _loadingPanel = System.Windows.Visibility.Hidden;
        public Visibility LoadingPanel
        {
            get { return _loadingPanel; }
            set
            {
                _loadingPanel = value;
                OnPropertyChanged("LoadingPanel");
            }
        }

        private bool _packageAndroid;
        public bool PackageAndroid
        {
            get { return _packageAndroid; }
            set { 
                _packageAndroid = value;
                GlobalVM.Instance.EncodeAndroid = value;
                OnPropertyChanged("PackageAndroid");
            }
        }

        private string _svnPath;
        public string SVNPath
        {
            get { return _svnPath; }
            set { _svnPath = value; OnPropertyChanged("SVNPath"); }
        }

        private string _beginVersion;
        public string BeginVersion
        {
            get { return _beginVersion; }
            set { _beginVersion = value; OnPropertyChanged("BeginVersion"); }
        }

        private string _endVersion;
        public string EndVersion
        {
            get { return _endVersion; }
            set { _endVersion = value; OnPropertyChanged("EndVersion"); }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set { _version = value; OnPropertyChanged("Version"); }
        }

        public string szDifferentOutPath;
        public string szResourceOutPath;
        private string _outPath;
        public string OutPath
        {
            get { return _outPath; }
            set {
                _outPath = value;
                szDifferentOutPath = PackageSubVM.Instance.OutPath + @"\edition";
                if (!System.IO.Directory.Exists(szDifferentOutPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(szDifferentOutPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                }
                szResourceOutPath = PackageSubVM.Instance.OutPath + @"\source";
                if (!System.IO.Directory.Exists(szResourceOutPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(szResourceOutPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                }
                OnPropertyChanged("OutPath"); 
            }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; OnPropertyChanged("Filter"); }
        }

        private bool _removeMEDebug;
        public bool RemoveMEDebug
        {
            get { return _removeMEDebug; }
            set { _removeMEDebug = value; OnPropertyChanged("RemoveMEDebug"); }
        }

        private bool _needReInstall;
        public bool NeedReInstall
        {
            get { return _needReInstall; }
            set { _needReInstall = value; OnPropertyChanged("NeedReInstall"); }
        }
        
        private string _reInstallURL;
        public string ReInstallURL
        {
            get { return _reInstallURL; }
            set { _reInstallURL = value; OnPropertyChanged("ReInstallURL"); }
        }
        
        private string _flushCDNUrl;
        public string FlushCDNUrl
        {
            get { return _flushCDNUrl; }
            set { _flushCDNUrl = value; OnPropertyChanged("FlushCDNUrl"); }
        }

        private string _uploadVersionUrl;
        public string UploadVersionUrl
        {
            get { return _uploadVersionUrl; }
            set { _uploadVersionUrl = value; OnPropertyChanged("UploadVersionUrl"); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { 
                _description = value;
                OnPropertyChanged("Description"); 
            }
        }

        private bool _isUseCDN = false;
        public bool IsUseCDN
        {
            get { return _isUseCDN; }
            set
            {
                _isUseCDN = value;
                GlobalVM.Instance.EncodeAndroid = value;
                OnPropertyChanged("IsUseCDN");
            }
        }

        private string _cndUserName = "sqsj";
        public string CDNUserName
        {
            get { return _cndUserName; }
            set { _cndUserName = value; OnPropertyChanged("CDNUserName"); }
        }

        private string _cdnPassword = "Abc@123";
        public string CDNPassword
        {
            get { return _cdnPassword; }
            set { _cdnPassword = value; OnPropertyChanged("CDNPassword"); }
        }

        private ObservableCollection<string> _CDNList;
        public ObservableCollection<string> CDNList
        {
            get { return _CDNList; }
            set { _CDNList = value; OnPropertyChanged("CDNList"); }
        }

        private string _CDNCompany = "无";
        public string CDNCompany
        {
            get { return _CDNCompany; }
            set 
            { 
                _CDNCompany = value; 
                OnPropertyChanged("CDNCompany");
                if (value == "无" || value == "")
                    PackageSubVM.Instance.IsUseCDN = false;
                else
                    PackageSubVM.Instance.IsUseCDN = true;
            }
        }

        public void updateDescription(string str)
        {
            Description = str + "\n" + Description;
        }

        //打开设置界面
        private Window SettingDialog = null;
        private ICommand _openSettingDialog;
        public ICommand OpenSettingDialog
        {
            get
            {
                if (_openSettingDialog == null)
                {
                    _openSettingDialog = new OpenSettingDialogCommand();
                }
                return _openSettingDialog;
            }
            set { _openSettingDialog = value; }
        }

        private class OpenSettingDialogCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                PackageSubVM.Instance.SettingDialog = new PackageSetting();
                PackageSubVM.Instance.SettingDialog.ShowDialog();
            }
        }
        
        // 打开导出目录
        private ICommand _openOutPutPath;
        public ICommand OpenOutPutPath 
        { 
            get 
            {
                if (_openOutPutPath == null)
                    _openOutPutPath = new OpenOutPutPathCommand();
                return _openOutPutPath;
            }
            set { _openOutPutPath = value; }
        }

        private class OpenOutPutPathCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                Process.Start("explorer.exe", PackageSubVM.Instance.OutPath);
            }
        }

        // 上传文件到CDN
        private ICommand _uploadFileToCDN;
        public ICommand UploadFileToCDN
        {
            get
            {
                if (_uploadFileToCDN == null)
                    _uploadFileToCDN = new UploadFileToCDNCommand();
                return _uploadFileToCDN;
            }
            set { _uploadFileToCDN = value; }
        }

        public string szLastVersion = "";
        private class UploadFileToCDNCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                if (PackageSubVM.Instance.szLastVersion == "")
                {
                    MessageBox.Show("没有生成版本文件.");
                    return;
                }
                bool bUploadResult = true;
                FtpHelper ftp = new FtpHelper("113.107.167.229", "root", "#!9BVAPlDJ2%Nj@z");
                bUploadResult = bUploadResult && ftp.sftpUpload(PackageSubVM.Instance.OutPath + @"\versions", PackageSubVM.Instance.UploadVersionUrl + "/versions");
                string editionFileName = PackageSubVM.Instance.szLastVersion + "-" + PackageSubVM.Instance.Version;
                bUploadResult = bUploadResult && ftp.sftpUpload(PackageSubVM.Instance.szDifferentOutPath + @"\" + editionFileName + ".zip", PackageSubVM.Instance.UploadVersionUrl + String.Format("/edition/{0}.zip", editionFileName));
                bUploadResult = bUploadResult && ftp.sftpUpload(PackageSubVM.Instance.szDifferentOutPath + @"\" + editionFileName + ".xml", PackageSubVM.Instance.UploadVersionUrl + String.Format("/edition/{0}.xml", editionFileName));
                if (bUploadResult)
                {
                    MessageBox.Show("上传成功!");
                }
                else
                {
                    MessageBox.Show("上传失败, 请重新上传.");
                }
            }
        }

        // 刷新CDN
        private ICommand _flushCDN;
        public ICommand FlushCDN
        {
            get
            {
                if (_flushCDN == null)
                    _flushCDN = new FlushCDNCommand();
                return _flushCDN;
            }
            set { _flushCDN = value; }
        }

        private string requestResult;
        private class FlushCDNCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                if (true || PackageSubVM.Instance.FlushCDNUrl != "")
                {
                    FtpHelper ftp = new FtpHelper();
                    PackageSubVM.Instance.requestResult = ftp.httpRequest("POST", PackageSubVM.Instance.FlushCDNUrl, "");
                    //ftp.TencentCDNRequest("asldkfj");
                }
                else
                {
                    string url = "http://push.dnion.com/cdnUrlPush.do?captcha=436bd644&type=0&url=http://shenqu.cdn.feiliu.com/update/";
                    System.Diagnostics.Process.Start(url);
                }
            }
        }

        //检查CDN刷新结果
        private ICommand _checkFlushResult;
        public ICommand CheckFlushResult
        {
            get
            {
                if (_checkFlushResult == null)
                    _checkFlushResult = new CheckFlushResultCommand();
                return _checkFlushResult;
            }
            set { _checkFlushResult = value; }
        }

        private class CheckFlushResultCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                FtpHelper ftp = new FtpHelper();
                bool ok = ftp.httpRequest("GET", "", PackageSubVM.Instance.requestResult) == "True";
                if (!ok)
                {
                    PackageSubVM.Instance.CheckFlushTimer = new System.Timers.Timer();
                    PackageSubVM.Instance.CheckFlushTimer.Elapsed += new System.Timers.ElapsedEventHandler(dt_Tick);
                    PackageSubVM.Instance.CheckFlushTimer.Interval = 20000;
                    PackageSubVM.Instance.CheckFlushTimer.Enabled = true;
                }
                else
                {
                    MessageBox.Show("刷新成功!");
                }
            }
        }

        System.Timers.Timer CheckFlushTimer;
        public static void dt_Tick(object sender, EventArgs e)
        {
            // check flush
            FtpHelper ftp = new FtpHelper();
            bool ok = ftp.httpRequest("GET", "", PackageSubVM.Instance.requestResult) == "True";
            if (ok)
            {
                PackageSubVM.Instance.CheckFlushTimer.Dispose();
                MessageBox.Show("刷新成功!");
            }
        }

        // 生成热更包
        private ICommand _generalUpdatePackage;
        public ICommand GeneralUpdatePackage
        {
            get
            {
                if (_generalUpdatePackage == null)
                    _generalUpdatePackage = new GeneralUpdatePackageCommand();
                return _generalUpdatePackage;
            }
            set { _generalUpdatePackage = value; }
        }

        private class GeneralUpdatePackageCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                GenerateUpdatePackage logic = new GenerateUpdatePackage();
                logic.doGenerate();
            }
        }

        private ICommand _settingOK;
        public ICommand SettingOK
        {
            get
            {
                if (_settingOK == null)
                {
                    _settingOK = new SettingOKCommand();
                }
                return _settingOK;
            }
            set { _settingOK = value; }
        }
        private class SettingOKCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                if (PackageSubVM.Instance.SVNPath == "")
                {
                    MessageBox.Show("SVN 路径不能为空!");
                    return;
                }
                PackageSubVM.Instance.SettingDialog.Close();
            }
        }
        #endregion

        #region  界面数据初始化
        public void saveData()
        {
            SaveData.GetInstance().setData("szOutPath", PackageSubVM.Instance.OutPath);
            SaveData.GetInstance().setData("szFilter", PackageSubVM.Instance.Filter);
            SaveData.GetInstance().setData("szPackageURL", PackageSubVM.Instance.ReInstallURL);
            SaveData.GetInstance().setData("szFlushCDNUrl", PackageSubVM.Instance.FlushCDNUrl);
            SaveData.GetInstance().setData("szUploadVersionUrl", PackageSubVM.Instance.UploadVersionUrl);
            SaveData.GetInstance().setData("szVersionPath", PackageSubVM.Instance.SVNPath);
            SaveData.GetInstance().setData("szBeginVersion", PackageSubVM.Instance.BeginVersion);
            SaveData.GetInstance().setData("szEndVersion", PackageSubVM.Instance.EndVersion);
            SaveData.GetInstance().setData("szEncodeType", PackageSubVM.Instance.PackageAndroid.ToString());
            SaveData.GetInstance().setData("IsUseCDN", PackageSubVM.Instance.IsUseCDN.ToString());
            SaveData.GetInstance().setData("szCDNUserName", PackageSubVM.Instance.CDNUserName);
            SaveData.GetInstance().setData("szCDNPassword", PackageSubVM.Instance.CDNPassword);
            SaveData.GetInstance().setData("szCDNCompany", PackageSubVM.Instance.CDNCompany);
        }

        public void loadData(string configName)
        {
            SaveData.GetInstance().loadConfig(configName);
            PackageSubVM.Instance.OutPath = SaveData.GetInstance().getData("szOutPath");
            PackageSubVM.Instance.Filter = SaveData.GetInstance().getData("szFilter");
            PackageSubVM.Instance.ReInstallURL = SaveData.GetInstance().getData("szPackageURL");
            PackageSubVM.Instance.FlushCDNUrl = SaveData.GetInstance().getData("szFlushCDNUrl");
            PackageSubVM.Instance.UploadVersionUrl = SaveData.GetInstance().getData("szUploadVersionUrl");
            PackageSubVM.Instance.SVNPath = SaveData.GetInstance().getData("szVersionPath");
            PackageSubVM.Instance.BeginVersion = SaveData.GetInstance().getData("szBeginVersion");
            PackageSubVM.Instance.EndVersion = SaveData.GetInstance().getData("szEndVersion");
            PackageSubVM.Instance.PackageAndroid = SaveData.GetInstance().getData("szEncodeType") == "True";
            PackageSubVM.Instance.IsUseCDN = SaveData.GetInstance().getData("IsUseCDN") == "True";
            PackageSubVM.Instance.CDNUserName = SaveData.GetInstance().getData("szCDNUserName");
            PackageSubVM.Instance.CDNPassword = SaveData.GetInstance().getData("szCDNPassword");
            PackageSubVM.Instance.CDNCompany = SaveData.GetInstance().getData("szCDNCompany");
        }
        #endregion
    }

    //[ValueConversion(typeof(bool), typeof(bool))]
    public class RadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class ButtonEnabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as String);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as String);
        }
    }
}
