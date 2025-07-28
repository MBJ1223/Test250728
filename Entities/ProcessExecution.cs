using System;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 공정 실행 기록
    /// </summary>
    public class ProcessExecution : BaseEntity
    {
        /// <summary>
        /// 작업 지시 ID
        /// </summary>
        public Guid WorkOrderId { get; set; }

        /// <summary>
        /// 작업 지시
        /// </summary>
        public virtual WorkOrder WorkOrder { get; set; } = null!;

        /// <summary>
        /// 제품 재고 ID
        /// </summary>
        public Guid ProductStockId { get; set; }

        /// <summary>
        /// 제품 재고
        /// </summary>
        public virtual ProductStock ProductStock { get; set; } = null!;

        /// <summary>
        /// 레시피 단계 ID
        /// </summary>
        public Guid RecipeStepId { get; set; }

        /// <summary>
        /// 레시피 단계
        /// </summary>
        public virtual RecipeStep RecipeStep { get; set; } = null!;

        /// <summary>
        /// 실행 상태
        /// </summary>
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

        /// <summary>
        /// 시작 위치 ID
        /// </summary>
        public int? StartLocationId { get; set; }

        /// <summary>
        /// 시작 위치
        /// </summary>
        public virtual Location? StartLocation { get; set; }

        /// <summary>
        /// 종료 위치 ID
        /// </summary>
        public int? EndLocationId { get; set; }

        /// <summary>
        /// 종료 위치
        /// </summary>
        public virtual Location? EndLocation { get; set; }

        /// <summary>
        /// 할당된 리소스 (AMR, Station 등)
        /// </summary>
        public string? AssignedResource { get; set; }

        /// <summary>
        /// 시작 시간
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 종료 시간
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 실행 데이터 (JSON)
        /// </summary>
        public JsonDocument? ExecutionData { get; set; }

        /// <summary>
        /// 결과 데이터 (JSON)
        /// </summary>
        public JsonDocument? ResultData { get; set; }

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 재시도 횟수
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 소요 시간 계산
        /// </summary>
        public TimeSpan? GetDuration()
        {
            if (StartTime.HasValue && EndTime.HasValue)
            {
                return EndTime.Value - StartTime.Value;
            }
            return null;
        }
    }
}