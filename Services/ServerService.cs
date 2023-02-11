using NuGet.ContentModel;
using NuJournalPro.Services.Interfaces;
using Org.BouncyCastle.Asn1.Ocsp;

namespace NuJournalPro.Services
{
    public class ServerService : IServerService
    {
        private readonly IImageService _imageService;

        public ServerService(IImageService imageService)
        {
            _imageService = imageService;
        }

        public string GetErrorMessageString(int code)
        {
            if (code == 400)
            {
                return "Bad request: The server cannot or will not process the request due to an apparent client error.";
            }
            else if (code == 401)
            {
                return "Unauthorized: The request has not been applied because it lacks valid authentication credentials for the target resource.";
            }
            else if (code == 402)
            {
                return "Payment Required: Reserved for future use.";
            }
            else if (code == 403)
            {
                return "Forbidden: The client does not have access rights to the content.";
            }
            else if (code == 404)
            {
                return "Not Found: The server cannot find the requested resource.";
            }
            else if (code == 405)
            {
                return "Method Not Allowed: The request method is known by the server but is not supported by the target resource.";
            }
            else if (code == 406)
            {
                return "Not Acceptable: The server cannot produce a response matching the list of acceptable values defined in the request's proactive content negotiation headers.";
            }
            else if (code == 407)
            {
                return "Proxy Authentication Required: The client must first authenticate itself with the proxy.";

            }
            else if (code == 408)
            {
                return "Request Timeout: The server timed out waiting for the request.";

            }
            else if (code == 409)
            {
                return "Conflict: The request could not be processed because of conflict in the request, such as an edit conflict in the case of multiple updates.";

            }
            else if (code == 410)
            {
                return "Gone: The target resource is no longer available at the origin server and that this condition is likely to be permanent.";
            }
            else if (code == 411)
            {
                return "Length Required: The server rejected the request because the Content-Length header field is not defined and the server requires it.";
            }
            else if (code == 412)
            {
                return "Precondition Failed: The server does not meet one of the preconditions that the requester put on the request.";

            }
            else if (code == 413)
            {
                return "Payload Too Large: The request is larger than the server is willing or able to process.";
            }
            else if (code == 414)
            {
                return "URI Too Long: The URI requested by the client is longer than the server is willing to interpret.";
            }
            else if (code == 415)
            {
                return "Unsupported Media Type: The origin server is refusing to service the request because the payload is in a format not supported by this method on the target resource.";
            }
            else if (code == 416)
            {
                return "Range Not Satisfiable: None of the ranges in the request's Range header field overlap the current extent of the selected resource or that the set of ranges requested has been rejected due to invalid ranges or an excessive request of small or overlapping ranges.";
            }
            else if (code == 417)
            {
                return "Expectation Failed: The expectation given in the request's Expect header field could not be met by at least one of the inbound servers.";
            }
            else if (code == 418)
            {
                return "I'm a teapot: The server refuses the attempt to brew coffee with a teapot.";
            }
            else if (code == 421)
            {
                return "Misdirected Request: The request was directed at a server that is not able to produce a response (for example because of connection reuse).";

            }
            else if (code == 422)
            {
                return "Unprocessable Entity: The request was well-formed but was unable to be followed due to semantic errors.";
            }
            else if (code == 423)
            {
                return "Locked: The resource that is being accessed is locked.";
            }
            else if (code == 424)
            {
                return "Failed Dependency: The request failed due to failure of a previous request.";
            }
            else if (code == 425)
            {
                return "Too Early: The server is unwilling to risk processing a request that might be replayed.";
            }
            else if (code == 426)
            {
                return "Upgrade Required: The server refuses to perform the request using the current protocol but might be willing to do so after the client upgrades to a different protocol.";
            }
            else if (code == 428)
            {
                return "Precondition Required: The origin server requires the request to be conditional.";
            }
            else if (code == 429)
            {
                return "Too Many Requests: The user has sent too many requests in a given amount of time (\"rate limiting\").";
            }
            else if (code == 431)
            {
                return "Request Header Fields Too Large: The server is unwilling to process the request because its header fields are too large.";
            }
            else if (code == 451)
            {
                return "Unavailable For Legal Reasons: The user agent requested a resource that cannot legally be provided, such as a web page censored by a government.";
            }
            else if (code == 500)
            {
                return "Internal Server Error: The server has encountered a situation it does not know how to handle.";
            }
            else if (code == 501)
            {
                return "Not Implemented: The request method is not supported by the server and cannot be handled.";
            }
            else if (code == 502)
            {
                return "Bad Gateway: The server, while working as a gateway to get a response needed to handle the request, got an invalid response.";
            }
            else if (code == 503)
            {
                return "Service Unavailable: The server is not ready to handle the request.";
            }
            else if (code == 504)
            {
                return "Gateway Timeout: The server is acting as a gateway and cannot get a response in time.";
            }
            else if (code == 505)
            {
                return "HTTP Version Not Supported: The HTTP version used in the request is not supported by the server.";
            }
            else if (code == 506)
            {
                return "Variant Also Negotiates: The server has an internal configuration error.";
            }
            else if (code == 507)
            {
                return "Insufficient Storage: The method could not be performed on the resource because the server is unable to store the representation needed to successfully complete the request.";
            }
            else if (code == 508)
            {
                return "Loop Detected: The server detected an infinite loop while processing the request.";
            }
            else if (code == 510)
            {
                return "Not Extended: Further extensions to the request are required for the server to fulfill it.";
            }
            else if (code == 511)
            {
                return "Network Authentication Required: Indicates that the client needs to authenticate to gain network access.";
            }
            else
            {
                return "Sorry, an error has occurred.";
            }
        }

        public string GetServerErrorImage()
        {
            string compressedErrorImageB64 = "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";
            return _imageService.DecompressDecodedImageB64(compressedErrorImageB64);
        }
    }
}