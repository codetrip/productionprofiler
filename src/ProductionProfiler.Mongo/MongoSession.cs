using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm;
using Norm.BSON;
using Norm.Collections;
using Norm.Linq;
using Norm.Protocol.Messages;
using Norm.Responses;
using ProductionProfiler.Core.Extensions;
using E = ProductionProfiler.Core.Persistence.Entities;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoSession : IDisposable
    {
        private readonly Norm.Mongo _mongo;

        private MongoSession(string database, string server, string port)
        {
            _mongo = new Norm.Mongo(database, server, port, string.Empty);
        }

        #region Connect / Construct

        public static MongoSession Connect(string database, string server, string port)
        {
            return new MongoSession(database, server, port);
        }

        #endregion

        #region Query

        public T Single<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return Items<T>().Where(expression).SingleOrDefault();
        }

        public IQueryable<T> Items<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return Items<T>().Where(expression);
        }

        public IQueryable<T> Items<T>() where T : class, new()
        {
            return _mongo.GetCollection<T>().AsQueryable();
        }

        public IEnumerable<TResult> Distinct<T, TResult>(string keyName) where T : class, new()
        {
            return _mongo.GetCollection<T>().Distinct<TResult>(keyName);
        }

        public E.Page<TResult> Distinct<T, TResult>(string keyName, E.PagingInfo pagingInfo) where T : class, new()
        {
            var page = _mongo.GetCollection<T>()
                 .Distinct<TResult>(keyName)
                 .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                 .Take(pagingInfo.PageSize).ToList();

            var count = _mongo.GetCollection<T>()
                 .Distinct<TResult>(keyName)
                 .Count();

            return new E.Page<TResult>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<TResult> Distinct<T, TSort, TResult>(string keyName, E.PagingInfo pagingInfo, Func<TResult, TSort> sortExpression, bool sortAscending) where T : class, new()
        {
            var page = _mongo.GetCollection<T>()
                .Distinct<TResult>(keyName)
                .SetSort(sortExpression, sortAscending)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = _mongo.GetCollection<T>()
                .Distinct<TResult>(keyName)
                .Count();

            return new E.Page<TResult>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<T> Page<T>(E.PagingInfo pagingInfo) where T : class, new()
        {
            var page = Items<T>()
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize).ToList();

            var count = Items<T>().Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<T> Page<T>(E.PagingInfo pagingInfo, Expression<Func<T, bool>> expression) where T : class, new()
        {
            var page = Items<T>()
                .Where(expression)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize).ToList();

            var count = Items<T>()
                .Where(expression)
                .Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<T> Page<T, TSort>(E.PagingInfo pagingInfo, Func<T, TSort> sortExpression, bool sortAscending) where T : class, new()
        {
            var page = Items<T>()
                .SetSort(sortExpression, sortAscending)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = Items<T>()
                .Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<T> Page<T, TSort>(E.PagingInfo pagingInfo, Expression<Func<T, bool>> expression, Func<T, TSort> sortExpression, bool sortAscending) where T : class, new()
        {
            var page = Items<T>()
                .Where(expression)
                .SetSort(sortExpression, sortAscending)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = Items<T>()
                .Where(expression)
                .Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<TResult> Page<T, TSort, TResult>(E.PagingInfo pagingInfo, Expression<Func<T, bool>> expression, Func<T, TSort> sortExpression, bool sortAscending, Func<T, TResult> selectExpression) where T : class, new()
        {
            var page = Items<T>()
                .Where(expression)
                .SetSort(sortExpression, sortAscending)
                .Select(selectExpression)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = Items<T>()
                .Where(expression)
                .Count();

            return new E.Page<TResult>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        #endregion

        #region Insert / Update

        public void Insert<T>(T item) where T : class, new()
        {
            _mongo.GetCollection<T>().Insert(item);
        }

        public void Save<T>(T item) where T : class, new()
        {
            _mongo.GetCollection<T>().Save(item);
        }

        public void Save<T>(IEnumerable<T> items) where T : class, new()
        {
            foreach (T item in items)
            {
                Save(item);
            }
        }

        #endregion

        #region Delete

        public void Delete<T, TTemplate>(TTemplate template) where T : class, new()
        {
            _mongo.GetCollection<T>().Delete(template);
        }

        public void Delete<T>(T item) where T : class, new()
        {
            _mongo.GetCollection<T>().Delete(item);
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            var items = Items<T>().Where(expression);
            foreach (T item in items)
            {
                Delete(item);
            }
        }

        public void DeleteAll<T>() where T : class, new()
        {
            _mongo.Database.DropCollection(typeof(T).Name);
        }

        #endregion

        #region Map Reduce

        //Helper for using map reduce in mongo
        public T MapReduce<T>(string map, string reduce)
        {
            MapReduce mr = _mongo.Database.CreateMapReduce();

            MapReduceResponse response =
                mr.Execute(new MapReduceOptions(typeof(T).Name)
                {
                    Map = map,
                    Reduce = reduce
                });

            IMongoCollection<MapReduceResult<T>> coll = response.GetCollection<MapReduceResult<T>>();
            MapReduceResult<T> r = coll.Find().FirstOrDefault();

            return r.Value;
        }

        #endregion

        #region indexing

        public void CreateIndex<TDocument, TField>(Expression<Func<TDocument, TField>> index, string indexName, bool unique, IndexOption option)
        {
            _mongo.GetCollection<TDocument>().CreateIndex(index, indexName, unique, option);
        }

        public int DeleteIndex<T>(string indexName)
        {
            int numDeleted;
            _mongo.GetCollection<T>().DeleteIndex(indexName, out numDeleted);
            return numDeleted;
        }

        #endregion

        public void Dispose()
        {
            _mongo.Dispose();
        }
    }
}