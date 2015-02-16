using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_Lua : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		Lua o;
		o=new Lua();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DoFile(IntPtr l) {
		try{
			Lua self=(Lua)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			System.Object ret=self.DoFile(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetFunction(IntPtr l) {
		try{
			Lua self=(Lua)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			SLua.LuaFunction ret=self.GetFunction(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_luaState(IntPtr l) {
		Lua o = (Lua)checkSelf(l);
		pushValue(l,o.luaState);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_luaState(IntPtr l) {
		Lua o = (Lua)checkSelf(l);
		SLua.LuaState v;
		checkType(l,2,out v);
		o.luaState=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_handle(IntPtr l) {
		Lua o = (Lua)checkSelf(l);
		pushValue(l,o.handle);
		return 1;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"Lua");
		addMember(l,DoFile);
		addMember(l,GetFunction);
		addMember(l,"luaState",get_luaState,set_luaState,true);
		addMember(l,"handle",get_handle,null,true);
		createTypeMetatable(l,constructor, typeof(Lua));
	}
}
