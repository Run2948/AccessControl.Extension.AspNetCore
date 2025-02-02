﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace AccessControls.Extension.AspNetCore
{
    /// <summary>
    /// AccessControlMiddleware
    /// </summary>
    internal sealed class AccessControlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly AccessControlOptions _option;

        /// <summary>
        /// Creates a new instance of <see cref="AccessControlMiddleware"/>
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options"></param>
        /// <param name="logger">The Logger Factory.</param>
        public AccessControlMiddleware(
            RequestDelegate next,
            IOptions<AccessControlOptions> options,
            ILogger<AccessControlMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
            _option = options.Value;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public Task Invoke(HttpContext context)
        {
            var accessKey = _option.AccessKeyResolver?.Invoke(context);

            var accessStrategy = context.RequestServices.GetService<IResourceAccessStrategy>();
            if (accessStrategy.IsCanAccess(accessKey))
            {
                return _next(context);
            }

            _logger.LogDebug($"Request {context.TraceIdentifier} was unauthorized, Request path:{context.Request.Path}");
            context.Response.StatusCode = context.User.Identity.IsAuthenticated ? 403 : 401;

            return _option.DefaultUnauthorizedOperation?.Invoke(context) ?? Task.CompletedTask;
        }
    }
}
