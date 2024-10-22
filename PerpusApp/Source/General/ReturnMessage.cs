using System;
using System.Net;

namespace PerpusApp.Source.General
{
    public class ReturnMessage
    {
        /// <summary>
        /// Error Code
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Error Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Menentukan apakah Error dari system atau bukan
        /// </summary>
        /// <param name="Exception">ex</param>
        public void Error(Exception ex)
        {
            if (ex.InnerException == null)
            {
                Code = -1;
                // Message = HttpStatusCode.InternalServerError.ToString();
                Message = ex.Message;
            }
            else
            {
                Code = 0;
                Message = ex.InnerException.Message;
            }
        }
    }
}