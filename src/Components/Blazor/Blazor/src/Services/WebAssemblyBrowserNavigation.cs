// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Routing;
using Interop = Microsoft.AspNetCore.Components.Browser.BrowserUriHelperInterop;

namespace Microsoft.AspNetCore.Blazor.Services
{
    internal sealed class WebAssemblyBrowserNavigation : IBrowserNavigation
    {
        public void EnableNavigationInterception()
        {
            WebAssemblyJSRuntime.Instance.Invoke<object>(
                Interop.EnableNavigationInterception,
                typeof(WebAssemblyUriHelper).Assembly.GetName().Name,
                nameof(WebAssemblyUriHelper.NotifyLocationChanged));
        }
    }
}
