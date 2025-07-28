using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 레시피 단계 - 각 공정 단계 정의
    /// </summary>
    public class RecipeStep : BaseEntity
    {
        /// <summary>
        /// 레시피 ID
        /// </summary>
        public Guid RecipeId { get; set; }

        /// <summary>
        /// 레시피
        /// </summary>
        public virtual Recipe Recipe { get; set; } = null!;

        /// <summary>
        /// 단계 번호 (실행 순서)
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// 단계명
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 공정 타입 (Transport, Process, Inspection 등)
        /// </summary>
        public ProcessType ProcessType { get; set; }

        /// <summary>
        /// 필요한 스테이션 타입
        /// </summary>
        public StationType? RequiredStationType { get; set; }

        /// <summary>
        /// 시작 가능 위치 (콤마 구분)
        /// </summary>
        public string? ValidStartLocations { get; set; }

        /// <summary>
        /// 목표 위치
        /// </summary>
        public string? TargetLocation { get; set; }

        /// <summary>
        /// 예상 소요시간 (분)
        /// </summary>
        public int EstimatedMinutes { get; set; }

        /// <summary>
        /// 타임아웃 (분)
        /// </summary>
        public int TimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// 필수 단계 여부
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// 단계별 파라미터 (JSON)
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        /// <summary>
        /// 다음 단계 조건 (JSON)
        /// </summary>
        public JsonDocument? NextStepConditions { get; set; }

        // Navigation Properties
        /// <summary>
        /// 이 단계의 실행 기록들
        /// </summary>
        public virtual ICollection<ProcessExecution> ProcessExecutions { get; set; } = new List<ProcessExecution>();
    }
}