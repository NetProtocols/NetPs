﻿namespace NetPs.Webdav
{
    internal interface IResponseParser<out TResponse>
    {
        TResponse Parse(string response, int statusCode, string description);
    }
}
