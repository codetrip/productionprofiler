using System;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace ProductionProfiler.Persistence.Mongo
{
    public static class MongoDatabaseExtensions
    {
        private static readonly Regex RxGenericTypeNameFinder = new Regex("[^`]+", RegexOptions.Compiled);

        public static MongoCollection<T> GetCollection<T>(this MongoDatabase database)
        {
            string collectionName = GetCollectionName(typeof(T));
            return database.GetCollection<T>(collectionName);
        }

        public static CommandResult DropCollection<T>(this MongoDatabase database)
        {
            string collectionName = GetCollectionName(typeof(T));
            return database.DropCollection(collectionName);
        }

        internal static string GetCollectionName(Type type)
        {
            return GetScrubbedGenericName(type);
        }

        public static string GetScrubbedGenericName(Type t)
        {
            string str = t.Name;
            if (t.IsGenericType)
            {
                str = RxGenericTypeNameFinder.Match(t.Name).Value;

                foreach (Type t1 in t.GetGenericArguments())
                {
                    str = str + "_" + GetScrubbedGenericName(t1);
                }
            }
            return str;
        }
    }
}