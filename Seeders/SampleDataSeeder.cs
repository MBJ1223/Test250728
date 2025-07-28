// (개발/테스트용)
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BODA.FMS.MES.Data.Entities;
using LogLevel = BODA.FMS.MES.Data.Entities.LogLevel;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 샘플 데이터 시더 - 개발 및 테스트용
    /// </summary>
    public class SampleDataSeeder : IDataSeeder
    {
        private readonly MesDbContext _context;
        private readonly ILogger<SampleDataSeeder> _logger;

        public int Order => 10; // 다른 시더들이 실행된 후 실행

        public SampleDataSeeder(MesDbContext context, ILogger<SampleDataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.WorkOrders.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("샘플 데이터가 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("샘플 데이터 시딩 시작...");

            // 필요한 데이터 조회
            var productA = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductCode == "DEMO-PART-001");
            var productB = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductCode == "DEMO-PART-002");
            var scenarioA = await _context.WorkScenarios
                .Include(s => s.Steps)
                .FirstOrDefaultAsync(s => s.ScenarioCode == "SCENARIO_A");
            var scenarioB = await _context.WorkScenarios
                .Include(s => s.Steps)
                .FirstOrDefaultAsync(s => s.ScenarioCode == "SCENARIO_B");

            if (productA == null || productB == null || scenarioA == null || scenarioB == null)
            {
                _logger.LogWarning("필수 데이터가 없습니다. 기본 시더를 먼저 실행하세요.");
                return;
            }

            // 완료된 작업 지시 생성
            var completedOrder = await CreateCompletedWorkOrderAsync(productA, scenarioA);

            // 진행 중인 작업 지시 생성
            var inProgressOrder = await CreateInProgressWorkOrderAsync(productA, scenarioA);

            // 예약된 작업 지시 생성
            var scheduledOrders = await CreateScheduledWorkOrdersAsync(productA, productB, scenarioA, scenarioB);

            // 실행 로그 샘플 추가
            await AddSampleLogsAsync(completedOrder);

            _logger.LogInformation("샘플 데이터 시딩 완료");
        }

        private async Task<WorkOrder> CreateCompletedWorkOrderAsync(Product product, WorkScenario scenario)
        {
            var workOrder = new WorkOrder
            {
                OrderNumber = "WO-20240101-0001",
                OrderName = "완료된 테스트 작업",
                ProductId = product.Id,
                Product = product,
                ScenarioId = scenario.Id,
                Scenario = scenario,
                Quantity = 10,
                Status = WorkOrderStatus.Completed,
                Priority = 50,
                ScheduledStartTime = DateTime.UtcNow.AddHours(-3),
                ActualStartTime = DateTime.UtcNow.AddHours(-2.5),
                ActualEndTime = DateTime.UtcNow.AddHours(-1),
                CurrentStepNumber = scenario.Steps.Count,
                ProgressPercentage = 100,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            };

            await _context.WorkOrders.AddAsync(workOrder);
            await _context.SaveChangesAsync();

            // 모든 실행 단계 생성 (완료 상태)
            foreach (var step in scenario.Steps.OrderBy(s => s.StepNumber))
            {
                var execution = new WorkOrderExecution
                {
                    WorkOrderId = workOrder.Id,
                    ScenarioStepId = step.Id,
                    Status = ExecutionStatus.Completed,
                    AssignedResource = GetResourceForStep(step),
                    StartTime = DateTime.UtcNow.AddHours(-2.5 + (step.StepNumber * 0.1)),
                    EndTime = DateTime.UtcNow.AddHours(-2.4 + (step.StepNumber * 0.1)),
                    ExecutionData = JsonSerializer.SerializeToDocument(new
                    {
                        stepCompleted = true,
                        duration = step.EstimatedDuration,
                        result = "Success"
                    }),
                    CreatedBy = "System"
                };

                await _context.WorkOrderExecutions.AddAsync(execution);
            }

            await _context.SaveChangesAsync();
            return workOrder;
        }

        private async Task<WorkOrder> CreateInProgressWorkOrderAsync(Product product, WorkScenario scenario)
        {
            var workOrder = new WorkOrder
            {
                OrderNumber = "WO-20240101-0002",
                OrderName = "진행 중인 테스트 작업",
                ProductId = product.Id,
                Product = product,
                ScenarioId = scenario.Id,
                Scenario = scenario,
                Quantity = 5,
                Status = WorkOrderStatus.InProgress,
                Priority = 80,
                ScheduledStartTime = DateTime.UtcNow.AddHours(-1),
                ActualStartTime = DateTime.UtcNow.AddMinutes(-30),
                CurrentStepNumber = 5, // 5단계 진행 중
                ProgressPercentage = 35,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };

            await _context.WorkOrders.AddAsync(workOrder);
            await _context.SaveChangesAsync();

            // 실행 단계 생성 (일부 완료, 하나는 진행 중, 나머지는 대기)
            var stepIndex = 0;
            foreach (var step in scenario.Steps.OrderBy(s => s.StepNumber))
            {
                stepIndex++;
                ExecutionStatus status;
                DateTime? startTime = null;
                DateTime? endTime = null;
                string? resource = null;

                if (stepIndex < 5)
                {
                    // 완료된 단계
                    status = ExecutionStatus.Completed;
                    startTime = DateTime.UtcNow.AddMinutes(-30 + (stepIndex * 5));
                    endTime = startTime.Value.AddSeconds(step.EstimatedDuration);
                    resource = GetResourceForStep(step);
                }
                else if (stepIndex == 5)
                {
                    // 진행 중인 단계
                    status = ExecutionStatus.InProgress;
                    startTime = DateTime.UtcNow.AddMinutes(-5);
                    resource = GetResourceForStep(step);
                }
                else
                {
                    // 대기 중인 단계
                    status = ExecutionStatus.Pending;
                }

                var execution = new WorkOrderExecution
                {
                    WorkOrderId = workOrder.Id,
                    ScenarioStepId = step.Id,
                    Status = status,
                    AssignedResource = resource,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedBy = "System"
                };

                await _context.WorkOrderExecutions.AddAsync(execution);
            }

            await _context.SaveChangesAsync();
            return workOrder;
        }

        private async Task<WorkOrder[]> CreateScheduledWorkOrdersAsync(
            Product productA, Product productB,
            WorkScenario scenarioA, WorkScenario scenarioB)
        {
            var orders = new[]
            {
                new WorkOrder
                {
                    OrderNumber = "WO-20240101-0003",
                    OrderName = "예약된 작업 1",
                    ProductId = productA.Id,
                    Product = productA,
                    ScenarioId = scenarioA.Id,
                    Scenario = scenarioA,
                    Quantity = 20,
                    Status = WorkOrderStatus.Scheduled,
                    Priority = 60,
                    ScheduledStartTime = DateTime.UtcNow.AddHours(1),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new WorkOrder
                {
                    OrderNumber = "WO-20240101-0004",
                    OrderName = "예약된 작업 2",
                    ProductId = productB.Id,
                    Product = productB,
                    ScenarioId = scenarioB.Id,
                    Scenario = scenarioB,
                    Quantity = 15,
                    Status = WorkOrderStatus.Scheduled,
                    Priority = 70,
                    ScheduledStartTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new WorkOrder
                {
                    OrderNumber = "WO-20240101-0005",
                    OrderName = "대기 중인 작업",
                    ProductId = productA.Id,
                    Product = productA,
                    ScenarioId = scenarioA.Id,
                    Scenario = scenarioA,
                    Quantity = 8,
                    Status = WorkOrderStatus.Created,
                    Priority = 90,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15)
                }
            };

            await _context.WorkOrders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // 각 작업의 실행 단계 생성 (모두 Pending)
            foreach (var order in orders)
            {
                var scenario = order.ScenarioId == scenarioA.Id ? scenarioA : scenarioB;
                foreach (var step in scenario.Steps)
                {
                    var execution = new WorkOrderExecution
                    {
                        WorkOrderId = order.Id,
                        ScenarioStepId = step.Id,
                        Status = ExecutionStatus.Pending,
                        CreatedBy = "System"
                    };

                    await _context.WorkOrderExecutions.AddAsync(execution);
                }
            }

            await _context.SaveChangesAsync();
            return orders;
        }

        private async Task AddSampleLogsAsync(WorkOrder workOrder)
        {
            var logs = new[]
            {
                new ExecutionLog
                {
                    WorkOrderId = workOrder.Id,
                    LogLevel = LogLevel.Info,
                    Category = "WorkOrder",
                    EventType = "WorkOrderCreated",
                    Message = $"작업 지시 생성됨: {workOrder.OrderNumber}",
                    SourceSystem = "MES",
                    Timestamp = workOrder.CreatedAt
                },
                new ExecutionLog
                {
                    WorkOrderId = workOrder.Id,
                    LogLevel = LogLevel.Info,
                    Category = "Execution",
                    EventType = "WorkOrderStarted",
                    Message = "작업 지시 실행 시작",
                    SourceSystem = "MES",
                    Timestamp = workOrder.ActualStartTime ?? DateTime.UtcNow
                },
                new ExecutionLog
                {
                    WorkOrderId = workOrder.Id,
                    LogLevel = LogLevel.Info,
                    Category = "Integration",
                    EventType = "AMSCommandSent",
                    Message = "AMS로 AMR 이동 명령 전송",
                    SourceSystem = "MES",
                    TargetSystem = "AMS",
                    ExecutionTimeMs = 125,
                    Timestamp = workOrder.ActualStartTime?.AddMinutes(1) ?? DateTime.UtcNow
                },
                new ExecutionLog
                {
                    WorkOrderId = workOrder.Id,
                    LogLevel = LogLevel.Warning,
                    Category = "Execution",
                    EventType = "RetryAttempt",
                    Message = "AMR 통신 재시도 (1/3)",
                    SourceSystem = "MES",
                    AdditionalData = JsonSerializer.SerializeToDocument(new
                    {
                        reason = "Timeout",
                        attemptNumber = 1,
                        maxAttempts = 3
                    }),
                    Timestamp = workOrder.ActualStartTime?.AddMinutes(5) ?? DateTime.UtcNow
                },
                new ExecutionLog
                {
                    WorkOrderId = workOrder.Id,
                    LogLevel = LogLevel.Info,
                    Category = "Execution",
                    EventType = "WorkOrderCompleted",
                    Message = "작업 지시 완료",
                    SourceSystem = "MES",
                    ExecutionTimeMs = (long)(workOrder.ActualEndTime - workOrder.ActualStartTime)?.TotalMilliseconds,
                    Timestamp = workOrder.ActualEndTime ?? DateTime.UtcNow
                }
            };

            await _context.ExecutionLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        private string GetResourceForStep(WorkScenarioStep step)
        {
            return step.ActionType switch
            {
                ActionType.Move or ActionType.ChargeMove => "THIRA_01",
                ActionType.Load or ActionType.Unload or ActionType.StationLoad or ActionType.StationUnload => "THIRA_01_ROBOT",
                ActionType.Process when step.TargetLocation?.Contains("Welding") == true => "WELD_01",
                ActionType.Process when step.TargetLocation?.Contains("Bolting") == true => "BOLT_01",
                _ => "UNKNOWN"
            };
        }
    }
}