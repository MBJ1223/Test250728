using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 시스템 통합 정보 - 외부 시스템과의 연동 설정
    /// </summary>
    public class SystemIntegration : BaseEntity
    {
        /// <summary>
        /// 시스템 코드 (AMS, EMS, TDVMS, ARMS)
        /// </summary>
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 시스템명
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 통합 타입
        /// </summary>
        public IntegrationType IntegrationType { get; set; }

        /// <summary>
        /// 엔드포인트 URL
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// 상태
        /// </summary>
        public IntegrationStatus Status { get; set; } = IntegrationStatus.Active;

        /// <summary>
        /// 마지막 통신 시간
        /// </summary>
        public DateTime? LastCommunication { get; set; }

        /// <summary>
        /// 마지막 헬스체크 시간
        /// </summary>
        public DateTime? LastHealthCheck { get; set; }

        /// <summary>
        /// 헬스체크 상태
        /// </summary>
        public bool IsHealthy { get; set; } = true;

        /// <summary>
        /// 설정 정보 (JSON)
        /// 인증 정보, 타임아웃 설정 등
        /// </summary>
        public JsonDocument? Configuration { get; set; }

        /// <summary>
        /// 통합 로그
        /// </summary>
        public virtual ICollection<IntegrationLog> IntegrationLogs { get; set; } = new List<IntegrationLog>();
    }

    /// <summary>
    /// 통합 타입
    /// </summary>
    public enum IntegrationType
    {
        REST,           // REST API
        MessageQueue,   // RabbitMQ 등
        SignalR,        // SignalR 실시간 통신
        RedisStream,    // Redis Streams
        gRPC            // gRPC
    }

    /// <summary>
    /// 통합 상태
    /// </summary>
    public enum IntegrationStatus
    {
        Active,         // 활성
        Inactive,       // 비활성
        Error,          // 오류
        Maintenance     // 유지보수
    }
}