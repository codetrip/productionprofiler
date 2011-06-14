
namespace ProductionProfiler.Interfaces
{
    public interface IRepository<TEntity, in TId>
    {
        TEntity GetById(TId id);
        void Delete(TId id);
        void Save(TEntity entity);
    }
}
