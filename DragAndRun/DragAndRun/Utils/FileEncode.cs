using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragAndRun.ViewModule;
using System.Diagnostics;
using System.Windows;

namespace DragAndRun.Utils
{
    class FileEncode
    {
        private static FileEncode _instance;
        public static FileEncode Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FileEncode();
                return _instance;
            }
        }

        public delegate void MyDelegate(int n);
        MyDelegate callBackDelegate;

        //加密lua
        public void encodeLua(string inputPath, string outputPath, MyDelegate callback = null)
        {
            GlobalVM.Instance.updateDescription("加密Lua: ");
            if (System.IO.Directory.Exists(inputPath) || (System.IO.File.Exists(inputPath) && System.IO.Path.GetExtension(inputPath) == ".lua"))
            {
                string executeFilePath;
                string param;
                callBackDelegate = callback;
                if (GlobalVM.Instance.EncodeAndroid)
                {
                    bool bMoveFile = false;
                    string oldPath = "";

                    if (System.IO.Path.GetExtension(inputPath) != "")
                    {
                        if (!System.IO.Directory.Exists(inputPath + "_temp"))
                        {
                            System.IO.Directory.CreateDirectory(inputPath + "_temp");
                        }
                        else
                        {
                            System.IO.Directory.Delete(inputPath + "_temp", true);
                            System.IO.Directory.CreateDirectory(inputPath + "_temp");
                        }
                        System.IO.File.Move(inputPath, inputPath + @"_temp\" + System.IO.Path.GetFileName(inputPath));
                        oldPath = inputPath;
                        inputPath = inputPath + "_temp";
                        bMoveFile = true;
                    }
                    executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                    executeFilePath = executeFilePath + @"\encodeTools\luaToJit\compile_scripts.bat";
                    param = "-i \"" + inputPath + "\" -o \"" + outputPath + "\" -m files -jit";
                    executeFile(executeFilePath, param);
                    if (bMoveFile)
                    {
                        System.IO.Directory.Delete(oldPath + "_temp", true);
                    }
                }
                else
                {
                    executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                    executeFilePath = executeFilePath + @"\encodeTools\LuaEncode.exe";
                    param = "-i \"" + inputPath + "\" -o \"" + outputPath;
                    executeFile(executeFilePath, param);
                }
            }
            else
            {
                GlobalVM.Instance.updateDescription("!!!!加密Lua路径或文件不存在!");
            }
        }

        //加密图片
        public void encodeImage(string inputPath, string outputPath, MyDelegate callback = null)
        {
            GlobalVM.Instance.updateDescription("加密图片: ");
            if (System.IO.Directory.Exists(inputPath) || System.IO.File.Exists(inputPath))
            {
                callBackDelegate = callback;
                string executeFilePath;
                string param;
                executeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                executeFilePath = System.IO.Path.GetDirectoryName(executeFilePath);
                executeFilePath = executeFilePath + @"\encodeTools\PngEncode.exe";
                param = "-i " + "\"" + inputPath + "\"" + " -o " + "\"" + outputPath + "\"";
                executeFile(executeFilePath, param);
            }
            else
            {
                GlobalVM.Instance.updateDescription("!!!!加密图片路径或文件不存在!");
            }
        }

        public void executeFile(string filePath, string shellParam)
        {
            //filePath = "C:\\Program Files (x86)\\神曲世界打包\\encodeTools\\luaToJit\\compile_scripts.bat";
            GlobalVM.Instance.updateDescription("执行文件: " + filePath);
            GlobalVM.Instance.updateDescription("执行参数: " + shellParam);
            GlobalVM.Instance.updateDescription("输出:");

            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("\"" + filePath + "\"", shellParam);
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.CreateNoWindow = true;

//             myProcess.Start();
//             while (!myProcess.HasExited)
//             {
//                 GlobalVM.Instance.updateDescription(myProcess.StandardOutput.ReadToEnd());
//                 myProcess.WaitForExit();
//             }
//             myProcess.Close();

            myProcess.OutputDataReceived += (sender, data) =>
            {
                GlobalVM.Instance.updateDescription((string)data.Data);
            };
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.WaitForExit();
            myProcess.Close();
        }
    }
}
