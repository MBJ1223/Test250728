using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Recipe Repository 구현체
    /// </summary>
    public class RecipeRepository : BaseRepository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(MesDbContext context) : base(context)
        {
        }

        // 레시피 코드로 조회
        public async Task<Recipe?> GetByCodeAsync(string recipeCode)
        {
            return await _dbSet
                .Include(r => r.Steps.OrderBy(s => s.StepNumber))
                .FirstOrDefaultAsync(r => r.RecipeCode == recipeCode && r.IsActive);
        }

        // 상세 정보 포함 조회
        public async Task<Recipe?> GetWithStepsAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.Steps.OrderBy(s => s.StepNumber))
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Recipe?> GetCompleteRecipeAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.Steps.OrderBy(s => s.StepNumber))
                .Include(r => r.Products)
                .Include(r => r.WorkOrders)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // 활성 레시피 조회
        public async Task<IEnumerable<Recipe>> GetActiveRecipesAsync()
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .Include(r => r.Steps)
                .OrderBy(r => r.RecipeCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetActiveRecipesByProductTypeAsync(ProductType productType)
        {
            return await _dbSet
                .Where(r => r.IsActive && r.ProductType == productType)
                .Include(r => r.Steps)
                .OrderBy(r => r.RecipeCode)
                .ToListAsync();
        }

        // 버전별 조회
        public async Task<IEnumerable<Recipe>> GetByVersionAsync(string version)
        {
            return await _dbSet
                .Where(r => r.Version == version)
                .Include(r => r.Steps)
                .OrderBy(r => r.RecipeCode)
                .ToListAsync();
        }

        public async Task<Recipe?> GetLatestVersionAsync(string recipeCode)
        {
            return await _dbSet
                .Where(r => r.RecipeCode.StartsWith(recipeCode))
                .Include(r => r.Steps)
                .OrderByDescending(r => r.Version)
                .ThenByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // 레시피 단계 관련
        public async Task<RecipeStep?> GetStepAsync(Guid recipeId, int stepNumber)
        {
            return await _context.RecipeSteps
                .FirstOrDefaultAsync(s => s.RecipeId == recipeId && s.StepNumber == stepNumber);
        }

        public async Task<IEnumerable<RecipeStep>> GetStepsByProcessTypeAsync(ProcessType processType)
        {
            return await _context.RecipeSteps
                .Where(s => s.ProcessType == processType)
                .Include(s => s.Recipe)
                .OrderBy(s => s.RecipeId)
                .ThenBy(s => s.StepNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<RecipeStep>> GetStepsByStationTypeAsync(StationType stationType)
        {
            return await _context.RecipeSteps
                .Where(s => s.RequiredStationType == stationType)
                .Include(s => s.Recipe)
                .OrderBy(s => s.RecipeId)
                .ThenBy(s => s.StepNumber)
                .ToListAsync();
        }

        // 레시피 사용 통계
        public async Task<int> GetUsageCountAsync(Guid recipeId)
        {
            return await _context.WorkOrders
                .CountAsync(w => w.RecipeId == recipeId);
        }

        public async Task<Dictionary<Guid, int>> GetUsageStatisticsAsync()
        {
            return await _context.WorkOrders
                .GroupBy(w => w.RecipeId)
                .Select(g => new { RecipeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RecipeId, x => x.Count);
        }

        // 레시피 복제
        public async Task<Recipe> CloneRecipeAsync(Guid recipeId, string newCode, string newName)
        {
            var originalRecipe = await GetWithStepsAsync(recipeId);
            if (originalRecipe == null)
                throw new InvalidOperationException($"Recipe with ID {recipeId} not found.");

            var clonedRecipe = new Recipe
            {
                RecipeCode = newCode,
                RecipeName = newName,
                ProductType = originalRecipe.ProductType,
                Description = originalRecipe.Description,
                Version = "1.0.0",
                IsActive = false,
                TotalEstimatedMinutes = originalRecipe.TotalEstimatedMinutes,
                Parameters = originalRecipe.Parameters != null
                    ? JsonDocument.Parse(originalRecipe.Parameters.RootElement.GetRawText())
                    : null
            };

            // 단계 복제
            foreach (var originalStep in originalRecipe.Steps.OrderBy(s => s.StepNumber))
            {
                var clonedStep = new RecipeStep
                {
                    StepNumber = originalStep.StepNumber,
                    StepName = originalStep.StepName,
                    ProcessType = originalStep.ProcessType,
                    RequiredStationType = originalStep.RequiredStationType,
                    ValidStartLocations = originalStep.ValidStartLocations,
                    TargetLocation = originalStep.TargetLocation,
                    EstimatedMinutes = originalStep.EstimatedMinutes,
                    TimeoutMinutes = originalStep.TimeoutMinutes,
                    IsMandatory = originalStep.IsMandatory,
                    Parameters = originalStep.Parameters != null
                        ? JsonDocument.Parse(originalStep.Parameters.RootElement.GetRawText())
                        : null,
                    NextStepConditions = originalStep.NextStepConditions != null
                        ? JsonDocument.Parse(originalStep.NextStepConditions.RootElement.GetRawText())
                        : null
                };

                clonedRecipe.Steps.Add(clonedStep);
            }

            await AddAsync(clonedRecipe);
            await SaveChangesAsync();

            return clonedRecipe;
        }

        // 레시피 검증
        public async Task<bool> ValidateRecipeAsync(Guid recipeId)
        {
            var errors = await GetValidationErrorsAsync(recipeId);
            return !errors.Any();
        }

        public async Task<List<string>> GetValidationErrorsAsync(Guid recipeId)
        {
            var errors = new List<string>();
            var recipe = await GetWithStepsAsync(recipeId);

            if (recipe == null)
            {
                errors.Add("레시피를 찾을 수 없습니다.");
                return errors;
            }

            // 기본 검증
            if (string.IsNullOrWhiteSpace(recipe.RecipeCode))
                errors.Add("레시피 코드가 비어있습니다.");

            if (string.IsNullOrWhiteSpace(recipe.RecipeName))
                errors.Add("레시피명이 비어있습니다.");

            if (!recipe.Steps.Any())
                errors.Add("레시피에 단계가 없습니다.");

            // 단계 검증
            var stepNumbers = recipe.Steps.Select(s => s.StepNumber).OrderBy(n => n).ToList();

            for (int i = 0; i < stepNumbers.Count; i++)
            {
                if (stepNumbers[i] != i + 1)
                {
                    errors.Add($"단계 번호가 연속적이지 않습니다. 예상: {i + 1}, 실제: {stepNumbers[i]}");
                }
            }

            // 각 단계 검증
            foreach (var step in recipe.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    errors.Add($"단계 {step.StepNumber}의 이름이 비어있습니다.");

                // 공정별 필수 정보 검증
                switch (step.ProcessType)
                {
                    case ProcessType.Transport:
                        if (string.IsNullOrWhiteSpace(step.TargetLocation))
                            errors.Add($"단계 {step.StepNumber}(Transport)의 목표 위치가 없습니다.");
                        break;

                    case ProcessType.Process:
                        if (!step.RequiredStationType.HasValue)
                            errors.Add($"단계 {step.StepNumber}(Process)의 필요 스테이션이 지정되지 않았습니다.");
                        break;
                }
            }

            return errors;
        }

        // 버전 관리
        public async Task<Recipe> CreateNewVersionAsync(Guid recipeId, string newVersion)
        {
            var originalRecipe = await GetWithStepsAsync(recipeId);
            if (originalRecipe == null)
                throw new InvalidOperationException($"Recipe with ID {recipeId} not found.");

            // 기존 레시피 비활성화
            originalRecipe.IsActive = false;
            Update(originalRecipe);

            // 새 버전으로 복제
            var newRecipe = await CloneRecipeAsync(
                recipeId,
                originalRecipe.RecipeCode,
                originalRecipe.RecipeName
            );

            newRecipe.Version = newVersion;
            newRecipe.IsActive = true;

            Update(newRecipe);
            await SaveChangesAsync();

            return newRecipe;
        }

        public async Task<bool> ActivateRecipeAsync(Guid recipeId)
        {
            var recipe = await GetByIdAsync(recipeId);
            if (recipe == null)
                return false;

            // 동일 코드의 다른 레시피 비활성화
            var sameCodeRecipes = await _dbSet
                .Where(r => r.RecipeCode == recipe.RecipeCode && r.Id != recipeId)
                .ToListAsync();

            foreach (var r in sameCodeRecipes)
            {
                r.IsActive = false;
            }

            recipe.IsActive = true;
            UpdateRange(sameCodeRecipes);
            Update(recipe);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateRecipeAsync(Guid recipeId)
        {
            var recipe = await GetByIdAsync(recipeId);
            if (recipe == null)
                return false;

            recipe.IsActive = false;
            Update(recipe);
            await SaveChangesAsync();

            return true;
        }

        // 코드 중복 확인
        public async Task<bool> IsCodeExistsAsync(string recipeCode, Guid? excludeId = null)
        {
            var query = _dbSet.Where(r => r.RecipeCode == recipeCode);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}