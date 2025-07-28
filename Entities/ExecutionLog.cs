using System;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 실행 로그 - 작업 실행 중 발생하는 모든 이벤트 기록
    /// </summary>
    public class ExecutionLog : BaseEntity
    {
        /// <summary>
        /// 작업 지시 ID (선택적)
        /// </summary>
        public Guid? WorkOrderId { get; set; }

        /// <summary>
        /// 작업 지시
        /// </summary>
        public virtual WorkOrder? WorkOrder { get; set; }

        /// <summary>
        /// 작업 실행 ID (선택적)
        /// </summary>
        public Guid? WorkOrderExecutionId { get; set; }

        /// <summary>
        /// 작업 실행
        /// </summary>
        public virtual WorkOrderExecution? WorkOrderExecution { get; set; }

        /// <summary>
        /// 로그 레벨
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 로그 카테고리 (예: System, Integration, Execution)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 이벤트 타입 (예: OrderCreated, StepStarted, AMSResponse)
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// 메시지
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 소스 시스템 (예: MES, AMS, EMS)
        /// </summary>
        public string? SourceSystem { get; set; }

        /// <summary>
        /// 대상 시스템
        /// </summary>
        public string? TargetSystem { get; set; }

        /// <summary>
        /// 추가 데이터 (JSON)
        /// </summary>
        public JsonDocument? AdditionalData { get; set; }

        /// <summary>
        /// 상관관계 ID (관련 이벤트 추적용)
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// 실행 시간 (밀리초)
        /// </summary>
        public long? ExecutionTimeMs { get; set; }

        /// <summary>
        /// 타임스탬프
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 정보성 로그 생성 헬퍼
        /// </summary>
        public static ExecutionLog Info(string message, string eventType, Guid? workOrderId = null)
        {
            return new ExecutionLog
            {
                LogLevel = LogLevel.Info,
                Message = message,
                EventType = eventType,
                WorkOrderId = workOrderId,
                Category = "Execution"
            };
        }

        /// <summary>
        /// 오류 로그 생성 헬퍼
        /// </summary>
        public static ExecutionLog Error(string message, string eventType, Exception? exception = null, Guid? workOrderId = null)
        {
            var log = new ExecutionLog
            {
                LogLevel = LogLevel.Error,
                Message = message,
                EventType = eventType,
                WorkOrderId = workOrderId,
                Category = "Error"
            };

            if (exception != null)
            {
                log.AdditionalData = JsonSerializer.SerializeToDocument(new
                {
                    ExceptionType = exception.GetType().Name,
                    ExceptionMessage = exception.Message,
                    StackTrace = exception.StackTrace
                });
            }

            return log;
        }
    }
}