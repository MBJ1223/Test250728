using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 제품 레시피 - 제품별 공정 정의
    /// </summary>
    public class Recipe : BaseEntity
    {
        /// <summary>
        /// 레시피 코드
        /// </summary>
        public string RecipeCode { get; set; } = string.Empty;

        /// <summary>
        /// 레시피명
        /// </summary>
        public string RecipeName { get; set; } = string.Empty;

        /// <summary>
        /// 제품 타입 (Welding, Bolting)
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// 설명
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 버전
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 활성 상태
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 총 예상 소요시간 (분)
        /// </summary>
        public int TotalEstimatedMinutes { get; set; }

        /// <summary>
        /// 레시피 파라미터 (JSON)
        /// </summary>
        public JsonDocument? Parameters { get; set; }

        // Navigation Properties
        /// <summary>
        /// 레시피 단계들
        /// </summary>
        public virtual ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();

        /// <summary>
        /// 이 레시피를 사용하는 제품들
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// 이 레시피를 사용하는 작업 지시들
        /// </summary>
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}