# CampusNetGuard
c#校园网认证客户端

环境：.netframework4.5

作者：mokeyish

QQ：21954595

开发工具v2013或vs2015

此源码可随意更改，但请保留作者信息，如果想修改此源码，不懂可以问我，如有更好的更改，也请告诉我，更新此处的源码

下载软件请到下面链接下载，以免恶意代码。

#功能介绍


当配置好认证登录账号密码，以及web认证的账号和密码。

ps：没有中兴客户端原版，那么多限制（如：不能开启多网卡，发射WIFI等），仅仅是自动化认证登录，以及自动化处理常规网络问题。可记录历史ip，以防dhcp服务瘫痪无法获取ip导致不能上网问题。

可选：开启服务   (未开启服务就是跟普通客户端差不多，不过依然会自动判断网络问题，自动联网，包括网线拔了重新插好依然能自动连好网)

当设置为服务运行时，开机的时候，（没有进入桌面之前）就会在后台自动完成端口认证登录，以为网页认证登录。电脑休眠，睡眠等情况，进入桌面时如果没有联网，也可以自动联网。如果存在多个windows系统，可共用一个文件。这一切自动化都不用再去点开客户端，如果点开客户端，客户端将会以client模式运行，连接至后台服务，可以发送控制命令到后台服务。


软件可以以两种模式运行，一种是运行在后台服务里消耗资源极低；一种是普通客户端模糊。
#软件设计
用户界面与执行核心分离。（用户界面可另行设计）

执行核心，根据配置自动启动运行模式（Client ，server ，none），

#改进的地方

1.将开启服务关闭服务集成在软件菜单中（目前需打开任务管理器进入计算机服务里找到，校园网服务开启，关闭）
2.当dhcp服务器瘫痪时，从历史Ip记录中，读取ip配置到网卡（目前需手动完成）


#编译版
下载软件请，以免恶意代码。
下载地址http://pan.baidu.com/s/1jGk45PO

