using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 팔레트 엔티티
    /// </summary>
    public class Pallet
    {
        public int Id { get; set; }

        /// <summary>팔레트 바코드/RFID</summary>
        public string PalletCode { get; set; } = string.Empty;

        /// <summary>팔레트 타입 (예: Standard, Heavy, Special)</summary>
        public string PalletType { get; set; } = string.Empty;

        /// <summary>현재 상태</summary>
        public PalletStatus Status { get; set; } = PalletStatus.Available;

        /// <summary>현재 위치 ID</summary>
        public int? CurrentLocationId { get; set; }

        /// <summary>적재된 자재 ID</summary>
        public int? MaterialId { get; set; }

        /// <summary>최대 적재 중량 (kg)</summary>
        public decimal MaxWeight { get; set; }

        /// <summary>현재 적재 중량 (kg)</summary>
        public decimal CurrentWeight { get; set; }

        /// <summary>사용 횟수</summary>
        public int UsageCount { get; set; }

        /// <summary>최대 사용 횟수</summary>
        public int MaxUsageCount { get; set; } = 1000;

        /// <summary>마지막 점검일</summary>
        public DateTime? LastInspectionDate { get; set; }

        /// <summary>생성일시</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>수정일시</summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        /// <summary>현재 위치</summary>
        public virtual Location? CurrentLocation { get; set; }

        /// <summary>위치 이력</summary>
        public virtual ICollection<PalletLocation> LocationHistory { get; set; } = new List<PalletLocation>();

        /// <summary>상태 변경 이력</summary>
        public virtual ICollection<PalletHistory> StatusHistory { get; set; } = new List<PalletHistory>();
    }

    /// <summary>
    /// 팔레트 위치 이력 엔티티
    /// </summary>
    public class PalletLocation
    {
        public int Id { get; set; }

        /// <summary>팔레트 ID</summary>
        public int PalletId { get; set; }

        /// <summary>위치 ID</summary>
        public int LocationId { get; set; }

        /// <summary>입고 일시</summary>
        public DateTime EntryTime { get; set; } = DateTime.UtcNow;

        /// <summary>출고 일시</summary>
        public DateTime? ExitTime { get; set; }

        /// <summary>현재 위치 여부</summary>
        public bool IsCurrent { get; set; } = true;

        /// <summary>이동 사유</summary>
        public string? MoveReason { get; set; }

        /// <summary>작업자 ID</summary>
        public string? OperatorId { get; set; }

        // Navigation Properties
        /// <summary>팔레트</summary>
        public virtual Pallet Pallet { get; set; } = null!;

        /// <summary>위치</summary>
        public virtual Location Location { get; set; } = null!;
    }

    /// <summary>
    /// 팔레트 상태 변경 이력 엔티티
    /// </summary>
    public class PalletHistory
    {
        public int Id { get; set; }

        /// <summary>팔레트 ID</summary>
        public int PalletId { get; set; }

        /// <summary>이전 상태</summary>
        public PalletStatus? PreviousStatus { get; set; }

        /// <summary>새 상태</summary>
        public PalletStatus NewStatus { get; set; }

        /// <summary>변경 일시</summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>변경 사유</summary>
        public string? ChangeReason { get; set; }

        /// <summary>작업자 ID</summary>
        public string? OperatorId { get; set; }

        /// <summary>위치 ID (변경 시점의 위치)</summary>
        public int? LocationId { get; set; }

        // Navigation Properties
        /// <summary>팔레트</summary>
        public virtual Pallet Pallet { get; set; } = null!;

        /// <summary>위치</summary>
        public virtual Location? Location { get; set; }
    }
}
