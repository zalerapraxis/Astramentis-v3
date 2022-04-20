using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Enums
{
    public enum CustomApiStatus
    {
        /// <summary>
        ///     All's well - this should only be used for the status check method
        /// </summary>
        OK,
        /// <summary>
        ///     API failed for unspecified reason
        /// </summary>
        APIFailure,
        /// <summary>
        ///     Sight API is under maintenance
        /// </summary>
        UnderMaintenance,
        /// <summary>
        ///     Lobby servers down, or account is unsubbed/banned
        /// </summary>
        AccessDenied,
        /// <summary>
        ///     Generic SE-side error
        /// </summary>
        ServiceUnavailable,
        /// <summary>
        ///     No results
        /// </summary>
        NoResults,
        /// <summary>
        ///     Characters aren't logged in
        /// </summary>
        NotLoggedIn
    }
}
