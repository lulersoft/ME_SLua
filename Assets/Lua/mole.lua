--老鼠的脚本
--小陆 QQ 2604904
--https://github.com/lulersoft/ME

local mole={}
local this
local gameObject
local transform

local animator
local image

local sprite4

local speed=0
local original
local to

mole.status = 0 	--1 出洞,0 回洞
mole.live = 1 		--1 活着,0 死了

function mole.Start()
	this=mole.this
	transform=mole.transform
	gameObject=mole.gameObject

	original=transform.localPosition	
	to=Vector3(original.x,original.y+60,original.z)
  
	image=gameObject:GetComponent("Image")
    animator=gameObject:GetComponent("Animator")

	local _text2d=MoleAtlas:LoadAsset("Assets/Atlas/MoleAtlas/Mole04.png")
	sprite4=Sprite.Create(_text2d,Rect(0,0,56,79),Vector2(0.5,0.5))

	EventListener.Get(gameObject).onClick=mole.onClick

end

function mole.reset()
	mole.live=1
	mole.status=0
	transform.localPosition=original
end

function  mole.onClick(go)
	Debug.Log("onClick")

	--如果已经挂了,返回
	if mole.live==0 then 
		return
	end
	
	mole.live=0

	LeanTween.cancel(gameObject)	

	--停止1秒后，缩回洞里
	--this:RunCoroutine(WaitForSeconds(1),mole.comIn,nil)
	this:LuaInvoke(1,mole.comIn,nil)
		
	--停止播放动画
	animator.enabled=false

	--改变sprite
	image.sprite=sprite4
	image:SetNativeSize()

	--发消息通知全世界,俺打中一头鼹鼠了。
	this:Broadcast("hit on one mole",gameObject)	
	
end


function mole.comeOut()
	--眨眼睛动画
	if(animator.enabled==false) then      
        animator.enabled=true
   	end

   	mole.status=1
   	

	--鼹鼠出洞
	--参数方法:setOrientToPath(true):setEase(LeanTweenType.easeInQuart):setDelay(1):setOnComplete(oncomplete):setOnCompleteParam(paths) --setOrientToPath(true):
	LeanTween.moveLocal( gameObject, to, 0.3):setEase(LeanTweenType.easeInQuad):setOnComplete(mole.onOutcomplete)
end
function mole.onOutcomplete()
	-- 出来半秒后，缩回去	
    mole.comIn()
end

function mole.comIn()
	--鼹鼠缩回洞里
	LeanTween.moveLocal( gameObject, original, 0.3):setEase( LeanTweenType.easeInQuad ):setDelay(0.5):setOnComplete(mole.onIncomplete)
end

function mole.onIncomplete()
	mole.status=0

	--如果活着缩回洞里
	if mole.live==1 then
		--发消息通知全世界,玩家失去一点血。
	    this:Broadcast("missing one mole",gameObject)
		
	end

	mole.live=1
	
end


return mole
