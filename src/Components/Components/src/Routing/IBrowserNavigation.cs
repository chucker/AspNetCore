// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// Infrastructure to managing browser navigation.
    /// <para>
    /// This type is an internal API that supports Components infrastructure and is not designed
    /// for use by application code.
    /// </para>
    /// </summary>
    public interface IBrowserNavigation
    {
        /// <summary>
        /// Sets up interception of hyperlinks in the browser.
        /// </summary>
        void EnableNavigationInterception();
    }
}
