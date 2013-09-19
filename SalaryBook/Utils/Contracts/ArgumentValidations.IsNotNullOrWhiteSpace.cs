﻿using System;
using System.Text;

namespace Pellared.Utils.Contracts
{
    public static partial class ArgumentValidations
    {
        private const string IsNotNullOrWhiteSpaceConditionDescription = "'{0}' should be not null or whitespace. Actual: {1}";

        private static bool IsNotNullOrWhiteSpace(string value)
        {
            bool result = !string.IsNullOrWhiteSpace(value);
            return result;
        }

        public static Argument<string> IsNotNullOrWhiteSpace<TException>(this Argument<string> validation, Func<Argument<string>, TException> exceptionDelegate)
            where TException : Exception
        {
            return validation.Is(IsNotNullOrWhiteSpace, exceptionDelegate);
        }

        public static Argument<string> IsNotNullOrWhiteSpace(this Argument<string> validation, string conditionDescription)
        {
            return validation.Is(IsNotNullOrWhiteSpace, conditionDescription);
        }

        public static Argument<string> IsNotNullOrWhiteSpace(this Argument<string> validation)
        {
            return validation.IsNotNullOrWhiteSpace(IsNotNullOrWhiteSpaceConditionDescription);
        }
    }
}