using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Text;
using System.Timers;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.ComponentModel;
using SLua;
using LuaInterface;

[CustomLuaClassAttribute]
public class API  {

    public static Hashtable BundleTable=new Hashtable();
    private static Lua lua;
    public static Lua env
    {
        get
        {
            if (lua == null)
            {
                lua = new Lua();               
                //设置lua脚本文件查找路径
                lua["package.path"] = lua["package.path"] + ";" + Application.persistentDataPath + "/lua/?.lua;";
            }
            return lua;
        }
    }

 
    //zip压缩
    public static void PackFiles(string filename, string directory)
    {
        try
        {
            FastZip fz = new FastZip();
            fz.CreateEmptyDirectories = true;
            fz.CreateZip(filename, directory, true, "");
            fz = null;
        }
        catch (Exception)
        {
            throw;
        }
    }

    //zip解压
    public static bool UnpackFiles(string file, string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            ZipInputStream s = new ZipInputStream(File.OpenRead(file));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {

                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (directoryName != String.Empty)
                    Directory.CreateDirectory(dir + directoryName);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(dir + theEntry.Name);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                }
            }
            s.Close();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    //异步HTTP
    public static WebClientEx SendRequest(string url, string data, UploadProgressChangedEventHandler progressHander, UploadStringCompletedEventHandler completehandler)
    {
        WebClientEx webClient = new WebClientEx();
        webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";  //采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
        webClient.Encoding = System.Text.UTF8Encoding.UTF8;
        System.Uri uri = new System.Uri(url);
        webClient.UploadProgressChanged += progressHander;
        webClient.UploadStringCompleted += completehandler;
        try
        {
            webClient.UploadStringAsync(uri, "POST", data);//得到返回字符流      
        }
        catch (System.Exception e)
        {
            Debug.Log("Post err " + e.Message);
        }
        return webClient;
    }
        

    //异步下载

    public static WebClient DownLoad(string src, string SavePath, DownloadProgressChangedEventHandler progressHander, AsyncCompletedEventHandler completeHander)
    {
        WebClient client = new WebClient();       
        client.DownloadProgressChanged += progressHander;
        client.DownloadFileCompleted += completeHander;
        try
        {
            client.DownloadFileAsync(new System.Uri(src), SavePath);
        }
        catch (System.Exception e)
        {
            Debug.Log("AsyncDownLoad err:" + e.Message);
        }

        //返回client ，可用 client.CancelAsync(); 中断下载
        return client;
    }

    //时钟

    public static Timer AddTimer(float interval, ElapsedEventHandler OnTimer)
    {
        Timer timer = new Timer();
        timer.Elapsed += OnTimer;
        timer.Interval = interval;
        timer.Enabled = true;
        return timer;
    }

    public static void KillTimer(Timer timer)
    {
        if (timer != null)
        {
            timer.Close();
            timer.Dispose();
        }
    }
}
