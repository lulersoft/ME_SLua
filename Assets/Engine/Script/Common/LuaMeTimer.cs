using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;
[CustomLuaClass]
public class LuaMeTimer : MonoBehaviour {

    public static GameObject obj=null;

    public static List<MeTimer> TimerList = new List<MeTimer>();
    
	void Awake () {
	    obj=this.gameObject;
	}
    private float deltaTime = 0;
    private float nowTime = 0;
    int now()
    {
        return (int)(nowTime * 1000);
    }

	void Update () {
        deltaTime = Time.deltaTime;
        nowTime += deltaTime;
        if (TimerList.Count > 0)
        {
            List<MeTimer> tmp = new List<MeTimer>();

            foreach (MeTimer t in TimerList)
            {
                if (t.close)
                {                   
                    tmp.Add(t);
                    continue;
                }                
                t.deltaTime += deltaTime;
                
                if ( t.deltaTime * 1000 >= t.interval)
                {
                    if (t.onTimer != null)
                         t.onTimer(t);
                    //是永久循环
                     if (t.loop == 0)
                     {
                         t.deltaTime = 0;
                     }
                     else
                     {
                         //计算已经循环多少次
                         if (t.count < t.loop)
                         {
                             t.count +=1;
                         }
                         else
                         {
                             if (t.onCompleted != null)
                                 t.onCompleted(t);
                             tmp.Add(t);
                         }
                     }
                } 
            }

            foreach (MeTimer t in tmp)
            {
                TimerList.Remove(t);
            }

            tmp.Clear();
        }
	}
}

[CustomLuaClass]
public class MeTimer
{
    public int id = 0;
    public Callback<MeTimer> onTimer = null;
    public Callback<MeTimer> onCompleted = null; //完成
    public float interval = 0;//间隔时间
    public float deltaTime=0;
    public int delay = 0;//延迟时间
    public int loop = 0;//循环次数,,0为永远循环
    public int count = 0;//已执行循环次数
    public bool enabled=true;
    public bool close = false; 

    static int gid=0;
    public MeTimer()
    {
        id = ++gid;
    }
}
