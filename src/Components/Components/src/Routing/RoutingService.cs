// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// Infrastructure to manage routing for components.
    /// <para>
    /// This type is an internal API that supports Components infrastructure and is not designed
    /// for use by application code.
    /// </para>
    /// </summary>
    public sealed class RoutingService
    {
        private static readonly char[] _queryOrHashStartChar = new[] { '?', '#' };
        private readonly IBrowserNavigation _browserNavigation;
        private bool _initialized;
        private RouteTable _routes;
        private string _baseUri;

        /// <summary>
        /// Initializes a new instance of <see cref="RoutingService" />.
        /// </summary>
        /// <param name="browserNavigation">The <see cref="IBrowserNavigation" /> instance.</param>
        public RoutingService(IBrowserNavigation browserNavigation)
        {
            _browserNavigation = browserNavigation;
        }

        /// <summary>
        /// Initializes <see cref="RoutingService" /> to use <see cref="RouteTable" />.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="baseUri"></param>
        internal void Initialize(RouteTable routes, string baseUri)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("RoutingService cannot be re-initialized.");
            }

            _routes = routes ?? throw new ArgumentNullException(nameof(routes));
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));

            _initialized = true;
            _browserNavigation.EnableNavigationInterception();
        }

        /// <summary>
        /// Routes to a component at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public Type Route(string path)
        {
            var context = GetRouteContext(path);
            _routes.Route(context);

            return context.Handler;
        }

        internal RouteContext GetRouteContext(string path)
        {
            var locationPath = ToBaseRelativePath(_baseUri, path);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);
            var context = new RouteContext(locationPath);
            return context;
        }

        private static string ToBaseRelativePath(string baseUri, string locationAbsolute)
        {
            if (locationAbsolute.StartsWith(baseUri, StringComparison.Ordinal))
            {
                // The absolute URI must be of the form "{baseUri}something" (where
                // baseUri ends with a slash), and from that we return "something"
                return locationAbsolute.Substring(baseUri.Length);
            }
            else if ($"{locationAbsolute}/".Equals(baseUri, StringComparison.Ordinal))
            {
                // Special case: for the base URI "/something/", if you're at
                // "/something" then treat it as if you were at "/something/" (i.e.,
                // with the trailing slash). It's a bit ambiguous because we don't know
                // whether the server would return the same page whether or not the
                // slash is present, but ASP.NET Core at least does by default when
                // using PathBase.
                return string.Empty;
            }

            var message = $"The URI '{locationAbsolute}' is not contained by the base URI '{baseUri}'.";
            throw new ArgumentException(message);
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0
                ? str
                : str.Substring(0, firstIndex);
        }
    }
}
