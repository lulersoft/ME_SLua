using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_MissionPack : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		MissionPack o;
		SLua.LuaFunction a1;
		checkType(l,2,out a1);
		System.Object[] a2;
		checkType(l,3,out a2);
		o=new MissionPack(a1,a2);
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Call(IntPtr l) {
		try{
			MissionPack self=(MissionPack)checkSelf(l);
			System.Object ret=self.Call();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_func(IntPtr l) {
		MissionPack o = (MissionPack)checkSelf(l);
		pushValue(l,o.func);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_func(IntPtr l) {
		MissionPack o = (MissionPack)checkSelf(l);
		SLua.LuaFunction v;
		checkType(l,2,out v);
		o.func=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_args(IntPtr l) {
		MissionPack o = (MissionPack)checkSelf(l);
		pushValue(l,o.args);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_args(IntPtr l) {
		MissionPack o = (MissionPack)checkSelf(l);
		System.Object[] v;
		checkType(l,2,out v);
		o.args=v;
		return 0;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"MissionPack");
		addMember(l,Call);
		addMember(l,"func",get_func,set_func,true);
		addMember(l,"args",get_args,set_args,true);
		createTypeMetatable(l,constructor, typeof(MissionPack));
	}
}
