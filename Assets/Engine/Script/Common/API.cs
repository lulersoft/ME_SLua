using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Text;
using System.Timers;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using SLua;
using LuaInterface;

[CustomLuaClassAttribute]
public class API  {

    public static Hashtable BundleTable=new Hashtable();
    private static Lua lua;

    //资源加密解密常量定义
    public static int Encrypt_Len = 256;
    public static string Encrypt_Key = "this is source encryption key for me game frame,please custom this key string";

    //是否启用调试
    public static bool Debug = true;    

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

    public static string AssetRoot
    {
        get
        {
            return Application.persistentDataPath + "/";
        }
    }
    #region
    public static void Log(object msg)
    {
        if (Debug)
        {
            DebugTools.log += msg.ToString() + "\n\r";
            if (DebugTools.obj == null)
            {
                DebugTools.obj = new GameObject("DebugTools");
                DebugTools.obj.AddComponent<DebugTools>();
            }
            UnityEngine.Debug.Log(msg);
        }
    }

    public static void LogError(object msg)
    {
        Log(msg);
    }

    public static void LogWarning(object msg)
    {
        Log(msg);
    }
    #endregion

    public static object AddComponent(GameObject obj, string classname)
    {
        Type t = Type.GetType(classname);
        return obj.AddComponent(t);
    }
    public static object AddMissComponent(GameObject obj, string classname)
    {
        Type t = Type.GetType(classname);
        object _out = obj.GetComponent(t);
        if (_out == null)
        {
            _out = obj.AddComponent(t);
        }
        return _out;
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
    public static WebClientEx SendRequest(string url, string data, LuaFunction progressHander, LuaFunction completeHandler)
    {
        WebClientEx webClient = new WebClientEx();
        webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";  //采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
        webClient.Encoding = System.Text.UTF8Encoding.UTF8;
        System.Uri uri = new System.Uri(url);

        webClient.UploadProgressChanged += (object sender, UploadProgressChangedEventArgs e) =>
        {
            progressHander.call(sender,e);
        };
        webClient.UploadStringCompleted += (object sender, UploadStringCompletedEventArgs e) =>
        {
            completeHandler.call(sender,e);
        };

        try
        {
            webClient.UploadStringAsync(uri, "POST", data);//得到返回字符流      
        }
        catch (Exception e)
        {
            API.Log("Post err " + e.Message);
        }
         //返回client ，可用 client.CancelAsync(); 中断下载
        return webClient;
    }
 
    //异步下载
    public static WebClient DownLoad(string src, string SavePath, LuaFunction progressHander, LuaFunction completeHander)
    {
        WebClient client = new WebClient();
        client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
        {
            progressHander.call(sender,e);
        };
        client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
        {
            completeHander.call(sender,e);
        };

        try
        {
            client.DownloadFileAsync(new System.Uri(src), SavePath);
        }
        catch (Exception e)
        {
            API.Log("AsyncDownLoad err:" + e.Message);
        }

        //返回client ，可用 client.CancelAsync(); 中断下载
        return client;
    }

   //时钟
    public static MeTimer AddTimer(float interval, Callback<MeTimer> onTimerHander)
    {
        return AddTimer(interval, 0, onTimerHander);
    }
    public static MeTimer AddTimer(float interval, int loop, Callback<MeTimer> onTimerHander)
    {
        if (LuaMeTimer.obj == null)
        {
            LuaMeTimer.obj = new GameObject("MeLuaTimer");
            LuaMeTimer.obj.AddComponent<LuaMeTimer>();
        }
        MeTimer timer = new MeTimer();
        timer.onTimer = onTimerHander;
        timer.interval = interval;
        timer.loop = loop;

        LuaMeTimer.TimerList.Add(timer);

        return timer;
    }

    public static void KillTimer(MeTimer timer)
    {
        if (timer != null)
        {
            timer.close = true;
        }
    }


    /// <summary>
    /// HashToMD5Hex
    /// </summary>
    public static string HashToMD5Hex(string sourceStr)
    {
        byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] result = md5.ComputeHash(Bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
                builder.Append(result[i].ToString("x2"));
            return builder.ToString();
        }
    }

    /// <summary>
    /// 计算字符串的MD5值
    /// </summary>
    public static string MD5(string source)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string MD5File(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }
    //RC4 字符串
    public static string RC4(string input, string key)
    {
        StringBuilder result = new StringBuilder();
        int x, y, j = 0;
        int[] box = new int[256];

        for (int i = 0; i < 256; i++)
        {
            box[i] = i;
        }

        for (int i = 0; i < 256; i++)
        {
            j = (key[i % key.Length] + box[i] + j) % 256;
            x = box[i];
            box[i] = box[j];
            box[j] = x;
        }

        for (int i = 0; i < input.Length; i++)
        {
            y = i % 256;
            j = (box[y] + j) % 256;
            x = box[y];
            box[y] = box[j];
            box[j] = x;

            result.Append((char)(input[i] ^ box[(box[y] + box[j]) % 256]));
        }
        return result.ToString();
    }

    //RC4 byte 数组
    public static byte[] RC4(byte[] input, string key)
    {
        byte[] result = new byte[input.Length];
        int x, y, j = 0;
        int[] box = new int[256];

        for (int i = 0; i < 256; i++)
        {
            box[i] = i;
        }

        for (int i = 0; i < 256; i++)
        {
            j = (key[i % key.Length] + box[i] + j) % 256;
            x = box[i];
            box[i] = box[j];
            box[j] = x;
        }

        for (int i = 0; i < input.Length; i++)
        {
            y = i % 256;
            j = (box[y] + j) % 256;
            x = box[y];
            box[y] = box[j];
            box[j] = x;

            result[i] = (byte)(input[i] ^ box[(box[y] + box[j]) % 256]);
        }
        return result;
    }

    //局部加密解密
    public static void Encrypt(ref byte[] input)
    {
        if (input.Length > Encrypt_Len)
        {
            byte[] tmp = new byte[Encrypt_Len];
            System.Array.Copy(input, 0, tmp, 0, Encrypt_Len);
            byte[] de = API.RC4(tmp, Encrypt_Key);
            for (int i = 0; i < Encrypt_Len; i++)
            {
                input[i] = de[i];
            }
        }
    }
    //发射线
    public static object Raycast(Ray ray, out RaycastHit hit)
    {
        return Physics.Raycast(ray, out hit);
    }
    public static object Raycast(Ray ray, out RaycastHit hit, float distance, int layerMask)
    {
        return Physics.Raycast(ray, out hit, distance, layerMask);
    }
}
