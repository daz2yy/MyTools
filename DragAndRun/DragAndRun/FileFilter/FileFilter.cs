using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

//MD5
using System.Security.Cryptography;
//XML
using System.Xml;
using System.Windows;

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
        private string[] szFilter;
        private MainWindow window;
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

            window.updateDescription("读取资源: " + szResourcePath);
            DirectoryInfo TheFolder = new DirectoryInfo(szResourcePath);
            this.TraversalDirector(TheFolder, doc, root);
            window.updateDescription("\n完成读取资源.");
            
            this.genDifferentXml(doc);
            this.outPutResource();
            this.encodeResource();
            doc.Save(szHistoryPath + @"\version_" + szVersion + ".xml");
        }
        private List<string> curFileList = new List<string>();
        private List<string> curMD5List = new List<string>();
        private List<long> curSizeList = new List<long>();
        private void TraversalDirector(DirectoryInfo TheFolder, XmlDocument doc, XmlElement root)
        {
            string relativeFilePath = "";
            string szMD5 = "";
            long size = 0;
            //遍历文件
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                for (int i = 0; i < szFilter.Count(); i++)
                {
                    if (szFilter[i] == NextFile.Extension)
                    {
                        continue;
                    }
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
        //生成差异文件
        private void genDifferentXml(XmlDocument curDoc)
        {
            //输出
            string outputPath = szDifferentOutPath + @"\" + szVersion;
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
            //遍历文件
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                XmlDocument newDoc = new XmlDocument();
                XmlDeclaration xmldecl = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                newDoc.AppendChild(xmldecl);
                XmlElement newRoot = newDoc.CreateElement("files");
                newDoc.AppendChild(newRoot);
                newRoot.SetAttribute("version", szVersion);
                
                List<string> oldFileList = new List<string>();
                List<string> oldMD5List = new List<string>();
                XmlDocument oldDoc = new XmlDocument();
                oldDoc.Load(NextFile.FullName);
                XmlElement oldRoot = oldDoc.DocumentElement;
                this.readXML(oldDoc, NextFile.FullName, out oldFileList, out oldMD5List);
                string oldVersion = oldRoot.GetAttribute("version");
                if (szVersion.CompareTo(oldVersion) <= 0)
                {
                    continue;
                }

                window.updateDescription("\n对比文件: " + NextFile.FullName);
                long fileSizes = 0;
                for (int i = 0; i < curFileList.Count; i++)
                {
                    if (oldFileList.IndexOf(curFileList[i]) == -1)
                    {
                        this.createXmlNode(newDoc, newRoot, curFileList[i], curMD5List[i], "0");
                        fileSizes += curSizeList[i];
                        finalResource.Add(curFileList[i]);
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
                            finalResource.Add(curFileList[i]);
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
                newRoot.SetAttribute("lastVersion", oldVersion);
                newRoot.SetAttribute("size", fileSizes.ToString());
                newDoc.Save(outputPath + @"\" + oldVersion + "-" + szVersion + ".xml");
                window.updateDescription("\n完成该文件对比, 输出到: " + outputPath + @"\" + oldVersion + "-" + szVersion + ".xml");
            }
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
            window.updateDescription("\n完成加密.");
        }
    }
}
