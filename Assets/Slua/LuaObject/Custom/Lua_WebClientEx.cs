using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_WebClientEx : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		int argc = LuaDLL.lua_gettop(l);
		WebClientEx o;
		if(argc==1){
			o=new WebClientEx();
			pushObject(l,o);
			return 1;
		}
		else if(argc==2){
			System.Int32 a1;
			checkType(l,2,out a1);
			o=new WebClientEx(a1);
			pushObject(l,o);
			return 1;
		}
		LuaDLL.luaL_error(l,"New object failed.");
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_Timeout(IntPtr l) {
		WebClientEx o = (WebClientEx)checkSelf(l);
		pushValue(l,o.Timeout);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_Timeout(IntPtr l) {
		WebClientEx o = (WebClientEx)checkSelf(l);
		int v;
		checkType(l,2,out v);
		o.Timeout=v;
		return 0;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"WebClientEx");
		addMember(l,"Timeout",get_Timeout,set_Timeout,true);
		createTypeMetatable(l,constructor, typeof(WebClientEx));
	}
}
