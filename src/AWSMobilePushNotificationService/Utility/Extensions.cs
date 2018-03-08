﻿using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace AWSMobilePushNotificationService.Utility
{
    internal static class Extensions
    {
        internal static dynamic ToDynamic<T>(this T obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                expando.Add(propertyInfo.Name, currentValue);
            }
            return expando as ExpandoObject;
        }

    }

}
