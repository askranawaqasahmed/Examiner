using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.Repositories;

public abstract class SqlKataRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity, new()
{
    protected readonly QueryFactory QueryFactory;
    protected readonly string TableName;

    protected SqlKataRepository(QueryFactory queryFactory, string tableName)
    {
        QueryFactory = queryFactory;
        TableName = tableName;
    }

    public virtual Task<IEnumerable<TEntity>> GetAllAsync()
        => QueryFactory.Query(TableName).GetAsync<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        var entity = await QueryFactory.Query(TableName).Where("Id", id).FirstOrDefaultAsync<TEntity>();
        return entity;
    }

    public virtual async Task<Guid> InsertAsync(TEntity entity)
    {
        var entityId = EnsureEntityId(entity);
        await QueryFactory.Query(TableName).InsertAsync(entity);
        return entityId;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        var affected = await QueryFactory.Query(TableName).Where("Id", entity.Id).UpdateAsync(entity);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var affected = await QueryFactory.Query(TableName).Where("Id", id).DeleteAsync();
        return affected > 0;
    }

    protected static Guid EnsureEntityId(TEntity entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        return entity.Id;
    }
}
