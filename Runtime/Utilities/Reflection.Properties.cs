/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SaltboxGames.Core.Utilities
{
    public static partial class Reflection
    {
        /// <summary>
        /// Creates a cached getter for a property on a given object instance, returning a <see cref="Func{TResult}"/> that retrieves the property value.
        /// </summary>
        /// <typeparam name="T1">The type of the target object.</typeparam>
        /// <typeparam name="T2">The type of the property value.</typeparam>
        /// <param name="target">The instance of the object whose property will be read.</param>
        /// <param name="propertyName">The name of the property to access.</param>
        /// <returns>A <see cref="Func{TResult}"/> that retrieves the specified property value from the given instance.</returns>
        public static Func<T2> GetPropertyGetter<T1, T2>(T1 target, string propertyName)
        {
            Func<T1, T2> getter = GetPropertyGetter<T1, T2>(propertyName);
            return () => getter(target);
        }

        
        /// <summary>
        /// Creates or retrieves a cached compiled getter delegate for the specified property on a type.
        /// </summary>
        /// <typeparam name="T1">The type that contains the property.</typeparam>
        /// <typeparam name="T2">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>A function that retrieves the property value from an instance of <typeparamref name="T1"/>.</returns>
        public static Func<T1, T2> GetPropertyGetter<T1, T2>(string propertyName)
        {
            _getterCache ??= new Dictionary<(Type, string), Delegate>();

            (Type, string propertyName) key = (typeof(T1), propertyName);
            if (_getterCache.TryGetValue(key, out Delegate existing))
            {
                return (Func<T1, T2>)existing;
            }

            Func<T1, T2> getter = CreatePropertyGetter_Internal<T1, T2>(propertyName);
            _getterCache[key] = getter;
            return getter;
        }

        /// <summary>
        /// Builds and compiles an expression tree for getting a property from an object.
        /// This method does not use or update the cache.
        /// </summary>
        /// <typeparam name="T1">The type that contains the property.</typeparam>
        /// <typeparam name="T2">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property to access.</param>
        /// <returns>A compiled delegate that retrieves the property value.</returns>
        /// <exception cref="ArgumentException">Thrown if the property is not found or is not readable.</exception>
        private static Func<T1, T2> CreatePropertyGetter_Internal<T1, T2>(string propertyName)
        {
            PropertyInfo propInfo = typeof(T1).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo == null || !propInfo.CanRead)
            {
                throw new ArgumentException($"Readable property '{propertyName}' not found on type '{typeof(T1)}'.");
            }

            ParameterExpression targetExp = Expression.Parameter(typeof(T1), "target");
            MemberExpression propExp = Expression.Property(targetExp, propInfo);

            return Expression.Lambda<Func<T1, T2>>(propExp, targetExp).Compile();
        }
    }
}
