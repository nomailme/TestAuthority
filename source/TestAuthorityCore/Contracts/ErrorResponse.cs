using System.Collections.Generic;

namespace TestAuthorityCore.Contracts
{
    /// <summary>
    /// Error response contract.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// List of errors.
        /// </summary>
        public List<ErrorModel> Errors { get; set; } = new List<ErrorModel>();
    }
}