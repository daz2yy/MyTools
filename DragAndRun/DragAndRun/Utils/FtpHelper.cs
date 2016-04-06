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
using System.Windows.Controls;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using Newtonsoft.Json;
using DragAndRun.ViewModule;

namespace DragAndRun.Utils
{
    class FtpHelper
    {
        string sftpServerIP;
        string sftpUserID;
        string sftpPassword;
        Sftp sftpClient;
        public FtpHelper(string serverIP, string userID, string passWord)
        {
            sftpServerIP = serverIP;
            sftpUserID = userID;
            sftpPassword = passWord;
            init();
        }
        public FtpHelper()
        {
            init();
        }
        private void init()
        {
            errorDict.Add("0", "0, UNKNOW, 刷新中...");
            errorDict.Add("200", "200, 刷新成功");
            errorDict.Add("401", "401, INPUT_ERROR, 参数不正确");
            errorDict.Add("402", "402, WRONG_PASSWORD, 用户名密码错误");
            errorDict.Add("403", "403, NO_USER_EXISTS, 用户被禁用");
            errorDict.Add("500", "500, 参数格式错误");
        }
        //=========================================== httpRequest ===========================================
        public class jsonResult
        {
            [JsonProperty("r_id")]
            public string id;
        }
        public class requestResultStatus
        {
            [JsonProperty("url")]
            public string url;

            [JsonProperty("code")]
            public string code;
        }
        public class requestResult
        {
            [JsonProperty("status")]
            public string status;

            [JsonProperty("finishedTime")]
            public string finishedTime;

            [JsonProperty("successRate")]
            public string successRate;

            [JsonProperty("totalTime")]
            public string totalTime;

            [JsonProperty("r_id")]
            public string r_id;
            
            [JsonProperty("createdTime")]
            public string createdTime;

            [JsonProperty("username")]
            public string username;

            [JsonProperty("urlStatus")]
            public requestResultStatus[] urlStatus;

        }
        Dictionary<string, string> errorDict = new Dictionary<string,string>();

        // http 蓝汛CDN
        public string httpRequest(string type, string url, string jsonContent)
        {
//             string resultStr = "{\"r_id\": \"568f5ae9949e6b7086f46496\" }";
//             jsonResult result = JsonConvert.DeserializeObject<jsonResult>(resultStr);
            //eg:
            //type = "POST";
            //url = "http://sqsj.cdn.7road.net/update/flmobile/yybb/android/VersionUpdate/versions";
            string urlAPI, responseFromServer;
            if (type == "POST")
            {
                try
                {
                    jsonContent = "{\n";
                    jsonContent = jsonContent + String.Format("\"username\" : \"{0}\", \"password\" : \"{1}\", \"task\" : ", PackageSubVM.Instance.CDNUserName, PackageSubVM.Instance.CDNPassword);
                    jsonContent = jsonContent + "{\n";
                    jsonContent = jsonContent + String.Format("\"urls\" : [");
                    string[] urls = url.Split(';');
                    foreach (var urlStr in urls)
                    {
                        jsonContent = jsonContent + String.Format("\n\"{0}\",", urlStr);
                    }
                    jsonContent = jsonContent.TrimEnd(',');
                    jsonContent = jsonContent + "\n]}\n}";

                    urlAPI = "https://r.chinacache.com/content/refresh";

                    WebRequest request = WebRequest.Create(urlAPI);
                    request.Method = type;
                    string postData = jsonContent;
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    request.ContentType = "application/json";
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    Console.WriteLine(responseFromServer);
                    MessageBox.Show("刷新提交结果：" + ((HttpWebResponse)response).StatusDescription);
                    PackageSubVM.Instance.updateDescription("刷新提交结果：" + ((HttpWebResponse)response).StatusDescription);
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return "False";
                }
                return responseFromServer;
            }
            else if (type == "GET")
            {
                jsonResult result = JsonConvert.DeserializeObject<jsonResult>(jsonContent);
                urlAPI = "https://r.chinacache.com/content/refresh/" + result.id;
                urlAPI = urlAPI + String.Format("?username={0}&password={1}&r_id={2}", PackageSubVM.Instance.CDNUserName, PackageSubVM.Instance.CDNPassword, result.id);
                string res;
                bool bIsOK = false;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        Console.WriteLine("查询刷新url:" + urlAPI);
                        PackageSubVM.Instance.updateDescription("查询刷新url:" + urlAPI);
                        res = client.DownloadString(urlAPI);
                        Console.WriteLine("刷新结果:" + res);
                        requestResult[] reqResultList = JsonConvert.DeserializeObject<requestResult[]>(res);
                        foreach (var reqRes in reqResultList)
                        {
                            if (reqRes.r_id == result.id)
                            {
                                string errorMsg = "未知错误:" + reqRes.urlStatus[0].code + ", " + reqRes.status;
                                if (errorDict.ContainsKey(reqRes.urlStatus[0].code))
                                {
                                    errorMsg = errorDict[reqRes.urlStatus[0].code];
                                    if (reqRes.urlStatus[0].code == "200")
                                    {
                                        bIsOK = true;
                                    }
                                }
                                //MessageBox.Show(errorMsg);
                                PackageSubVM.Instance.updateDescription("刷新结果:" + errorMsg);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return "False";
                    }
                }
                return bIsOK.ToString();
            }
            return "error type.";
        }

        // 腾讯CDN刷新 
        public class TXResult
        {
            public int code;
            public string message;
        }
        public string TencentCDNRequest(string url)
        {
//             url = "http://sqsj.cdn.7road.net/testTXCDN.log";
//             FileEncode.Instance.executeFile("D:/1temp/CDN_tx_API", url);
            using (WebClient client = new WebClient())
            {
                try
                {
                    string urlAPI = "http://cdn.api.qcloud.com/v2/index.php?Action=RefreshCdnUrl";
                    string[] urls = url.Split(';');
                    for (int i = 0; i < urls.Length; ++i)
                    {
                        urlAPI = urlAPI + String.Format("&urls.{0}={1}", i, urls[i]);
                    }
                    string res = client.DownloadString(urlAPI);
                    Console.WriteLine("res:" + res);
                    TXResult reqResultList = JsonConvert.DeserializeObject<TXResult>(res);
                    Console.WriteLine(String.Format("刷新结果:code:{0}, message:{1}", reqResultList.code, reqResultList.message));
                    MessageBox.Show(String.Format("刷新结果:code:{0}, message:{1}", reqResultList.code, reqResultList.message));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return "false";
                }
            }
            return "";
        }
        //=========================================== UpLoadFile ===========================================
        public bool sftpUpload(string fromFilePath, string toFilePath)
        {
            GlobalVM.Instance.updateDescription(String.Format("上传文件: {0} -> {1}", fromFilePath, toFilePath));
            bool bConnectSuccess = false;
            int nTryTime = 0;
            while(true)
            {
                if (nTryTime >= 3)
                    break;
                try
                {
                    ++nTryTime;
                    sftpClient = new Sftp(this.sftpServerIP, this.sftpUserID, this.sftpPassword);
                    sftpClient.Connect(16333);
                    bConnectSuccess = true;
                    break;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            if (bConnectSuccess)
            {
                //sftpClient.OnTransferProgress
                sftpClient.Put(fromFilePath, toFilePath);
                sftpClient.Close();
            }
            else
            {
                MessageBox.Show("连接服务器失败！");
            }
            return bConnectSuccess;
        }
    }
}
