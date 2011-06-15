
namespace ProductionProfiler.Interfaces
{
    public interface IRepository<TEntity, in TId>
    {
        TEntity GetById(TId id);
        void Delete<TTemplate>(TTemplate template);
        void Save(TEntity entity);
    }
}
