using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//XML
using System.Xml;

namespace DragAndRun
{
    class SaveData
    {
        private static SaveData _data;
        private static XmlDocument xmlDoc;
        private static XmlElement root;
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
                    xmlDoc = new XmlDocument();
                    XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xmlDoc.AppendChild(xmldecl);
                    root = xmlDoc.CreateElement("data");
                    xmlDoc.AppendChild(root);
                    xmlDoc.Save("localData.xml");
                }
                else
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load("localData.xml");
                    root = xmlDoc.DocumentElement;
                }
	        }
            return _data;
        }
        public void setData(string key, string value)
        {
            foreach (XmlElement childElement in root)
            {
                if (childElement.Name == key)
                {
                    childElement.InnerText = value;
                    xmlDoc.Save("localData.xml");
                    return;
                }
            }
            XmlElement keyElement = xmlDoc.CreateElement(key);
            keyElement.InnerText = value;
            root.AppendChild(keyElement);
            xmlDoc.Save("localData.xml");
        }

        public string getData(string key)
        {
            foreach (XmlElement childElement in root)
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
