using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_MessengerHelper : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		MessengerHelper o;
		o=new MessengerHelper();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnDisable(IntPtr l) {
		try{
			MessengerHelper self=(MessengerHelper)checkSelf(l);
			self.OnDisable();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnApplicationQuit(IntPtr l) {
		try{
			MessengerHelper self=(MessengerHelper)checkSelf(l);
			self.OnApplicationQuit();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"MessengerHelper");
		addMember(l,OnDisable);
		addMember(l,OnApplicationQuit);
		createTypeMetatable(l,constructor, typeof(MessengerHelper),typeof(UnityEngine.MonoBehaviour));
	}
}
