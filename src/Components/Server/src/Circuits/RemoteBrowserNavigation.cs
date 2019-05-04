// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Interop = Microsoft.AspNetCore.Components.Browser.BrowserUriHelperInterop;

namespace Microsoft.AspNetCore.Components.Server.Circuits
{
    internal sealed class RemoteBrowserNavigation : IBrowserNavigation
    {
        private bool _invokeNavigationOnJsRuntimeAttach;
        private RemoteJSRuntime _jsRuntime;

        internal void AttachJsRuntime(IJSRuntime jsRuntime)
        {
            Debug.Assert(jsRuntime != null);
            _jsRuntime = (RemoteJSRuntime)jsRuntime;

            if (_invokeNavigationOnJsRuntimeAttach)
            {
                EnableNavigationInterception(jsRuntime);
            }
        }

        public void EnableNavigationInterception()
        {
            _invokeNavigationOnJsRuntimeAttach = true;

            if (_jsRuntime != null && _jsRuntime.ClientProxy.Connected)
            {
                EnableNavigationInterception(_jsRuntime);
            }
        }

        private static void EnableNavigationInterception(IJSRuntime jsRuntime)
        {
            jsRuntime.InvokeAsync<object>(
                Interop.EnableNavigationInterception,
                typeof(RemoteUriHelper).Assembly.GetName().Name,
                nameof(RemoteUriHelper.NotifyLocationChanged));
        }
    }
}
