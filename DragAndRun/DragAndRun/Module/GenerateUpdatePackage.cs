using DragAndRun.Utils;
using DragAndRun.ViewModule;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DragAndRun.Module
{
    class GenerateUpdatePackage
    {
        public string szLastVersion = "";

        public void doGenerate()
        {
            if (PackageSubVM.Instance.BeginVersion == "" || PackageSubVM.Instance.EndVersion == "" || PackageSubVM.Instance.SVNPath == "")
            {
                MessageBox.Show("SVN信息没填完整!");
                return;
            }
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            if (!window.checkProjectSelect())
            {
                MessageBox.Show("请选着需要打包的项目");
                return;
            }
            PackageSubVM.Instance.Description = "======================= 开始打热更包 =============================";
            //save data
            PackageSubVM.Instance.saveData();
            //new run
            Thread executeThread = new Thread(new ThreadStart(this.testAsync));
            executeThread.Start();
        }

        public void testAsync()
        {
            PackageSubVM.Instance.LoadingPanel = System.Windows.Visibility.Visible;
            PackageSubVM.Instance.szLastVersion = this.beginPackage();
            string preStr = "Android分包";
            if (!PackageSubVM.Instance.PackageAndroid)
            {
                preStr = "IOS分包";
            }
            MessageBox.Show(preStr + "打包完成.");
            PackageSubVM.Instance.LoadingPanel = System.Windows.Visibility.Hidden;
        }

        public string beginPackage()
        {
            szLastVersion = "";
            // get update file
            XmlDocument editionXML = this.getSVNDifferFiles();
            // encode
            this.encodeResource();
            // add to zip
            this.zipOutPutResource();
            // save update file name
            editionXML.Save(PackageSubVM.Instance.szDifferentOutPath + @"\" + szLastVersion + "-" + PackageSubVM.Instance.Version + ".xml");
            // save to version file
            this.saveCurrentVersion();
            return szLastVersion;
        }

        //================================================ use svn ================================================
        //获取最大的版本号, 保存在szLastVersion
        public void getLastVersion()
        {
            if (System.IO.File.Exists(PackageSubVM.Instance.OutPath + @"\versions"))
            {
                StreamReader sr = new StreamReader(PackageSubVM.Instance.OutPath + @"\versions");
                while (sr.Peek() > 0)
                {
                    string line = sr.ReadLine();
                    int begin = line.IndexOf(":");
                    int end = line.IndexOf(",");
                    if (end == -1)
                    {
                        end = line.IndexOf(";");
                    }
                    string version = line.Substring(begin + 1, end - begin - 1);
                    if (version.CompareTo(szLastVersion) > 0 && version.CompareTo(PackageSubVM.Instance.Version) < 0)
                        szLastVersion = version;
                }
                sr.Close();
            }
            else
            {
                FileStream file = System.IO.File.Create(PackageSubVM.Instance.OutPath + @"\versions");
                file.Close();
            }
        }

        public XmlDocument getSVNDifferFiles()
        {
            //获取最大的版本号
            this.getLastVersion();
            // diff xml
            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmldecl = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            newDoc.AppendChild(xmldecl);
            XmlElement newRoot = newDoc.CreateElement("files");
            newDoc.AppendChild(newRoot);
            newRoot.SetAttribute("version", PackageSubVM.Instance.Version);

            long fileSizes = 0;
            int index = 0;
            string[] szFilter = PackageSubVM.Instance.Filter.Split(new char[1] { ';' });
            SvnClient svnClient = new SvnClient();
            //List<SvnDiffSummaryEventArgs> list;
            System.Collections.ObjectModel.Collection<SvnDiffSummaryEventArgs> list;
            svnClient.GetDiffSummary(new SvnUriTarget(PackageSubVM.Instance.SVNPath, Int32.Parse(PackageSubVM.Instance.BeginVersion)), new SvnUriTarget(PackageSubVM.Instance.SVNPath, Int32.Parse(PackageSubVM.Instance.EndVersion)), out list);
            foreach (SvnDiffSummaryEventArgs item in list)
            {
                if (szFilter.Contains(Path.GetExtension(item.Path)) || szFilter.Contains(Path.GetFileName(item.Path)))
                {
                    continue;
                }

                string fullFileName = PackageSubVM.Instance.szResourceOutPath + "\\" + item.Path;
                if (item.NodeKind == SvnNodeKind.Directory)
                {
                    if (Directory.Exists(fullFileName) == false)
                    {
                        DirectoryInfo info = Directory.CreateDirectory(fullFileName);
                    }
                    continue;
                }

                string outPath = Path.GetDirectoryName(fullFileName);
                if (Directory.Exists(outPath) == false)
                {
                    DirectoryInfo info = Directory.CreateDirectory(outPath);
                }
                try
                {
                    SvnClient exportSVN = new SvnClient();
                    SvnExportArgs ex = new SvnExportArgs();
                    ex.Overwrite = true;
                    long size = 0;
                    if (item.DiffKind != SvnDiffKind.Deleted)
                    {
                        exportSVN.Export(new SvnUriTarget(item.ToUri, Int32.Parse(PackageSubVM.Instance.EndVersion)), fullFileName, ex);
                        index = index + 1;
                        this.createXmlNode(newDoc, newRoot, item.Path, this.createFileMD5(fullFileName, out size), "1", index);
                        fileSizes += size;
                    }
                    else
                    {
                        this.createXmlNode(newDoc, newRoot, item.Path, "this is a delete file.", "0", index);
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                    Console.WriteLine(ee.Message);
                }
            }

            newRoot.SetAttribute("lastVersion", szLastVersion);
            newRoot.SetAttribute("size", fileSizes.ToString());
            return newDoc;
        }

        void createXmlNode(XmlDocument doc, XmlElement root, string fileName, string md5, string change, int index)
        {
            XmlElement node = doc.CreateElement("file_" + index);
            node.SetAttribute("fileName", fileName);
            node.SetAttribute("md5", md5);
            if (change != "")
            {
                node.SetAttribute("change", change);
            }
            root.AppendChild(node);
        }

        private string createFileMD5(string filePath, out long size)
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(file);
            size = file.Length;
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private void encodeResource()
        {
            GlobalVM.Instance.updateDescription("\n开始加密输出的资源");
            FileEncode.Instance.encodeLua(PackageSubVM.Instance.szResourceOutPath, PackageSubVM.Instance.szResourceOutPath);
            FileEncode.Instance.encodeImage(PackageSubVM.Instance.szResourceOutPath, PackageSubVM.Instance.szResourceOutPath);
        }

        private void encodeResource_delegate()
        {
            bool bIsEncoding = true;
            GlobalVM.Instance.updateDescription("\n开始加密输出的资源");
            FileEncode.Instance.encodeLua(PackageSubVM.Instance.szResourceOutPath, PackageSubVM.Instance.szResourceOutPath,
                (n) => { bIsEncoding = false; });
            while (bIsEncoding)
                System.Threading.Thread.Sleep(500);
            bIsEncoding = true;
            FileEncode.Instance.encodeImage(PackageSubVM.Instance.szResourceOutPath, PackageSubVM.Instance.szResourceOutPath,
                    (n) =>
                    {
                        string preStr = "Android分包";
                        if (!PackageSubVM.Instance.PackageAndroid)
                        {
                            preStr = "IOS分包";
                        }
                        GlobalVM.Instance.updateDescription("\n" + preStr + "完成加密.");
                        bIsEncoding = false;
                    });
            while(bIsEncoding)
                System.Threading.Thread.Sleep(500);
        }

        private void zipOutPutResource()
        {
            GlobalVM.Instance.updateDescription("\n开始压缩文件:");
            string outFileFullName = "";
            if (System.IO.Directory.Exists(PackageSubVM.Instance.szResourceOutPath))
            {
                //add to zip
                string executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                executeFilePath = executeFilePath + @"\WinRAR";
                outFileFullName = string.Format(@"{0}\{1}-{2}.zip", PackageSubVM.Instance.szDifferentOutPath, szLastVersion, PackageSubVM.Instance.Version);
                Process p = new Process();
                p.StartInfo.FileName = executeFilePath;
                p.StartInfo.Arguments = string.Format(@"m -r -afzip -ed -m3 -ep1 {0} {1}", outFileFullName, PackageSubVM.Instance.szResourceOutPath + @"\*");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.ErrorDialog = false;
                p.Start();
                while (!p.HasExited)
                {
                    p.WaitForExit();
                }
                p.Close();
                p.Dispose();
                System.IO.Directory.Delete(PackageSubVM.Instance.szResourceOutPath, true);
            }
            else
            {
                GlobalVM.Instance.updateDescription("\n没有文件.:");
            }
            GlobalVM.Instance.updateDescription("\n压缩文件完成:" + outFileFullName);
        }

        private void saveCurrentVersion()
            {
                // new: save version to file
                string content = "";
                if (System.IO.File.Exists(PackageSubVM.Instance.OutPath + @"\versions"))
                {
                    StreamReader sr = new StreamReader(PackageSubVM.Instance.OutPath + @"\versions");
                    while (sr.Peek() > 0)
                    {
                        string line = sr.ReadLine();
                        if (!line.Contains(PackageSubVM.Instance.Version))
                        {
                            content += line + "\n";
                        }
                        else
                        {
                            MessageBox.Show("注意, 这是重复的版本号!!!");
                        }
                    }
                    sr.Close();
                }
                else
                {
                    FileStream file = System.IO.File.Create(PackageSubVM.Instance.OutPath + @"\versions");
                    file.Close();
                }

                using (FileStream fs = new FileStream(PackageSubVM.Instance.OutPath + @"\versions", FileMode.Truncate, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        string outString = "version:" + PackageSubVM.Instance.Version;
                        if (PackageSubVM.Instance.RemoveMEDebug)
                        {
                            outString += "," + "RemoveMEDebug";
                        }
                        //                     if (window.removeAll.IsChecked == true)
                        //                     {
                        //                         outString += "," + "RemoveAll";
                        //                     }
                        if (PackageSubVM.Instance.NeedReInstall == true)
                        {
                            outString += "," + "NeedReInstall:" + PackageSubVM.Instance.ReInstallURL + " ";
                        }
                        sw.Write(content + outString + ";\n");
                    }
                }
            }
    }
}
