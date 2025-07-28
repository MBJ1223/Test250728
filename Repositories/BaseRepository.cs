using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// 기본 Repository 구현체
    /// </summary>
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly MesDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(MesDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        // 조회
        public virtual async Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<TEntity?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool includeDeleted = false)
        {
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        // 조건 조회
        public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)
        {
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
            return await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)
        {
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllWithIncludesAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        // 페이징
        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Expression<Func<TEntity, object>>? orderBy = null,
            bool descending = false)
        {
            var query = _dbSet.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = descending
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }
            else
            {
                // 기본 정렬: 생성일시 내림차순
                query = query.OrderByDescending(e => e.CreatedAt);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // 존재 확인
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }

        // 추가
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList);
            return entityList;
        }

        // 수정
        public virtual TEntity Update(TEntity entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public virtual IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            _dbSet.UpdateRange(entityList);
            return entityList;
        }

        // 삭제
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id, true);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        public virtual void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        // 소프트 삭제
        public virtual async Task SoftDeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                SoftDelete(entity);
            }
        }

        public virtual void SoftDelete(TEntity entity)
        {
            entity.IsDeleted = true;
            Update(entity);
        }

        public virtual void SoftDeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
            }
            UpdateRange(entities);
        }

        // 복원
        public virtual async Task RestoreAsync(Guid id)
        {
            var entity = await GetByIdAsync(id, true);
            if (entity != null)
            {
                Restore(entity);
            }
        }

        public virtual void Restore(TEntity entity)
        {
            entity.IsDeleted = false;
            Update(entity);
        }

        public virtual void RestoreRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = false;
            }
            UpdateRange(entities);
        }

        // 저장
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Queryable
        public virtual IQueryable<TEntity> Query(bool includeDeleted = false)
        {
            return includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
        }

        public virtual IQueryable<TEntity> QueryNoTracking(bool includeDeleted = false)
        {
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;
            return query.AsNoTracking();
        }
    }
}