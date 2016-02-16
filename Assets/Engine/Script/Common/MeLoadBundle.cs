using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeLoadBundle : MonoBehaviour {

    public static Hashtable BundleTable = new Hashtable();

    public static GameObject obj = null;
    public static MeLoadBundle self = null;

    private static List<QuequePack> LoadQueueList = new List<QuequePack>();
    private bool isReady = true;

    struct QuequePack{
        public string fname;
        public Callback<string, AssetBundle,object> handler;
        public object arg;
    }

    private AssetBundleManifest _manifest=null;

    protected AssetBundleManifest manifest
    {
        get { 
            if(_manifest==null)
            {
                string uri = API.AssetPath + "AssetBundles";
                byte[] data = System.IO.File.ReadAllBytes(uri);
                API.Encrypt(ref data);
                AssetBundle assetbundle = AssetBundle.LoadFromMemory(data);
                manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            return _manifest;
        }
        set { _manifest = value; }
    }
    private string[] m_Variants = { };

	void Awake () {
        obj = this.gameObject;
        self = this;
        DontDestroyOnLoad(gameObject);  //防止销毁自己
	}
    
    void Update()
    {
        if (LoadQueueList.Count>0 && isReady)
        {
            QuequePack pack = LoadQueueList[0];
            LoadQueueList.RemoveAt(0);
            StartLoadBundle(pack.fname,pack.handler,pack.arg);
        }
    }

    AssetBundle LoadBundle(string fname)
    {    
        AssetBundle bundle = null;
        if (!BundleTable.ContainsKey(fname))
        {
            byte[] data = null;
          
            data = System.IO.File.ReadAllBytes(API.AssetPath + fname);

            API.Encrypt(ref data);
            bundle = AssetBundle.LoadFromMemory(data); 
            BundleTable.Add(fname, bundle);
        }
        else
        {
           bundle= BundleTable[fname] as AssetBundle;
        }
        return bundle;
    }

    public void LoadBundle(string fname, Callback<string, AssetBundle,object> handler,object arg)
    {
        QuequePack pack = new QuequePack();
        pack.fname = fname;
        pack.handler = handler;
        pack.arg=arg;
        LoadQueueList.Add(pack);
    }

    public void StartLoadBundle(string fname, Callback<string, AssetBundle,object> handler,object arg)
    {
        isReady=false;
        if (BundleTable.ContainsKey(fname))
        {
            isReady = true;
            AssetBundle bundle = BundleTable[fname] as AssetBundle;
            if (handler != null) handler(fname, bundle,arg);
        }
        else
        {
            StartCoroutine(onLoadBundle(fname, handler,arg));
        }
    }

    public void UnLoadAllBundle()
    {
        foreach (AssetBundle bundle in BundleTable.Values)
        {
            if (bundle != null)
                bundle.Unload(false);
        }
        BundleTable.Clear();
    }

    //释放某AssetBundle
    public  void UnLoadBundle(AssetBundle bundle)
    {
        string key = "";
        foreach (DictionaryEntry de in BundleTable)
        {
        	if (bundle == (AssetBundle)de.Value){
            	key = de.Key.ToString();        	
            	break; 
        	}
        }

        if (BundleTable.ContainsKey(key))
        {
            BundleTable.Remove(key);
        }
        if (bundle != null)
            bundle.Unload(false);
    }
    //释放某AssetBundle
    public  void UnLoadBundle(string key)
    {
        if (BundleTable.ContainsKey(key))
        {
            AssetBundle bundle = BundleTable[key] as AssetBundle;
            if (bundle != null)
            {
                bundle.Unload(false);
            }
            BundleTable.Remove(key);
        }
    }

    string getFullPathName(string fname)
    {
        string uri = "";
        if (fname.LastIndexOf(".") != -1)
        {
            uri = "file:///" + API.AssetPath + fname;
        }
        else
        {
            uri = "file:///" + API.AssetPath + fname + API.assetbundle_extension;
        }
        return uri;
    }

    protected IEnumerator onLoadBundle(string name, Callback<string, AssetBundle,object> handler,object arg)
    {
        string uri = getFullPathName(name);

        //检测加载依赖
        LoadDependencies(uri);

        WWW www = new WWW(uri);
        yield return www;
        if (www.error != null)
        {
            isReady = true;
            API.Log("Warning erro: " + www.error);
            StopCoroutine("onLoadBundle");
            yield break;
        }
        while (!www.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        byte[] data = www.bytes;

        //资源解密
        API.Encrypt(ref data);

        AssetBundle bundle = AssetBundle.LoadFromMemory(data);

        yield return new WaitForEndOfFrame();

        try
        {
            isReady = true;
            BundleTable[name] = bundle;
            if (handler != null) handler(name, bundle,arg);
        }
        catch (System.Exception e)
        {
            isReady = true;
            Debug.LogError(FormatException(e), gameObject);
        }
    }

    public static string FormatException(System.Exception e)
    {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }

    /// <summary>
    /// 载入依赖
    /// </summary>
    /// <param name="name"></param>
    void LoadDependencies(string fname)
    {
        if (manifest == null)
        {
            Debug.LogError("manifest==null");
            return;
        }

        string dpname = fname.Replace("\\", "/");
        int idx = dpname.LastIndexOf("/") + 1;
        dpname = dpname.Substring(idx, dpname.Length - idx);

        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = manifest.GetAllDependencies(dpname);
       
        if (dependencies.Length == 0) return;

        for (int i = 0; i < dependencies.Length; i++)
            dependencies[i] = RemapVariantName(dependencies[i]);

        // Record and load all dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            Debug.Log(fname+":加载依赖库:" + dependencies[i]);
            LoadBundle(dependencies[i]);
        }
    }

    // Remaps the asset bundle name to the best fitting asset bundle variant.
    string RemapVariantName(string assetBundleName)
    {
        string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

        // If the asset bundle doesn't have variant, simply return.
        if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            return assetBundleName;

        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (int i = 0; i < bundlesWithVariant.Length; i++)
        {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = System.Array.IndexOf(m_Variants, curSplit[1]);
            if (found != -1 && found < bestFit)
            {
                bestFit = found;
                bestFitIndex = i;
            }
        }
        if (bestFitIndex != -1)
            return bundlesWithVariant[bestFitIndex];
        else
            return assetBundleName;
    }

    protected void OnDestroy()
    {
        UnLoadAllBundle();       
    }
}
