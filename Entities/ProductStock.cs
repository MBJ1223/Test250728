using System;
using System.Collections.Generic;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 제품 재고 - 입고된 개별 제품 관리
    /// </summary>
    public class ProductStock : BaseEntity
    {
        /// <summary>
        /// 재고 번호 (바코드/RFID)
        /// </summary>
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// 제품 ID
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// 제품
        /// </summary>
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// 팔레트 ID
        /// </summary>
        public int PalletId { get; set; }

        /// <summary>
        /// 팔레트
        /// </summary>
        public virtual Pallet Pallet { get; set; } = null!;

        /// <summary>
        /// 팔레트 내 위치 (1: 용접제품, 2: 볼팅제품)
        /// </summary>
        public int PositionOnPallet { get; set; }

        /// <summary>
        /// 현재 위치 ID
        /// </summary>
        public int CurrentLocationId { get; set; }

        /// <summary>
        /// 현재 위치
        /// </summary>
        public virtual Location CurrentLocation { get; set; } = null!;

        /// <summary>
        /// 재고 상태
        /// </summary>
        public StockStatus Status { get; set; } = StockStatus.Available;

        /// <summary>
        /// 입고 일시
        /// </summary>
        public DateTime InboundDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 출고 일시
        /// </summary>
        public DateTime? OutboundDate { get; set; }

        /// <summary>
        /// 현재 레시피 단계 (진행 중인 경우)
        /// </summary>
        public int? CurrentRecipeStep { get; set; }

        /// <summary>
        /// 품질 상태
        /// </summary>
        public QualityStatus QualityStatus { get; set; } = QualityStatus.Good;

        /// <summary>
        /// LOT 번호
        /// </summary>
        public string? LotNumber { get; set; }

        /// <summary>
        /// 비고
        /// </summary>
        public string? Remarks { get; set; }

        // Navigation Properties
        /// <summary>
        /// 이 재고에 대한 작업 지시들
        /// </summary>
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

        /// <summary>
        /// 공정 실행 이력
        /// </summary>
        public virtual ICollection<ProcessExecution> ProcessExecutions { get; set; } = new List<ProcessExecution>();
    }
}