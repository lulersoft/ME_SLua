using UnityEngine;
using System.Collections;
using SLua;
[CustomLuaClass]
public class DebugTools : MonoBehaviour {

   public static string log = "";
   public static GameObject obj = null;

   void Awake()
   {
       obj = this.gameObject;
   }

   void OnGUI()
   {
       if (GUI.Button(new Rect(Screen.width-45, Screen.height-30, 40, 24), "clean"))
       {
           log = "";
       }
       GUI.Label(new Rect(5, 0, Screen.width, Screen.height), log);
   }
}
