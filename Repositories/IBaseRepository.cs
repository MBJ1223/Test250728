using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// 기본 Repository 인터페이스
    /// </summary>
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        // 조회
        Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false);
        Task<TEntity?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllAsync(bool includeDeleted = false);
        Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes);

        // 조건 조회
        Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false);
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false);
        Task<IEnumerable<TEntity>> FindAllWithIncludesAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

        // 페이징
        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false);

        // 존재 확인
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        // 추가
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

        // 수정
        TEntity Update(TEntity entity);
        IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities);

        // 삭제
        Task DeleteAsync(Guid id);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);

        // 소프트 삭제
        Task SoftDeleteAsync(Guid id);
        void SoftDelete(TEntity entity);
        void SoftDeleteRange(IEnumerable<TEntity> entities);

        // 복원
        Task RestoreAsync(Guid id);
        void Restore(TEntity entity);
        void RestoreRange(IEnumerable<TEntity> entities);

        // 저장
        Task<int> SaveChangesAsync();

        // Queryable
        IQueryable<TEntity> Query(bool includeDeleted = false);
        IQueryable<TEntity> QueryNoTracking(bool includeDeleted = false);
    }
}