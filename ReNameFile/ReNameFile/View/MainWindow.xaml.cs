using System;
using System.Collections.Generic;
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

namespace ReNameFile
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
        private bool bIslegal()
        {
            if (String.IsNullOrEmpty(Address.Text)
                || String.IsNullOrEmpty(From.Text)
                || String.IsNullOrEmpty(To.Text))
            {
                return false;
            }
            return true;
        }
        private void ReplaceFile(object sender, RoutedEventArgs e)
        {
            //Address.Text = @"D:\Study\C#\ReNameFile\text";
            //From.Text = @"2";
            //To.Text = @"123";
            if (!bIslegal())
            {
                return;
            }
            DirectoryInfo TheFolder = new DirectoryInfo(Address.Text);
            foreach (FileInfo item in TheFolder.GetFiles())
            {
                String fullPath = item.FullName.Replace("/", @"\");
                String path = fullPath.Substring(0, fullPath.LastIndexOf(@"\")+1);
                String extension = System.IO.Path.GetExtension(item.Name);
                String fileName = System.IO.Path.GetFileNameWithoutExtension(item.Name);
                String newName = fileName.Replace(From.Text, To.Text);
                if (fileName != newName)
                {
                    File.Move(item.FullName, path + newName + extension);
                }
            }
        }

        private void ReplaceFolder(object sender, RoutedEventArgs e)
        {
            if (!bIslegal())
            {
                return;
            }
            DirectoryInfo TheFolder = new DirectoryInfo(Address.Text);
            foreach (DirectoryInfo item in TheFolder.GetDirectories())
            {
                String fullPath = item.FullName.Replace("/", @"\");
                String path = fullPath.Substring(0, fullPath.LastIndexOf(@"\") + 1);
                String newName = item.Name.Replace(From.Text, To.Text);
                if (item.Name != newName)
                {
                    Directory.Move(item.FullName, path + newName);
                }
            }
        }
    }
}
