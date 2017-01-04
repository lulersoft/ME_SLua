using UnityEngine;
using SLua;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


[CustomLuaClassAttribute]
public class Lua /*: IDisposable */{     
    public LuaState luaState;

    public bool isReady = false;
    LuaSvr l;

    public Lua()
    {      
        LuaState.loaderDelegate += luaLoader;

        l = new LuaSvr();
        luaState = l.luaState;
        l.init(null, () =>
        {
            isReady = true;
            luaState = l.luaState;
        });

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
            return luaState[path];
        }
        set
        {
            luaState[path]=value;
        }
    }   

    public LuaFunction  GetFunction(string fn)
    {
        return luaState.getFunction(fn);
    }    

}
