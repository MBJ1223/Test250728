using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 지시 - 개별 제품에 대한 작업
    /// </summary>
    public class WorkOrder : BaseEntity
    {
        /// <summary>
        /// 작업 지시 번호
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 작업 지시명
        /// </summary>
        public string OrderName { get; set; } = string.Empty;

        /// <summary>
        /// 제품 재고 ID
        /// </summary>
        public Guid ProductStockId { get; set; }

        /// <summary>
        /// 제품 재고
        /// </summary>
        public virtual ProductStock ProductStock { get; set; } = null!;

        /// <summary>
        /// 레시피 ID
        /// </summary>
        public Guid RecipeId { get; set; }

        /// <summary>
        /// 레시피
        /// </summary>
        public virtual Recipe Recipe { get; set; } = null!;

        /// <summary>
        /// 작업 지시 상태
        /// </summary>
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;

        /// <summary>
        /// 우선순위 (1-100)
        /// </summary>
        public int Priority { get; set; } = 50;

        /// <summary>
        /// 현재 레시피 단계
        /// </summary>
        public int CurrentRecipeStep { get; set; } = 1;

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

        // Navigation Properties
        /// <summary>
        /// 공정 실행 기록
        /// </summary>
        public virtual ICollection<ProcessExecution> ProcessExecutions { get; set; } = new List<ProcessExecution>();

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