using System.Threading.Tasks;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 데이터 시더 인터페이스
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>
        /// 시드 우선순위 (낮을수록 먼저 실행)
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 시드 데이터 생성
        /// </summary>
        Task SeedAsync();

        /// <summary>
        /// 이미 시드되었는지 확인
        /// </summary>
        Task<bool> HasDataAsync();
    }
}