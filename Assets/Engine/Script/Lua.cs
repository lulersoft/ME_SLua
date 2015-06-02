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
    bool errorReported = false;

    public Lua()
    {

        luaState = new LuaState();
        LuaState.loaderDelegate += luaLoader;
        LuaObject.init(luaState.L);
        bindAll(luaState.L);

        GameObject go = new GameObject("LuaSvrProxy");
        lgo = go.AddComponent<LuaSvrGameObject>();
        GameObject.DontDestroyOnLoad(go);
        lgo.state = luaState;
        lgo.onUpdate = this.tick;

        LuaTimer.reg(luaState.L);
        LuaCoroutine.reg(luaState.L, lgo);
        Helper.reg(luaState.L);       
        LuaValueType.reg(luaState.L);
        LuaDLL.luaS_openextlibs(luaState.L);         

		if (LuaDLL.lua_gettop(luaState.L) != errorReported)
		{
			Debug.LogError("Some function not remove temp value from lua stack. You should fix it.");
			errorReported = LuaDLL.lua_gettop(luaState.L);
		}
	}		

	void tick()
	{
		if (LuaDLL.lua_gettop(luaState.L) != errorReported)
		{
			errorReported = LuaDLL.lua_gettop(luaState.L);
			Debug.LogError(string.Format("Some function not remove temp value({0}) from lua stack. You should fix it.",LuaDLL.luaL_typename(luaState.L,errorReported)));
		}

		luaState.checkRef();
		LuaTimer.tick(Time.deltaTime);
	}

    void bindAll(IntPtr l)
    {
        Assembly[] ams = AppDomain.CurrentDomain.GetAssemblies();

        List<Type> bindlist = new List<Type>();
        foreach (Assembly a in ams)
        {
            Type[] ts = a.GetExportedTypes();
            foreach (Type t in ts)
            {
                if (t.GetCustomAttributes(typeof(LuaBinderAttribute), false).Length > 0)
                {
                    bindlist.Add(t);
                }
            }
        }

        bindlist.Sort(new System.Comparison<Type>((Type a, Type b) =>
        {
            LuaBinderAttribute la = (LuaBinderAttribute)a.GetCustomAttributes(typeof(LuaBinderAttribute), false)[0];
            LuaBinderAttribute lb = (LuaBinderAttribute)b.GetCustomAttributes(typeof(LuaBinderAttribute), false)[0];

            return la.order.CompareTo(lb.order);
        })
        );

        foreach (Type t in bindlist)
        {
            t.GetMethod("Bind").Invoke(null, new object[] { l });
        }
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
        API.Encrypt(ref bytes);
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
