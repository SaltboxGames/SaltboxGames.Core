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
        public static Func<T2> GetPropertyGetter<T1, T2>(T1 target, string propertyName)
        {
            Func<T1, T2> getter = CreatePropertyGetter<T1, T2>(propertyName);
            return () => getter(target);
        }

        public static Func<T1, T2> CreatePropertyGetter<T1, T2>(string propertyName)
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
