using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using E = ProductionProfiler.Core.Persistence.Entities;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoSession : IDisposable
    {
        private readonly MongoServer _server;
        private readonly MongoDatabase _database;

        public MongoServer Server
        {
            get { return _server; }
        }

        public MongoDatabase Database
        {
            get { return _database; }
        }

        private MongoSession(string database, string server, string port)
        {
            var settings = new MongoServerSettings { Server = new MongoServerAddress(server, int.Parse(port)) };
            _server = MongoServer.Create(settings);
            _database = _server.GetDatabase(database);
        }

        public static MongoSession Connect(string database, string server, string port)
        {
            return new MongoSession(database, server, port);
        }

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
            return _database.GetCollection<T>().AsQueryable();
        }

        public IEnumerable<TResult> Distinct<T, TResult>(Func<T, TResult> selectExpression) where T : class, new()
        {
            return _database.GetCollection<T>().AsQueryable().Select(selectExpression).Distinct<TResult>();
        }

        public E.Page<TResult> Distinct<T, TResult>(Func<T, TResult> selectExpression, E.PagingInfo pagingInfo) where T : class, new()
        {
            var page = _database.GetCollection<T>().AsQueryable()
                .Select(selectExpression)
                .Distinct<TResult>()
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToList();

            var count = _database.GetCollection<T>().AsQueryable()
                .Select(selectExpression)
                .Distinct<TResult>()
                .Count();

            return new E.Page<TResult>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<TResult> Distinct<T, TResult>(Func<T, TResult> selectExpression, E.PagingInfo pagingInfo, Func<T, TResult> sortExpression, bool sortAscending) where T : class, new()
        {
            List<TResult> page;

            if (sortAscending)
            {
                page = _database.GetCollection<T>().AsQueryable()
                    .OrderBy(sortExpression)
                    .Select(selectExpression)
                    .Distinct<TResult>()
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }
            else
            {
                page = _database.GetCollection<T>().AsQueryable()
                    .OrderByDescending(sortExpression)
                    .Select(selectExpression)
                    .Distinct<TResult>()
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }

            var count = _database.GetCollection<T>().AsQueryable()
                .Select(selectExpression)
                .Distinct<TResult>()
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

        public E.Page<T> Page<T>(E.PagingInfo paging, Expression<Func<T, bool>> whereExpression) where T : class, new()
        {
            var page = Items<T>()
                .Where(whereExpression)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize).ToList();

            var count = Items<T>()
                .Where(whereExpression)
                .Count();

            return new E.Page<T>(page, new E.Pagination(paging.PageSize, paging.PageNumber, count));
        }

        public E.Page<T> Page<T, TSort>(E.PagingInfo pagingInfo, Func<T, TSort> orderExpression, bool sortAscending) where T : class, new()
        {
            List<T> page;

            if (sortAscending)
            {
                page = Items<T>()
                    .OrderBy(orderExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }
            else
            {
                page = Items<T>()
                    .OrderByDescending(orderExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }

            var count = Items<T>().Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<T> Page<T, TSort>(E.PagingInfo pagingInfo, Expression<Func<T, bool>> whereExpression, Func<T, TSort> sortExpression, bool sortAscending) where T : class, new()
        {
            List<T> page;

            if (sortAscending)
            {
                page = Items<T>()
                    .Where(whereExpression)
                    .OrderBy(sortExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }
            else
            {
                page = Items<T>()
                    .Where(whereExpression)
                    .OrderByDescending(sortExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }

            var count = Items<T>()
                .Where(whereExpression)
                .Count();

            return new E.Page<T>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public E.Page<TResult> Page<T, TSort, TResult>(E.PagingInfo pagingInfo, Expression<Func<T, bool>> whereExpression, Func<T, TSort> sortExpression, bool sortAscending, Func<T, TResult> selectExpression) where T : class, new()
        {
            List<TResult> page;

            if (sortAscending)
            {
                page = Items<T>()
                    .Where(whereExpression)
                    .OrderBy(sortExpression)
                    .Select(selectExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }
            else
            {
                page = Items<T>()
                    .Where(whereExpression)
                    .OrderByDescending(sortExpression)
                    .Select(selectExpression)
                    .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }

            var count = Items<T>()
                .Where(whereExpression)
                .Count();

            return new E.Page<TResult>(page, new E.Pagination(pagingInfo.PageSize, pagingInfo.PageNumber, count));
        }

        public void Insert<T>(T item) where T : class, new()
        {
            _database.GetCollection<T>().Insert(item);
        }

        public void Save<T>(T item) where T : class, new()
        {
            if (item is IEnumerable)
            {
                throw new InvalidOperationException("You should be calling MongoSession.SaveAll<T>");
            }

            _database.GetCollection<T>().Save(item);
        }

        public void SaveAll<T>(IEnumerable<T> items) where T : class, new()
        {
            foreach (var item in items)
            {
                Save(item);
            }
        }

        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            var items = Items<T>().Where(expression);

            foreach (T item in items)
            {
                Delete(item);
            }
        }

        public void Delete<T>(T item) where T : class, new()
        {
            if (item is IEnumerable)
            {
                throw new InvalidOperationException("You should be calling MongoSession.DeleteAll<T>");
            }

            var classMap = BsonClassMap.LookupClassMap(typeof(T));
            var id = classMap.IdMemberMap.Getter.Invoke(item);

            IMongoQuery query;
            if (id is Guid)
            {
                query = Query.EQ("_id", new BsonBinaryData((Guid)id));
            }
            else
            {
                query = Query.EQ("_id", id.ToString());
            }

            _database.GetCollection<T>().Remove(query);
        }

        public void DeleteAll<T>() where T : class, new()
        {
            _database.DropCollection<T>();
        }

        public void CreateIndex<TDocument>(string field, bool ascending, bool unique)
        {
            var keys = ascending ? new IndexKeysBuilder().Ascending(field) : new IndexKeysBuilder().Descending(field);

            _database.GetCollection<TDocument>().EnsureIndex(keys, IndexOptions.SetUnique(unique));
        }

        public void CreateIndex<TDocument, TField>(Expression<Func<TDocument, TField>> index, IndexKeysBuilder keys, IndexOptionsBuilder indexOptions)
        {
            _database.GetCollection<TDocument>().EnsureIndex(keys, indexOptions);
        }
        
        public void DropIndexbyName<T>(string indexName)
        {
            _database.GetCollection<T>().DropIndexByName(indexName);
        }

        public void DropIndexbyName<T>(IndexKeysBuilder keys)
        {
            _database.GetCollection<T>().DropIndex(keys);
        }

        public void Dispose()
        {
        }
    }
}