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
        [ThreadStatic]
        private static Dictionary<(Type targetType, string fieldName), Delegate> _setterCache;
        [ThreadStatic]
        private static Dictionary<(Type targetType, string fieldName), Delegate> _getterCache;
        
        public static Action<T2> GetFieldSetter<T1, T2>(T1 target, string fieldName)
        {
            Action<T1, T2> setter = CreateFieldSetter<T1, T2>(fieldName);
            return (value) => setter(target, value);
        }
        
        public static Func<T2> GetFieldGetter<T1, T2>(T1 target, string fieldName)
        {
            Func<T1, T2> getter = CreateFieldGetter<T1, T2>(fieldName);
            return () => getter(target);
        }
        
        public static Action<T1, T2> CreateFieldSetter<T1, T2>(string fieldName)
        {
            _setterCache ??= new Dictionary<(Type, string), Delegate>();

            (Type, string fieldName) key = (typeof(T1), fieldName);
            if (_setterCache.TryGetValue(key, out Delegate existing))
            {
                return (Action<T1, T2>)existing;
            }

            Action<T1, T2> setter = CreateFieldSetter<T1, T2>(fieldName);
            _setterCache[key] = setter;
            return setter;
        }

        private static Action<T1, T2> CreateFieldSetter_Internal<T1, T2>(string fieldName)
        {
            FieldInfo fieldInfo = typeof(T1).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(T1)}'.");
            }

            ParameterExpression targetExp = Expression.Parameter(typeof(T1), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(T2), "value");

            MemberExpression fieldExp = Expression.Field(targetExp, fieldInfo);
            BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

            return Expression.Lambda<Action<T1, T2>>(assignExp, targetExp, valueExp).Compile();
        }
        
        public static Func<T1, T2> CreateFieldGetter<T1, T2>(string fieldName)
        {
            _getterCache ??= new Dictionary<(Type, string), Delegate>();

            (Type, string fieldName) key = (typeof(T1), fieldName);
            if (_getterCache.TryGetValue(key, out Delegate existing))
            {
                return (Func<T1, T2>)existing;
            }

            Func<T1, T2> getter = CreateFieldGetter_Internal<T1, T2>(fieldName);
            _getterCache[key] = getter;
            return getter;
        }
        
        private static Func<T1, T2> CreateFieldGetter_Internal<T1, T2>(string fieldName)
        {
            FieldInfo fieldInfo = typeof(T1).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(T1)}'.");
            }

            ParameterExpression targetExp = Expression.Parameter(typeof(T1), "target");
            MemberExpression fieldExp = Expression.Field(targetExp, fieldInfo);

            return Expression.Lambda<Func<T1, T2>>(fieldExp, targetExp).Compile();
        }
    }
}
