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
    /// 작업 시나리오 데이터 시더 - 병렬 처리 방식
    /// </summary>
    public class WorkScenarioSeeder : IDataSeeder
    {
        private readonly MesDbContext _context;
        private readonly ILogger<WorkScenarioSeeder> _logger;

        public int Order => 2;

        public WorkScenarioSeeder(MesDbContext context, ILogger<WorkScenarioSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.WorkScenarios.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("시나리오 데이터가 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("병렬 처리 시나리오 데이터 시딩 시작...");

            // 시나리오 A: Zone A에서 용접-볼팅 병렬 처리
            var scenarioA = CreateParallelScenarioA();
            await _context.WorkScenarios.AddAsync(scenarioA);

            // 시나리오 B: Zone B에서 용접-볼팅 병렬 처리
            var scenarioB = CreateParallelScenarioB();
            await _context.WorkScenarios.AddAsync(scenarioB);

            await _context.SaveChangesAsync();

            _logger.LogInformation("병렬 처리 시나리오 데이터 시딩 완료");
        }

        private WorkScenario CreateParallelScenarioA()
        {
            var scenario = new WorkScenario
            {
                ScenarioCode = "PARALLEL_SCENARIO_A",
                ScenarioName = "용접-볼팅 병렬 공정 (Zone A)",
                Description = "LoadingZone A에서 시작하여 용접과 볼팅을 병렬로 진행하고 순차적으로 회수 후 UnloadingZone A로 이동",
                Version = "2.0.0",
                IsActive = true,
                EstimatedDuration = 30, // 30분 (병렬 처리로 단축)
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    loadingZone = "LoadingZoneA",
                    unloadingZone = "UnloadingZoneA",
                    requiredStations = new[] { "WeldingStation", "BoltingStation" },
                    parallelProcessing = true,
                    maxConcurrentProcesses = 2
                }),
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            // === 1단계: 자재 준비 및 배송 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 1,
                StepName = "LoadingZone A로 AMR 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneA",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    amrCode = "THIRA_01",
                    speed = "normal",
                    route = "optimal"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 2,
                StepName = "LoadingZone A에서 첫 번째 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneA",
                ActionType = ActionType.Load,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromZone",
                    gripperType = "standard",
                    materialType = "weldingPart",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 3,
                StepName = "LoadingZone A에서 두 번째 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneA",
                ActionType = ActionType.Load,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromZone",
                    gripperType = "standard",
                    materialType = "boltingPart",
                    position = "slot2"
                })
            });

            // === 2단계: 용접 스테이션 배송 및 작업 시작 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 4,
                StepName = "용접 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    route = "optimal",
                    payloadCheck = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 5,
                StepName = "용접 스테이션에 자재 하역",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.StationLoad,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToStation",
                    stationId = "WELD_01",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 6,
                StepName = "용접 작업 시작",
                StepType = StepType.EquipmentWork,
                TargetSystem = TargetSystem.EMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Process,
                NextStepCondition = NextStepCondition.Immediate, // 바로 다음 단계로
                AllowParallelExecution = true, // 병렬 실행 허용
                EstimatedDuration = 900, // 15분
                TimeoutSeconds = 1200, // 20분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    equipmentId = "WELD_01",
                    processType = "welding",
                    program = "DEMO_WELD_01",
                    parallelId = "WELD_PROCESS"
                })
            });

            // === 3단계: 볼팅 스테이션 배송 및 작업 시작 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 7,
                StepName = "볼팅 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    route = "optimal"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 8,
                StepName = "볼팅 스테이션에 자재 하역",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.StationLoad,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToStation",
                    stationId = "BOLT_01",
                    position = "slot2"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 9,
                StepName = "볼팅 작업 시작",
                StepType = StepType.EquipmentWork,
                TargetSystem = TargetSystem.EMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Process,
                NextStepCondition = NextStepCondition.Immediate, // 바로 다음 단계로
                AllowParallelExecution = true, // 병렬 실행 허용
                EstimatedDuration = 600, // 10분
                TimeoutSeconds = 900, // 15분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    equipmentId = "BOLT_01",
                    processType = "bolting",
                    program = "DEMO_BOLT_01",
                    parallelId = "BOLT_PROCESS"
                })
            });

            // === 4단계: AMR 대기 위치 이동 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 10,
                StepName = "대기 위치로 AMR 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WaitingZone",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    parkingMode = true
                })
            });

            // === 5단계: 용접 완료 대기 및 회수 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 11,
                StepName = "용접 작업 완료 대기",
                StepType = StepType.Wait,
                TargetSystem = TargetSystem.MES,
                ActionType = ActionType.WaitForSignal,
                NextStepCondition = NextStepCondition.OnCondition,
                EstimatedDuration = 0,
                TimeoutSeconds = 1500, // 25분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    waitForStepNumber = 6,
                    signalType = "ProcessComplete",
                    parallelId = "WELD_PROCESS",
                    equipmentId = "WELD_01"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 12,
                StepName = "용접 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    priority = "high"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 13,
                StepName = "용접 완료 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.StationUnload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromStation",
                    stationId = "WELD_01",
                    position = "slot1",
                    productType = "weldedPart"
                })
            });

            // === 6단계: 볼팅 완료 대기 및 회수 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 14,
                StepName = "볼팅 작업 완료 대기",
                StepType = StepType.Wait,
                TargetSystem = TargetSystem.MES,
                ActionType = ActionType.WaitForSignal,
                NextStepCondition = NextStepCondition.OnCondition,
                EstimatedDuration = 0,
                TimeoutSeconds = 1200, // 20분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    waitForStepNumber = 9,
                    signalType = "ProcessComplete",
                    parallelId = "BOLT_PROCESS",
                    equipmentId = "BOLT_01"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 15,
                StepName = "볼팅 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    priority = "high"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 16,
                StepName = "볼팅 완료 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.StationUnload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromStation",
                    stationId = "BOLT_01",
                    position = "slot2",
                    productType = "boltedPart"
                })
            });

            // === 7단계: 완제품 배송 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 17,
                StepName = "UnloadingZone A로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneA",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    payloadCheck = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 18,
                StepName = "완제품 하역 (용접품)",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneA",
                ActionType = ActionType.Unload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToZone",
                    productType = "weldedPart",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 19,
                StepName = "완제품 하역 (볼팅품)",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneA",
                ActionType = ActionType.Unload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToZone",
                    productType = "boltedPart",
                    position = "slot2"
                })
            });

            // === 8단계: 후처리 작업 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 20,
                StepName = "충전소로 이동 (조건부)",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "ChargingStation",
                ActionType = ActionType.ChargeMove,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                IsSkippable = true,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    checkBattery = true,
                    minBatteryLevel = 30,
                    autoSkipIfSufficient = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 21,
                StepName = "아길록스 AMR 팔레트 운송",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneA",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 300, // 5분
                TimeoutSeconds = 600, // 10분
                AllowParallelExecution = true,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Agilox",
                    amrCode = "AGILOX_01",
                    task = "PalletTransfer",
                    from = "UnloadingZoneA",
                    to = "LoadingZoneA",
                    priority = "low"
                })
            });

            return scenario;
        }

        private WorkScenario CreateParallelScenarioB()
        {
            var scenario = new WorkScenario
            {
                ScenarioCode = "PARALLEL_SCENARIO_B",
                ScenarioName = "용접-볼팅 병렬 공정 (Zone B)",
                Description = "LoadingZone B에서 시작하여 용접과 볼팅을 병렬로 진행하고 순차적으로 회수 후 UnloadingZone B로 이동",
                Version = "2.0.0",
                IsActive = true,
                EstimatedDuration = 30, // 30분 (병렬 처리로 단축)
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    loadingZone = "LoadingZoneB",
                    unloadingZone = "UnloadingZoneB",
                    requiredStations = new[] { "WeldingStation", "BoltingStation" },
                    parallelProcessing = true,
                    maxConcurrentProcesses = 2
                }),
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            // === 1단계: 자재 준비 및 배송 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 1,
                StepName = "LoadingZone B로 AMR 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneB",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    amrCode = "THIRA_02",
                    speed = "normal",
                    route = "optimal"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 2,
                StepName = "LoadingZone B에서 첫 번째 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneB",
                ActionType = ActionType.Load,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromZone",
                    gripperType = "standard",
                    materialType = "weldingPart",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 3,
                StepName = "LoadingZone B에서 두 번째 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneB",
                ActionType = ActionType.Load,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromZone",
                    gripperType = "standard",
                    materialType = "boltingPart",
                    position = "slot2"
                })
            });

            // === 2단계: 용접 스테이션 배송 및 작업 시작 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 4,
                StepName = "용접 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    route = "optimal",
                    payloadCheck = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 5,
                StepName = "용접 스테이션에 자재 하역",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.StationLoad,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToStation",
                    stationId = "WELD_01",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 6,
                StepName = "용접 작업 시작",
                StepType = StepType.EquipmentWork,
                TargetSystem = TargetSystem.EMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Process,
                NextStepCondition = NextStepCondition.Immediate, // 바로 다음 단계로
                AllowParallelExecution = true, // 병렬 실행 허용
                EstimatedDuration = 900, // 15분
                TimeoutSeconds = 1200, // 20분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    equipmentId = "WELD_01",
                    processType = "welding",
                    program = "DEMO_WELD_01",
                    parallelId = "WELD_PROCESS_B"
                })
            });

            // === 3단계: 볼팅 스테이션 배송 및 작업 시작 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 7,
                StepName = "볼팅 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    route = "optimal"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 8,
                StepName = "볼팅 스테이션에 자재 하역",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.StationLoad,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToStation",
                    stationId = "BOLT_01",
                    position = "slot2"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 9,
                StepName = "볼팅 작업 시작",
                StepType = StepType.EquipmentWork,
                TargetSystem = TargetSystem.EMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Process,
                NextStepCondition = NextStepCondition.Immediate, // 바로 다음 단계로
                AllowParallelExecution = true, // 병렬 실행 허용
                EstimatedDuration = 600, // 10분
                TimeoutSeconds = 900, // 15분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    equipmentId = "BOLT_01",
                    processType = "bolting",
                    program = "DEMO_BOLT_01",
                    parallelId = "BOLT_PROCESS_B"
                })
            });

            // === 4단계: AMR 대기 위치 이동 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 10,
                StepName = "대기 위치로 AMR 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WaitingZone",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    parkingMode = true
                })
            });

            // === 5단계: 용접 완료 대기 및 회수 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 11,
                StepName = "용접 작업 완료 대기",
                StepType = StepType.Wait,
                TargetSystem = TargetSystem.MES,
                ActionType = ActionType.WaitForSignal,
                NextStepCondition = NextStepCondition.OnCondition,
                EstimatedDuration = 0,
                TimeoutSeconds = 1500, // 25분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    waitForStepNumber = 6,
                    signalType = "ProcessComplete",
                    parallelId = "WELD_PROCESS_B",
                    equipmentId = "WELD_01"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 12,
                StepName = "용접 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    priority = "high"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 13,
                StepName = "용접 완료 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "WeldingStation",
                ActionType = ActionType.StationUnload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromStation",
                    stationId = "WELD_01",
                    position = "slot1",
                    productType = "weldedPart"
                })
            });

            // === 6단계: 볼팅 완료 대기 및 회수 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 14,
                StepName = "볼팅 작업 완료 대기",
                StepType = StepType.Wait,
                TargetSystem = TargetSystem.MES,
                ActionType = ActionType.WaitForSignal,
                NextStepCondition = NextStepCondition.OnCondition,
                EstimatedDuration = 0,
                TimeoutSeconds = 1200, // 20분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    waitForStepNumber = 9,
                    signalType = "ProcessComplete",
                    parallelId = "BOLT_PROCESS_B",
                    equipmentId = "BOLT_01"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 15,
                StepName = "볼팅 스테이션으로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    priority = "high"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 16,
                StepName = "볼팅 완료 자재 적재",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "BoltingStation",
                ActionType = ActionType.StationUnload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "LoadFromStation",
                    stationId = "BOLT_01",
                    position = "slot2",
                    productType = "boltedPart"
                })
            });

            // === 7단계: 완제품 배송 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 17,
                StepName = "UnloadingZone B로 이동",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneB",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 180, // 3분
                TimeoutSeconds = 360, // 6분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    payloadCheck = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 18,
                StepName = "완제품 하역 (용접품)",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneB",
                ActionType = ActionType.Unload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToZone",
                    productType = "weldedPart",
                    position = "slot1"
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 19,
                StepName = "완제품 하역 (볼팅품)",
                StepType = StepType.RobotWork,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "UnloadingZoneB",
                ActionType = ActionType.Unload,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 60, // 1분
                TimeoutSeconds = 180, // 3분
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    robotAction = "UnloadToZone",
                    productType = "boltedPart",
                    position = "slot2"
                })
            });

            // === 8단계: 후처리 작업 ===
            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 20,
                StepName = "충전소로 이동 (조건부)",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "ChargingStation",
                ActionType = ActionType.ChargeMove,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 120, // 2분
                TimeoutSeconds = 300, // 5분
                IsSkippable = true,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Thira",
                    checkBattery = true,
                    minBatteryLevel = 30,
                    autoSkipIfSufficient = true
                })
            });

            scenario.Steps.Add(new WorkScenarioStep
            {
                StepNumber = 21,
                StepName = "아길록스 AMR 팔레트 운송",
                StepType = StepType.AMRMove,
                TargetSystem = TargetSystem.AMS,
                TargetLocation = "LoadingZoneB",
                ActionType = ActionType.Move,
                NextStepCondition = NextStepCondition.OnComplete,
                EstimatedDuration = 300, // 5분
                TimeoutSeconds = 600, // 10분
                AllowParallelExecution = true,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    amrType = "Agilox",
                    amrCode = "AGILOX_02",
                    task = "PalletTransfer",
                    from = "UnloadingZoneB",
                    to = "LoadingZoneB",
                    priority = "low"
                })
            });

            return scenario;
        }
    }
}