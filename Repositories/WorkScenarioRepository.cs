using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// WorkScenario Repository 구현체
    /// </summary>
    public class WorkScenarioRepository : BaseRepository<WorkScenario>, IWorkScenarioRepository
    {
        public WorkScenarioRepository(MesDbContext context) : base(context)
        {
        }

        // 시나리오 코드로 조회
        public async Task<WorkScenario?> GetByCodeAsync(string scenarioCode)
        {
            return await _dbSet
                .Include(s => s.Steps.OrderBy(st => st.StepNumber))
                .FirstOrDefaultAsync(s => s.ScenarioCode == scenarioCode && s.IsActive);
        }

        // 상세 정보 포함 조회
        public async Task<WorkScenario?> GetWithStepsAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Steps.OrderBy(st => st.StepNumber))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<WorkScenario?> GetCompleteScenarioAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Steps.OrderBy(st => st.StepNumber))
                .Include(s => s.WorkOrders)
                    .ThenInclude(w => w.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // 활성 시나리오 조회
        public async Task<IEnumerable<WorkScenario>> GetActiveScenarios()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .Include(s => s.Steps)
                .OrderBy(s => s.ScenarioCode)
                .ToListAsync();
        }

        // 버전별 조회
        public async Task<IEnumerable<WorkScenario>> GetByVersionAsync(string version)
        {
            return await _dbSet
                .Where(s => s.Version == version)
                .Include(s => s.Steps)
                .OrderBy(s => s.ScenarioCode)
                .ToListAsync();
        }

        public async Task<WorkScenario?> GetLatestVersionAsync(string scenarioCode)
        {
            return await _dbSet
                .Where(s => s.ScenarioCode.StartsWith(scenarioCode))
                .Include(s => s.Steps)
                .OrderByDescending(s => s.Version)
                .ThenByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // 사용 가능한 시나리오 조회
        public async Task<IEnumerable<WorkScenario>> GetAvailableScenariosAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive && s.Steps.Any())
                .Include(s => s.Steps)
                .OrderBy(s => s.ScenarioCode)
                .ToListAsync();
        }

        // 시나리오 단계 관련
        public async Task<WorkScenarioStep?> GetStepAsync(Guid scenarioId, int stepNumber)
        {
            return await _context.WorkScenarioSteps
                .FirstOrDefaultAsync(s => s.ScenarioId == scenarioId && s.StepNumber == stepNumber);
        }

        public async Task<IEnumerable<WorkScenarioStep>> GetStepsByTypeAsync(StepType stepType)
        {
            return await _context.WorkScenarioSteps
                .Where(s => s.StepType == stepType)
                .Include(s => s.Scenario)
                .OrderBy(s => s.ScenarioId)
                .ThenBy(s => s.StepNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkScenarioStep>> GetStepsByTargetSystemAsync(TargetSystem targetSystem)
        {
            return await _context.WorkScenarioSteps
                .Where(s => s.TargetSystem == targetSystem)
                .Include(s => s.Scenario)
                .OrderBy(s => s.ScenarioId)
                .ThenBy(s => s.StepNumber)
                .ToListAsync();
        }

        // 시나리오 사용 통계
        public async Task<int> GetUsageCountAsync(Guid scenarioId)
        {
            return await _context.WorkOrders
                .CountAsync(w => w.ScenarioId == scenarioId);
        }

        public async Task<Dictionary<Guid, int>> GetUsageStatisticsAsync()
        {
            return await _context.WorkOrders
                .GroupBy(w => w.ScenarioId)
                .Select(g => new { ScenarioId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ScenarioId, x => x.Count);
        }

        // 시나리오 복제
        public async Task<WorkScenario> CloneScenarioAsync(Guid scenarioId, string newCode, string newName)
        {
            var originalScenario = await GetWithStepsAsync(scenarioId);
            if (originalScenario == null)
                throw new InvalidOperationException($"Scenario with ID {scenarioId} not found.");

            var clonedScenario = new WorkScenario
            {
                ScenarioCode = newCode,
                ScenarioName = newName,
                Description = originalScenario.Description,
                Version = "1.0.0",
                IsActive = false, // 기본적으로 비활성 상태로 생성
                EstimatedDuration = originalScenario.EstimatedDuration,
                Parameters = originalScenario.Parameters != null
                    ? JsonDocument.Parse(originalScenario.Parameters.RootElement.GetRawText())
                    : null
            };

            // 단계 복제
            foreach (var originalStep in originalScenario.Steps.OrderBy(s => s.StepNumber))
            {
                var clonedStep = new WorkScenarioStep
                {
                    StepNumber = originalStep.StepNumber,
                    StepName = originalStep.StepName,
                    StepType = originalStep.StepType,
                    TargetSystem = originalStep.TargetSystem,
                    TargetLocation = originalStep.TargetLocation,
                    ActionType = originalStep.ActionType,
                    NextStepCondition = originalStep.NextStepCondition,
                    EstimatedDuration = originalStep.EstimatedDuration,
                    TimeoutSeconds = originalStep.TimeoutSeconds,
                    MaxRetryCount = originalStep.MaxRetryCount,
                    AllowParallelExecution = originalStep.AllowParallelExecution,
                    IsSkippable = originalStep.IsSkippable,
                    Parameters = originalStep.Parameters != null
                        ? JsonDocument.Parse(originalStep.Parameters.RootElement.GetRawText())
                        : null,
                    ConditionExpression = originalStep.ConditionExpression,
                    NextStepMapping = originalStep.NextStepMapping
                };

                clonedScenario.Steps.Add(clonedStep);
            }

            await AddAsync(clonedScenario);
            await SaveChangesAsync();

            return clonedScenario;
        }

        // 시나리오 검증
        public async Task<bool> ValidateScenarioAsync(Guid scenarioId)
        {
            var errors = await GetValidationErrorsAsync(scenarioId);
            return !errors.Any();
        }

        public async Task<List<string>> GetValidationErrorsAsync(Guid scenarioId)
        {
            var errors = new List<string>();
            var scenario = await GetWithStepsAsync(scenarioId);

            if (scenario == null)
            {
                errors.Add("시나리오를 찾을 수 없습니다.");
                return errors;
            }

            // 기본 검증
            if (string.IsNullOrWhiteSpace(scenario.ScenarioCode))
                errors.Add("시나리오 코드가 비어있습니다.");

            if (string.IsNullOrWhiteSpace(scenario.ScenarioName))
                errors.Add("시나리오명이 비어있습니다.");

            if (!scenario.Steps.Any())
                errors.Add("시나리오에 단계가 없습니다.");

            // 단계 검증
            var stepNumbers = scenario.Steps.Select(s => s.StepNumber).OrderBy(n => n).ToList();

            // 단계 번호 연속성 검증
            for (int i = 0; i < stepNumbers.Count; i++)
            {
                if (stepNumbers[i] != i + 1)
                {
                    errors.Add($"단계 번호가 연속적이지 않습니다. 예상: {i + 1}, 실제: {stepNumbers[i]}");
                }
            }

            // 각 단계 검증
            foreach (var step in scenario.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    errors.Add($"단계 {step.StepNumber}의 이름이 비어있습니다.");

                // Decision 타입일 경우 조건식 검증
                if (step.StepType == StepType.Decision && string.IsNullOrWhiteSpace(step.ConditionExpression))
                    errors.Add($"단계 {step.StepNumber}(Decision)의 조건식이 비어있습니다.");

                // AMR/Robot 작업일 경우 대상 위치 검증
                if ((step.StepType == StepType.AMRMove || step.StepType == StepType.RobotWork) &&
                    string.IsNullOrWhiteSpace(step.TargetLocation))
                    errors.Add($"단계 {step.StepNumber}의 대상 위치가 지정되지 않았습니다.");
            }

            return errors;
        }

        // 버전 관리
        public async Task<WorkScenario> CreateNewVersionAsync(Guid scenarioId, string newVersion)
        {
            var originalScenario = await GetWithStepsAsync(scenarioId);
            if (originalScenario == null)
                throw new InvalidOperationException($"Scenario with ID {scenarioId} not found.");

            // 기존 시나리오 비활성화
            originalScenario.IsActive = false;
            Update(originalScenario);

            // 새 버전으로 복제
            var newScenario = await CloneScenarioAsync(
                scenarioId,
                originalScenario.ScenarioCode,
                originalScenario.ScenarioName
            );

            newScenario.Version = newVersion;
            newScenario.IsActive = true;

            Update(newScenario);
            await SaveChangesAsync();

            return newScenario;
        }

        public async Task<bool> ActivateScenarioAsync(Guid scenarioId)
        {
            var scenario = await GetByIdAsync(scenarioId);
            if (scenario == null)
                return false;

            // 동일 코드의 다른 시나리오 비활성화
            var sameCodeScenarios = await _dbSet
                .Where(s => s.ScenarioCode == scenario.ScenarioCode && s.Id != scenarioId)
                .ToListAsync();

            foreach (var s in sameCodeScenarios)
            {
                s.IsActive = false;
            }

            scenario.IsActive = true;
            UpdateRange(sameCodeScenarios);
            Update(scenario);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateScenarioAsync(Guid scenarioId)
        {
            var scenario = await GetByIdAsync(scenarioId);
            if (scenario == null)
                return false;

            scenario.IsActive = false;
            Update(scenario);
            await SaveChangesAsync();

            return true;
        }
    }
}