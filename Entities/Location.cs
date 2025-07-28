using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BODA.FMS.MES.Data.Entities
{
    /// <summary>
    /// 위치 엔티티
    /// </summary>
    public class Location : BaseEntity
    {
        /// <summary>위치 코드 (예: A-01, B-02)</summary>
        public string LocationCode { get; set; } = string.Empty;

        /// <summary>위치 명칭</summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>위치 타입</summary>
        public LocationType LocationType { get; set; }

        /// <summary>상위 위치 ID (계층 구조)</summary>
        public Guid? ParentLocationId { get; set; }

        /// <summary>최대 팔레트 수용 용량</summary>
        public int MaxCapacity { get; set; }

        /// <summary>현재 팔레트 수량</summary>
        public int CurrentCount { get; set; }

        /// <summary>X 좌표</summary>
        public decimal? CoordinateX { get; set; }

        /// <summary>Y 좌표</summary>
        public decimal? CoordinateY { get; set; }

        /// <summary>Z 좌표 (높이)</summary>
        public decimal? CoordinateZ { get; set; }

        /// <summary>활성 상태</summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        /// <summary>상위 위치</summary>
        public virtual Location? ParentLocation { get; set; }

        /// <summary>하위 위치들</summary>
        public virtual ICollection<Location> SubLocations { get; set; } = new List<Location>();

        /// <summary>이 위치에 있는 팔레트들</summary>
        public virtual ICollection<PalletLocation> PalletLocations { get; set; } = new List<PalletLocation>();
    }
}
