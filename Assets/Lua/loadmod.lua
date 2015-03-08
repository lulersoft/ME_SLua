
--加载模型测试

local loadmod={}

local this

function loadmod.Start()
	this=loadmod.this	

	local name="gui.ab"
	this:LoadBundle(name,loadmod.onLoadComplete)
end


function loadmod.onLoadComplete(uri,bundle)

	Debug.Log(uri)

	if uri=="gui.ab" then

		local prefab =bundle:LoadAsset("GUI")		

		local guiGo=GameObject.Instantiate(prefab)
		guiGo.name="Cube"

		guiGo.transform.localScale = Vector3.one
        guiGo.transform.localPosition = Vector3(0,1,1)
	end

end

return loadmod