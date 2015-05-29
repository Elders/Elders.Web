using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Web.Api
{
    public class QueryResult
    {
        public static readonly QueryResult Success = new QueryResult();

        public QueryResult() { }

        public QueryResult(params string[] errors)
        {
            this.Errors = errors;
        }

        public IEnumerable<string> Errors { get; private set; }

        public bool IsSuccess
        {
            get { return Errors == null || !Errors.Any(); }
        }
    }

    public class QueryResult<T> : ResponseResult
    {
        public QueryResult(T result)
        {
            Result = result;
        }

        public QueryResult(T result, params string[] errors)
            : base(errors)
        {
            Result = result;
        }
        public QueryResult(params string[] errors) : base(errors) { }

        public T Result { get; private set; }
    }

    static class QueryResultResultExtensions
    {
        public static ErrorModel ToError(this ResponseResult result)
        {
            if (result == null) throw new ArgumentNullException("result");

            return new ErrorModel
            {
                Errors = result.Errors.ToArray()
            };
        }
    }
}
