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
    /// Provides a root service scope and convenience methods for managing application services.
    /// </summary>
    public sealed class ServiceManager : IServiceManager
    {
        private readonly ServiceScope _rootScope;

        /// <summary>
        /// Creates a new service manager with a root scope.
        /// </summary>
        /// <param name="rootScopeName">The display name for the root scope.</param>
        public ServiceManager(string rootScopeName = "App")
        {
            _rootScope = new ServiceScope(rootScopeName);
        }

        /// <summary>
        /// Gets the current lifecycle state of the root scope.
        /// </summary>
        public ServiceScopeState State => _rootScope.State;

        /// <summary>
        /// Registers a service instance with the root scope.
        /// </summary>
        /// <typeparam name="TService">The declared service type.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public void Register<TService>(TService service) where TService : class, IService
        {
            _rootScope.Register(service);
        }

        /// <summary>
        /// Initializes all services registered with the root scope.
        /// </summary>
        /// <returns>A task that completes when initialization has finished.</returns>
        public Task InitializeAsync()
        {
            return _rootScope.InitializeAsync();
        }

        /// <summary>
        /// Starts all services registered with the root scope.
        /// </summary>
        /// <returns>A completed task after all services have been started.</returns>
        public Task Start()
        {
            return _rootScope.StartServices();
        }

        /// <summary>
        /// Stops child scopes and root services in reverse lifecycle order.
        /// </summary>
        /// <returns>A task that completes when all eligible services have stopped.</returns>
        public Task Stop()
        {
            return _rootScope.StopServices();
        }

        /// <summary>
        /// Attaches a child scope to the root scope.
        /// </summary>
        /// <param name="child">The child scope to attach.</param>
        public void AttachScope(IServiceScope child)
        {
            _rootScope.AttachScope(child);
        }
        
        /// <summary>
        /// Shuts down child scopes and root services in reverse lifecycle order.
        /// </summary>
        /// <returns>A task that completes when shutdown has finished.</returns>
        public Task ShutdownAsync()
        {
            return _rootScope.ShutdownAsync();
        }

        /// <inheritdoc />
        public T GetService<T>() where T : class, IService
        {
            return _rootScope.GetService<T>();
        }

        /// <inheritdoc />
        public bool TryGetService<T>(out T service) where T : class, IService
        {
            return _rootScope.TryGetService(out service);
        }

    }
}
