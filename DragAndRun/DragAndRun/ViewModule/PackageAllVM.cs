using DragAndRun.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DragAndRun.ViewModule
{
    class PackageAllVM : INotifyPropertyChanged
    {
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

        private static PackageAllVM _instance;
        static public PackageAllVM Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PackageAllVM();
                }
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

        private bool _allPackageAndroid;
        public bool AllPackageAndroid
        {
            get { return _allPackageAndroid; }
            set
            {
                _allPackageAndroid = value;
                GlobalVM.Instance.EncodeAndroid = value;
                OnPropertyChanged("AllPackageAndroid");
            }
        }

        private bool _allPackageEncodeAll;
        public bool AllPackageEncodeAll
        {
            get { return _allPackageEncodeAll; }
            set
            {
                _allPackageEncodeAll = value;
                OnPropertyChanged("AllPackageEncodeAll");
            }
        }

        private bool _allPackageEncodeLua;
        public bool AllPackageEncodeLua
        {
            get { return _allPackageEncodeLua; }
            set
            {
                _allPackageEncodeLua = value;
                OnPropertyChanged("AllPackageEncodeLua");
            }
        }

        private bool _allPackageEncodeImage;
        public bool AllPackageEncodeImage
        {
            get { return _allPackageEncodeImage; }
            set
            {
                _allPackageEncodeImage = value;
                OnPropertyChanged("AllPackageEncodeImage");
            }
        }

        private string _outPutPath;
        public string OutPutPath
        {
            get { return _outPutPath; }
            set { _outPutPath = value; OnPropertyChanged("OutPutPath"); }
        }

        private string _allDescription;
        public string AllDescription
        {
            get { return _allDescription; }
            set { _allDescription = value; OnPropertyChanged("AllDescription"); }
        }
        public void updateDescription(string msg)
        {
            AllDescription = msg + "\n" + AllDescription;
        }

        #region 
        System.Array fileArray;
        public void handleDropItems(System.Array array)
        {
            fileArray = array;
            System.Threading.Thread executeThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.testAsync));
            executeThread.IsBackground = true;
            executeThread.Start();
        }

        public void testAsync()
        {
            System.Array array = fileArray;
            PackageAllVM.Instance.LoadingPanel = System.Windows.Visibility.Visible;
            string msg = "";
            for (int i = 0; i < array.Length; i++)
            {
                msg = array.GetValue(i).ToString();
                string inputPath = msg;

                string fileExt = System.IO.Path.GetExtension(msg);
                string outputPath = fileExt == "" ? msg : System.IO.Path.GetDirectoryName(msg);
                string path = PackageAllVM.Instance.OutPutPath;
                if (!String.IsNullOrEmpty(path))
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

                if (PackageAllVM.Instance.AllPackageEncodeAll)
                {
                    FileEncode.Instance.encodeLua(inputPath, outputPath);
                    FileEncode.Instance.encodeImage(inputPath, outputPath);
                }
                else if (PackageAllVM.Instance.AllPackageEncodeLua)
                {
                    FileEncode.Instance.encodeLua(inputPath, outputPath);
                }
                else if (PackageAllVM.Instance.AllPackageEncodeImage)
                {
                    FileEncode.Instance.encodeImage(inputPath, outputPath);
                }
            }
            MessageBox.Show("完成加密!");
            PackageAllVM.Instance.LoadingPanel = System.Windows.Visibility.Hidden;
        }
        #endregion
    }
}
