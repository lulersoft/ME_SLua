using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Packager
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle" };
    static bool CanCopy(string ext)
    {   //能不能复制
        foreach (string e in exts)
        {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    [MenuItem("ME Tools/清理缓存,让一切重新开始")]
    static void CleanCacheFiles()
    {
        string CachePath = Application.persistentDataPath;
        DirectoryInfo di = new DirectoryInfo(CachePath);
        di.Delete(true);

        Debug.Log("目录:" + CachePath + " 已经删除!");
    }

    [MenuItem("ME Tools/制作更新包：把Data目录压缩为一个zip包并放入StreamingAssets目录")]
    static void PackFiles()
    {   
  
        string assetPath = Application.dataPath + "/StreamingAssets/";
        string srcPath = Application.dataPath + "/Data/";

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }

        cleanMeta(srcPath);

        //压缩文件    
        API.PackFiles(assetPath + "/data.zip", srcPath);
        AssetDatabase.Refresh();

        Debug.Log("Data目录压缩打包成功，文件：" + assetPath + "/data.zip");
        Debug.Log("把这个文件上传到web更新服务器目录吧");

    }
    /*
    [MenuItem("Game/UnZIP Data folder ")] 
    static void UnpackFiles()
    {
        //解压文件
        Util.UnpackFiles(Application.dataPath + "/data.zip", Application.dataPath + "/data/");
        AssetDatabase.Refresh();
    }
    */
    /// <summary>
    /// 载入素材
    /// </summary>
    static UnityEngine.Object LoadAsset(string file)
    {
        if (file.EndsWith(".lua")) file += ".txt";
        return AssetDatabase.LoadMainAssetAtPath("Assets/Builds/" + file);
    }

    //打包单个
    [MenuItem("ME Tools/独立打包选中目录下的各个对象并放入Data目录")]
    static void CreateAssetBunldesMain()
    {
        //获取在Project视图中选择的所有游戏对象
        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        BuildTarget target = GetTargetPlatform();
        string assetPath = Application.dataPath + "/Data/asset/" + target.ToString().ToLower() + "/";

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }

        //遍历所有的游戏对象 
        foreach (Object obj in SelectedAsset)
        {            
            //本地测试：建议最后将Assetbundle放在StreamingAssets文件夹下，如果没有就创建一个，因为移动平台下只能读取这个路径
            //StreamingAssets是只读路径，不能写入
            //服务器下载：就不需要放在这里，服务器上客户端用www类进行下载。
            if (obj is GameObject)
            {
                string targetPath = assetPath + obj.name + ".assetbundle";
                if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies, target))
                {
                    Debug.Log(obj.name + " ==>资源打包成功");
                }
                else
                {
                    Debug.Log(obj.name + " ==>资源打包失败");
                }
            }
        }
        //刷新编辑器 
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 生成绑定素材
    /// </summary>
    [MenuItem("ME Tools/把Builds目录下的资源进行依赖打包并放入Data目录")]
    public static void BuildAssetResource()
    {
        Debug.Log("请自行修改以下依赖代码");
        /*
        Object mainAsset = null;        //主素材名，单个
        Object[] addis = null;     //附加素材名，多个
        string assetfile = string.Empty;  //素材文件名

        BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies |
                                          BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

        BuildTarget target = GetTargetPlatform();

        string assetPath = Application.dataPath + "/Data/asset/" + target.ToString().ToLower() + "/";
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        ///-----------------------------生成共享的关联性素材绑定-------------------------------------
        BuildPipeline.PushAssetDependencies();

        assetfile = assetPath + "shared.assetbundle";
        mainAsset = LoadAsset("Shared/Atlas/Dialog.prefab");
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options,target);

        ///------------------------------生成PromptPanel素材绑定-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Prompt/Prefabs/PromptPanel.prefab");
        addis = new Object[1];
        addis[0] = LoadAsset("Prompt/Prefabs/PromptItem.prefab");
        assetfile = assetPath + "prompt.assetbundle";
        BuildPipeline.BuildAssetBundle(mainAsset, addis, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///------------------------------生成MessagePanel素材绑定-----------------------------------
        BuildPipeline.PushAssetDependencies();
        mainAsset = LoadAsset("Message/Prefabs/MessagePanel.prefab");
        assetfile = assetPath + "message.assetbundle";
        BuildPipeline.BuildAssetBundle(mainAsset, null, assetfile, options, target);
        BuildPipeline.PopAssetDependencies();

        ///-------------------------------刷新---------------------------------------
        BuildPipeline.PopAssetDependencies();
        AssetDatabase.Refresh();
         * */
    }


    [MenuItem("ME Tools/把Atlas目录下的.png图片作为图集资源并放入Data目录")]
    static private void BuildUnityGUIAssetBundle()
    {
       // string dir = Application.dataPath + "/StreamingAssets";
        BuildTarget target = GetTargetPlatform();
        string assetPath = Application.dataPath + "/Data/asset/" + target.ToString().ToLower() + "/Atlas/";

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }
        DirectoryInfo rootDirInfo = new DirectoryInfo(Application.dataPath + "/Atlas");
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            List<Sprite> assets = new List<Sprite>();
            string path = assetPath + "/" + dirInfo.Name + ".assetbundle";
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
            {

                string allPath = pngFile.FullName; Debug.Log(allPath);
                string dir = allPath.Substring(allPath.IndexOf("Assets"));
                assets.Add(Resources.LoadAssetAtPath<Sprite>(dir));
            }
            if (BuildPipeline.BuildAssetBundle(null, assets.ToArray(), path, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies, target))
            {
            }
        }	
    }


    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    static private BuildTarget GetBuildTarget()
    {
        BuildTarget target = BuildTarget.WebPlayer;
#if UNITY_STANDALONE
			target = BuildTarget.StandaloneWindows;
#elif UNITY_IPHONE
			target = BuildTarget.iPhone;
#elif UNITY_ANDROID
			target = BuildTarget.Android;
#endif
        return target;
    }

    //资源目标平台
    static BuildTarget GetTargetPlatform()
    {
         BuildTarget target;
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            target = BuildTarget.iPhone;
        }
        else
        {
            target = BuildTarget.Android;
        }
        return target;

    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void cleanMeta(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta"))
            {
                //Debug.Log(filename);
                File.Delete(@filename);
            }

            foreach (string dir in dirs)
            {
                cleanMeta(dir);
            }

        }        
    }
}