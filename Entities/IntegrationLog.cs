using System;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 통합 로그 - 시스템 간 통신 기록
    /// </summary>
    public class IntegrationLog : BaseEntity
    {
        /// <summary>
        /// 시스템 통합 ID
        /// </summary>
        public Guid SystemIntegrationId { get; set; }

        /// <summary>
        /// 시스템 통합
        /// </summary>
        public virtual SystemIntegration SystemIntegration { get; set; } = null!;

        /// <summary>
        /// 액션 (예: SendCommand, ReceiveResponse, HealthCheck)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// HTTP 메소드 (REST API인 경우)
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 엔드포인트
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// 요청 데이터
        /// </summary>
        public string? RequestData { get; set; }

        /// <summary>
        /// 응답 데이터
        /// </summary>
        public string? ResponseData { get; set; }

        /// <summary>
        /// 상태 코드 (HTTP 상태 코드 등)
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 결과
        /// </summary>
        public IntegrationResult Result { get; set; }

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 실행 시간 (밀리초)
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// 로그 날짜
        /// </summary>
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 상관관계 ID (요청-응답 매칭용)
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// 작업 지시 ID (관련된 경우)
        /// </summary>
        public Guid? WorkOrderId { get; set; }

        /// <summary>
        /// 메타데이터 (JSON)
        /// </summary>
        public JsonDocument? Metadata { get; set; }
    }

    /// <summary>
    /// 통합 결과
    /// </summary>
    public enum IntegrationResult
    {
        Success,        // 성공
        Failed,         // 실패
        Timeout,        // 타임아웃
        Cancelled       // 취소됨
    }
}