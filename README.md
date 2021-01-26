# Tubumu.Abp.Meeting

基于 [Mediasoup](https://github.com/versatica/mediasoup) 的 [Abp](https://www.abp.io/) vNext 视频会议模块。

## 一、安装模块

创建 Abp 项目后，通过常规方式安装 `Tubumu.Abp.Meeting` 模块。

``` C#
// ...
    typeof(AbpSwashbuckleModule),
    // 配置点：1
    typeof(TubumuAbpMeetingModule)
    )]
public class SampleWebModule : AbpModule
// ...
```

## 二、配置

1. 将 Sample 的 Web 项目中的 [mediasoupsettings.json](https://github.com/albyho/Tubumu.Abp.Meeting/blob/main/samples/Tubumu.Abp.Meeting.Sample/src/Tubumu.Abp.Meeting.Sample.Web/mediasoupsettings.json) 配置文件复制到新建的 Abp 解决方案的 Web 项目中。
2. 打开 `mediasoupsettings.json` 配置文件，搜索 `AnnouncedIp` 键将值修改为本机在局域网中的 IP。

``` json
// ...
    "WebRtcTransportSettings": {
      "ListenIps": [
        {
          "Ip": "0.0.0.0",
          "AnnouncedIp": "192.168.1.5" // 修改为本机在在局域网中的 IP 。
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
        "AnnouncedIp": "192.168.1.5" // 修改为本机在在局域网中的 IP 。
      },
      "MaxSctpMessageSize": 262144
    }
// ...
```

## 三、Web 前端

Sample 的前端项目的源码是 [tubumu-abp-meeting-sample-client](https://github.com/albyho/Tubumu.Abp.Meeting/tree/main/samples/Tubumu.Abp.Meeting.Sample/src/tubumu-abp-meeting-sample-client)。该项目使用 `Vue` 开发。

## 四、截图

[Screenshots](https://github.com/albyho/Tubumu.Abp.Meeting/blob/main/Screenshots.md)
