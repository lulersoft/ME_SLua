using UnityEngine;
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
	[System.NonSerialized]
    public bool usingUpdate = false;
    [System.NonSerialized]
    public bool usingFixedUpdate = false;
    protected bool isLuaReady = false;

    protected LuaTable table;

    //保存的lua 数据存取
    public LuaTable data{get;set;} 

    protected Lua env
    {
        get 
        {
            return API.env;
        }
    }

    protected void Update()
    {
        if (usingUpdate)
        {
            CallMethod("Update");
        }
    }

    protected void FixedUpdate()
    {
        if (usingFixedUpdate)
        {
            CallMethod("FixedUpdate");
        }
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

    //设置执行的table对象
    public void setBehaviour(LuaTable myTable)
    {
        table = myTable;

        table["this"] = this;
        table["transform"] = transform;
        table["gameObject"] = gameObject;

        CallMethod("Start");

        isLuaReady = true;
    }
    //加载脚本文件
    public void DoFile(string fn)
    {
        try
        {
            object chunk = env.DoFile(fn);
                      
            if (chunk != null  && (chunk is LuaTable))
            {
                setBehaviour((LuaTable)chunk);               
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
    //挂接回调调用函数：一般用于jni或者invoke等操作
    public void MeMessage(object arg)
    {
        Messenger.Broadcast<object>(this.name + "MeMessage", arg);
    }

    //挂接回调调用函数：一般用于jin或者invoke等操作
    public void MeMessageAll(object arg)
    {
        Messenger.Broadcast<object>("MeMessageAll", arg);
    }
}

