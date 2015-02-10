
local json = require ("json.dkjson")


local Vector3 = UnityEngine.Vector3
local Vector2 = UnityEngine.Vector2
local GameObject = UnityEngine.GameObject
local Profiler = UnityEngine.Profiler
local Debug = UnityEngine.Debug
local PlayerPrefs=UnityEngine.PlayerPrefs
local LayerMask=UnityEngine.LayerMask
local Time = UnityEngine.Time

version=1
appid=1
weburl="http://192.168.1.30/version" --升级网址
AssetRoot=nil


local this
local tmpfile

local main={}
local uiCanvas
local uiLevel
local uibg1
local uibg2
local uibg3
local uibgArr={}

local moleArr={}

local bar 	--生命条 显示
local score --游戏得分显示
local maxscore --游戏最高得分显示
local point=0 --得分

local startBnt --开始按钮
local panel --遮罩层

local hp=10	--生命值

local maxpoint=0 --历史最高得分

local MoleAtlas --mole sprite atlas

local runtime=true

local timer
local gameOver=true

function main.Start()

	this=main.this
	AssetRoot=this.AssetRoot	

	--检测更新 
    main.checkVersion()
    
end

function main.onHitMole(go)
	--计分	
	point=point+1
	if maxpoint<point then
		maxpoint=point
		maxscore.text="max score:"..tostring(maxpoint)
		PlayerPrefs.SetInt("maxpoint",maxpoint)
	end
	score.text="score:"..tostring(point)
end

function main.onMissMole(go)	
	--扣生命点
	hp=hp-1
	local v=hp/10*200
	bar.sizeDelta = Vector2(v, 21)

	if hp<1 then
		gameOver=true
		Debug.Log("Game Over")
		panel.gameObject:SetActive(true)--亮出开始按钮
	end

end


function main.OnDestroy()
	API.KillTimer(time)
end


--退出系统
main.touchTime=0
function main.onKeyBackDown()
	if Input.GetKeyUp(KeyCode.Escape) then		
		if Time.realtimeSinceStartup-main.touchTime >=2000 then	
			--Debug.Log(Time.realtimeSinceStartup-touchTime)
			main.touchTime=Time.realtimeSinceStartup
		else
			Application.Quit()
		end		
	end
end

function main.onLoadComplete(uri,bundle)

	if uri=="GUI" then
		local prefab = bundle:Load("GUI")
		local guiGo=GameObject.Instantiate(prefab)
		guiGo.name="GUI"

		uiCanvas = guiGo.transform:FindChild("Canvas") --GameObject:FindGameObjectWithTag("GuiCamera")  
		uiLevel=  uiCanvas:FindChild("Level")
		uibg1=uiLevel:FindChild("bg1")
		uibg2=uiLevel:FindChild("bg2")
		uibg3=uiLevel:FindChild("bg3")

		uibgArr[1]=uibg1
		uibgArr[2]=uibg2
		uibgArr[3]=uibg3	

		--进度条
		local uibar=uiLevel:FindChild("bar")
		bar=uibar:GetComponent("RectTransform")
		bar.sizeDelta = Vector2(200, 21)	

		--得分
		local uiscore=uiLevel:FindChild("score")
		score=uiscore:GetComponent("Text")
		score.text="score:0"

		--最高得分
		local uimaxscore=uiLevel:FindChild("maxscore")
		maxscore=uimaxscore:GetComponent("Text")
		maxpoint=PlayerPrefs.GetInt("maxpoint",0)
		maxscore.text="max score:"..maxpoint

		panel=uiLevel:FindChild("Panel")

		--开始按钮
		local uistartBnt=uiLevel:FindChild("Panel/startBnt")
		startBnt=uistartBnt:GetComponent("Button")
		EventListener.Get(uistartBnt.gameObject).onClick=main.onStartBntClick

		--版本信息
		local uiver=uiLevel:FindChild("Panel/version")
		local versionText=uiver:GetComponent("Text")
		versionText.text="version:"..tostring(version)		
		
		--加载鼹鼠sprite Altas
		local name = "Atlas/MoleAtlas"
		this:LoadBundle(name,main.onLoadComplete)  

	elseif uri=="Atlas/MoleAtlas" then
		MoleAtlas = bundle

		--加载鼹鼠prefab
		local name = "molePre"
		this:LoadBundle(name,main.onLoadComplete)            

 	elseif uri=="molePre" then
 		local name="molePre"
 		local prefab = bundle:Load(name) 

 		local idx=0
 		local x=0
 		local y=0
 		--实例化9只鼹鼠
 		for i=1,9 do

 			--摆好鼹鼠位置，默认全部缩在洞下面
 			if (i-1)%3==0 then
 				idx=idx+1 
 			end

 			x=30+102*((i-1)%3)-130 

 			if i<4 then 
 				y=-120 
 			else 
 				y=-70 
 			end 

 			local moleGo=GameObject.Instantiate(prefab)
    		moleGo.name = name..tostring(i)
        	moleGo.layer = LayerMask.NameToLayer("UI")
        	--moleGo.transform.parent = uibgArr[idx] 这个方法在ugui已经过期，使用SetParent()替换
        	moleGo.transform:SetParent(uibgArr[idx])
        	moleGo.transform.localScale = Vector3.one
        	moleGo.transform.localPosition = Vector3(x,y,0)
        	
        	local lb=moleGo:AddComponent("LuaBehaviour")        
        	lb:SetEnv("MoleAtlas",MoleAtlas,true)
        	lb:DoFile("mole")

        	--获取绑定的lua脚本
        	local mole_script=lb:GetChunk()
        	table.insert(moleArr,mole_script)        	

 		end	
      
      	--定时器      	
      	timer=API.AddTimer(1000,main.onTimer)
      	
      	this.usingUpdate=true  --运行执行 main.Update

	end
	
end

--开始按钮
function main.onStartBntClick(go)
	
	panel.gameObject:SetActive(false)--隐藏开始按钮

	gameOver=false
	for i,v in ipairs(moleArr) do
		moleArr[i].reset()
	end

	hp=10
	point=0

	score.text="score:"..tostring(point)
	bar.sizeDelta = Vector2(200, 21)

end


function main.Update()
	main.onKeyBackDown()
end


function main.onTimer(source,e)	
	--切换为主线程执行
	if gameOver==false then
		this:AddMission(main.moleComeOut,nil)     
	end
end

--鼹鼠出洞
function main.moleComeOut()
	
	local idx=math.floor(8 * math.random())+1
	local mole=moleArr[idx]
		
	if mole then
		if mole.status==1 then
			main.moleComeOut()
		else
			mole.comeOut()
		end
	end

end

--检查更新
function  main.checkVersion()	
	local data="version="..version.."&appid="..appid..string.format("&Cache=%d",os.time())
	Debug.Log("checkVersion:"..weburl..data)
	local webclient=API.SendRequest(weburl,data,main.OnProgessChanged,main.OnSendRequestCompleted)	
end
--异步HTTP进度
function main.OnProgessChanged(sender,e)
	Debug.Log("OnProgessChanged  http")
end
--异步HTTP完成
function main.OnSendRequestCompleted(sender,e)

	Debug.Log("OnSendRequestCompleted http")

 	if e.Error~=null then

		--Debug.Log("SendRequestCompleted err :"..e.Message)

		--开始游戏逻辑
		main.StartGame()

	elseif e.Cancelled then

		--Debug.Log("Cancelled")

		--开始游戏逻辑
		main.StartGame()

	else		
		local str=e.Result
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
		  		local webclinet=API.DownLoad(obj.downurl,tmpfile,main.OnDownloadProgressChanged,main.OnDownloadCompleted)
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
	--切换为主线程执行
	this:AddMission(main.Run,nil)
end

function main.Run()
		--初始化随机种子
	math.randomseed(Time.realtimeSinceStartup)

	--添加侦听（击中鼹鼠）
	this:AddListener2("hit on one mole",main.onHitMole)

	--添加侦听 跑了一只鼹鼠
	this:AddListener2("missing one mole",main.onMissMole)

	--加载UI
	local name = "GUI"
	this:LoadBundle(name,main.onLoadComplete)
end

--下载进度回调
function main.OnDownloadProgressChanged(sender,e)
	--切换为主线程执行
	this:AddMission(main.OnDownLoadProgress,e)
end
function main.OnDownLoadProgress(e)
	local str=string.format("downloaded %s of %s bytes. %s complete...",    
        e.BytesReceived, 
        e.TotalBytesToReceive,
        e.ProgressPercentage) 
	Debug.Log(str)
end
--下载完成
function main.OnDownloadCompleted(sender,e)
	--切换为主线程执行
	this:AddMission(main.OnDownData,e)
end
function main.OnDownData(e)
	if e.Error then
		Debug.Log("lua download err  :"..e.Message)
		main.StartGame()
	elseif e.Cancelled then
		Debug.Log("lua:download Cancelled")
		main.StartGame()
	else
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
	end
end


return main
