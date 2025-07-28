using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 시스템 통합 설정 데이터 시더
    /// </summary>
    public class SystemIntegrationSeeder : IDataSeeder
    {
        private readonly MesDbContext _context;
        private readonly ILogger<SystemIntegrationSeeder> _logger;

        public int Order => 3;

        public SystemIntegrationSeeder(MesDbContext context, ILogger<SystemIntegrationSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.SystemIntegrations.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("시스템 통합 설정이 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("시스템 통합 설정 시딩 시작...");

            var systems = new[]
            {
                // AMS (AMR Management System)
                new SystemIntegration
                {
                    SystemCode = "AMS",
                    SystemName = "AMR Management System",
                    IntegrationType = IntegrationType.REST,
                    Endpoint = "http://ams-api",
                    Status = IntegrationStatus.Active,
                    IsHealthy = true,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        serviceDiscovery = true,
                        healthCheckEndpoint = "/health",
                        healthCheckInterval = 30, // 30초
                        timeout = 30, // 30초
                        retryPolicy = new
                        {
                            maxRetries = 3,
                            baseDelay = 1000, // 1초
                            maxDelay = 10000 // 10초
                        },
                        endpoints = new
                        {
                            orders = "/api/v1/orders",
                            vehicles = "/api/v1/vehicles",
                            workflows = "/api/v1/workflows",
                            routing = "/api/v1/routing"
                        },
                        amrTypes = new[]
                        {
                            new { type = "Thira", codes = new[] { "THIRA_01", "THIRA_02" } },
                            new { type = "Agilox", codes = new[] { "AGILOX_01" } }
                        }
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // EMS (Equipment Management System)
                new SystemIntegration
                {
                    SystemCode = "EMS",
                    SystemName = "Equipment Management System",
                    IntegrationType = IntegrationType.REST,
                    Endpoint = "http://ems-api",
                    Status = IntegrationStatus.Active,
                    IsHealthy = true,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        serviceDiscovery = true,
                        healthCheckEndpoint = "/health",
                        healthCheckInterval = 30,
                        timeout = 60, // 60초 (장비 작업이 오래 걸릴 수 있음)
                        retryPolicy = new
                        {
                            maxRetries = 3,
                            baseDelay = 2000,
                            maxDelay = 15000
                        },
                        endpoints = new
                        {
                            equipment = "/api/v1/equipment",
                            monitoring = "/api/v1/monitoring",
                            maintenance = "/api/v1/maintenance",
                            alarms = "/api/v1/alarms",
                            sensorData = "/api/v1/sensor-data"
                        },
                        stations = new[]
                        {
                            new {
                                id = "WELD_01",
                                name = "Welding Station 1",
                                type = "Welding",
                                capabilities = new[] { "MIG", "TIG" }
                            },
                            new {
                                id = "WELD_02",
                                name = "Welding Station 2",
                                type = "Welding",
                                capabilities = new[] { "MIG" }
                            },
                            new {
                                id = "BOLT_01",
                                name = "Bolting Station 1",
                                type = "Bolting",
                                capabilities = new[] { "M8", "M10", "M12" }
                            }
                        }
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // Redis Streams (EMS 장비 통신용)
                new SystemIntegration
                {
                    SystemCode = "EMS_REDIS",
                    SystemName = "EMS Redis Streams",
                    IntegrationType = IntegrationType.RedisStream,
                    Endpoint = "redis-cache:6379",
                    Status = IntegrationStatus.Active,
                    IsHealthy = true,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        connectionString = "redis-cache:6379",
                        streams = new
                        {
                            equipmentCommands = "ems:equipment:commands",
                            equipmentResponses = "ems:equipment:responses",
                            sensorData = "ems:sensor:data",
                            alarms = "ems:alarms"
                        },
                        consumerGroup = "mes-consumer-group",
                        maxRetries = 3,
                        ackTimeout = 30000, // 30초
                        options = new
                        {
                            abortConnect = false,
                            connectTimeout = 5000,
                            syncTimeout = 5000
                        }
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // Message Queue (이벤트 기반 통신)
                new SystemIntegration
                {
                    SystemCode = "MESSAGE_QUEUE",
                    SystemName = "RabbitMQ Message Broker",
                    IntegrationType = IntegrationType.MessageQueue,
                    Endpoint = "rabbitmq://rabbitmq",
                    Status = IntegrationStatus.Active,
                    IsHealthy = true,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        host = "rabbitmq",
                        port = 5672,
                        username = "guest",
                        password = "guest",
                        virtualHost = "/",
                        exchanges = new
                        {
                            mesEvents = "mes.events",
                            amsEvents = "ams.events",
                            emsEvents = "ems.events"
                        },
                        queues = new
                        {
                            mesCommands = "mes.commands",
                            workOrderEvents = "mes.workorder.events",
                            executionEvents = "mes.execution.events",
                            integrationEvents = "mes.integration.events"
                        },
                        prefetchCount = 10,
                        durable = true,
                        autoDelete = false
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // SignalR Hub (실시간 모니터링)
                new SystemIntegration
                {
                    SystemCode = "SIGNALR",
                    SystemName = "SignalR Real-time Communication",
                    IntegrationType = IntegrationType.SignalR,
                    Endpoint = "http://web-server/hubs",
                    Status = IntegrationStatus.Active,
                    IsHealthy = true,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        hubs = new
                        {
                            workOrder = "/hubs/workorder",
                            execution = "/hubs/execution",
                            monitoring = "/hubs/monitoring",
                            notification = "/hubs/notification"
                        },
                        reconnectPolicy = new
                        {
                            maxRetries = 5,
                            retryDelays = new[] { 0, 2000, 10000, 30000, 60000 }
                        },
                        keepAliveInterval = 15000, // 15초
                        serverTimeout = 30000 // 30초
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // TDVMS (향후 구현 예정)
                new SystemIntegration
                {
                    SystemCode = "TDVMS",
                    SystemName = "3D Vision Measurement System",
                    IntegrationType = IntegrationType.REST,
                    Endpoint = "http://tdvms-api",
                    Status = IntegrationStatus.Inactive,
                    IsHealthy = false,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        serviceDiscovery = true,
                        healthCheckEndpoint = "/health",
                        note = "향후 구현 예정"
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },

                // ARMS (향후 구현 예정)
                new SystemIntegration
                {
                    SystemCode = "ARMS",
                    SystemName = "Automated Robot Management System",
                    IntegrationType = IntegrationType.REST,
                    Endpoint = "http://arms-api",
                    Status = IntegrationStatus.Inactive,
                    IsHealthy = false,
                    Configuration = JsonSerializer.SerializeToDocument(new
                    {
                        serviceDiscovery = true,
                        healthCheckEndpoint = "/health",
                        note = "향후 구현 예정"
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.SystemIntegrations.AddRangeAsync(systems);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"시스템 통합 설정 {systems.Length}개 시딩 완료");
        }
    }
}