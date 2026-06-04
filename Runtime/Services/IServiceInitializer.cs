// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Resolves registered services during service initialization.
    /// </summary>
    public interface IServiceInitializer
    {
        /// <summary>
        /// Gets a service asynchronously, initializing it first if needed.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>A task containing the resolved service instance.</returns>
        public Task<T> GetServiceAsync<T>() where T : class, IService;
    }
}
