using UnityEngine;
using System.Collections;
using System.IO;
using SLua;

/// <summary>
/// Activity
/// @author 小陆  QQ:2604904  
/// </summary>
[CustomLuaClassAttribute]
public class Activity : LuaBehaviour
{

   protected bool isDebug = false;
   protected string _name = "main.lua";

    void Awake()
    {       
        InitAsstes(); 
        /*
    var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
 	var material=(cube.GetComponent("Renderer") as Renderer).material;
 	Debug.Log(material.color);
 	Debug.Log(Color.red);
    material.color = Color.red;
        */
    }

    IEnumerator loadStreamingAssets()
    {
        string sorucefilename = "data.zip";
        string filename = API.AssetRoot + sorucefilename; 
        string log = "";       

        byte[] bytes = null;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

        string sourcepath = "file:///" + Application.streamingAssetsPath + "/" + sorucefilename;
        log += "asset path is: " + sourcepath;
        WWW www = new WWW(sourcepath);
        yield return www;
        if (www.error != null)
        {
            Debug.Log("Warning errow: " + "loadStreamingAssets");
            yield break;
        }
        bytes = www.bytes;

#elif UNITY_WEBPLAYER 
        string sourcepath = "StreamingAssets/" + sorucefilename;        log += "asset path is: " + sourcepath; 
        WWW www = new WWW(sourcepath); 			
        yield return www;
        if (www.error != null)
        {           
            Debug.Log("Warning errow: " + "loadStreamingAssets");
            yield break;
        }
		bytes = www.bytes; 
#elif UNITY_IPHONE 
		string sourcepath = Application.dataPath + "/Raw/" + sorucefilename;     log += "asset path is: " + sourcepath;     
		try{ 
			using ( FileStream fs = new FileStream(sourcepath, FileMode.Open, FileAccess.Read, FileShare.Read) ){ 
				bytes = new byte[fs.Length]; 
				fs.Read(bytes,0,(int)fs.Length); 
			}   
		} catch (System.Exception e){ 
			log +=  "\nTest Fail with Exception " + e.ToString(); 
			log +=  "\n"; 
		} 
#elif UNITY_ANDROID 
		string sourcepath = "jar:file://" + Application.dataPath + "!/assets/"+sorucefilename; 			
		//NGUIDebug.Log("文件路径为：" + sourcepath); 
		log += "asset path is: " + sourcepath; 
		WWW www = new WWW(sourcepath); 
        yield return www;
        if (www.error != null)
        {           
            Debug.Log("Warning errow: " + "loadStreamingAssets");
            yield break;
        }
		bytes = www.bytes; 
		//Debug.Log("字节长度为：" + bytes.Length); 
#endif
        if (bytes != null)
        {
            // 
            // 
            // copy zip  file into cache folder 
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length);
                log += "\nCopy res form streaminAssets to persistentDataPath: " + filename;
                fs.Close();
            }

            //解压缩
            API.UnpackFiles(filename, API.AssetRoot);

            yield return new WaitForEndOfFrame();

            //删除临时zip包
            File.Delete(@filename);

            yield return new WaitForEndOfFrame();

            log += string.Format("\n{0} created!  ", "res");
            Debug.Log(log);

            //加载入口文件 main.lua
            DoFile(_name,null);   
        }
    }
    void InitAsstes()
    {       
        string mainfile =API.AssetRoot+ "lua/"+_name;
        string assetFolder = API.AssetRoot + "/asset/";
        //如果入口主main.lua未找到       
        if (!Directory.Exists(assetFolder) || !File.Exists(mainfile) || isDebug)
        {
            //解压主资源文件
            StartCoroutine(loadStreamingAssets());             
        }
        else
        {
            DoFile(_name,null); 
        }
    }
    //重置框架,相当于重新启动游戏
    public void ReStart()
    {
        StartCoroutine(onReStart()); 
    }
    IEnumerator onReStart()
    {
        yield return new WaitForEndOfFrame();
        if (table != null)
        {
            table.Dispose();
        }
        yield return new WaitForEndOfFrame();
        API.CleanLuaEnv();
        yield return new WaitForEndOfFrame();
        InitAsstes();
    }
}
