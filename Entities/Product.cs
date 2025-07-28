using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 제품 정보
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// 제품 코드
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// 제품명
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 제품 사양
        /// </summary>
        public string? Specification { get; set; }

        /// <summary>
        /// 단위
        /// </summary>
        public string Unit { get; set; } = "EA";

        /// <summary>
        /// 제품 타입
        /// </summary>
        public ProductType ProductType { get; set; } = ProductType.None;

        /// <summary>
        /// 레시피 ID
        /// </summary>
        public Guid? RecipeId { get; set; }

        /// <summary>
        /// 레시피
        /// </summary>
        public virtual Recipe? Recipe { get; set; }

        /// <summary>
        /// 활성 상태
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 추가 데이터 (JSON)
        /// </summary>
        public JsonDocument? AdditionalData { get; set; }

        // Navigation Properties
        /// <summary>
        /// 이 제품의 재고들
        /// </summary>
        public virtual ICollection<ProductStock> Stocks { get; set; } = new List<ProductStock>();
    }
}