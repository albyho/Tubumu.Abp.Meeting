using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tubumu.Mediasoup;
using Tubumu.Meeting.Server;
using Tubumu.Utils.Extensions;
using Tubumu.Utils.Extensions.Ip;
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
            // Tubumu.Meeting.Server 不是 AbpModule，故手工注册。
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

                    // 如果没有设置 ListenIps 则获取本机所有的 IPv4 地址进行设置。
                    var listenIps = options.MediasoupSettings.WebRtcTransportSettings.ListenIps;
                    if (listenIps.IsNullOrEmpty())
                    {
                        var localIPv4IPAddresses = IPAddressExtensions.GetLocalIPAddresses(AddressFamily.InterNetwork).Where(m => m != IPAddress.Loopback);
                        if (localIPv4IPAddresses.IsNullOrEmpty())
                        {
                            throw new ArgumentException("无法获取本机 IPv4 配置 WebRtcTransport。");
                        }

                        listenIps = (from ip in localIPv4IPAddresses
                                     let ipString = ip.ToString()
                                     select new TransportListenIp
                                     {
                                         Ip = ipString,
                                         AnnouncedIp = ipString
                                     }).ToArray();
                        options.MediasoupSettings.WebRtcTransportSettings.ListenIps = listenIps;
                    }
                    else
                    {
                        var localIPv4IPAddress = IPAddressExtensions.GetLocalIPv4IPAddress();
                        if (localIPv4IPAddress == null)
                        {
                            throw new ArgumentException("无法获取本机 IPv4 配置 WebRtcTransport。");
                        }

                        foreach (var listenIp in listenIps)
                        {
                            if(string.IsNullOrWhiteSpace(listenIp.AnnouncedIp))
                            {
                                // 如果没有设置 AnnouncedIp：
                                // 如果 Ip 属性的值不是 Any 则赋值为 Ip 属性的值，否则取本机的任意一个 IPv4 地址进行设置。(注意：可能获取的并不是正确的 IP)
                                listenIp.AnnouncedIp = listenIp.Ip == IPAddress.Any.ToString() ? localIPv4IPAddress.ToString() : listenIp.Ip;
                            }
                        }
                    }
                }

                // PlainTransportSettings
                if (plainTransportSettings != null)
                {
                    options.MediasoupSettings.PlainTransportSettings.ListenIp = plainTransportSettings.ListenIp;
                    options.MediasoupSettings.PlainTransportSettings.MaxSctpMessageSize = plainTransportSettings.MaxSctpMessageSize;

                    var localIPv4IPAddress = IPAddressExtensions.GetLocalIPv4IPAddress();
                    if (localIPv4IPAddress == null)
                    {
                        throw new ArgumentException("无法获取本机 IPv4 配置 PlainTransport。");
                    }

                    var listenIp = options.MediasoupSettings.PlainTransportSettings.ListenIp;
                    if (listenIp == null)
                    {
                        listenIp = new TransportListenIp
                        {
                            Ip = localIPv4IPAddress.ToString(),
                            AnnouncedIp = localIPv4IPAddress.ToString(),
                        };
                        options.MediasoupSettings.PlainTransportSettings.ListenIp = listenIp;
                    }
                    else if (string.IsNullOrWhiteSpace(listenIp.AnnouncedIp))
                    {
                        // 如果没有设置 AnnouncedIp：
                        // 如果 Ip 属性的值不是 Any 则赋值为 Ip 属性的值，否则取本机的任意一个 IPv4 地址进行设置。(注意：可能获取的并不是正确的 IP)
                        listenIp.AnnouncedIp = listenIp.Ip == IPAddress.Any.ToString() ? localIPv4IPAddress.ToString() : listenIp.Ip;
                    }
                }
            });

            // Meeting server
            context.Services.AddMeetingServer(options =>
            {
                options.ServeMode = ServeMode.Open;
            });

            // SignalR
            context.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            })
            .AddJsonProtocol(options =>
            {
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


