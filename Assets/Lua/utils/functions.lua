--老鼠的脚本
--小陆 QQ 2604904
--https://github.com/lulersoft/ME

local functions={}

--克隆source table
function functions.copyTab(st)
		if st == nil then
			return nil
		end
		
    local tab = {}
    for k, v in pairs(st) do
        if type(v) ~= "table" then
            tab[k] = v
        else
            tab[k] = functions.copyTab(v)
        end
    end
    return tab
end

return functions
