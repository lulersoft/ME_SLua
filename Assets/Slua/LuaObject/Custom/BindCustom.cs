using System;
namespace SLua {
	public partial class LuaObject {
		public static void BindCustom(IntPtr l) {
			Lua_MessengerHelper.reg(l);
			Lua_API.reg(l);
			Lua_LuaBehaviour.reg(l);
			Lua_Activity.reg(l);
			Lua_MissionPack.reg(l);
			Lua_WebClientEx.reg(l);
			Lua_EventListener.reg(l);
			Lua_LeanTweenType.reg(l);
			Lua_LTDescr.reg(l);
			Lua_LTRect.reg(l);
			Lua_LeanTween.reg(l);
			Lua_Lua.reg(l);
			Lua_Custom.reg(l);
			Lua_Deleg.reg(l);
			Lua_HelloWorld.reg(l);
		}
	}
}
