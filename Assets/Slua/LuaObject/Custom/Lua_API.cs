using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_API : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		API o;
		o=new API();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int PackFiles_s(IntPtr l) {
		try{
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			API.PackFiles(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int UnpackFiles_s(IntPtr l) {
		try{
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			System.Boolean ret=API.UnpackFiles(a1,a2);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SendRequest_s(IntPtr l) {
		try{
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			SLua.LuaFunction a3;
			checkType(l,3,out a3);
			SLua.LuaFunction a4;
			checkType(l,4,out a4);
			WebClientEx ret=API.SendRequest(a1,a2,a3,a4);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DownLoad_s(IntPtr l) {
		try{
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			SLua.LuaFunction a3;
			checkType(l,3,out a3);
			SLua.LuaFunction a4;
			checkType(l,4,out a4);
			System.Net.WebClient ret=API.DownLoad(a1,a2,a3,a4);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int AddTimer_s(IntPtr l) {
		try{
			System.Single a1;
			checkType(l,1,out a1);
			SLua.LuaFunction a2;
			checkType(l,2,out a2);
			System.Timers.Timer ret=API.AddTimer(a1,a2);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int KillTimer_s(IntPtr l) {
		try{
			System.Timers.Timer a1;
			checkType(l,1,out a1);
			API.KillTimer(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_BundleTable(IntPtr l) {
		pushValue(l,API.BundleTable);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_BundleTable(IntPtr l) {
		System.Collections.Hashtable v;
		checkType(l,2,out v);
		API.BundleTable=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_env(IntPtr l) {
		pushValue(l,API.env);
		return 1;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"API");
		addMember(l,PackFiles_s);
		addMember(l,UnpackFiles_s);
		addMember(l,SendRequest_s);
		addMember(l,DownLoad_s);
		addMember(l,AddTimer_s);
		addMember(l,KillTimer_s);
		addMember(l,"BundleTable",get_BundleTable,set_BundleTable,false);
		addMember(l,"env",get_env,null,false);
		createTypeMetatable(l,constructor, typeof(API));
	}
}
