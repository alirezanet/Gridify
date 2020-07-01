using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TuxTeam.Gridify
{
    public class QueryColumnMapper<T>
    {

        public QueryColumnMapper(bool caseSensitive = false)
        {
            CaseSensitive = caseSensitive;
            Mappings = caseSensitive ? new Dictionary<string, Expression<Func<T, object>>>() :
                                       new Dictionary<string, Expression<Func<T, object>>>(StringComparer.OrdinalIgnoreCase);
        }
        public Dictionary<string, Expression<Func<T, object>>> Mappings { get; set; }
        public bool CaseSensitive { get; }
        public QueryColumnMapper<T> GenerateMappings()
        {
            foreach (var item in typeof(T).GetProperties())
            {
                var name = Char.ToLowerInvariant(item.Name[0]) + item.Name.Substring(1); //camel-case name

                // add to mapper object
                Mappings.Add(name, GetExpression(item.Name));
            }
            return this;
        }
        public QueryColumnMapper<T> AddMap(string propertyName, Expression<Func<T, object>> column, bool replaceOldMapping = true)
        {
            if (Mappings.ContainsKey(propertyName))
            {
                if (replaceOldMapping)
                {
                    RemoveMap(propertyName);
                    Mappings.Add(propertyName, column);
                }
            }
            else
            {
                Mappings.Add(propertyName, column);
            }
            return this;
        }

        public QueryColumnMapper<T> RemoveMap(string propertyName)
        {
            Mappings.Remove(propertyName);
            return this;
        }

        public Expression<Func<T, object>> GetExpression(string propertyName)
        {
            // x =>
            var parameter = Expression.Parameter(typeof(T));
            // x.Name
            var mapProperty = Expression.Property(parameter, propertyName);
            // (object)x.Name
            var convertedExpression = Expression.Convert(mapProperty, typeof(object));
            // x => (object)x.Name
            return Expression.Lambda<Func<T, object>>(convertedExpression, parameter);
        }
    }
}
