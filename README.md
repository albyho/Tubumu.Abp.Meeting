# Tubumu.Abp.Meeting

[![NuGet](https://img.shields.io/nuget/v/Tubumu.Abp.Meeting.svg)](https://www.nuget.org/packages/Tubumu.Abp.Meeting)

基于 [Mediasoup](https://github.com/versatica/mediasoup) 的 [Abp](https://www.abp.io/) vNext 视频会议模块。

## 一、安装

### 1、创建项目

``` shell
# 当前目录：任意
mkdir Sample && cd Sample
abp new Sample
```

### 2、安装 Tubumu.Abp.Meeting 模块

使用 Abp CLI 安装:

``` shell
# 当前目录：Sample
cd src/Sample.Web
# 当前目录：Sample/src/Sample.Web
abp add-package Tubumu.Abp.Meeting
```

或者手工安装，在 Nuget 搜索 [Tubumu.Abp.Meeting](https://www.nuget.org/packages/Tubumu.Abp.Meeting/) 并安装，然后修改 `SampleWebAbpModule`:

``` C#
// File: Sample/src/Sample.Web/SampleWebModule.cs
// ...
    typeof(AbpSwashbuckleModule),
    // 配置点：1
    typeof(TubumuAbpMeetingModule)
    )]
public class SampleWebModule : AbpModule
// ...
```

### 3、下载配置文件及修改 IP

将 [mediasoupsettings.json](https://raw.githubusercontent.com/albyho/Tubumu.Abp.Meeting/main/samples/Tubumu.Abp.Meeting.Sample/src/Tubumu.Abp.Meeting.Sample.Web/mediasoupsettings.json) 配置文件下载到 `Sample.Web` 项目中。

``` shell
# 当前目录：Sample/src/Sample.Web
curl -o mediasoupsettings.json https://raw.githubusercontent.com/albyho/Tubumu.Abp.Meeting/main/samples/Tubumu.Abp.Meeting.Sample/src/Tubumu.Abp.Meeting.Sample.Web/mediasoupsettings.json
```

打开 `mediasoupsettings.json` 配置文件，搜索 `AnnouncedIp` 键将值修改为本机在局域网中的 IP 或者公网 IP。

``` json
// File: Sample/src/Sample.Web/mediasoupsettings.json
// ...
    "WebRtcTransportSettings": {
      "ListenIps": [
        {
          "Ip": "0.0.0.0",
          "AnnouncedIp": "192.168.1.5" // 修改为本机在在局域网中的 IP 或者公网 IP 。
        }
      ],
      "InitialAvailableOutgoingBitrate": 1000000,
      "MinimumAvailableOutgoingBitrate": 600000,
      "MaxSctpMessageSize": 262144,
      // Additional options that are not part of WebRtcTransportOptions.
      "MaximumIncomingBitrate": 1500000
    },
    // 用于 FFmpeg 推流
    "PlainTransportSettings": {
      "ListenIp": {
        "Ip": "0.0.0.0",
        "AnnouncedIp": "192.168.1.5" // 修改为本机在在局域网中的 IP 或者公网 IP 。
      },
      "MaxSctpMessageSize": 262144
    }
// ...
```

### 4、Web 前端

可将 Sample 的前端项目的源码是 [tubumu-abp-meeting-sample-client](https://github.com/albyho/Tubumu.Abp.Meeting/tree/main/samples/Tubumu.Abp.Meeting.Sample/src/tubumu-abp-meeting-sample-client) 编译并复制到 Sample.Web 项目的 wwwroot 目录下。比如：`Sample/src/Sample.Web/wwwroot/meeting` 目录。

``` shell
# 当前目录：tubumu-abp-meeting-sample-client
yarn build
cp -R ./dist/* xxxx/Sample.Web/meeting
```

> 注意：如有必要，请修改 `index.html` 文件中的 `css` 和 `js` 的路径。

### 5、新增菜单

菜单链接至 Web 前端的首页。

``` C#
// File: Sample/src/Sample.Web/Menus/SampleMenus.cs
public class SampleMenus
{
    private const string Prefix = "Sample";
    public const string Home = Prefix + ".Home";

    //Add your menu items here...

    // `Meeting` menu item
    public const string Meeting = Prefix + ".Meeting";
}
```

``` C#
// File: Sample/src/Sample.Web/Menus/SampleMenuContributor.cs
private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
{
    if (!MultiTenancyConsts.IsEnabled)
    {
        var administration = context.Menu.GetAdministration();
        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
    }

    var l = context.GetLocalizer<SampleResource>();

    context.Menu.Items.Insert(0, new ApplicationMenuItem(SampleMenus.Home, l["Menu:Home"], "~/"));
    // `Meeting` menu item
    context.Menu.Items.Insert(1, new ApplicationMenuItem(SampleMenus.Meeting, "Meeting", "~/meeting/index.html"));
}
```

## 二、启动

1. 将 Sample.Web 设为启动项进行启动。

2. 打开一个或多个浏览器并**登录**。如果是局域网或公网还可以通过其他电脑或手机访问。

3. 访问会议页面。操作流程请参考录屏和截图。

## 三、录屏

![](https://github.com/albyho/Tubumu.Abp.Meeting/raw/main/art/ScreenCAP01.gif)

## 四、截图

[Screenshots](https://github.com/albyho/Tubumu.Abp.Meeting/blob/main/Screenshots.md)
