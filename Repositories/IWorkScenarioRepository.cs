using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// WorkScenario Repository 인터페이스
    /// </summary>
    public interface IWorkScenarioRepository : IBaseRepository<WorkScenario>
    {
        // 시나리오 코드로 조회
        Task<WorkScenario?> GetByCodeAsync(string scenarioCode);

        // 상세 정보 포함 조회
        Task<WorkScenario?> GetWithStepsAsync(Guid id);
        Task<WorkScenario?> GetCompleteScenarioAsync(Guid id);

        // 활성 시나리오 조회
        Task<IEnumerable<WorkScenario>> GetActiveScenarios();

        // 버전별 조회
        Task<IEnumerable<WorkScenario>> GetByVersionAsync(string version);
        Task<WorkScenario?> GetLatestVersionAsync(string scenarioCode);

        // 사용 가능한 시나리오 조회
        Task<IEnumerable<WorkScenario>> GetAvailableScenariosAsync();

        // 시나리오 단계 관련
        Task<WorkScenarioStep?> GetStepAsync(Guid scenarioId, int stepNumber);
        Task<IEnumerable<WorkScenarioStep>> GetStepsByTypeAsync(StepType stepType);
        Task<IEnumerable<WorkScenarioStep>> GetStepsByTargetSystemAsync(TargetSystem targetSystem);

        // 시나리오 사용 통계
        Task<int> GetUsageCountAsync(Guid scenarioId);
        Task<Dictionary<Guid, int>> GetUsageStatisticsAsync();

        // 시나리오 복제
        Task<WorkScenario> CloneScenarioAsync(Guid scenarioId, string newCode, string newName);

        // 시나리오 검증
        Task<bool> ValidateScenarioAsync(Guid scenarioId);
        Task<List<string>> GetValidationErrorsAsync(Guid scenarioId);

        // 버전 관리
        Task<WorkScenario> CreateNewVersionAsync(Guid scenarioId, string newVersion);
        Task<bool> ActivateScenarioAsync(Guid scenarioId);
        Task<bool> DeactivateScenarioAsync(Guid scenarioId);
    }
}