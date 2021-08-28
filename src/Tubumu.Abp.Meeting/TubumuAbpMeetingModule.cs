using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tubumu.Utils.Extensions;
using Tubumu.Utils.Extensions.Ip;
using Tubumu.Mediasoup;
using Tubumu.Meeting.Server;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Modularity;

namespace Tubumu.Abp.Meeting
{
    [DependsOn(
        typeof(AbpAspNetCoreSignalRModule)
    )]
    public class TubumuAbpMeetingModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // Tubumu.Meeting.Server 不是 AbpModule，故手工注册
            context.Services.AddTransient<MeetingHub>();

            var configuration = BuildConfiguration();

            // Mediasoup
            var mediasoupStartupSettings = configuration.GetSection("MediasoupStartupSettings").Get<MediasoupStartupSettings>();
            var mediasoupSettings = configuration.GetSection("MediasoupSettings").Get<MediasoupSettings>();
            var workerSettings = mediasoupSettings.WorkerSettings;
            var routerSettings = mediasoupSettings.RouterSettings;
            var webRtcTransportSettings = mediasoupSettings.WebRtcTransportSettings;
            var plainTransportSettings = mediasoupSettings.PlainTransportSettings;
            context.Services.AddMediasoup(options =>
            {
                // MediasoupStartupSettings
                if (mediasoupStartupSettings != null)
                {
                    options.MediasoupStartupSettings.MediasoupVersion = mediasoupStartupSettings.MediasoupVersion;
                    options.MediasoupStartupSettings.WorkerPath = mediasoupStartupSettings.WorkerPath;
                    options.MediasoupStartupSettings.NumberOfWorkers = !mediasoupStartupSettings.NumberOfWorkers.HasValue || mediasoupStartupSettings.NumberOfWorkers <= 0 ? Environment.ProcessorCount : mediasoupStartupSettings.NumberOfWorkers;
                }

                // WorkerSettings
                if (workerSettings != null)
                {
                    options.MediasoupSettings.WorkerSettings.LogLevel = workerSettings.LogLevel;
                    options.MediasoupSettings.WorkerSettings.LogTags = workerSettings.LogTags;
                    options.MediasoupSettings.WorkerSettings.RtcMinPort = workerSettings.RtcMinPort;
                    options.MediasoupSettings.WorkerSettings.RtcMaxPort = workerSettings.RtcMaxPort;
                    options.MediasoupSettings.WorkerSettings.DtlsCertificateFile = workerSettings.DtlsCertificateFile;
                    options.MediasoupSettings.WorkerSettings.DtlsPrivateKeyFile = workerSettings.DtlsPrivateKeyFile;
                }

                // RouteSettings
                if (routerSettings != null && !routerSettings.RtpCodecCapabilities.IsNullOrEmpty())
                {
                    options.MediasoupSettings.RouterSettings = routerSettings;

                    // Fix RtpCodecCapabilities[x].Parameters 。从配置文件反序列化时将数字转换成了字符串，这里进行修正。
                    foreach (var codec in routerSettings.RtpCodecCapabilities.Where(m => m.Parameters != null))
                    {
                        foreach (var key in codec.Parameters.Keys.ToArray())
                        {
                            var value = codec.Parameters[key];
                            if (value != null && Int32.TryParse(value.ToString(), out var intValue))
                            {
                                codec.Parameters[key] = intValue;
                            }
                        }
                    }
                }

                // WebRtcTransportSettings
                if (webRtcTransportSettings != null)
                {
                    options.MediasoupSettings.WebRtcTransportSettings.ListenIps = webRtcTransportSettings.ListenIps;
                    options.MediasoupSettings.WebRtcTransportSettings.InitialAvailableOutgoingBitrate = webRtcTransportSettings.InitialAvailableOutgoingBitrate;
                    options.MediasoupSettings.WebRtcTransportSettings.MinimumAvailableOutgoingBitrate = webRtcTransportSettings.MinimumAvailableOutgoingBitrate;
                    options.MediasoupSettings.WebRtcTransportSettings.MaxSctpMessageSize = webRtcTransportSettings.MaxSctpMessageSize;
                }

                // 如果没有设置 AnnouncedIp 则取本机的 IPv4 地址
                var localIPv4IPAddress = IPAddressExtensions.GetLocalIPv4IPAddress()?.ToString();
                foreach (var listenIp in options.MediasoupSettings.WebRtcTransportSettings.ListenIps)
                {
                    if (string.IsNullOrWhiteSpace(listenIp.AnnouncedIp))
                    {
                        listenIp.AnnouncedIp = localIPv4IPAddress;
                    }
                }

                // PlainTransportSettings
                if (plainTransportSettings != null)
                {
                    options.MediasoupSettings.PlainTransportSettings.ListenIp = plainTransportSettings.ListenIp;
                    options.MediasoupSettings.PlainTransportSettings.MaxSctpMessageSize = plainTransportSettings.MaxSctpMessageSize;
                }

                // 如果没有设置 AnnouncedIp 则取本机的 IPv4 地址
                if (string.IsNullOrWhiteSpace(options.MediasoupSettings.PlainTransportSettings.ListenIp.AnnouncedIp))
                {
                    options.MediasoupSettings.PlainTransportSettings.ListenIp.AnnouncedIp = localIPv4IPAddress;
                }
            });

            // Meeting server
            context.Services.AddMeetingServer();

            // SignalR
            context.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            })
            .AddJsonProtocol(options => {
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();

            // Mediasoup
            app.UseMediasoup();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("mediasoupsettings.json", optional: false);

            return builder.Build();
        }
    }
}


