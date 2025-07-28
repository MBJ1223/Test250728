using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 지시 - 실제 실행될 작업
    /// </summary>
    public class WorkOrder : BaseEntity
    {
        /// <summary>
        /// 작업 지시 번호 (고유 식별자)
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 작업 지시명
        /// </summary>
        public string OrderName { get; set; } = string.Empty;

        /// <summary>
        /// 제품 ID
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// 제품
        /// </summary>
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// 시나리오 ID
        /// </summary>
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// 시나리오
        /// </summary>
        public virtual WorkScenario Scenario { get; set; } = null!;

        /// <summary>
        /// 수량
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 작업 지시 상태
        /// </summary>
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;

        /// <summary>
        /// 우선순위 (1-100, 높을수록 우선)
        /// </summary>
        public int Priority { get; set; } = 50;

        /// <summary>
        /// 예정 시작 시간
        /// </summary>
        public DateTime? ScheduledStartTime { get; set; }

        /// <summary>
        /// 예정 종료 시간
        /// </summary>
        public DateTime? ScheduledEndTime { get; set; }

        /// <summary>
        /// 실제 시작 시간
        /// </summary>
        public DateTime? ActualStartTime { get; set; }

        /// <summary>
        /// 실제 종료 시간
        /// </summary>
        public DateTime? ActualEndTime { get; set; }

        /// <summary>
        /// 현재 실행 중인 단계 번호
        /// </summary>
        public int? CurrentStepNumber { get; set; }

        /// <summary>
        /// 진행률 (0-100)
        /// </summary>
        public decimal ProgressPercentage { get; set; } = 0;

        /// <summary>
        /// 작업 파라미터 (JSON)
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        /// <summary>
        /// 비고
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 작업 지시 실행 기록
        /// </summary>
        public virtual ICollection<WorkOrderExecution> Executions { get; set; } = new List<WorkOrderExecution>();

        /// <summary>
        /// 작업 지시 로그
        /// </summary>
        public virtual ICollection<ExecutionLog> Logs { get; set; } = new List<ExecutionLog>();

        /// <summary>
        /// 총 소요 시간 계산
        /// </summary>
        public TimeSpan? GetDuration()
        {
            if (ActualStartTime.HasValue && ActualEndTime.HasValue)
            {
                return ActualEndTime.Value - ActualStartTime.Value;
            }
            return null;
        }
    }
}