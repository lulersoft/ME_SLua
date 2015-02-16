using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_Activity : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		Activity o;
		o=new Activity();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReStart(IntPtr l) {
		try{
			Activity self=(Activity)checkSelf(l);
			self.ReStart();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"Activity");
		addMember(l,ReStart);
		createTypeMetatable(l,constructor, typeof(Activity),typeof(LuaBehaviour));
	}
}
