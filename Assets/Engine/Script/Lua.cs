using UnityEngine;
using System.Collections;
using LuaInterface;
using SLua;
using System.Reflection;
using System;
using System.IO;
using System.Collections.Generic;
[CustomLuaClassAttribute]
public class Lua /*: IDisposable */{     
    public LuaState luaState;
    static LuaSvrGameObject lgo;
    bool errorReported = false;
   
    public Lua()
    {

        LuaState.loaderDelegate += luaLoader;
        luaState = new LuaState();

        LuaDLL.lua_pushstdcallcfunction(luaState.L, import);
        LuaDLL.lua_setglobal(luaState.L, "using");


        LuaObject.init(luaState.L);
        bind("BindUnity");
        bind("BindUnityUI");
        bind("BindDll");
        bind("BindCustom");
        bind("BindExtend"); // if you want to extend slua, can implemented BindExtend function like BindCustom etc.

        GameObject go = new GameObject("LuaSvrProxy");
        lgo = go.AddComponent<LuaSvrGameObject>();
        GameObject.DontDestroyOnLoad(go);
        lgo.state = luaState;
        lgo.onUpdate = this.tick;

        LuaTimer.reg(luaState.L);
        LuaCoroutine.reg(luaState.L, lgo);
        Helper.reg(luaState.L);
    }


    void bind(string name)
    {
        MethodInfo mi = typeof(LuaObject).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        if (mi != null) mi.Invoke(null, new object[] { luaState.L });
        else if (name == "BindUnity") Debug.LogError(string.Format("Miss {0}, click SLua=>Make to regenerate them", name));
    }

    void tick()
    {
        if (LuaDLL.lua_gettop(luaState.L) != 0 && !errorReported)
        {
            Debug.LogError("Some function not remove temp value from lua stack. You should fix it.");
            errorReported = true;
        }

        luaState.checkRef();
        LuaTimer.tick(Time.deltaTime);
    }

    public IntPtr handle
    {
        get
        {
            return luaState.handle;
        }
    }

    string script = "";
    byte[] luaLoader(string fn)
    {
        if (fn.EndsWith(".lua"))
        {
			script = API.AssetRoot + "lua/" + fn;
        }
        else
        {
            fn = fn.Replace(".", "/");
			script = API.AssetRoot + "lua/" + fn + ".lua";
        }

        FileStream fs = File.Open(script, FileMode.Open);
        long length = fs.Length;
        byte[] bytes = new byte[length];
        fs.Read(bytes, 0, bytes.Length);
        fs.Close();
        if (API.usingEncryptLua)
        {
            API.EncryptAll(ref bytes); //RC4 解密lua文件
        }
        return bytes;
    } 

    public void Dispose()
    {       
        //GC.SuppressFinalize(this);
    }

    public object DoFile(string fn)
    {
         return luaState.doFile(fn); 
    }

    public object this[string path]
    {
        get
        {
            return luaState.getObject(path);
        }
        set
        {
            luaState.setObject(path, value);
        }
    }

    public LuaFunction  GetFunction(string fn)
    {
        return luaState.getFunction(fn);
    }

    private static HashSet<string> ms_includedFiles = new HashSet<string>();



    [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
    internal static int import(IntPtr l)
    {

        LuaDLL.luaL_checktype(l, 1, LuaTypes.LUA_TSTRING);
        string str = LuaDLL.lua_tostring(l, 1);
        if (ms_includedFiles.Contains (str)) {
            return 0;
        } else {
            ms_includedFiles.Add (str);
        }
        return LuaState.import (l);

    }
    

}
