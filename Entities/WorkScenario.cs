using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 작업 시나리오 - 미리 정의된 작업 흐름
    /// </summary>
    public class WorkScenario : BaseEntity
    {
        /// <summary>
        /// 시나리오 코드 (예: SCENARIO_A, SCENARIO_B)
        /// </summary>
        public string ScenarioCode { get; set; } = string.Empty;

        /// <summary>
        /// 시나리오명
        /// </summary>
        public string ScenarioName { get; set; } = string.Empty;

        /// <summary>
        /// 시나리오 설명
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 버전 (시나리오 수정 시 버전 관리)
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 활성 상태
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 예상 소요 시간 (분)
        /// </summary>
        public int EstimatedDuration { get; set; }

        /// <summary>
        /// 시나리오 파라미터 (JSON)
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        /// <summary>
        /// 시나리오 단계 목록
        /// </summary>
        public virtual ICollection<WorkScenarioStep> Steps { get; set; } = new List<WorkScenarioStep>();

        /// <summary>
        /// 이 시나리오를 사용하는 작업 지시 목록
        /// </summary>
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}