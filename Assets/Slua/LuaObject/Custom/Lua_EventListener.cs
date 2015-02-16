using UnityEngine;
using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_EventListener : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		EventListener o;
		o=new EventListener();
		pushObject(l,o);
		return 1;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnPointerClick(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerClick(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnPointerDown(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerDown(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnPointerEnter(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerEnter(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnPointerExit(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerExit(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnPointerUp(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerUp(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnSelect(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.BaseEventData a1;
			checkType(l,2,out a1);
			self.OnSelect(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int OnUpdateSelected(IntPtr l) {
		try{
			EventListener self=(EventListener)checkSelf(l);
			UnityEngine.EventSystems.BaseEventData a1;
			checkType(l,2,out a1);
			self.OnUpdateSelected(a1);
			return 0;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Get_s(IntPtr l) {
		try{
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			EventListener ret=EventListener.Get(a1);
			pushValue(l,ret);
			return 1;
		}
		catch(Exception e) {
			LuaDLL.luaL_error(l, e.ToString());
			return 0;
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onClick(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onClick=v;
		else if(op==1) o.onClick+=v;
		else if(op==2) o.onClick-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onDown(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onDown=v;
		else if(op==1) o.onDown+=v;
		else if(op==2) o.onDown-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onEnter(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onEnter=v;
		else if(op==1) o.onEnter+=v;
		else if(op==2) o.onEnter-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onExit(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onExit=v;
		else if(op==1) o.onExit+=v;
		else if(op==2) o.onExit-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUp(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUp=v;
		else if(op==1) o.onUp+=v;
		else if(op==2) o.onUp-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onSelect(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onSelect=v;
		else if(op==1) o.onSelect+=v;
		else if(op==2) o.onSelect-=v;
		return 0;
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_onUpdateSelect(IntPtr l) {
		EventListener o = (EventListener)checkSelf(l);
		EventListener.VoidDelegate v;
		int op=checkDelegate(l,2,out v);
		if(op==0) o.onUpdateSelect=v;
		else if(op==1) o.onUpdateSelect+=v;
		else if(op==2) o.onUpdateSelect-=v;
		return 0;
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"EventListener");
		addMember(l,OnPointerClick);
		addMember(l,OnPointerDown);
		addMember(l,OnPointerEnter);
		addMember(l,OnPointerExit);
		addMember(l,OnPointerUp);
		addMember(l,OnSelect);
		addMember(l,OnUpdateSelected);
		addMember(l,Get_s);
		addMember(l,"onClick",null,set_onClick,true);
		addMember(l,"onDown",null,set_onDown,true);
		addMember(l,"onEnter",null,set_onEnter,true);
		addMember(l,"onExit",null,set_onExit,true);
		addMember(l,"onUp",null,set_onUp,true);
		addMember(l,"onSelect",null,set_onSelect,true);
		addMember(l,"onUpdateSelect",null,set_onUpdateSelect,true);
		createTypeMetatable(l,constructor, typeof(EventListener),typeof(UnityEngine.EventSystems.EventTrigger));
	}
}
