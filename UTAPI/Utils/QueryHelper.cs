using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Web;
using UTAPI.Types;

namespace UTAPI.Utils
{
    /// <summary>
    /// Provides helper methods to apply filtering, sorting, and pagination to an IQueryable collection.
    /// </summary>
    public static class QueryHelper
    {
        /// <summary>
        /// Applies filters, sorting, and pagination to the given IQueryable collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IQueryable collection.</typeparam>
        /// <param name="iq">The IQueryable collection to apply the filters and sorting to.</param>
        /// <param name="filter">The filter query containing the filtering, sorting, and pagination criteria.</param>
        /// <summary>
        /// Applies filters, sorting, and pagination to the given IQueryable collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IQueryable collection.</typeparam>
        /// <param name="iq">The IQueryable collection to apply the filters and sorting to.</param>
        /// <param name="filter">The filter query containing the filtering, sorting, and pagination criteria.</param>
        public static void ApplyListFilters<T>(ref IQueryable<T> iq, FilterQuery filter)
        {
            try
            {
                // Decode the 'where' filter to apply specific conditions
                string decodedWhereJson = HttpUtility.UrlDecode(filter.Where);

                // If a 'where' filter is provided
                if (!string.IsNullOrEmpty(decodedWhereJson))
                {
                    // Create an expression parameter for the IQueryable type
                    var parameter = Expression.Parameter(typeof(T), "o");

                    // Deserialize the 'where' filter JSON into a list of conditions
                    List<Dictionary<string, object>> conditions = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(decodedWhereJson);

                    // Initialize a predicate (expression to filter by)
                    Expression? predicate = null;

                    // Loop through each condition to build the filter expression
                    foreach (var condition in conditions)
                    {
                        foreach (var kvp in condition)
                        {
                            // Find the property of the entity that matches the condition key
                            var propertyInfo = typeof(T).GetProperties()
                                .FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                            // If the property is found
                            if (propertyInfo != null)
                            {
                                // Create an expression for the property
                                var property = Expression.Property(parameter, propertyInfo);

                                // Get the property type for further processing
                                Type propertyType = property.Type;

                                // Get the value to filter by, handling JSON elements properly
                                object value = kvp.Value is JsonElement valueElement ? GetValueFromJsonElement(valueElement) : kvp.Value;

                                // If the property is a string and the value is also a string, apply a case-insensitive substring search
                                if (propertyType == typeof(string) && value is string)
                                {
                                    // Methods for the contains check and toLower conversion
                                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                                    // Create the expression for the property to convert to lowercase
                                    var propertyToLower = Expression.Call(property, toLowerMethod);

                                    // Create the constant value expression, also converted to lowercase
                                    var constantToLower = Expression.Constant(value.ToString().ToLower());

                                    // Build the 'contains' expression for case-insensitive search
                                    var contains = Expression.Call(propertyToLower, containsMethod, constantToLower);

                                    // If there was a previous predicate, combine it with the current 'contains' condition
                                    predicate = predicate == null ? contains : Expression.AndAlso(predicate, contains);
                                }
                                else
                                {
                                    // For other types, perform a direct equality check
                                    var constant = Expression.Constant(Convert.ChangeType(value, propertyType));
                                    var equality = Expression.Equal(property, constant);

                                    // Combine the previous predicate with the current equality condition
                                    predicate = predicate == null ? equality : Expression.AndAlso(predicate, equality);
                                }
                            }
                        }
                    }

                    // If we have a valid predicate, apply the filter to the IQueryable
                    if (predicate != null)
                    {
                        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
                        iq = iq.Where(lambda);
                    }
                }

                // Sorting: If a 'sort' query parameter is provided
                if (!string.IsNullOrEmpty(filter.Sort))
                {
                    // Split the sort parameter into the property and the order (ASC/DESC)
                    var sortParams = filter.Sort.Split(',');
                    if (sortParams.Length == 2)
                    {
                        var propertyName = sortParams[0];
                        var sortOrder = sortParams[1];

                        // Get the property info of the entity to sort by
                        var propertyInfo = typeof(T).GetProperties()
                            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                        // If the property is found
                        if (propertyInfo != null)
                        {
                            // Create the expression for sorting
                            var parameter = Expression.Parameter(typeof(T), "x");
                            var property = Expression.Property(parameter, propertyInfo);
                            var orderByExpression = Expression.Lambda(property, parameter);

                            // Apply the sorting based on the specified order (ASC or DESC)
                            iq = sortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase)
                                ? iq.Provider.CreateQuery<T>(
                                    Expression.Call(
                                        typeof(Queryable),
                                        "OrderBy",
                                        new Type[] { typeof(T), property.Type },
                                        iq.Expression,
                                        orderByExpression))
                                : iq.Provider.CreateQuery<T>(
                                    Expression.Call(
                                        typeof(Queryable),
                                        "OrderByDescending",
                                        new Type[] { typeof(T), property.Type },
                                        iq.Expression,
                                        orderByExpression));
                        }
                    }
                }

                // Pagination: If 'page' and 'limit' are provided, apply pagination
                if (filter.NPage != null && filter.NPage.Value > 0 && filter.Limit != null && filter.Limit.Value > 0)
                {
                    // Calculate the number of records to skip based on the page and limit
                    int skipCount = (filter.NPage.Value - 1) * filter.Limit.Value;

                    // Apply skip and take for pagination
                    iq = iq.Skip(skipCount).Take(filter.Limit.Value);
                }
                else if (filter.Limit != null && filter.Limit.Value > 0)
                {
                    // If only limit is specified, take the specified number of items
                    iq = iq.Take(filter.Limit.Value);
                }
                else
                {
                    // Default to a limit of 10 if no limit is specified
                    iq = iq.Take(10);
                }
            }
            catch (Exception ex)
            {
                // Log or handle any exceptions that occur during filtering, sorting, or pagination
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Applies filters, ignoring specific variables, to the given IQueryable collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IQueryable collection.</typeparam>
        /// <param name="iq">The IQueryable collection to apply the filters to.</param>
        /// <param name="filter">The filter query containing the filtering criteria.</param>
        /// <param name="varsToIgnore">An array of variable names to ignore during filtering.</param>
        public static void ApplyListFiltersWithoutVars<T>(ref IQueryable<T> iq, FilterQuery filter, string[] varsToIgnore)
        {
            string decodedWhereJson = HttpUtility.UrlDecode(filter.Where);

            if (!string.IsNullOrEmpty(decodedWhereJson))
            {
                var parameter = Expression.Parameter(typeof(T), "o");
                List<Dictionary<string, object>> conditions = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(decodedWhereJson);
                Expression? predicate = null;

                foreach (var condition in conditions)
                {
                    foreach (var kvp in condition)
                    {
                        var propertyInfo = typeof(T).GetProperties()
                            .FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                        if (propertyInfo == null || varsToIgnore.Contains(propertyInfo.Name, StringComparer.OrdinalIgnoreCase))
                            continue;

                        var property = Expression.Property(parameter, propertyInfo);
                        Type propertyType = property.Type;

                        object value = kvp.Value is JsonElement valueElement ? GetValueFromJsonElement(valueElement) : kvp.Value;

                        if (propertyType == typeof(string) && value is string)
                        {
                            // Pesquisa por substrings insensível a maiúsculas/minúsculas
                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                            var propertyToLower = Expression.Call(property, toLowerMethod);
                            var constantToLower = Expression.Constant(value.ToString().ToLower());
                            var contains = Expression.Call(propertyToLower, containsMethod, constantToLower);

                            predicate = predicate == null ? contains : Expression.AndAlso(predicate, contains);
                        }
                        else
                        {
                            // Filtro padrão para outros tipos
                            var constant = Expression.Constant(Convert.ChangeType(value, propertyType));
                            var equality = Expression.Equal(property, constant);
                            predicate = predicate == null ? equality : Expression.AndAlso(predicate, equality);
                        }
                    }
                }

                if (predicate != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
                    iq = iq.Where(lambda);
                }
            }
        }

        /// <summary>
        /// Converts a JsonElement to the appropriate .NET value type.
        /// </summary>
        /// <param name="valueElement">The JsonElement to convert.</param>
        /// <returns>The corresponding .NET value.</returns>
        private static object GetValueFromJsonElement(JsonElement valueElement)
        {
            return valueElement.ValueKind switch
            {
                JsonValueKind.String => Guid.TryParse(valueElement.GetString(), out var guidValue) ? guidValue : valueElement.GetString(),
                JsonValueKind.Number => valueElement.TryGetInt32(out int intValue) ? intValue : valueElement.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => throw new InvalidOperationException("Unsupported JSON value kind")
            };
        }
    }
}
