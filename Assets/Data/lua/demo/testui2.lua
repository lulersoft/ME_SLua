--性能测试
--https://github.com/lulersoft/ME
local testui2={}

local this
local gameObject
local transform
local material

local colors={Color.red, Color.yellow, Color.blue, Color.white, Color.black, Color.cyan}

function testui2.Start()

	this=testui2.this
	gameObject=testui2.gameObject
	transform=testui2.transform

	this.usingUpdate=true	

	material=gameObject:GetComponent(Renderer).material
	material.color=colors[math.floor(6 * math.random())+1]
	--this.renderer.material.color = colors[idx] -淘汰的方法
	
	--35秒后销毁
	API.RunCoroutine(WaitForSeconds(35),testui2.DestoryMe,nil)
end

function testui2.Update()
	transform:Rotate(0, 5, 0)
	material.color=colors[math.floor(6 * math.random())+1]
end

function  testui2.DestoryMe()
	GameObject.Destroy(gameObject)
end

return testui2