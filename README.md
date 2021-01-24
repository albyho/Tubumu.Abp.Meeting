# Tubumu.Abp.Meeting

基于 Mediasoup 和 Abp 的视频会议系统。

## 一、安装模块

创建 Abp 项目后，通过常规方式安装 `Tubumu.Abp.Meeting` 模块。

## 二、配置

1. 将 `mediasoupsettings.json` 配置文件复制到 Web 项目。
2. 在 `mediasoupsettings.json` 配置文件中的 `AnnouncedIp` 修改为本地局域的 IP。
3. 修改 `Program.cs` 的 `CreateHostBuilder` 方法如下。

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

4. 修改 `XXXWebModule.cs` 的 `ConfigureServices` 方法如下。

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
    // 配置点：2
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

5. 如果要运行 Sample，请将 `Tubumu.Abp.Meeting.csproj` 修改如下。

``` xml
    <ItemGroup>
        <PackageReference Include="Tubumu.Mediasoup" Version="1.0.3" />
        <PackageReference Include="Tubumu.Mediasoup.Executable" Version="1.0.3" />
        <PackageReference Include="Tubumu.Mediasoup.Common" Version="1.0.3" />
        <PackageReference Include="Tubumu.Meeting.Server" Version="1.0.3" />
        <PackageReference Include="Tubumu.Core" Version="1.0.3" />
        <PackageReference Include="Volo.Abp.MultiTenancy" Version="4.1.2" />
        <PackageReference Include="Volo.Abp.AspNetCore" Version="4.1.2" />
    </ItemGroup>

    <!--
    <ItemGroup>
      <ProjectReference Include="..\..\..\..\Tubumu.Mediasoup\Tubumu.Mediasoup.csproj" />
      <ProjectReference Include="..\..\..\..\Tubumu.Mediasoup.Executable\Tubumu.Mediasoup.Executable.csproj" />
      <ProjectReference Include="..\..\..\..\Tubumu.Mediasoup.Common\Tubumu.Mediasoup.Common.csproj" />
      <ProjectReference Include="..\..\..\..\Tubumu.Meeting.Server\Tubumu.Meeting.Server.csproj" />
      <ProjectReference Include="..\..\..\..\Tubumu.Core\Tubumu.Core.csproj" />
    </ItemGroup>
    -->

```

