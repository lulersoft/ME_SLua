using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP
{
    /// <summary>
    /// Possible logical states of a HTTTPRequest object.
    /// </summary>
    public enum HTTPRequestStates
    {
        /// <summary>
        /// Initial status of a request
        /// </summary>
        Initial,

        /// <summary>
        /// Waiting in a queue to be processed
        /// </summary>
        Queued,

        /// <summary>
        /// Processing of the request started
        /// </summary>
        Processing,

        /// <summary>
        /// The request finished without problem.
        /// </summary>
        Finished,

        /// <summary>
        /// The request finished with an unexpected error. The request's Exception property may contain more info about the error.
        /// </summary>
        Error,

        /// <summary>
        /// The request aborted by the client.
        /// </summary>
        Aborted,

        /// <summary>
        /// Ceonnecting to the server is timed out.
        /// </summary>
        ConnectionTimedOut,

        /// <summary>
        /// The request didn't finished in the given time.
        /// </summary>
        TimedOut
    }
}