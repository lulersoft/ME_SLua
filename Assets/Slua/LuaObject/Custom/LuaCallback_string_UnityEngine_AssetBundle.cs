
using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace SLua
{
    public partial class LuaObject
    {

        static internal int checkDelegate(IntPtr l,int p,out Callback<System.String,UnityEngine.AssetBundle> ua) {
            int op = extractFunction(l,p);
			if(LuaDLL.lua_isnil(l,-1)) {
				ua=null;
				return op;
			}
            else if (LuaDLL.lua_isuserdata(l, p)==1)
            {
                ua = (Callback<System.String,UnityEngine.AssetBundle>)checkObj(l, p);
                return op;
            }
            LuaDelegate ld;
            checkType(l, -1, out ld);
            if(ld.d!=null)
            {
                ua = (Callback<System.String,UnityEngine.AssetBundle>)ld.d;
                return op;
            }
			LuaDLL.lua_pop(l,1);
            ua = (string a1,UnityEngine.AssetBundle a2) =>
            {
                int error = pushTry(l);

				pushValue(l,a1);
				pushValue(l,a2);
				ld.call(2, error);
				LuaDLL.lua_settop(l, error-1);
			};
			ld.d=ua;
			return op;
		}
	}
}
