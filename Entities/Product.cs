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
        /// 활성 상태
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// BOM(Bill of Materials) 데이터 (JSON)
        /// </summary>
        public JsonDocument? BomData { get; set; }

        /// <summary>
        /// 표준 작업 시간 (분)
        /// </summary>
        public decimal StandardWorkTime { get; set; }

        // Navigation Properties
        /// <summary>현재 위치</summary>
        public virtual Location? CurrentLocation { get; set; }

        ///// <summary>
        ///// 이 제품의 작업 지시 목록
        ///// </summary>
        //public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }

    /// <summary>
    /// 제품 타입
    /// </summary>
    public enum ProductType
    {
        None,                // 없음
        //EmptyPallet,        // 빈 팔레트
        //FullPallet,        // 완제품 팔레트
        WeldingProduct,        // 용접 완제품
        BoltingProduct,        // 볼팅 완제품
        WeldingMaterial,        // 용접
        BoltingMaterial,    // 볼팅
    }
}