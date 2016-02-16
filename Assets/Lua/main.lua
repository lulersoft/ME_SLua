--主逻辑 
--升级检测 启动游戏逻辑
--小陆 QQ 2604904
--https://github.com/lulersoft/ME

import 'UnityEngine'
--以上整个项目只需要导入一次

local json = require "json.dkjson"

version=1
appid=1
weburl="http://192.168.1.30/version" --升级网址
AssetRoot=nil --资源目录


local this
local tmpfile
local main={}

function main.Start()

	this=main.this
	AssetRoot=API.AssetRoot	

	--检测更新 
    --main.checkVersion() 

    --性能测试
 	--main.testdemo()

 	--直接启动打地鼠游戏
 	main.RunMoleGame() 	

--[[
	local cube=GameObject.CreatePrimitive(PrimitiveType.Cube)
 	local material=cube:GetComponent(Renderer).material
 	Debug.Log(material)
 	Debug.Log(Color.red)
	material.color=Color.red
--]]

end

--性能测试
function main.testdemo()
	local game = GameObject("Performance")
	--local lb = game:AddComponent(LuaBehaviour)
	local lb = API.AddComponent(game,"LuaBehaviour")
	lb:DoFile("demo/demo",nil)
end

--打地鼠游戏
function main.RunMoleGame( )
	local game = GameObject("moleGame")
	local lb = API.AddComponent(game,"LuaBehaviour")
	lb:DoFile("demo/moleGame",nil)
end

--检查更新
function  main.checkVersion()	
	local data="version="..version.."&appid="..appid..string.format("&Cache=%d",os.time())
	Debug.Log("checkVersion:"..weburl..data)
	local webclient=API.SendRequest(weburl,data,main.OnSendRequestCompleted)	
end

--异步HTTP完成
function main.OnSendRequestCompleted(req,resp)
	Debug.Log("OnSendRequestCompleted")
	if resp.IsSuccess then
 
		local str=resp.DataAsText
		Debug.Log("json:"..str)
		local obj, pos, err = json.decode (str, 1, nil)

		if err then
		  --Debug.Log ("checkUpdate json Error:"..err)
		  --开始游戏逻辑
		  main.StartGame()
		else			
			if obj.status>0 then
			  	if obj.downurl then
			  		--开始升级
			  		tmpfile=AssetRoot.."/tmp.zip"
			  		local _arg="test"
			  		local webclinet=API.DownLoad(obj.downurl,tmpfile,_arg,main.OnDownloadProgressChanged,main.OnDownloadCompleted)
			  	else
			  		--开始游戏逻辑
					main.StartGame()
			  	end
			else
			  	--开始游戏逻辑
				main.StartGame()
			end
		end
	end
end

function main.StartGame()
	main.RunMoleGame()
end

--下载进度回调
function main.OnDownloadProgressChanged(p)
	local str="downloading  "..tostring(p*100).."% completed"
	Debug.Log(str)
end
--下载完成
function main.OnDownloadCompleted(req,resp,p)
	Debug.Log(p) --参数传递测试
	
	if resp.IsSuccess then
		Debug.Log("download completed.. now unzip")
		--tmpfile=this.AssetRoot.."/tmp.zip"
		--解压缩
        API.UnpackFiles(tmpfile,AssetRoot)
        --删除下载的压缩包
        os.remove(tmpfile)
 		--File:Delete(tmpfile)
        Debug.Log("update completed!")

        --重新加载main.lua --或开始执行旧游戏
        --this:ReStart() --立即重新加载游戏（请确保你的版本升级服务器代码生效，如果status一直为1，,将会不断的循环升级）
        main.StartGame() --执行旧的游戏，等下次启动游戏自动应用新版本
    else 
    	Debug.Log("DownLoad file err")
    	main.StartGame()
	end
end


return main
