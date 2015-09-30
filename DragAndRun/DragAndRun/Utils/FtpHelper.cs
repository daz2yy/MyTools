using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Reflection;

namespace DragAndRun.Utils
{
    class FtpHelper
    {
        string ftpServerIP;
        string ftpUserID;
        string ftpPassword;
        string ftpURI;
        public FtpHelper(string serverIP, string userID, string passWord)
        {
            ftpServerIP = serverIP;
            ftpUserID = userID;
            ftpPassword = passWord;
            ftpURI = "ftp://" + ftpServerIP;
        }
        private static FtpWebRequest GetRequest(string URI, string username, string password)
        {
            //根据服务器信息FtpWebRequest创建类的对象
            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
            //提供身份验证信息
            result.Credentials = new System.Net.NetworkCredential(username, password);
            //设置请求完成之后是否保持到FTP服务器的控制连接，默认值为true
            result.KeepAlive = false;
            return result;
        }

        public void UpLoad(string fileNmae)
        {
            fileNmae = @"D:\1temp\DefaultRes.proto";
            FileInfo fileInfo = new FileInfo(fileNmae);
            string uri = ftpURI + ":16333/game/update";


            try
            {
                System.Net.FtpWebRequest ftp = GetRequest(uri, ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectory;

                FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();
                response.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("asdfklj");
            }




            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInfo.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInfo.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FtpWeb", "Upload Error --> " + ex.Message);
            }
        }

        public void webUpload()
        {
            try
            {
                WebClient myWebClient = new WebClient();
                string uri = ftpURI + ":16333/game/update/DefaultRes.proto";
                string fileNamePath = @"D:\1temp\DefaultRes.proto";
                myWebClient.UploadFile(uri, fileNamePath);
            }
            catch (Exception err)
            {
                MessageBox.Show("上传日志文件异常！");
            }


            return;
            //myWebClient.Credentials = CredentialCache.DefaultCredentials;
            //// 要上传的文件
            //FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            ////FileStream fs = OpenFile();
            //BinaryReader r = new BinaryReader(fs);
            //byte[] postArray = r.ReadBytes((int)fs.Length);
            //Stream postStream = myWebClient.OpenWrite(uri, "PUT");
            //try
            //{

            //    //使用UploadFile方法可以用下面的格式
            //    //myWebClient.UploadFile(uriString,"PUT",fileNamePath);
            //    if (postStream.CanWrite)
            //    {
            //        postStream.Write(postArray, 0, postArray.Length);
            //        postStream.Close();
            //        fs.Dispose();
            //        MessageBox.Show("上传日志文件成功！", "Log");
            //    }
            //    else
            //    {
            //        postStream.Close();
            //        fs.Dispose();
            //        MessageBox.Show("上传日志文件失败，文件不可写！", "Log");
            //    }

            //}
            //catch (Exception err)
            //{
            //    postStream.Close();
            //    fs.Dispose();
            //    //Utility.LogWriter log = new Utility.LogWriter();
            //    MessageBox.Show("上传日志文件异常！");
            //    throw err;
            //}
            //finally
            //{
            //    postStream.Close();
            //    fs.Dispose();
            //}
        }
    }
}
