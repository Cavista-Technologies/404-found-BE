using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.ApiResponse
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public bool IsError { get; set; }
        public object Data { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse(bool hasError, int statusCode, string message = default, object data = default)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            IsError = hasError;
        }
    }
}
