using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm;
using Norm.Collections;
using Norm.Linq;
using Norm.Responses;
using ProductionProfiler.Extensions;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Mongo
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

        public Page<TResult> Distinct<T, TResult>(string keyName, PagingInfo pagingInfo) where T : class, new()
        {
            var page = _mongo.GetCollection<T>()
                 .Distinct<TResult>(keyName)
                 .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                 .Take(pagingInfo.PageSize).ToList();

            var count = _mongo.GetCollection<T>()
                 .Distinct<TResult>(keyName)
                 .Count();

            return new Page<TResult>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<TResult> Distinct<T, TSort, TResult>(string keyName, PagingInfo pagingInfo, Func<TResult, TSort> sortExpression, bool sortAscending) where T : class, new()
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

            return new Page<TResult>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<T> Page<T>(PagingInfo pagingInfo) where T : class, new()
        {
            var page = Items<T>()
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize).ToList();

            var count = Items<T>().Count();

            return new Page<T>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<T> Page<T>(PagingInfo pagingInfo, Expression<Func<T, bool>> expression) where T : class, new()
        {
            var page = Items<T>()
                .Where(expression)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize).ToList();

            var count = Items<T>()
                .Where(expression)
                .Count();

            return new Page<T>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<T> Page<T, TSort>(PagingInfo pagingInfo, Func<T, TSort> sortExpression, bool sortAscending) where T : class, new()
        {
            var page = Items<T>()
                .SetSort(sortExpression, sortAscending)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = Items<T>()
                .Count();

            return new Page<T>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<T> Page<T, TSort>(PagingInfo pagingInfo, Expression<Func<T, bool>> expression, Func<T, TSort> sortExpression, bool sortAscending) where T : class, new()
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

            return new Page<T>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public Page<TResult> Page<T, TSort, TResult>(PagingInfo pagingInfo, Expression<Func<T, bool>> expression, Func<T, TSort> sortExpression, bool sortAscending, Func<T, TResult> selectExpression) where T : class, new()
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

            return new Page<TResult>(page, new Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
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

        public void Dispose()
        {
            _mongo.Dispose();
        }
    }
}