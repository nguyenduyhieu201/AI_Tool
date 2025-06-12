using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.Domain
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public T? Value { get; }
        public bool IsFail => !IsSuccess;

        private Result(bool isSuccess, string? error, T? value)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, null, value);
        }

        public static Result<T> Fail(string error)
        {
            return new Result<T>(false, error, default);
        }
    }

}
