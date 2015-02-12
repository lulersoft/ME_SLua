using UnityEngine;
using System.Collections;
using LuaInterface;
using SLua;
using System.Reflection;
using System;
using System.IO;
[CustomLuaClassAttribute]
public class Lua /*: IDisposable */{     
    public LuaState luaState;
    static LuaSvrGameObject lgo;

    public Lua()
    {

        luaState = new LuaState();
        LuaState.loaderDelegate += luaLoader;
        LuaObject.init(luaState.L);
        bind("BindUnity");
        bind("BindUnityUI");
        bind("BindCustom");

        GameObject go = new GameObject("LuaSvrProxy");
        lgo = go.AddComponent<LuaSvrGameObject>();
        GameObject.DontDestroyOnLoad(go);
        lgo.state = luaState;
        lgo.onUpdate = this.tick;

        LuaTimer.reg(luaState.L);
        LuaCoroutine.reg(luaState.L, lgo);       
    }


    void bind(string name)
    {
        MethodInfo mi = typeof(LuaObject).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        if (mi != null) mi.Invoke(null, new object[] { luaState.L });
        else if (name == "BindUnity") Debug.LogError(string.Format("Miss {0}, click SLua=>Make to regenerate them", name));
    }

    void tick()
    {
        if (LuaDLL.lua_gettop(luaState.L) != 0)
            Debug.LogError("Some function not remove temp value from lua stack. You should fix it.");

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
