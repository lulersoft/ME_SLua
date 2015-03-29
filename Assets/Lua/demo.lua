--性能测试
--https://github.com/lulersoft/ME

local demo={}
local this
local gameObject
local transform
local r=10
function demo.Start()

	this=demo.this
	gameObject=demo.gameObject
	transform=demo.transform	

	--添加个照相机
	local carm=GameObject("carm")
	API.AddComponent(carm,"Camera")
	
	carm.transform.localPosition=Vector3(9,3,-17)

	--来一束光照亮测试场景
	local light = API.AddComponent(carm,"Light")
	light.type=LightType.Directional

	demo.test1()
end

function demo.test1()
	local i=0
	while (i < 1000) do
		i = i + 1
		this:RunCoroutine(WaitForSeconds(i * 0.1), demo.cor, i)
	end
end
function demo.cor(i)
   demo.createCube(i)
end
function demo.createCube(ii)
	local cube=GameObject.CreatePrimitive(PrimitiveType.Cube)
	cube.name=tostring(ii)
	cube.transform.position = Vector3(10 + r * math.cos((ii + 10) * Mathf.Deg2Rad), 10 + r * math.sin((ii + 10) * Mathf.Deg2Rad), 10 + r * math.sin((ii + 10) * Mathf.Deg2Rad))	

	local lb = API.AddComponent(cube,"LuaBehaviour")
	lb:DoFile("testui2")
end

--[[
local a=Vector3(0,0,0)
function demo.test2( )  
	--local t1=Time.realtimeSinceStartup
	local t1=os.time()
    Debug.Log('lua start at:'..t1)  
    for i=1,50000 do         
        transform.position=a
    end  
    --local t2=Time.realtimeSinceStartup
    local t2=os.time()
    Debug.Log('lua end at:'..t2)

    Debug.Log("耗时:"..tostring(t2-t1))
end
--]]

--[[
function demo.hello()
	print("hello ulua")
	warn("hello ulua warn ...")

	local bundle=this:loadBundle("Windmill2")
	warn(this.name)

	local prefab = bundle.mainAsset
	local go=GameObject.Instantiate(prefab)
	go.name=">>>>>>>>>>>>>>>>>>>>>>"


end
--]]


return demo