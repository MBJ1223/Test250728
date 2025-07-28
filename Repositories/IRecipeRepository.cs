using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Recipe Repository 인터페이스
    /// </summary>
    public interface IRecipeRepository : IBaseRepository<Recipe>
    {
        // 레시피 코드로 조회
        Task<Recipe?> GetByCodeAsync(string recipeCode);

        // 상세 정보 포함 조회
        Task<Recipe?> GetWithStepsAsync(Guid id);
        Task<Recipe?> GetCompleteRecipeAsync(Guid id);

        // 활성 레시피 조회
        Task<IEnumerable<Recipe>> GetActiveRecipesAsync();
        Task<IEnumerable<Recipe>> GetActiveRecipesByProductTypeAsync(ProductType productType);

        // 버전별 조회
        Task<IEnumerable<Recipe>> GetByVersionAsync(string version);
        Task<Recipe?> GetLatestVersionAsync(string recipeCode);

        // 레시피 단계 관련
        Task<RecipeStep?> GetStepAsync(Guid recipeId, int stepNumber);
        Task<IEnumerable<RecipeStep>> GetStepsByProcessTypeAsync(ProcessType processType);
        Task<IEnumerable<RecipeStep>> GetStepsByStationTypeAsync(StationType stationType);

        // 레시피 사용 통계
        Task<int> GetUsageCountAsync(Guid recipeId);
        Task<Dictionary<Guid, int>> GetUsageStatisticsAsync();

        // 레시피 복제
        Task<Recipe> CloneRecipeAsync(Guid recipeId, string newCode, string newName);

        // 레시피 검증
        Task<bool> ValidateRecipeAsync(Guid recipeId);
        Task<List<string>> GetValidationErrorsAsync(Guid recipeId);

        // 버전 관리
        Task<Recipe> CreateNewVersionAsync(Guid recipeId, string newVersion);
        Task<bool> ActivateRecipeAsync(Guid recipeId);
        Task<bool> DeactivateRecipeAsync(Guid recipeId);

        // 코드 중복 확인
        Task<bool> IsCodeExistsAsync(string recipeCode, Guid? excludeId = null);
    }
}