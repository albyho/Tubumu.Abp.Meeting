# Tubumu.Abp.Meeting

基于 Mediasoup 的 Abp vNext 的视频会议模块。

## 一、安装模块

创建 Abp 项目后，通过常规方式安装 `Tubumu.Abp.Meeting` 模块。

``` C#
// ...
    typeof(AbpSwashbuckleModule),
    // 配置点：1
    typeof(TubumuAbpMeetingModule),
    typeof(AbpAspNetCoreSignalRModule)
    )]
public class SampleWebModule : AbpModule
// ...
```

## 二、配置

1. 将 `mediasoupsettings.json` 配置文件复制到 Web 项目。
2. 将 `mediasoupsettings.json` 配置文件中的 `AnnouncedIp` 修改为本机在局域网的 IP。
3. 修改 `Program.cs` 的 `CreateHostBuilder` 方法如下，用于加载配置。

``` C#
internal static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            // 配置点：2
            var configs = new ConfigurationBuilder()
            .AddJsonFile("mediasoupsettings.json", optional: false)
            .Build();

            webBuilder.UseConfiguration(configs);
            webBuilder.UseStartup<Startup>();
        })
        .UseAutofac()
        .UseSerilog();
```

## 三、Web 前端

Sample 的前端项目的源码位于 `samples/Tubumu.Abp.Meeting.Sample/src/tubumu-abp-meeting-sample-client` 。

## 四、截图

[Screenshots](https://github.com/albyho/Tubumu.Abp.Meeting/blob/main/Screenshots.md)
