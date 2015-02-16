using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_LTDescr : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		LTDescr o;
		o=new LTDescr();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int cancel(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.cancel();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int reset(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			self.reset();
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int pause(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.pause();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int resume(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.resume();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setAxis(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			UnityEngine.Vector3 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setAxis(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setDelay(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Single a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setDelay(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setEase(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(LeanTweenType))){
				LTDescr self=(LTDescr)checkSelf(l);
				LeanTweenType a1;
				checkEnum(l,2,out a1);
				LTDescr ret=self.setEase(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.AnimationCurve))){
				LTDescr self=(LTDescr)checkSelf(l);
				UnityEngine.AnimationCurve a1;
				checkType(l,2,out a1);
				LTDescr ret=self.setEase(a1);
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
	static public int setTo(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			UnityEngine.Vector3 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setTo(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setFrom(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			UnityEngine.Vector3 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setFrom(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setHasInitialized(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setHasInitialized(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setId(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.UInt32 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setId(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setRepeat(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setRepeat(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setLoopType(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LeanTweenType a1;
			checkEnum(l,2,out a1);
			LTDescr ret=self.setLoopType(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setUseEstimatedTime(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setUseEstimatedTime(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setUseFrames(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setUseFrames(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setLoopCount(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setLoopCount(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setLoopOnce(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.setLoopOnce();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setLoopClamp(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.setLoopClamp();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setLoopPingPong(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTDescr ret=self.setLoopPingPong();
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnComplete(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(System.Action))){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action a1;
				checkDelegate(l,2,out a1);
				LTDescr ret=self.setOnComplete(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(SLua.LuaFunction))){
				LTDescr self=(LTDescr)checkSelf(l);
				SLua.LuaFunction a1;
				checkType(l,2,out a1);
				LTDescr ret=self.setOnComplete(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(System.Action<object>))){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<System.Object> a1;
				checkDelegate(l,2,out a1);
				LTDescr ret=self.setOnComplete(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(argc==3){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<System.Object> a1;
				checkDelegate(l,2,out a1);
				System.Object a2;
				checkType(l,3,out a2);
				LTDescr ret=self.setOnComplete(a1,a2);
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
	static public int setOnCompleteParam(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Object a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setOnCompleteParam(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnUpdate(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(argc==2){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<System.Single> a1;
				checkDelegate(l,2,out a1);
				LTDescr ret=self.setOnUpdate(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(System.Action<System.Single,object>),typeof(System.Object))){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<System.Single,System.Object> a1;
				checkDelegate(l,2,out a1);
				System.Object a2;
				checkType(l,3,out a2);
				LTDescr ret=self.setOnUpdate(a1,a2);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(System.Action<UnityEngine.Vector3,object>),typeof(System.Object))){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<UnityEngine.Vector3,System.Object> a1;
				checkDelegate(l,2,out a1);
				System.Object a2;
				checkType(l,3,out a2);
				LTDescr ret=self.setOnUpdate(a1,a2);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(System.Action<UnityEngine.Vector3>),typeof(System.Object))){
				LTDescr self=(LTDescr)checkSelf(l);
				System.Action<UnityEngine.Vector3> a1;
				checkDelegate(l,2,out a1);
				System.Object a2;
				checkType(l,3,out a2);
				LTDescr ret=self.setOnUpdate(a1,a2);
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
	static public int setOnUpdateObject(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Action<System.Single,System.Object> a1;
			checkDelegate(l,2,out a1);
			LTDescr ret=self.setOnUpdateObject(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnUpdateVector3(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Action<UnityEngine.Vector3> a1;
			checkDelegate(l,2,out a1);
			LTDescr ret=self.setOnUpdateVector3(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnUpdateColor(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Action<UnityEngine.Color> a1;
			checkDelegate(l,2,out a1);
			LTDescr ret=self.setOnUpdateColor(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnUpdateParam(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Object a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setOnUpdateParam(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOrientToPath(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setOrientToPath(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOrientToPath2d(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setOrientToPath2d(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setRect(IntPtr l) {
		try{
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(LTRect))){
				LTDescr self=(LTDescr)checkSelf(l);
				LTRect a1;
				checkType(l,2,out a1);
				LTDescr ret=self.setRect(a1);
				pushValue(l,ret);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Rect))){
				LTDescr self=(LTDescr)checkSelf(l);
				UnityEngine.Rect a1;
				checkType(l,2,out a1);
				LTDescr ret=self.setRect(a1);
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
	static public int setPath(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			LTBezierPath a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setPath(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setPoint(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			UnityEngine.Vector3 a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setPoint(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setDestroyOnComplete(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setDestroyOnComplete(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setAudio(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Object a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setAudio(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int setOnCompleteOnRepeat(IntPtr l) {
		try{
			LTDescr self=(LTDescr)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			LTDescr ret=self.setOnCompleteOnRepeat(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_toggle(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.toggle);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_toggle(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.toggle=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_useEstimatedTime(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.useEstimatedTime);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_useEstimatedTime(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.useEstimatedTime=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_useFrames(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.useFrames);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_useFrames(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.useFrames=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_hasInitiliazed(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.hasInitiliazed);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_hasInitiliazed(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.hasInitiliazed=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_hasPhysics(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.hasPhysics);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_hasPhysics(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.hasPhysics=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_passed(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.passed);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_passed(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Single v;
		checkType(l,2,out v);
		o.passed=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_delay(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.delay);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_delay(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Single v;
		checkType(l,2,out v);
		o.delay=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_time(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.time);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_time(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Single v;
		checkType(l,2,out v);
		o.time=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_lastVal(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.lastVal);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_lastVal(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Single v;
		checkType(l,2,out v);
		o.lastVal=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_loopCount(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.loopCount);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_loopCount(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Int32 v;
		checkType(l,2,out v);
		o.loopCount=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_counter(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.counter);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_counter(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.UInt32 v;
		checkType(l,2,out v);
		o.counter=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_direction(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.direction);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_direction(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Single v;
		checkType(l,2,out v);
		o.direction=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_destroyOnComplete(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.destroyOnComplete);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_destroyOnComplete(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.destroyOnComplete=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_trans(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.trans);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_trans(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Transform v;
		checkType(l,2,out v);
		o.trans=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_ltRect(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.ltRect);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_ltRect(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		LTRect v;
		checkType(l,2,out v);
		o.ltRect=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_from(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.from);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_from(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Vector3 v;
		checkType(l,2,out v);
		o.from=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_to(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.to);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_to(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Vector3 v;
		checkType(l,2,out v);
		o.to=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_diff(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.diff);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_diff(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Vector3 v;
		checkType(l,2,out v);
		o.diff=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_point(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.point);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_point(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Vector3 v;
		checkType(l,2,out v);
		o.point=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_axis(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.axis);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_axis(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Vector3 v;
		checkType(l,2,out v);
		o.axis=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_origRotation(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.origRotation);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_origRotation(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.Quaternion v;
		checkType(l,2,out v);
		o.origRotation=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_path(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.path);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_path(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		LTBezierPath v;
		checkType(l,2,out v);
		o.path=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_spline(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.spline);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_spline(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		LTSpline v;
		checkType(l,2,out v);
		o.spline=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_type(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushEnum(l,(int)o.type);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_type(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		TweenAction v;
		checkEnum(l,2,out v);
		o.type=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_tweenType(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushEnum(l,(int)o.tweenType);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_tweenType(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		LeanTweenType v;
		checkEnum(l,2,out v);
		o.tweenType=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_animationCurve(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.animationCurve);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_animationCurve(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		UnityEngine.AnimationCurve v;
		checkType(l,2,out v);
		o.animationCurve=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_loopType(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushEnum(l,(int)o.loopType);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_loopType(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		LeanTweenType v;
		checkEnum(l,2,out v);
		o.loopType=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateFloat(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<System.Single> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateFloat=v;
		else if(op==1) o.onUpdateFloat+=v;
		else if(op==2) o.onUpdateFloat-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateFloatObject(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<System.Single,System.Object> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateFloatObject=v;
		else if(op==1) o.onUpdateFloatObject+=v;
		else if(op==2) o.onUpdateFloatObject-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateVector3(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<UnityEngine.Vector3> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateVector3=v;
		else if(op==1) o.onUpdateVector3+=v;
		else if(op==2) o.onUpdateVector3-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateVector3Object(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<UnityEngine.Vector3,System.Object> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateVector3Object=v;
		else if(op==1) o.onUpdateVector3Object+=v;
		else if(op==2) o.onUpdateVector3Object-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateColor(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<UnityEngine.Color> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateColor=v;
		else if(op==1) o.onUpdateColor+=v;
		else if(op==2) o.onUpdateColor-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onComplete(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onComplete=v;
		else if(op==1) o.onComplete+=v;
		else if(op==2) o.onComplete-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_onLuaComplete(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.onLuaComplete);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onLuaComplete(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		SLua.LuaFunction v;
		checkType(l,2,out v);
		o.onLuaComplete=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onCompleteObject(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Action<System.Object> v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onCompleteObject=v;
		else if(op==1) o.onCompleteObject+=v;
		else if(op==2) o.onCompleteObject-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_onCompleteParam(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.onCompleteParam);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onCompleteParam(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Object v;
		checkType(l,2,out v);
		o.onCompleteParam=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_onUpdateParam(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.onUpdateParam);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateParam(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Object v;
		checkType(l,2,out v);
		o.onUpdateParam=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_onCompleteOnRepeat(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.onCompleteOnRepeat);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onCompleteOnRepeat(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Boolean v;
		checkType(l,2,out v);
		o.onCompleteOnRepeat=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_optional(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.optional);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_optional(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		System.Collections.Hashtable v;
		checkType(l,2,out v);
		o.optional=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_uniqueId(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.uniqueId);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_id(IntPtr l) {
		LTDescr o = (LTDescr)checkSelf(l);
		pushValue(l,o.id);
		return 1;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"LTDescr");
		addMember(l,cancel);
		addMember(l,reset);
		addMember(l,pause);
		addMember(l,resume);
		addMember(l,setAxis);
		addMember(l,setDelay);
		addMember(l,setEase);
		addMember(l,setTo);
		addMember(l,setFrom);
		addMember(l,setHasInitialized);
		addMember(l,setId);
		addMember(l,setRepeat);
		addMember(l,setLoopType);
		addMember(l,setUseEstimatedTime);
		addMember(l,setUseFrames);
		addMember(l,setLoopCount);
		addMember(l,setLoopOnce);
		addMember(l,setLoopClamp);
		addMember(l,setLoopPingPong);
		addMember(l,setOnComplete);
		addMember(l,setOnCompleteParam);
		addMember(l,setOnUpdate);
		addMember(l,setOnUpdateObject);
		addMember(l,setOnUpdateVector3);
		addMember(l,setOnUpdateColor);
		addMember(l,setOnUpdateParam);
		addMember(l,setOrientToPath);
		addMember(l,setOrientToPath2d);
		addMember(l,setRect);
		addMember(l,setPath);
		addMember(l,setPoint);
		addMember(l,setDestroyOnComplete);
		addMember(l,setAudio);
		addMember(l,setOnCompleteOnRepeat);
		addMember(l,"toggle",get_toggle,set_toggle,true);
		addMember(l,"useEstimatedTime",get_useEstimatedTime,set_useEstimatedTime,true);
		addMember(l,"useFrames",get_useFrames,set_useFrames,true);
		addMember(l,"hasInitiliazed",get_hasInitiliazed,set_hasInitiliazed,true);
		addMember(l,"hasPhysics",get_hasPhysics,set_hasPhysics,true);
		addMember(l,"passed",get_passed,set_passed,true);
		addMember(l,"delay",get_delay,set_delay,true);
		addMember(l,"time",get_time,set_time,true);
		addMember(l,"lastVal",get_lastVal,set_lastVal,true);
		addMember(l,"loopCount",get_loopCount,set_loopCount,true);
		addMember(l,"counter",get_counter,set_counter,true);
		addMember(l,"direction",get_direction,set_direction,true);
		addMember(l,"destroyOnComplete",get_destroyOnComplete,set_destroyOnComplete,true);
		addMember(l,"trans",get_trans,set_trans,true);
		addMember(l,"ltRect",get_ltRect,set_ltRect,true);
		addMember(l,"from",get_from,set_from,true);
		addMember(l,"to",get_to,set_to,true);
		addMember(l,"diff",get_diff,set_diff,true);
		addMember(l,"point",get_point,set_point,true);
		addMember(l,"axis",get_axis,set_axis,true);
		addMember(l,"origRotation",get_origRotation,set_origRotation,true);
		addMember(l,"path",get_path,set_path,true);
		addMember(l,"spline",get_spline,set_spline,true);
		addMember(l,"type",get_type,set_type,true);
		addMember(l,"tweenType",get_tweenType,set_tweenType,true);
		addMember(l,"animationCurve",get_animationCurve,set_animationCurve,true);
		addMember(l,"loopType",get_loopType,set_loopType,true);
		addMember(l,"onUpdateFloat",null,set_onUpdateFloat,true);
		addMember(l,"onUpdateFloatObject",null,set_onUpdateFloatObject,true);
		addMember(l,"onUpdateVector3",null,set_onUpdateVector3,true);
		addMember(l,"onUpdateVector3Object",null,set_onUpdateVector3Object,true);
		addMember(l,"onUpdateColor",null,set_onUpdateColor,true);
		addMember(l,"onComplete",null,set_onComplete,true);
		addMember(l,"onLuaComplete",get_onLuaComplete,set_onLuaComplete,true);
		addMember(l,"onCompleteObject",null,set_onCompleteObject,true);
		addMember(l,"onCompleteParam",get_onCompleteParam,set_onCompleteParam,true);
		addMember(l,"onUpdateParam",get_onUpdateParam,set_onUpdateParam,true);
		addMember(l,"onCompleteOnRepeat",get_onCompleteOnRepeat,set_onCompleteOnRepeat,true);
		addMember(l,"optional",get_optional,set_optional,true);
		addMember(l,"uniqueId",get_uniqueId,null,true);
		addMember(l,"id",get_id,null,true);
		createTypeMetatable(l,constructor, typeof(LTDescr));
	}
}
