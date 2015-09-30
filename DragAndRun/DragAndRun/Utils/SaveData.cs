using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//XML
using System.Xml;

namespace DragAndRun
{
    class SaveData
    {
        private static SaveData _data;
        private static XmlDocument _configXmlDoc;
        private static XmlElement _configRoot;
        private static XmlDocument _globalXmlDoc;
        private static XmlElement _globalRoot;
        private static string _configName;
        private SaveData()
        {

        }

        public static SaveData GetInstance()
        {
            if (_data == null)
	        {
                _data = new SaveData();
                if (System.IO.File.Exists("localData.xml") == false)
                {
                    _globalXmlDoc = new XmlDocument();
                    XmlDeclaration xmldecl = _globalXmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    _globalXmlDoc.AppendChild(xmldecl);
                    _globalRoot = _globalXmlDoc.CreateElement("data");
                    _globalXmlDoc.AppendChild(_globalRoot);
                    _globalXmlDoc.Save("localData.xml");
                }
                else
                {
                    _globalXmlDoc = new XmlDocument();
                    _globalXmlDoc.Load("localData.xml");
                    _globalRoot = _globalXmlDoc.DocumentElement;
                }
	        }
            return _data;
        }
        public void loadConfig(string fileName)
        {
            if (System.IO.File.Exists("localData.xml") == false)
            {
                MessageBox.Show("配置文件不存在, 重新创建一个:" + fileName);
                return;
            }
            _configXmlDoc = new XmlDocument();
            _configXmlDoc.Load("PackageConfig/" + fileName + ".xml");
            _configRoot = _configXmlDoc.DocumentElement;
            _configName = "PackageConfig/" + fileName + ".xml";
        }

        public void addConfig(string fileName)
        {
            _configXmlDoc = new XmlDocument();
            XmlDeclaration xmldecl = _configXmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            _configXmlDoc.AppendChild(xmldecl);
            _configRoot = _configXmlDoc.CreateElement("data");
            _configXmlDoc.AppendChild(_configRoot);
            if (!System.IO.Directory.Exists("PackageConfig"))
            {
                System.IO.Directory.CreateDirectory("PackageConfig");
            }
            _configXmlDoc.Save("PackageConfig/" + fileName + ".xml");
            _configName = "PackageConfig/" + fileName + ".xml";
        }
        public void setData(string key, string value)
        {
            foreach (XmlElement childElement in _configRoot)
            {
                if (childElement.Name == key)
                {
                    childElement.InnerText = value;
                    _configXmlDoc.Save(_configName);
                    return;
                }
            }
            XmlElement keyElement = _configXmlDoc.CreateElement(key);
            keyElement.InnerText = value;
            _configRoot.AppendChild(keyElement);
            _configXmlDoc.Save(_configName);
        }

        public string getData(string key)
        {
            foreach (XmlElement childElement in _configRoot)
            {
                if (childElement.Name == key)
                {
                    return childElement.InnerText;
                }
            }
            return "";
        }

        public void setGlobalData(string key, string value)
        {
            foreach (XmlElement childElement in _globalRoot)
            {
                if (childElement.Name == key)
                {
                    childElement.InnerText = value;
                    _globalXmlDoc.Save("localData.xml");
                    return;
                }
            }
            XmlElement keyElement = _globalXmlDoc.CreateElement(key);
            keyElement.InnerText = value;
            _globalRoot.AppendChild(keyElement);
            _globalXmlDoc.Save("localData.xml");
        }

        public string getGlobalData(string key)
        {
            foreach (XmlElement childElement in _globalRoot)
            {
                if (childElement.Name == key)
                {
                    return childElement.InnerText;
                }
            }
            return "";
        }
    }
}
