using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Timers;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using BestHTTP;
using SLua;


[CustomLuaClassAttribute]
public class API
{

    public static Hashtable BundleTable = new Hashtable();
    private static Lua lua;

    //资源加密解密常量定义
    public static int Encrypt_Len = 256;
    public static string Encrypt_Key = "this is source encryption key for me game frame,please custom this key string";

    //生成的资源包的扩展名
    public static string assetbundle_extension = ".ab";

    //是否启用调试
    public static bool usingDebug = true;
    //是否进行lua文件rc4加密解密
    public static bool usingEncryptLua = false;


    public static Lua env
    {
        get
        {
            if (lua == null)
            {
                lua = new Lua();
                //设置lua脚本文件查找路径
                //lua["package.path"] = lua["package.path"] + ";" + AssetRoot + "lua/?.lua;";
            }
            return lua;
        }
    }
    public static void CleanLuaEnv()
    {
        if (lua != null)
        {
            lua.Dispose();
            lua = null;
        }
    }

    static string _assetRoot = "";
    public static string AssetRoot
    {
        get
        {
            if (_assetRoot != "")
            {
                return _assetRoot;
            }
            else
            {
                return Application.persistentDataPath + "/";
            }
        }
        set
        {
            _assetRoot = value;
        }
    }
    static string _assetPath = "";
    public static string AssetPath
    {
        get
        {
            if (_assetPath != "")
            {
                return _assetPath;
            }
            else
            {
                return API.AssetRoot + "asset/" + API.GetTargetPlatform + "/";
            }
        }
        set
        {
            _assetPath = value;
        }
    }
    static private string _GetTargetPlatform = null;
    //资源目标平台
    static public string GetTargetPlatform
    {
        get
        {
            if (_GetTargetPlatform != null)
                return _GetTargetPlatform;
            string target = "webplayer";
#if UNITY_STANDALONE_WIN
            target = "standalonewindows";
#elif UNITY_IPHONE
            target = "ios";
#elif UNITY_ANDROID
            target = "android";
#endif
            return target;
        }
    }
    #region DebugTools
    public static void Log(object msg)
    {
        if (usingDebug)
        {
            DebugTools.log += msg.ToString() + "\n\r";
            if (DebugTools.obj == null)
            {
                DebugTools.obj = new GameObject("~DebugTools");
                DebugTools.obj.AddComponent<DebugTools>();
                GameObject meGo = GameObject.Find("~ME~");
                if (meGo == null)
                {
                    meGo = new GameObject("~ME~");
                }
                DebugTools.obj.transform.SetParent(meGo.transform);
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

    public static object AddComponent(GameObject obj, Type classname)
    {
        return obj.AddComponent(classname);
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
    public static void StartCoroutine(IEnumerator ie)
    {
        if (MeLoadBundle.obj == null)
        {
            MeLoadBundle.obj = new GameObject("~MeLoadBundle");
            MeLoadBundle.obj.AddComponent<MeLoadBundle>();
            GameObject meGo = GameObject.Find("~ME~");
            if (meGo == null)
            {
                meGo = new GameObject("~ME~");
            }
            MeLoadBundle.obj.transform.SetParent(meGo.transform);
        }
        MeLoadBundle.self.StartCoroutine(ie);
    }

    public static void RunCoroutine(YieldInstruction ins, LuaFunction func, object args)
    {
        API.StartCoroutine(doCoroutine(ins, func, args)); 
    }

    private static IEnumerator doCoroutine(YieldInstruction ins, LuaFunction func, object args)
    {
        yield return ins;
        if (args != null)
        {
            func.call(args);
        }
        else
        {
            func.call(); 
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
    public static void SendRequest(string url, string data, LuaFunction completeHandler)
    {
        //如果web页面是静态返回数据，请用HTTPMethods.Get
        var request = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, resp) =>
        {
            if (completeHandler != null)
            {
                completeHandler.call(req, resp);  //req, resp 需要暴露给slua导出         
            }
        });
        request.RawData = Encoding.UTF8.GetBytes(data);
        request.ConnectTimeout = TimeSpan.FromSeconds(3);//3秒超时
        request.Send();
    }

    //异步下载，参数  complete_param 是完成回调的执行参数
    public static void DownLoad(string SrcFilePath, string SaveFilePath, object complete_param, LuaFunction progressHander, LuaFunction completeHander)
    {
        var request = new HTTPRequest(new Uri(SrcFilePath), (req, resp) =>
        {
            List<byte[]> fragments = resp.GetStreamedFragments();
            // Write out the downloaded data to a file:
            using (FileStream fs = new FileStream(SaveFilePath, FileMode.Append))
                foreach (byte[] data in fragments)
                    fs.Write(data, 0, data.Length);
            if (resp.IsStreamingFinished)
            {
                if (completeHander != null)
                {
                    if (complete_param != null)
                    {
                        completeHander.call(req, resp,complete_param);
                    }
                    else
                    {
                        completeHander.call(req, resp);
                    }
                    Debug.Log("Download finished!");
                }
            }
        });
        request.OnProgress = (req, downloaded, length) =>
        {
            if (progressHander != null)
            {
                double pg =Math.Round( (float)downloaded / (float)length,2);                
                progressHander.call(pg);
            }
        };
        request.UseStreaming = true;
        request.StreamFragmentSize = 1 * 1024 * 1024; // 1 megabyte
        request.DisableCache = true; // already saving to a file, so turn off caching
        request.Send();
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
            LuaMeTimer.obj = new GameObject("~MeLuaTimer");
            LuaMeTimer.obj.AddComponent<LuaMeTimer>();
            GameObject meGo = GameObject.Find("~ME~");
            if (meGo == null)
            {
                meGo = new GameObject("~ME~");
            }
            LuaMeTimer.obj.transform.SetParent(meGo.transform);
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
    public static string RC4(string str, String pass)
    {
        Byte[] data =System.Text.Encoding.UTF8.GetBytes(str);
        Byte[] bt=RC4(data, pass);
        return System.Text.Encoding.UTF8.GetString(bt);        
    }

    public static Byte[] RC4(Byte[] data, String pass)
    {
        if (data == null || pass == null) return null;
        Byte[] output = new Byte[data.Length];
        Int64 i = 0;
        Int64 j = 0;
        Byte[] mBox = GetKey(System.Text.Encoding.UTF8.GetBytes(pass), 256);

        // 加密
        for (Int64 offset = 0; offset < data.Length; offset++)
        {
            i = (i + 1) % mBox.Length;
            j = (j + mBox[i]) % mBox.Length;
            Byte temp = mBox[i];
            mBox[i] = mBox[j];
            mBox[j] = temp;
            Byte a = data[offset];
            //Byte b = mBox[(mBox[i] + mBox[j] % mBox.Length) % mBox.Length];
            // mBox[j] 一定比 mBox.Length 小，不需要在取模
            Byte b = mBox[(mBox[i] + mBox[j]) % mBox.Length];
            output[offset] = (Byte)((Int32)a ^ (Int32)b);
        }

        data = output;

        return output;
    }
    static private Byte[] GetKey(Byte[] pass, Int32 kLen)
    {
        Byte[] mBox = new Byte[kLen];

        for (Int64 i = 0; i < kLen; i++)
        {
            mBox[i] = (Byte)i;
        }
        Int64 j = 0;
        for (Int64 i = 0; i < kLen; i++)
        {
            j = (j + mBox[i] + pass[i % pass.Length]) % kLen;
            Byte temp = mBox[i];
            mBox[i] = mBox[j];
            mBox[j] = temp;
        }
        return mBox;
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
    //整个文件加密
    public static void EncryptAll(ref byte[] input)
    {
        byte[] tmp = new byte[input.LongLength];
        System.Array.Copy(input, 0, tmp, 0, Encrypt_Len);
        byte[] de = API.RC4(tmp, Encrypt_Key);
        System.Array.Copy(de, 0, input, 0, Encrypt_Len);
        tmp = null;
        de = null;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - -  
* Stream 和 byte[] 之间的转换 
* - - - - - - - - - - - - - - - - - - - - - - - */
    /// <summary> 
    /// 将 Stream 转成 byte[] 
    /// </summary> 
    public static byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary> 
    /// 将 byte[] 转成 Stream 
    /// </summary> 
    public static Stream BytesToStream(byte[] bytes)
    {
        Stream stream = new MemoryStream(bytes);
        return stream;
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


    //加载 AssetBundle
    public static void LoadBundle(string fname, Callback<string, AssetBundle, object> handler)
    {
        LoadBundle(fname, handler, null);
    }

    public static void LoadBundle(string fname, Callback<string, AssetBundle, object> handler, object arg)
    {
        if (MeLoadBundle.obj == null)
        {
            MeLoadBundle.obj = new GameObject("~MeLoadBundle");
            MeLoadBundle.obj.AddComponent<MeLoadBundle>();
            GameObject meGo = GameObject.Find("~ME~");
            if (meGo == null)
            {
                meGo = new GameObject("~ME~");
            }
            MeLoadBundle.obj.transform.SetParent(meGo.transform);
        }
        MeLoadBundle.self.LoadBundle(fname, handler, arg);
    }
    //释放所有AssetBundle
    public static void UnLoadAllBundle()
    {
        if (MeLoadBundle.self != null)
        {
            MeLoadBundle.self.UnLoadAllBundle();
        }
    }
    //释放某AssetBundle
    public static void UnLoadBundle(AssetBundle bundle)
    {
        if (MeLoadBundle.self != null)
        {
            MeLoadBundle.self.UnLoadBundle(bundle);
        }
    }
    //释放某AssetBundle
    public static void UnLoadBundle(string key)
    {
        if (MeLoadBundle.self != null)
        {
            MeLoadBundle.self.UnLoadBundle(key);
        }
    }
    //停止加载携程，请在有使用动态加载的脚本的OnDestroy内调用
    public static void StopAllLoadBundle()
    {
        if (MeLoadBundle.self != null)
        {
            MeLoadBundle.self.StopAllCoroutines();
        }
    }

    #region 消息中心
    //添加消息侦听
    public static void AddListener(string eventType, Callback handler)
    {
        Messenger.AddListener(eventType, handler);
    }

    public static void AddListener2(string eventType, Callback<object> handler)
    {
        Messenger.AddListener<object>(eventType, handler);
    }

    //移除一事件侦听
    public static void RemoveListener(string eventType, Callback handler)
    {
        Messenger.RemoveListener(eventType, handler);
    }
    public static void RemoveListener2(string eventType, Callback<object> handler)
    {
        Messenger.RemoveListener<object>(eventType, handler);
    }

    //触发消息广播
    public static void Broadcast(string eventType)
    {
        Messenger.Broadcast(eventType);
    }

    public static void Broadcast(string eventType, object args)
    {
        Messenger.Broadcast<object>(eventType, args);
    }
    #endregion

    //判断是否点击在uGUI上
    public static bool IsPointerOverUIObject()
    {
        // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
        // the ray cast appears to require only eventData.position.
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    /// Cast a ray to test if screenPosition is over any UI object in canvas. This is a replacement
    /// for IsPointerOverGameObject() which does not work on Android in 4.6.0f3
    /// </summary>
    public static bool IsPointerOverUIObject(Canvas canvas, Vector2 screenPosition)
    {
        // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
        // the ray cast appears to require only eventData.position.
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventDataCurrentPosition.position = screenPosition;

        UnityEngine.UI.GraphicRaycaster uiRaycaster = canvas.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        uiRaycaster.Raycast(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
