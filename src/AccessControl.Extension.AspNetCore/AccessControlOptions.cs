﻿using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace AccessControls.Extension.AspNetCore
{
    public class AccessControlOptions
    {
        public bool UseAsDefaultPolicy { get; set; }

        public Func<HttpContext, string> AccessKeyResolver { get; set; } = context =>
            context.Request.Headers.TryGetValue("X-Access-Key", out var val) ? val.ToString() : null;

        public Func<HttpContext, Task> DefaultUnauthorizedOperation { get; set; }
    }
}
