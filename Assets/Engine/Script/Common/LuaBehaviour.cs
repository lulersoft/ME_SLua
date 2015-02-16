﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SLua;
using System.ComponentModel;
using System.Text;
using LuaInterface;

/// <summary>
/// LuaBehaviour
/// @author 小陆  QQ:2604904
/// </summary>
[CustomLuaClassAttribute]
public class LuaBehaviour : MonoBehaviour
{
    public bool usingUpdate = false;
    protected bool isLuaReady = false;

    protected LuaTable table;
    protected List<MissionPack> MissionList = new List<MissionPack>();

    protected Lua env
    {
        get 
        {
            return API.env;
        }
    }
    protected void Update()
    {
        if (MissionList.Count > 0)
        {
            MissionPack pack = MissionList[0];
            MissionList.RemoveAt(0); 
            pack.Call();
        }

        if (usingUpdate)
        {
            CallMethod("Update");
        }
    }

    public void AddMission(LuaFunction func, params object[] args)
    {
        MissionList.Add(new MissionPack(func, args));
    }

    public void Hello(LuaFunction func)
    {
        func.call();
    }

    public string AssetPath
    {
        get
        {
            string target = string.Empty;
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXEditor)
            {
                target = "iphone";
            }
            else
            {
                target = "android";
            }
            return Application.persistentDataPath + "/asset/" + target + "/";
        }
    }

    public string AssetRoot
    {
        get
        {
            return Application.persistentDataPath + "/";
        }
    }

    public void LoadBundle(string fname, Callback<string, AssetBundle> handler)
    {
        if (API.BundleTable.ContainsKey(fname))
        {
            AssetBundle bundle = API.BundleTable[fname] as AssetBundle;
            if (handler != null) handler(name, bundle);
        }
        else
        {
            StartCoroutine(onLoadBundle(fname, handler));
        }
    }

    public void UnLoadAllBundle()
    {
        foreach (AssetBundle bundle in API.BundleTable.Values)
        {
            bundle.Unload(true);
        }
        API.BundleTable.Clear();
    }


    IEnumerator onLoadBundle(string name, Callback<string, AssetBundle> handler)
    {
        string uri = "file:///" + AssetPath + name.ToLower() + ".assetbundle";

        WWW www = new WWW(uri);
        yield return www;
        if (www.error != null)
        {
            Debug.Log("Warning erro: " + uri);
            Debug.Log("Warning erro: " + "loadStreamingAssets");
            Debug.Log("Warning erro: " + www.error);
            StopCoroutine("onLoadBundle");
            yield break;
        }
        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        byte[] data = www.bytes;

        AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(data);

        yield return new WaitForEndOfFrame();

        try
        {
            API.BundleTable[name] = bundle;
            if (handler != null) handler(name, bundle);
        }
        catch (System.Exception e)
        {
            Debug.LogError(FormatException(e), gameObject);
        }
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    protected void OnDestroy()
    {

        CallMethod("OnDestroy");

        if (table != null)
        {
            table.Dispose();
        }
    }
/*
    public IEnumerator RunCoroutine()
    {
        object result = new object();
        if (table == null) return (IEnumerator)result;
        result = CallMethod("RunCoroutine");
        return (IEnumerator)result;
    }
*/
    //加载脚本文件
    public void DoFile(string fn)
    {
        try
        {
            object chunk = env.DoFile(fn);
                      
            if (chunk != null  && (chunk is LuaTable))
            {
                table = (LuaTable)chunk;
          
                table["this"] = this;              
                table["transform"] = transform;
                table["gameObject"] = gameObject;

                CallMethod("Start");

                isLuaReady = true;
            }
          
        }
        catch (System.Exception e)
        {
            isLuaReady = false;
            Debug.LogError(FormatException(e), gameObject);
        }
    }
    //获取绑定的lua脚本
    public LuaTable GetChunk()
    {
        return table;
    }

    //设置lua脚本可直接使用变量
    public void SetEnv(string key, object val, bool isGlobal)
    {
        if (isGlobal)
        {
            env[key] = val;
        }
        else
        {
            if (table != null)
            {
                table[key] = val;
            }
        }
    }

    //延迟执行
    public void LuaInvoke(float delaytime,LuaFunction func,params object[] args)
    {
        StartCoroutine(doInvoke(delaytime, func, args));
    }
    private IEnumerator doInvoke(float delaytime, LuaFunction func, params object[] args)
    {
        yield return new  WaitForSeconds(delaytime);
        if (args != null)
        {
            func.call(args);
        }
        else
        {
            func.call(); 
        }
    }

    //协程
    public void RunCoroutine(YieldInstruction ins, LuaFunction func, params System.Object[] args)
    {
        StartCoroutine(doCoroutine(ins, func, args));
    }
    public void CancelCoroutine(YieldInstruction ins, LuaFunction func, params System.Object[] args)
    {
        StopCoroutine(doCoroutine(ins, func, args));      
    }
    private IEnumerator doCoroutine(YieldInstruction ins, LuaFunction func, params System.Object[] args)
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

    public object CallMethod(string function, params object[] args)
    {
        if (table == null || table[function] == null || !(table[function] is LuaFunction)) return null;

        LuaFunction func = (LuaFunction)table[function];

        if (func == null) return null;
        try
        {
            if (args != null)
            {
                return func.call(args);
            }
            func.call();
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(FormatException(e), gameObject);
        }
        return null;
    }

    public object CallMethod(string function)
    {
        return CallMethod(function, null);
    }

    public static string FormatException(System.Exception e)
    {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }

    #region 消息中心
    //添加消息侦听
    public void AddListener(string eventType, Callback handler)
    {
        Messenger.AddListener(eventType, handler);
    }

    public void AddListener2(string eventType, Callback<object> handler)
    {
        Messenger.AddListener<object>(eventType, handler);
    }

    //移除一事件侦听
    public void RemoveListener(string eventType, Callback handler)
    {
        Messenger.RemoveListener(eventType, handler);
    }
    public void RemoveListener2(string eventType, Callback<object> handler)
    {
        Messenger.RemoveListener<object>(eventType, handler);
    }

    //触发消息广播
    public void Broadcast(string eventType)
    {
        Messenger.Broadcast(eventType);
    }

    public void Broadcast(string eventType, object args)
    {
        Messenger.Broadcast<object>(eventType, args);
    }
    #endregion

}

[CustomLuaClassAttribute]
public class MissionPack
{
    public LuaFunction func;
    public object[] args;

    public MissionPack(LuaFunction _func, params object[] _args)
    {
        func = _func;
        args = _args;
    }

    public object Call()
    {
        if (args != null)
        {
            return func.call(args);
        }
        func.call();
        return null;
    }
}
