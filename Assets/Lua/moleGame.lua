--打地鼠游戏
--小陆 QQ 2604904
--https://github.com/lulersoft/ME
local game={}

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


function game.Start()
	this=game.this
	game.Run()
end

function game.Run()
	--初始化随机种子
	math.randomseed(Time.realtimeSinceStartup)

	--添加侦听（击中鼹鼠）
	this:AddListener2("hit on one mole",game.onHitMole)

	--添加侦听 跑了一只鼹鼠
	this:AddListener2("missing one mole",game.onMissMole)

	--加载共享的图集
	local name = "moleatlas.ab"
	this:LoadBundle(name,game.onLoadComplete)
end

function game.onHitMole(go)
	--计分	
	point=point+1
	if maxpoint<point then
		maxpoint=point
		maxscore.text="max score:"..tostring(maxpoint)
		PlayerPrefs.SetInt("maxpoint",maxpoint)
	end
	score.text="score:"..tostring(point)
end

function game.onMissMole(go)
	if gameOver then return end
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


function game.OnDestroy()
	API.KillTimer(time)
end


--退出系统
local touchTime=0
function game.onKeyBackDown()
	
	if Input.GetKeyUp(KeyCode.Escape) then
		
		if Time.realtimeSinceStartup-touchTime >=2 then	
			API.Log("再按一次退出")
			touchTime=Time.realtimeSinceStartup			
		else
			Application.Quit()
		end	
			
	end

end

function game.onLoadComplete(uri,bundle)

	API.Log(uri)

	if uri=="gui.ab" then

		local prefab = bundle:LoadAsset("Assets/Builds/GUI/Prefab/GUI.prefab")
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
		EventListener.Get(uistartBnt.gameObject).onClick=game.onStartBntClick

		--版本信息
		local uiver=uiLevel:FindChild("Panel/version")
		local versionText=uiver:GetComponent("Text")
		versionText.text="version:"..tostring(version)		
		
		--加载鼹鼠prefab
		local name = "molepre.ab"
		this:LoadBundle(name,game.onLoadComplete) 		

	elseif uri=="moleatlas.ab" then
		MoleAtlas = bundle

		--加载鼹鼠sprite Altas
		local name = "gui.ab"
		this:LoadBundle(name,game.onLoadComplete)  

 	elseif uri=="molepre.ab" then
 		local name="Assets/Builds/GUI/Prefab/molePre.prefab"
 		local prefab = bundle:LoadAsset(name)--bundle.mainAsset--bundle:Load(name) 

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
        
        	--local lb=moleGo:AddComponent("LuaBehaviour") --这个方法在unity 5.x已经过期
        	local lb = API.AddComponent(moleGo,"LuaBehaviour")
        	lb:SetEnv("MoleAtlas",MoleAtlas,true)
        	lb:DoFile("mole")

        	--获取绑定的lua脚本
        	local mole_script=lb:GetChunk()
        	table.insert(moleArr,mole_script)        	

 		end	
      
      	--定时器      	
      	timer=API.AddTimer(1000,game.onTimer)
      	
      	this.usingUpdate=true  --运行执行 game.Update

	end
	
end

--开始按钮
function game.onStartBntClick(go)
	
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


function game.Update()
	game.onKeyBackDown()
end


function game.onTimer(source)	
	--切换为主线程执行
	if gameOver==false then
		game.moleComeOut()    
	end
end

--鼹鼠出洞
function game.moleComeOut()
	
	local idx=math.floor(8 * math.random())+1
	local mole=moleArr[idx]
		
	if mole and gameOver==false then
		if mole.status==1 then
			game.moleComeOut()
		else
			mole.comeOut()
		end
	end

end

return game
