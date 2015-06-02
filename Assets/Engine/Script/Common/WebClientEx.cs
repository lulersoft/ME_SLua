using UnityEngine;
using System.Collections;
using System.Net;
using SLua;
[CustomLuaClass]
public class WebClientEx : WebClient
{
	private int _timeout;
	/// <summary>
	/// 超时时间(毫秒)
	/// </summary>
	public int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
		}
	}

	public WebClientEx()
	{
		this._timeout = 3000;
	}

	public WebClientEx(int timeout)
	{
		this._timeout = timeout;
	}
	public void downFile(string src,string SavePath){
		try
		{
			DownloadFileAsync(new System.Uri(src), SavePath);
		}
		catch (System.Exception e)
		{
			Debug.Log("AsyncDownLoad err:" + e.Message);
		}
	}
	protected override WebRequest GetWebRequest(System.Uri address)
	{
		var result = base.GetWebRequest(address);
		result.Timeout = this._timeout;
		return result;
	}
	protected override void OnDownloadFileCompleted (System.ComponentModel.AsyncCompletedEventArgs e)
	{
		if (complete != null)
			complete (e);
	}
	protected override void OnDownloadProgressChanged (DownloadProgressChangedEventArgs e)
	{
		if (progress != null)
			progress (e);
	}
	public void Close(){

		Dispose ();
	}
	public DownProgress progress;
	public DownComplete complete;

	public delegate void DownProgress(DownloadProgressChangedEventArgs e);
	public delegate void DownComplete(System.ComponentModel.AsyncCompletedEventArgs e);
}

