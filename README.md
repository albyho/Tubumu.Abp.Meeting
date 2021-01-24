# Tubumu.Abp.Meeting

基于 Mediasoup 的 Abp vNext 的视频会议模块。

## 一、安装模块

创建 Abp 项目后，通过常规方式安装 `Tubumu.Abp.Meeting` 模块。

## 二、配置

1. 将 `mediasoupsettings.json` 配置文件复制到 Web 项目。
2. 将 `mediasoupsettings.json` 配置文件中的 `AnnouncedIp` 修改为本机在局域网的 IP。
3. 修改 `Program.cs` 的 `CreateHostBuilder` 方法如下，用于加载配置。

``` C#
internal static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            // 配置点：1
            var configs = new ConfigurationBuilder()
            .AddJsonFile("mediasoupsettings.json", optional: false)
            .Build();

            webBuilder.UseConfiguration(configs);
            webBuilder.UseStartup<Startup>();
        })
        .UseAutofac()
        .UseSerilog();
```

4. 修改 `XXXWebModule.cs` 的 `ConfigureServices` 方法如下，主要目的是配置 `NewtonsoftJson`。

``` C#
// ...
    typeof(AbpSwashbuckleModule),
    // 配置点：2
    typeof(TubumuAbpMeetingModule),
    typeof(AbpAspNetCoreSignalRModule)
    )]
public class SampleWebModule : AbpModule
// ...

```

``` C#
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
    {
        options.AddAssemblyResource(
            typeof(SampleResource),
            typeof(SampleDomainModule).Assembly,
            typeof(SampleDomainSharedModule).Assembly,
            typeof(SampleApplicationModule).Assembly,
            typeof(SampleApplicationContractsModule).Assembly,
            typeof(SampleWebModule).Assembly
        );
    });

    // 配置点：3
    context.Services.PreConfigure<AbpJsonOptions>(options =>
    {
        options.UseHybridSerializer = false;
    });
}
```

``` C#
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var hostingEnvironment = context.Services.GetHostingEnvironment();
    var configuration = context.Services.GetConfiguration();

    ConfigureUrls(configuration);
    ConfigureBundles();
    ConfigureAuthentication(context, configuration);
    ConfigureAutoMapper();
    ConfigureVirtualFileSystem(hostingEnvironment);
    ConfigureLocalizationServices();
    ConfigureNavigationServices();
    ConfigureAutoApiControllers();
    ConfigureSwaggerServices(context.Services);
    // 配置点：4
    ConfigureMeeting(context);

}

private void ConfigureMeeting(ServiceConfigurationContext context)
{
    // TODO: (alby)使用 System.Text.Json
    context.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
    })
    .AddNewtonsoftJsonProtocol(options =>
    {
        var settings = options.PayloadSerializerSettings;
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        settings.Converters = new JsonConverter[] { new EnumStringValueConverter() };
    });
}
```

## 三、截图

[Screenshots](https://github.com/albyho/Tubumu.Abp.Meeting/blob/main/Screenshots.md)
