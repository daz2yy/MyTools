using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

//MD5
using System.Security.Cryptography;
//XML
using System.Xml;
using System.Windows;
using System.Diagnostics;
//svn
using SharpSvn;

namespace DragAndRun.FileFilter
{
    public class CFileFilter
    {
        public CFileFilter()
        {
        }
        private string szVersion = "";
        private string szResourcePath = "";
        private string szOutputPath = "";
        private string szHistoryPath = "";
        private string szHistoryOutPath = "";
        private string szDifferentOutPath = "";
        private string szResourceOutPath = "";
        private string szUploadZipOutPath = "";
        private string[] szFilter;
        private MainWindow window;
        private string nBeginVersion;
        private string nEndVersion;
        private string szSVNPath;
        private bool bUseSVN;
        public void setVersion(string version)
        {
            szVersion = version;
        }

        public void setResourcePath(string path)
        {
            szResourcePath = path;
        }

        public void setOutPutPath(string path)
        {
            szOutputPath = path;
        }

        public void setHistoryPath(string path)
        {
            szHistoryPath = path;
        }

        public void setOutHistoryPath(string path)
        {
            szHistoryOutPath = path;
        }

        public void setOutDifferentPath(string path)
        {
            szDifferentOutPath = path;
        }

        public void setOutResourcePath(string path)
        {
            szResourceOutPath = path;
        }

        public void setOutUploadZipPath(string path)
        {
            szUploadZipOutPath = path;
        }

        public void setUseSVN(bool bUse, string nBeginVer, string nEndVer, string szSVN)
        {
            bUseSVN = bUse;
            nBeginVersion = nBeginVer;
            nEndVersion = nEndVer;
            szSVNPath = szSVN;
        }

        public void setFileFilter(string str)
        {
            szFilter = str.Split(new char[1]{';'});
        }

        public void setWindowHandler(MainWindow win)
        {
            window = win;
        }

        public void genCurVersionXML()
        {
            curFileList = new List<string>();
            curMD5List = new List<string>();
            curSizeList = new List<long>();
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmldecl);
            XmlElement root = doc.CreateElement("files");
            root.SetAttribute("version", szVersion);
            doc.AppendChild(root);

            if (!bUseSVN)
            {
                window.updateDescription("读取资源: " + szResourcePath);
                DirectoryInfo TheFolder = new DirectoryInfo(szResourcePath);
                this.TraversalDirector(TheFolder, doc, root);
                window.updateDescription("\n完成读取资源.");
                doc.Save(szHistoryPath + @"\version_" + szVersion + ".xml");
                this.genDifferentXml_new();
                this.outPutResource();
            }
            else
            {
                this.getSVNDifferFiles();
            }
            this.encodeResource();
            this.zipOutPutResource();
            this.saveCurrentVersion();
        }
        private List<string> curFileList = new List<string>();
        private List<string> curMD5List = new List<string>();
        private List<long> curSizeList = new List<long>();
        private void TraversalDirector(DirectoryInfo TheFolder, XmlDocument doc, XmlElement root)
        {
            string relativeFilePath = "";
            string szMD5 = "";
            long size = 0;
            bool bNeedFilter = true;
            //遍历文件
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                bNeedFilter = false;
                for (int i = 0; i < szFilter.Count(); i++)
                {
                    if (szFilter[i] == NextFile.Extension || szFilter[i] == NextFile.Name)
                    {
                        bNeedFilter = true;
                        break;
                    }
                }
                if (bNeedFilter)
                {
                    continue;
                }
                relativeFilePath = NextFile.FullName.Replace(szResourcePath + "\\", "").Replace("\\", "/");
                szMD5 = this.createFileMD5(NextFile.FullName, out size);
                this.createXmlNode(doc, root, relativeFilePath, szMD5);
                curFileList.Add(relativeFilePath);
                curMD5List.Add(szMD5);
                curSizeList.Add(size);
            }
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                this.TraversalDirector(NextFolder, doc, root);
            }
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

        void createXmlNode(XmlDocument doc, XmlElement root, string fileName, string md5, string change = "")
        {
            XmlElement node = doc.CreateElement("f");
            node.SetAttribute("p", fileName);
            node.SetAttribute("m", md5);
            if (change != "")
            {
                node.SetAttribute("c", change);
            }
            root.AppendChild(node);
        }

        //记录有用的文件
        private List<string> finalResource;
        private string szLastVersion = "";
        //生成差异文件
        private void genDifferentXml_new()
        {
            //输出
            string outputPath = szDifferentOutPath;
            if (Directory.Exists(outputPath) == false)
            {
                Directory.CreateDirectory(outputPath);
            }
            finalResource = new List<string>();

            window.updateDescription("\n和以前的版本对比,生成差异文件:");
            if (Directory.Exists(szHistoryPath) == false)
            {
                Directory.CreateDirectory(szHistoryPath);
            }
            DirectoryInfo TheFolder = new DirectoryInfo(szHistoryPath);
            FileInfo NextFile = null;
            List<string> oldFileList = new List<string>();
            List<string> oldMD5List = new List<string>();
            XmlDocument oldDoc = null;
            szLastVersion = "";
            foreach (FileInfo File in TheFolder.GetFiles())
            {
                XmlDocument tempDoc = new XmlDocument();
                tempDoc.Load(File.FullName);
                XmlElement oldRoot = tempDoc.DocumentElement;
                string oldVersion = oldRoot.GetAttribute("version");
                if (szLastVersion.CompareTo(oldVersion) < 0 && oldVersion != szVersion)
                {
                    szLastVersion = oldVersion;
                    NextFile = new FileInfo(File.FullName);
                    oldDoc = tempDoc;
                }
            }
            if (NextFile == null)
            {
                return;
            }
            this.readXML(oldDoc, NextFile.FullName, out oldFileList, out oldMD5List);

            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmldecl = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            newDoc.AppendChild(xmldecl);
            XmlElement newRoot = newDoc.CreateElement("files");
            newDoc.AppendChild(newRoot);
            newRoot.SetAttribute("version", szVersion);

            window.updateDescription("\n对比文件: " + NextFile.FullName);
            long fileSizes = 0;
            for (int i = 0; i < curFileList.Count; i++)
            {
                if (oldFileList.IndexOf(curFileList[i]) == -1)
                {
                    this.createXmlNode(newDoc, newRoot, curFileList[i], curMD5List[i], "0");
                    fileSizes += curSizeList[i];
                    if (finalResource.IndexOf(curFileList[i]) == -1)
                    {
                        finalResource.Add(curFileList[i]);   
                    }
                }
                else
                {
                    int index = oldFileList.IndexOf(curFileList[i]);
                    if (oldMD5List[index] != curMD5List[i])
                    {
                        this.createXmlNode(newDoc, newRoot, curFileList[i], curMD5List[i], "0");
                        oldFileList.RemoveAt(index);
                        oldMD5List.RemoveAt(index);
                        fileSizes += curSizeList[i];
                        if (finalResource.IndexOf(curFileList[i]) == -1)
                        {
                            finalResource.Add(curFileList[i]);
                        }
                    }
                }
            }
            int k = 0;
            for (int i = 0; i < oldFileList.Count; i++)
            {
                //删除的资源
                if (curFileList.IndexOf(oldFileList[k]) == -1)
                {
                    this.createXmlNode(newDoc, newRoot, oldFileList[k], oldMD5List[i], "1");
                }
                k = k + 1;
            }
            newRoot.SetAttribute("lastVersion", szLastVersion);
            newRoot.SetAttribute("size", fileSizes.ToString());
            newDoc.Save(outputPath + @"\" + szLastVersion + "-" + szVersion + ".xml");
            window.updateDescription("\n完成该文件对比, 输出到: " + outputPath + @"\" + szLastVersion + "-" + szVersion + ".xml");
            window.updateDescription("\n完成对比,生成差异文件.");
        }
        private void readXML(XmlDocument doc, string filePath, out List<string> outFileList, out List<string> outMD5List)
        {
            List<string> fileList = new List<string>();
            List<string> md5List = new List<string>();
            XmlElement root = doc.DocumentElement;

            List<string> list = new List<string>();
            foreach (XmlElement childElement in root)
            {
                fileList.Add(childElement.GetAttribute("p"));
                md5List.Add(childElement.GetAttribute("m"));
            }
            outFileList = fileList;
            outMD5List = md5List;
        }

        //输出资源
        private void outPutResource()
        {
            if (Directory.Exists(szResourceOutPath) == true)
            {
                Directory.Delete(szResourceOutPath, true);
            }
            Directory.CreateDirectory(szResourceOutPath);
            window.updateDescription("\n开始输出资源到:" + szResourceOutPath);
            for (int i = 0; i < finalResource.Count; i++)
            {
                string path = Path.GetDirectoryName(szResourceOutPath + "\\" + finalResource[i]);
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                System.IO.File.Copy(szResourcePath + "\\" + finalResource[i], szResourceOutPath + "\\" + finalResource[i]);
            }
            window.updateDescription("\n输出资源完成:" + finalResource.Count +　"个文件.");
        }

        private void encodeResource()
        {
            window.updateDescription("\n开始加密输出的资源");
            window.encodeLua(szResourceOutPath, szResourceOutPath);
            window.encodeImage(szResourceOutPath, szResourceOutPath);
            string preStr = "Android分包";
            if (window.subEncodIOSBtn.IsChecked == true)
            {
                preStr = "IOS分包";
            }
            window.updateDescription("\n" + preStr + "完成加密.");
        }

        private void zipOutPutResource()
        {
            if (System.IO.Directory.Exists(szDifferentOutPath))
            {
                System.IO.Directory.Delete(szDifferentOutPath, true);
                System.IO.Directory.CreateDirectory(szDifferentOutPath);
            }
            window.updateDescription("\n开始压缩文件:");
            //add to zip
            string executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
            executeFilePath = executeFilePath + @"\WinRAR";
            string outFileFullName = string.Format(@"{0}\{1}-{2}.zip", szDifferentOutPath, szLastVersion, szVersion);
            Process p = new Process();
            p.StartInfo.FileName = executeFilePath;
            p.StartInfo.Arguments = string.Format(@"m -r -afzip -ed -m3 -ep1 {0} {1}", outFileFullName, szResourceOutPath + @"\*");
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
            System.IO.Directory.Delete(szResourceOutPath, true);
            window.updateDescription("\n压缩文件完成:" + outFileFullName);
        }

        private void saveCurrentVersion()
        {
            // new: save version to file
            string content = "";
            if (System.IO.File.Exists(szOutputPath + @"\versions"))
            {
                StreamReader sr = new StreamReader(szOutputPath + @"\versions");
                while (sr.Peek() > 0)
                {
                    string line = sr.ReadLine();
                    if (!line.Contains(szVersion))
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
                FileStream file = System.IO.File.Create(szOutputPath + @"\versions");
                file.Close();
            }

            using (FileStream fs = new FileStream(szOutputPath + @"\versions", FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    string outString = "version:" + szVersion;
                    if (window.removeDebug.IsChecked == true)
                    {
                        outString += "," + "RemoveMEDebug";
                    }
                    if (window.removeAll.IsChecked == true)
                    {
                        outString += "," + "RemoveAll";
                    }
                    if (window.needInstall.IsChecked == true)
                    {
                        outString += "," + "NeedReInstall:" + window.comPackageURL.Text + " ";
                    }
                    sw.Write(content + outString + ";\n");
                }
            }
        }

        //================================================ use svn ================================================
        //获取最大的版本号, 保存在szLastVersion
        private bool getLastVersion()
        {
            string outputPath = szDifferentOutPath;
            if (Directory.Exists(outputPath) == false)
            {
                Directory.CreateDirectory(outputPath);
            }

            window.updateDescription("\n和以前的版本对比,生成差异文件:");
            if (Directory.Exists(szHistoryPath) == false)
            {
                Directory.CreateDirectory(szHistoryPath);
            }
            DirectoryInfo TheFolder = new DirectoryInfo(szHistoryPath);
            FileInfo NextFile = null;
            List<string> oldFileList = new List<string>();
            List<string> oldMD5List = new List<string>();
            XmlDocument oldDoc = null;
            szLastVersion = "";
            foreach (FileInfo File in TheFolder.GetFiles())
            {
                XmlDocument tempDoc = new XmlDocument();
                tempDoc.Load(File.FullName);
                XmlElement oldRoot = tempDoc.DocumentElement;
                string oldVersion = oldRoot.GetAttribute("version");
                if (szLastVersion.CompareTo(oldVersion) < 0 && oldVersion != szVersion)
                {
                    szLastVersion = oldVersion;
                    NextFile = new FileInfo(File.FullName);
                    oldDoc = tempDoc;
                }
            }
            if (NextFile == null)
            {
                return false;
            }
            return true;
        }

        public void getSVNDifferFiles()
        {
            // diff xml
            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmldecl = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            newDoc.AppendChild(xmldecl);
            XmlElement newRoot = newDoc.CreateElement("files");
            newDoc.AppendChild(newRoot);
            newRoot.SetAttribute("version", szVersion);

            long fileSizes = 0;
            SvnClient svnClient = new SvnClient();
            //List<SvnDiffSummaryEventArgs> list;
            System.Collections.ObjectModel.Collection<SvnDiffSummaryEventArgs> list;
            svnClient.GetDiffSummary(new SvnUriTarget(szSVNPath, Int32.Parse(nBeginVersion)), new SvnUriTarget(szSVNPath, Int32.Parse(nEndVersion)), out list);
            foreach (SvnDiffSummaryEventArgs item in list)
            {
                if (szFilter.Contains(Path.GetExtension(item.Path)))
                {
                    continue;
                }

                string fullFileName = szResourceOutPath + "\\" + item.Path;
                string outPath = Path.GetDirectoryName(fullFileName);
                if (Directory.Exists(outPath) == false)
                {
                    Directory.CreateDirectory(outPath);
                }
                try
                {
                    SvnClient exportSVN = new SvnClient();
                    SvnExportArgs ex = new SvnExportArgs();
                    ex.Overwrite = true;
                    long size = 0;
                    if (item.DiffKind != SvnDiffKind.Deleted)
                    { 
                        exportSVN.Export(new SvnUriTarget(item.ToUri, Int32.Parse(nEndVersion)), fullFileName, ex);
                        this.createXmlNode(newDoc, newRoot, item.Path, this.createFileMD5(fullFileName, out size), "1");
                        fileSizes += size;
                    }
                    else
                    {
                        this.createXmlNode(newDoc, newRoot, item.Path, this.createFileMD5(fullFileName, out size), "0");
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
            if (Directory.Exists(szDifferentOutPath) == false)
            {
                Directory.CreateDirectory(szDifferentOutPath);
            }
            //获取最大的版本号
            if (this.getLastVersion() == false)
            {
                return;
            }
            newDoc.Save(szDifferentOutPath + @"\" + szLastVersion + "-" + szVersion + ".xml");
        }
    }
}
