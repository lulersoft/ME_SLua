# ME_SLua
ME_SLua是一个基于unity+slua技术的全LUA免费开源（带有资源下载功能）游戏框架

基于https://github.com/pangweiwei/slua<br>

小陆(QQ2604904)<br>

// ----------------框架目标-----------<br>

<b>使用lua 编写2D,3D全平台运行游戏，一次编写，到处发布</b><br>

//------------------核心思路----------<br>
ME框架的思路就是 一个GameObject 对应挂个LuaBehaviour.cs脚本，这个LuaBehaviour.cs脚本DoFile一个lua脚本，lua脚本和LuaBehaviour.cs和GameObject都可以交互控制。<br>

各各GameObject间通过Lua 消息进行通信。<br>

框架附带打地鼠游戏截图:<br>

![](demo.jpg)

//-----------------安装说明------------<br>

Assets/Engine 为本框所有架核心代码<br>
Assets/Atlas 将要打包的ui图片<br>
Assets/Builds 将要打包的预置物<br>
Assets/Data 为资源输出目录和lua脚本目录，此文件夹将被压缩为zip放入Assets/StreamingAssets<br>
Assets/StreamingAssets 为zip更新包输出目录<br>

其它文件请和 https://github.com/pangweiwei/slua 保持同步更新.<br>

wwwroot目录为web服务器目录，请上传到您的web服务器 请修改version内json参数,status： 0 无更新，1有更新包;force 1强制更新,0不强制更新,downrul 更新包路径<br>

u3d工程目录内:<br>
Assets/Data为资源文件，所有已打包的文件存放这里(含lua)。 <br>
Assets/Data/lua为 lua 脚本代码<br>
Assets/StreamingAssets 存放的是生成好的更新包(data.zip),内容为data资源文件夹。<br>
生成后，请把一份上传到wwwroot服务器目录内，供客户端下载更新。<br>

运行前，请先执行菜单slua下面生成需要的库文件。
详细slua使用方法，请看官网。谢谢。

测试场景 Engine/Scene/Demo.unity<br>

首次启动游戏安装会从 StreamingAssets 中复制资源文件data.zip并解压到可读目录，然后加载主入口文件main.lua执行 所有逻辑尽可能在lua端执行 如：在lua中进行版本检测，下载zip，解压覆盖以后每次运行，会检测远程服务器版本，如果有更新，则下载，并自动解压覆盖资源目录。<br>


特性：<br>
带zip资源生成和下载发布<br>
全lua代码编写游戏。<br>
unity3d editor编辑器负责可视化资源的创建和打包。<br>
使用内置ugui来做UI甚至游戏。<br>
热更新更彻底：可通过热更新来更新成不同类型的游戏。<br>
通过纯lua“打地鼠”小游戏实战来提高框架的实用性.<br>


已知问题：<br>
WaitForSeconds 无法在lua中使用，等待slua作者解决。谢谢。


