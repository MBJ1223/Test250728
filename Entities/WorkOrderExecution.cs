using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 지시 실행 - 각 시나리오 단계의 실행 기록
    /// </summary>
    public class WorkOrderExecution : BaseEntity
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
        /// 시나리오 단계 ID
        /// </summary>
        public Guid ScenarioStepId { get; set; }

        /// <summary>
        /// 시나리오 단계
        /// </summary>
        public virtual WorkScenarioStep ScenarioStep { get; set; } = null!;

        /// <summary>
        /// 실행 상태
        /// </summary>
        public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

        /// <summary>
        /// 할당된 리소스 (예: THIRA_01, WELDING_STATION_01)
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
        /// 재시도 횟수
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 실행 데이터 (JSON)
        /// 요청/응답 데이터, 중간 결과 등
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
        /// 오류 상세 (스택 트레이스 등)
        /// </summary>
        public string? ErrorDetail { get; set; }

        /// <summary>
        /// 실행 로그
        /// </summary>
        public virtual ICollection<ExecutionLog> Logs { get; set; } = new List<ExecutionLog>();

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

        /// <summary>
        /// 실행 컨텍스트 설정
        /// </summary>
        public void SetExecutionContext(string key, object value)
        {
            if (ExecutionData == null)
            {
                var doc = new Dictionary<string, object>();
                ExecutionData = JsonSerializer.SerializeToDocument(doc);
            }

            // JSON 수정 로직 구현 필요
        }

        /// <summary>
        /// 실행 컨텍스트 가져오기
        /// </summary>
        public T? GetExecutionContext<T>(string key)
        {
            if (ExecutionData == null) return default;

            // JSON 파싱 로직 구현 필요
            return default;
        }
    }
}