// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Resolves initialized services from a service scope.
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        /// Gets an initialized service synchronously.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public T GetService<T>() where T : class, IService;
        
        /// <summary>
        /// Attempts to get an initialized service synchronously.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="service">The resolved service instance, or null when no service is registered.</param>
        /// <returns>True when the service is registered; otherwise, false.</returns>
        public bool TryGetService<T>(out T service) where T : class, IService;
    }
}
