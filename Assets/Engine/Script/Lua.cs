using UnityEngine;
using System.Collections;
using LuaInterface;
using SLua;
using System.Reflection;
using System;
using System.IO;
[CustomLuaClassAttribute]
public class Lua : IDisposable {     
    private LuaState luaState;

    public Lua()
    {
        luaState = new LuaState();
        LuaState.loaderDelegate += luaLoader;
        LuaObject.init(luaState.handle);
        bind("BindUnity");
        bind("BindUnityUI");
        bind("BindCustom");

        GameObject go = new GameObject("LuaSvrProxy");
        LuaSvrGameObject lgo = go.AddComponent<LuaSvrGameObject>();
        GameObject.DontDestroyOnLoad(go);
        lgo.state = luaState;

        string import = @"
function import(name)
local t=_G[name]
for k,v in pairs(t) do
_G[k]=v
end
end
";
        if (LuaDLL.luaL_dostring(luaState.L, import) != 0)
        {
            Debug.LogError("import function err.");
        }

        if (LuaDLL.lua_gettop(luaState.handle) != 0)
              Debug.LogError("Some function not remove temp value from lua stack.");
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
            script = Application.persistentDataPath + "/lua/" + fn;
        }
        else
        {
            fn = fn.Replace(".", "/");
            script = Application.persistentDataPath + "/lua/" + fn + ".lua";
        }

        FileStream fs = File.Open(script, FileMode.Open);
        long length = fs.Length;
        byte[] bytes = new byte[length];
        fs.Read(bytes, 0, bytes.Length);
        fs.Close();
        return bytes;
    }

    void bind(string name)
    {
        MethodInfo mi = typeof(LuaObject).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        if (mi != null) mi.Invoke(null, new object[] { luaState.handle });
        else if (name == "BindUnity") Debug.LogError(string.Format("Miss {0}, click SLua=>Make to regenerate them", name));
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

}
