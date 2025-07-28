using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 시나리오 단계 - 시나리오의 개별 실행 단계
    /// </summary>
    public class WorkScenarioStep : BaseEntity
    {
        /// <summary>
        /// 시나리오 ID
        /// </summary>
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// 시나리오
        /// </summary>
        public virtual WorkScenario Scenario { get; set; } = null!;

        /// <summary>
        /// 단계 번호 (실행 순서)
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// 단계명
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 단계 타입
        /// </summary>
        public StepType StepType { get; set; }

        /// <summary>
        /// 대상 시스템
        /// </summary>
        public TargetSystem TargetSystem { get; set; }

        /// <summary>
        /// 대상 위치 (예: LoadingZoneA, WeldingStation)
        /// </summary>
        public string? TargetLocation { get; set; }

        /// <summary>
        /// 액션 타입
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// 다음 단계 진행 조건
        /// </summary>
        public NextStepCondition NextStepCondition { get; set; } = NextStepCondition.OnComplete;

        /// <summary>
        /// 예상 소요 시간 (초)
        /// </summary>
        public int EstimatedDuration { get; set; }

        /// <summary>
        /// 타임아웃 (초) - 0이면 무제한
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// 재시도 횟수
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 병렬 실행 가능 여부
        /// </summary>
        public bool AllowParallelExecution { get; set; } = false;

        /// <summary>
        /// 건너뛸 수 있는지 여부
        /// </summary>
        public bool IsSkippable { get; set; } = false;

        /// <summary>
        /// 단계별 파라미터 (JSON)
        /// 예: { "amrType": "Thira", "amrCode": "THIRA_01", "robotAction": "Load" }
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        /// <summary>
        /// 조건식 (Decision 타입일 때 사용)
        /// </summary>
        public string? ConditionExpression { get; set; }

        /// <summary>
        /// 다음 단계 매핑 (조건부 분기 시 사용)
        /// </summary>
        public string? NextStepMapping { get; set; }

        /// <summary>
        /// 이 단계의 실행 기록들
        /// </summary>
        public virtual ICollection<WorkOrderExecution> Executions { get; set; } = new List<WorkOrderExecution>();
    }
}