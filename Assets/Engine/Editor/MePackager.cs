using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class MePackager
{
    [MenuItem("ME Tools/1.清理缓存,让一切重新开始")]
    public static void CleanCacheFiles()
    {     
        string cachePath = API.AssetRoot;
        DirectoryInfo di = new DirectoryInfo(cachePath);
        di.Delete(true);

        Debug.Log("缓存目录:" + cachePath + " 已经删除!");
    }



    [MenuItem("ME Tools/2.制作新资源包 : 生成新资源并放入Data->asset目录 (for Unity5.0+)")]
    public static void BuildAssets()
    {       
        //复制lua
        EncryptLua();

        BuildTarget target = GetTargetPlatform();
        string mPath = Application.dataPath + "/Builds/AssetBundles/";
        string toPath = Application.dataPath + "/Data/asset/" + target.ToString().ToLower() + "/";

        
        if (!Directory.Exists(toPath))
        {
            Debug.Log("创建" + toPath);
            Directory.CreateDirectory(toPath);
        }
        

        if (!Directory.Exists(mPath))
        {
            Debug.Log("创建" + mPath);
            Directory.CreateDirectory(mPath);
        }
        DirectoryInfo mDirInfo = new DirectoryInfo(mPath);

        Debug.Log("打包资源到" + mPath+"目录");
        BuildPipeline.BuildAssetBundles(mPath,BuildAssetBundleOptions.None, target);

        Debug.Log("AssetBundle打包完成！");

        //复制manifest索引文件
        EncryptFile(mPath + "AssetBundles", toPath + "AssetBundles",false);

        //复制资源        
        foreach (FileInfo mFile in mDirInfo.GetFiles("*" + API.assetbundle_extension, SearchOption.AllDirectories))
        {
            string from = mFile.FullName;
            string to = from.Replace("\\", "/");           
            to = to.Replace(mPath, toPath);
            EncryptFile(from, to,false);
        }
        Debug.Log("新资源文件已复制到" + toPath);
    }
    
    [MenuItem("ME Tools/3.制作ZIP更新包 : 把Data目录压缩为一个zip包并放入StreamingAssets目录")]
    public static void PackFiles()
    {
        //本地测试：建议最后将Assetbundle放在StreamingAssets文件夹下，如果没有就创建一个，因为移动平台下只能读取这个路径
        //StreamingAssets是只读路径，不能写入
        //服务器下载：把最后生成的压缩包zip复制出来放到服务器上，客户端用www类进行下载。
        string assetPath = Application.dataPath + "/StreamingAssets/";
        string srcPath = Application.dataPath + "/Data/";

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }
        //复制Lua
        EncryptLua();

        //清理,meta
        cleanMeta(srcPath);

        //压缩文件    
        API.PackFiles(assetPath + "data.zip", srcPath);
        AssetDatabase.Refresh();

        Debug.Log("Data目录压缩打包成功，文件：" + assetPath + "data.zip");
        Debug.Log("把这个文件上传到web更新服务器目录吧");

    }
    /*
    [MenuItem("ME Tools/UnZIP Data folder ")] 
    static void UnpackFiles()
    {
        //解压文件
       string assetPath = Application.dataPath + "/StreamingAssets/";
       string CachePath = API.AssetRoot;
        Util.UnpackFiles(assetPath + "data.zip", CachePath);
        AssetDatabase.Refresh();
    }
    */

    public static void EncryptFile(string form, string to, bool wholeFile)
    {
        FileInfo fi = new FileInfo(form);
        long len = fi.Length;
        FileStream fs = new FileStream(form, FileMode.Open);
        byte[] buffer = new byte[len];
        fs.Read(buffer, 0, (int)len);
        fs.Close();

        //rc4
        if (wholeFile)
        {
            API.EncryptAll(ref buffer);
        }
        else
        {
            API.Encrypt(ref buffer);
        }
        //写入文件
        FileStream out_fs = new FileStream(to, FileMode.OpenOrCreate);
        out_fs.Write(buffer, 0, buffer.Length);
        out_fs.Close();

        //删除临时文件
        if (!form.Equals(to))
        {
            //File.Delete(form);  //不要删除Builds/AssetBundles下的ab文件，这样unity5.0新的打包机制可以发挥作用，对没有变化的ab资源不会重复打包，大大加快打包速度
        }

        //Debug.Log(to + " ==>ok!!");
    }

    public static void EncryptLua()
    {

        string srcPath = Application.dataPath + "/Lua/";
        string toPath = Application.dataPath + "/Data/lua/";

        DirectoryInfo srcDirInfo = new DirectoryInfo(srcPath);
        DirectoryInfo toDirInfo = new DirectoryInfo(toPath);
        
        if (toDirInfo.Exists)
        {
            Debug.Log("删除" + toPath);
            toDirInfo.Delete(true);

            Debug.Log("创建" + toPath);
            Directory.CreateDirectory(toPath);
        }

        //复制
        foreach (FileInfo luaFile in srcDirInfo.GetFiles("*.lua", SearchOption.AllDirectories))
        {
            string form = luaFile.FullName;
            string to = "";
            to = form.Replace("\\", "/");
            to = to.Replace(srcPath, toPath);

            string path = Path.GetDirectoryName(to);
            if (!Directory.Exists(path))
            {
                //Debug.LogWarning("创建" + path);
                Directory.CreateDirectory(path);
            }
            File.Copy(form, to, true);
            FileInfo fi = new FileInfo(to);
         
            if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1) 
            {
                fi.Attributes = FileAttributes.Normal; //去除只读属性
            }          
        }

        //以下是进行lua脚本的加密,（加密:windows版l已OK（luac批处理）,ios的请高手提交代码; 解密:待完善,即让lua requrie函数使用c#函数来执行解密）
        /*
          //luac 
         runLuac();
        */

        if (API.usingEncryptLua)
        {
            //rc4 lua files
            foreach (FileInfo luaFile in toDirInfo.GetFiles("*.lua", SearchOption.AllDirectories))
            {
                string allPath = luaFile.FullName;
                Debug.Log("加密" + allPath);
                EncryptFile(allPath, allPath,true); //进行RC4
            }
        }
    }

    //luac for windows
    static void runLuac()
    {
        /*
#if UNITY_EDITOR_WIN
        string runPath = Application.dataPath + "/Engine/Tools/run.bat";
        string luacPath = Application.dataPath+"/Engine/Tools/luac.exe";
        string encodePath = Application.dataPath + "/Data/lua/";

        System.Diagnostics.Process  p = new System.Diagnostics.Process();
        p.StartInfo.FileName = runPath;  
        p.StartInfo.UseShellExecute = false;  
        // p.StartInfo.CreateNoWindow = true;  
        p.StartInfo.Arguments = luacPath + " " + encodePath;
        p.Start();               
        //p.StandardInput.WriteLine("exit");  
        p.WaitForExit();  
        p.Dispose();
        p.Close();
#endif
         */ 
    }

    [MenuItem("ME Tools/4.同步代码到缓存 : 直接复制Asset->Lua目录(内含所有lua脚本文件)到缓存")]
    public static void copyToCache()
    {
        string targetPath = API.AssetRoot + "lua";
        string srcPath = Application.dataPath + "/Lua";

        List<FileInfo> files = new List<FileInfo>();
        getSubFiles(srcPath, files, 1);

        for (int i = 0; files != null && i < files.Count; i++)
        {
            string subPath = files[i].FullName.Substring(srcPath.Length);
            string targetFullFilePath = targetPath + subPath;

            string path = Path.GetDirectoryName(targetFullFilePath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            byte[] fileBytes = File.ReadAllBytes(files[i].FullName);
            File.WriteAllBytes(targetFullFilePath, fileBytes);

            if (API.usingEncryptLua)
            {
                EncryptFile(targetFullFilePath, targetFullFilePath,true); //进行RC4
            }
        }      

        Debug.Log(srcPath + " 文件夹已复制到 " + targetPath);
    }

    [MenuItem("ME Tools/5.清理旧资源包 : 清除Builds->AssetBundles,Data->asset目录,建议仅在有删除资源时使用 (for Unity5.0+)")]
    public static void ClearAssets()
    {
        //清空
        string mPath = Application.dataPath + "/Builds/AssetBundles/";
        DirectoryInfo mDirInfo = new DirectoryInfo(mPath);
        if (mDirInfo.Exists)
        {
            mDirInfo.Delete(true);
        }
        Debug.LogWarning("已清除" + mPath);

        string toPath = Application.dataPath + "/Data/asset/";
        DirectoryInfo toDirInfo = new DirectoryInfo(toPath);
        if (toDirInfo.Exists)
        {
            toDirInfo.Delete(true);
        }
        Debug.Log("已清除" + Application.dataPath + "/Data/asset/");
    }

    protected static void getSubFiles(string path, List<FileInfo> fileList, int type)
    {
        if (type == 1) //目录
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] subFolders = dir.GetDirectories();
            FileInfo[] subFiles = dir.GetFiles();
            for (int i = 0; subFolders != null && i < subFolders.Length; i++)
            {
                getSubFiles(subFolders[i].FullName, fileList, 1);
            }

            for (int i = 0; subFiles != null && i < subFiles.Length; i++)
            {
                getSubFiles(subFiles[i].FullName, fileList, 2);
            }
        }
        else
            if (type == 2) //文件
            {
                FileInfo file = new FileInfo(path);
                //Debug.LogWarning("file.Extension=" + file.Extension);
                if (!file.Extension.Equals(".meta"))
                {
                    fileList.Add(file);
                }
            }
    }


    //资源目标平台
    public static BuildTarget GetTargetPlatform()
    {
        BuildTarget target = BuildTarget.Android;
#if UNITY_STANDALONE
        Debug.Log("build for win");
        target = BuildTarget.StandaloneWindows;
#elif UNITY_IPHONE
		Debug.Log ("build for ios");
			target = BuildTarget.iOS;
#elif UNITY_ANDROID
			Debug.Log ("build for android");
			target = BuildTarget.Android;
#endif
        return target;
    }

    public static void cleanMeta(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta"))
            {
                File.Delete(@filename);
            }

            foreach (string dir in dirs)
            {
                cleanMeta(dir);
            }
        }
    }
}