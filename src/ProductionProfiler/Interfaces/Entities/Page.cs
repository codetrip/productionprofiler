using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProductionProfiler.Interfaces.Entities
{
    /// <summary>
    /// Represents one page of entities, along with information about which page it is.
    /// </summary>
    [Serializable]
    public class Page<TEntity> : IEnumerable<TEntity>
    {
        private readonly IEnumerable<TEntity> _collection;

        public Page(IEnumerable<TEntity> collection, Pagination pagination)
        {
            _collection = collection.ToList(); //.ToList() is not ideal with respect to db optimisations but it helps to guarantee that the entities are loaded when the object is sent to distributed cache
            Pagination = pagination;
        }

        public Pagination Pagination { get; private set; }

        public Page<TOtherEntity> ConvertPage<TOtherEntity>(Func<TEntity, TOtherEntity> projection)
        {
            return new Page<TOtherEntity>(this.Select(projection), Pagination);
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerator<TEntity> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Page<TEntity> Empty(PagingInfo pagingInfo)
        {
            return new Page<TEntity>(new TEntity[0], new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, 0));
        }
    }
}
