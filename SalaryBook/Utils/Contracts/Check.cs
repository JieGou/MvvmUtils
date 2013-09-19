﻿using System;
using System.Linq.Expressions;

namespace Pellared.Utils.Contracts
{
    public static class Check
    {
         public static Argument<T> Argument<T>(Expression<Func<T>> argumentExpression)
         {
             return new Argument<T>(argumentExpression);
         }
    }
}