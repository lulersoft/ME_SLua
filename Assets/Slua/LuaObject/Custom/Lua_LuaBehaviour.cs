using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_LuaBehaviour : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		LuaBehaviour o;
		o=new LuaBehaviour();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int AddMission(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			SLua.LuaFunction a1;
			checkType(l,2,out a1);
			System.Object[] a2;
			checkParams(l,3,out a2);
			self.AddMission(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Hello(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			SLua.LuaFunction a1;
			checkType(l,2,out a1);
			self.Hello(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadBundle(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Callback<System.String,UnityEngine.AssetBundle> a2;
			checkDelegate(l,3,out a2);
			self.LoadBundle(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int UnLoadAllBundle(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			self.UnLoadAllBundle();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DestroyMe(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			self.DestroyMe();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DoFile(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			self.DoFile(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetChunk(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			SLua.LuaTable ret=self.GetChunk();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SetEnv(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			System.Object a2;
			checkType(l,3,out a2);
			System.Boolean a3;
			checkType(l,4,out a3);
			self.SetEnv(a1,a2,a3);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LuaInvoke(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.Single a1;
			checkType(l,2,out a1);
			SLua.LuaFunction a2;
			checkType(l,3,out a2);
			System.Object[] a3;
			checkParams(l,4,out a3);
			self.LuaInvoke(a1,a2,a3);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RunCoroutine(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			UnityEngine.YieldInstruction a1;
			checkType(l,2,out a1);
			SLua.LuaFunction a2;
			checkType(l,3,out a2);
			System.Object[] a3;
			checkParams(l,4,out a3);
			self.RunCoroutine(a1,a2,a3);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CancelCoroutine(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			UnityEngine.YieldInstruction a1;
			checkType(l,2,out a1);
			SLua.LuaFunction a2;
			checkType(l,3,out a2);
			System.Object[] a3;
			checkParams(l,4,out a3);
			self.CancelCoroutine(a1,a2,a3);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CallMethod(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(argc==3){
				LuaBehaviour self=(LuaBehaviour)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Object[] a2;
				checkParams(l,3,out a2);
				System.Object ret=self.CallMethod(a1,a2);
				pushValue(l,ret);
				return 1;
			}
			else if(argc==2){
				LuaBehaviour self=(LuaBehaviour)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Object ret=self.CallMethod(a1);
				pushValue(l,ret);
				return 1;
			}
			LuaDLL.luaL_error(l,"No matched override function to call");
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int AddListener(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Callback a2;
			checkDelegate(l,3,out a2);
			self.AddListener(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int AddListener2(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Callback<System.Object> a2;
			checkDelegate(l,3,out a2);
			self.AddListener2(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RemoveListener(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Callback a2;
			checkDelegate(l,3,out a2);
			self.RemoveListener(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RemoveListener2(IntPtr l) {
		try{
			LuaBehaviour self=(LuaBehaviour)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Callback<System.Object> a2;
			checkDelegate(l,3,out a2);
			self.RemoveListener2(a1,a2);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Broadcast(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(argc==2){
				LuaBehaviour self=(LuaBehaviour)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				self.Broadcast(a1);
				return 0;
			}
			else if(argc==3){
				LuaBehaviour self=(LuaBehaviour)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Object a2;
				checkType(l,3,out a2);
				self.Broadcast(a1,a2);
				return 0;
			}
			LuaDLL.luaL_error(l,"No matched override function to call");
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int FormatException_s(IntPtr l) {
		try{
			System.Exception a1;
			checkType(l,1,out a1);
			System.String ret=LuaBehaviour.FormatException(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_usingUpdate(IntPtr l) {
		LuaBehaviour o = (LuaBehaviour)checkSelf(l);
		pushValue(l,o.usingUpdate);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_usingUpdate(IntPtr l) {
		LuaBehaviour o = (LuaBehaviour)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.usingUpdate=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_AssetPath(IntPtr l) {
		LuaBehaviour o = (LuaBehaviour)checkSelf(l);
		pushValue(l,o.AssetPath);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_AssetRoot(IntPtr l) {
		LuaBehaviour o = (LuaBehaviour)checkSelf(l);
		pushValue(l,o.AssetRoot);
		return 1;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"LuaBehaviour");
		addMember(l,AddMission);
		addMember(l,Hello);
		addMember(l,LoadBundle);
		addMember(l,UnLoadAllBundle);
		addMember(l,DestroyMe);
		addMember(l,DoFile);
		addMember(l,GetChunk);
		addMember(l,SetEnv);
		addMember(l,LuaInvoke);
		addMember(l,RunCoroutine);
		addMember(l,CancelCoroutine);
		addMember(l,CallMethod);
		addMember(l,AddListener);
		addMember(l,AddListener2);
		addMember(l,RemoveListener);
		addMember(l,RemoveListener2);
		addMember(l,Broadcast);
		addMember(l,FormatException_s);
		addMember(l,"usingUpdate",get_usingUpdate,set_usingUpdate,true);
		addMember(l,"AssetPath",get_AssetPath,null,true);
		addMember(l,"AssetRoot",get_AssetRoot,null,true);
		createTypeMetatable(l,constructor, typeof(LuaBehaviour),typeof(UnityEngine.MonoBehaviour));
	}
}
