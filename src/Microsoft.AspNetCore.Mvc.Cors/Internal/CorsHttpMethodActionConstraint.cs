// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.Cors.Internal
{
    public class CorsHttpMethodActionConstraint : HttpMethodActionConstraint, IActionConstraint
    {
        private readonly string OriginHeader = "Origin";
        private readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
        private readonly string PreflightHttpMethod = "OPTIONS";

        public CorsHttpMethodActionConstraint(HttpMethodActionConstraint constraint)
            : base(constraint.HttpMethods)
        {
        }

        bool IActionConstraint.Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var methods = (ReadOnlyCollection<string>)HttpMethods;
            if (methods.Count == 0)
            {
                return true;
            }

            var request = context.RouteContext.HttpContext.Request;
            var method = request.Method;
            if (request.Headers.ContainsKey(OriginHeader))
            {
                // Update the http method if it is preflight request.
                var accessControlRequestMethod = request.Headers[AccessControlRequestMethod];
                if (string.Equals(
                        request.Method,
                        PreflightHttpMethod,
                        StringComparison.OrdinalIgnoreCase) &&
                    !StringValues.IsNullOrEmpty(accessControlRequestMethod))
                {
                    for (var i = 0; i < methods.Count; i++)
                    {
                        var supportedMethod = methods[i];
                        if (string.Equals(supportedMethod, accessControlRequestMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return base.Accept(context);
        }
    }
}
