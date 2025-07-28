// 이 파일은 WorkScenario 의존성으로 인해 비활성화되었습니다.
// Recipe 기반 구조로 변경 후 새로운 샘플 데이터 시더를 작성해야 합니다.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 샘플 데이터 시더 - 개발 및 테스트용 (비활성화됨)
    /// </summary>
    public class SampleDataSeeder : IDataSeeder
    {
        private readonly ILogger<SampleDataSeeder> _logger;

        public int Order => 10;

        public SampleDataSeeder(ILogger<SampleDataSeeder> logger)
        {
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await Task.FromResult(true); // 항상 true 반환하여 실행되지 않도록 함
        }

        public async Task SeedAsync()
        {
            await Task.CompletedTask;
        }
    }
}