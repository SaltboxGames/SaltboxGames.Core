// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Owns a set of services and coordinates their lifecycle.
    /// </summary>
    public interface IServiceScope : IServiceManager
    {
        /// <summary>
        /// Gets the display name for the scope.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the current lifecycle state of the scope.
        /// </summary>
        public ServiceScopeState State { get; }
        
        /// <summary>
        /// Initializes all services registered directly with this scope.
        /// </summary>
        /// <returns>A task that completes when initialization has finished.</returns>
        public Task InitializeAsync();
        
        /// <summary>
        /// Starts all services registered directly with this scope.
        /// </summary>
        /// <returns>A completed task after all services have been started.</returns>
        public Task StartServices();
        
        /// <summary>
        /// Stops child scopes and registered services in reverse lifecycle order.
        /// </summary>
        /// <returns>A task that completes when all eligible services have stopped.</returns>
        public Task StopServices();
        
        /// <summary>
        /// Shuts down child scopes and registered services in reverse lifecycle order.
        /// </summary>
        /// <returns>A task that completes when shutdown has finished.</returns>
        public Task ShutdownAsync();
        
        /// <summary>
        /// Registers a service instance under its declared type, implementation type, and service interfaces.
        /// </summary>
        /// <typeparam name="TService">The declared service type.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public void Register<TService>(TService service) 
            where TService : class, IService;

        /// <summary>
        /// Registers a service instance under its implementation type and service interfaces.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        public void Register(IService service);

        /// <summary>
        /// Attaches a child scope that can resolve services from this parent scope.
        /// </summary>
        /// <param name="child">The child scope to attach.</param>
        public void AttachScope(IServiceScope child);

        /// <summary>
        /// Gets or sets the parent scope used for service fallback resolution.
        /// </summary>
        internal IServiceScope Parent { get; set; }
        
        /// <summary>
        /// Removes a child scope from this scope.
        /// </summary>
        /// <param name="child">The child scope to remove.</param>
        internal void RemoveChild(IServiceScope child);

        /// <summary>
        /// Attempts to find a registered service entry in this scope or any parent scope.
        /// </summary>
        /// <typeparam name="T">The service type to find.</typeparam>
        /// <param name="service">The matching service entry, or null when no service is registered.</param>
        /// <returns>True when the service entry is found; otherwise, false.</returns>
        internal bool TryFindService<T>(out ServiceEntry service)
            where T : class, IService;
    }
}
