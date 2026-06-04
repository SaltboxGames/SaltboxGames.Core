// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Owns a named group of services and manages their lifecycle.
    /// </summary>
    public sealed class ServiceScope : IServiceScope
    {
        private IServiceScope parent;
        IServiceScope IServiceScope.Parent
        {
            get => parent;
            set => parent = value;
        }
        
        private readonly Dictionary<Type, ServiceEntry> servicesByType = new Dictionary<Type, ServiceEntry>();
        private readonly List<ServiceEntry> services = new List<ServiceEntry>();
        private readonly List<ServiceEntry> initializedServices = new List<ServiceEntry>();
        private readonly List<IServiceScope> children = new List<IServiceScope>();
        private readonly AsyncLocal<Stack<ServiceEntry>> initializationStack = new AsyncLocal<Stack<ServiceEntry>>();

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ServiceScopeState State { get; private set; } = ServiceScopeState.Registered;
        
        /// <summary>
        /// Creates a service scope with the provided display name.
        /// </summary>
        /// <param name="name">The display name for the scope.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        public ServiceScope(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public void AttachScope(IServiceScope child)
        {
            EnsureCanAttachScope(child);
            children.Add(child);
            child.Parent = this;
        }

        /// <inheritdoc />
        public void Register<TService>(TService service) 
            where TService : class, IService
        {
            if(service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            EnsureCanRegister();

            ServiceEntry entry = GetOrCreateEntry(service);
            Type implementationType = service.GetType();
            
            RegisterType(typeof(TService), entry);
            
            if(implementationType != typeof(TService))
            {
                RegisterType(implementationType, entry);
            }
            
            RegisterServiceInterfaces(implementationType, entry);
        }

        /// <inheritdoc />
        public void Register(IService service)
        {
            if(service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            EnsureCanRegister();

            ServiceEntry entry = GetOrCreateEntry(service);
            Type implementationType = service.GetType();

            RegisterType(implementationType, entry);
            RegisterServiceInterfaces(implementationType, entry);
        }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            EnsureCanInitialize();
            State = ServiceScopeState.Initializing;

            try
            {
                foreach(ServiceEntry service in services)
                {
                    await EnsureInitializedAsync(service);
                }

                State = ServiceScopeState.Initialized;
            }
            catch
            {
                State = ServiceScopeState.Registered;
                throw;
            }
        }

        /// <inheritdoc />
        public Task StartServices()
        {
            if(State == ServiceScopeState.Started)
            {
                return Task.CompletedTask;
            }

            if(State != ServiceScopeState.Initialized)
            {
                throw new InvalidOperationException($"Service scope '{Name}' cannot start from state '{State}'.");
            }

            foreach(ServiceEntry service in initializedServices)
            {
                service.Instance.StartService();
                service.State = ServiceScopeState.Started;
            }

            State = ServiceScopeState.Started;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopServices()
        {
            if(State == ServiceScopeState.Stopped || State == ServiceScopeState.Shutdown)
            {
                return;
            }

            if(State != ServiceScopeState.Started && State != ServiceScopeState.Initialized)
            {
                throw new InvalidOperationException($"Service scope '{Name}' cannot stop from state '{State}'.");
            }

            State = ServiceScopeState.Stopping;

            for(int i = children.Count - 1; i >= 0; i--)
            {
                IServiceScope child = children[i];
                if(child.State == ServiceScopeState.Started || child.State == ServiceScopeState.Initialized)
                {
                    await child.StopServices();
                }
            }

            for(int i = initializedServices.Count - 1; i >= 0; i--)
            {
                ServiceEntry service = initializedServices[i];
                service.Instance.StopService();

                service.State = ServiceScopeState.Stopped;
            }

            State = ServiceScopeState.Stopped;
        }

        /// <inheritdoc />
        public async Task ShutdownAsync()
        {
            if(State == ServiceScopeState.Shutdown)
            {
                parent?.RemoveChild(this);
                return;
            }

            if(State != ServiceScopeState.Registered &&
               State != ServiceScopeState.Initialized &&
               State != ServiceScopeState.Started &&
               State != ServiceScopeState.Stopped)
            {
                throw new InvalidOperationException($"Service scope '{Name}' cannot shutdown from state '{State}'.");
            }

            for(int i = children.Count - 1; i >= 0; i--)
            {
                await children[i].ShutdownAsync();
            }

            children.Clear();
            
            if(State == ServiceScopeState.Started || State == ServiceScopeState.Initialized)
            {
                await StopServices();
            }

            State = ServiceScopeState.ShuttingDown;

            for(int i = initializedServices.Count - 1; i >= 0; i--)
            {
                ServiceEntry service = initializedServices[i];
                await service.Instance.ShutdownAsync();

                service.State = ServiceScopeState.Shutdown;
            }

            servicesByType.Clear();
            services.Clear();
            initializedServices.Clear();
            State = ServiceScopeState.Shutdown;
            parent?.RemoveChild(this);
        }

        /// <inheritdoc />
        public T GetService<T>() where T : class, IService
        {
            if(TryGetService(out T service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service '{typeof(T).Name}' is not registered for scope '{Name}' or any parent scope.");
        }

        /// <inheritdoc />
        public bool TryGetService<T>(out T service) where T : class, IService
        {
            EnsureCanResolveService();
            if(!TryFindService<T>(out ServiceEntry entry))
            {
                service = null;
                return false;
            }
            
            if(entry.State != ServiceScopeState.Initialized &&
               entry.State != ServiceScopeState.Started &&
               entry.State != ServiceScopeState.Stopping &&
               entry.State != ServiceScopeState.Stopped)
            {
                throw new InvalidOperationException(
                    $"Service '{typeof(T).Name}' in scope '{entry.ScopeName}' is not ready. Current state is '{entry.State}'.");
            }

            service = (T)entry.Instance;
            return true;
        }

        private void RemoveChild(IServiceScope child)
        {
            if(children.Remove(child) && ReferenceEquals(child.Parent, this))
            {
                child.Parent = null;
            }
        }
        
        void IServiceScope.RemoveChild(IServiceScope child)
        {
            RemoveChild(child);
        }
        

        private async Task EnsureInitializedAsync(ServiceEntry service)
        {
            if(service.State == ServiceScopeState.Initialized || service.State == ServiceScopeState.Started)
            {
                return;
            }

            if(service.InitializeTask != null)
            {
                Stack<ServiceEntry> activeStack = service.Scope.initializationStack.Value;
                if(activeStack != null && activeStack.Contains(service))
                {
                    throw new InvalidOperationException(BuildCycleMessage(activeStack, service));
                }

                await service.InitializeTask;
                return;
            }

            Stack<ServiceEntry> stack = initializationStack.Value;
            if(stack == null)
            {
                stack = new Stack<ServiceEntry>();
                initializationStack.Value = stack;
            }

            if(stack.Contains(service))
            {
                throw new InvalidOperationException(BuildCycleMessage(stack, service));
            }

            stack.Push(service);
            service.State = ServiceScopeState.Initializing;
            service.InitializeTask = InitializeServiceAsync(service);

            try
            {
                await service.InitializeTask;
                service.State = ServiceScopeState.Initialized;
                initializedServices.Add(service);
            }
            finally
            {
                stack.Pop();
                if(stack.Count == 0)
                {
                    initializationStack.Value = null;
                }
            }
        }

        private async Task InitializeServiceAsync(ServiceEntry service)
        {
            var initializer = new ServiceInitializer(this);

            try
            {
                await service.Instance.InitializeAsync(initializer);
            }
            finally
            {
                initializer.Invalidate();
            }
        }

        private async Task<T> ResolveServiceForInitializationAsync<T>() where T : class, IService
        {
            EnsureCanResolveService();
            if(!TryFindService<T>(out ServiceEntry service))
            {
                throw new InvalidOperationException($"Service '{typeof(T).Name}' is not registered for scope '{Name}' or any parent scope.");
            }
            
            await service.Scope.EnsureInitializedAsync(service);
            return (T)service.Instance;
        }

        private bool TryFindService<T>(out ServiceEntry service)
            where T : class, IService
        {
            Type serviceType = typeof(T);
            if(servicesByType.TryGetValue(serviceType, out service))
            {
                return true;
            }
        
            if(parent != null)
            {
                return parent.TryFindService<T>(out service);
            }

            service = null;
            return false;
        }
        
        bool IServiceScope.TryFindService<T>(out ServiceEntry service)
        {
            return TryFindService<T>(out service);
        }

        private void RegisterServiceInterfaces(Type implementationType, ServiceEntry entry)
        {
            Type[] interfaces = implementationType.GetInterfaces();
            for(int i = 0; i < interfaces.Length; i++)
            {
                Type interfaceType = interfaces[i];

                if(interfaceType == typeof(IService))
                {
                    continue;
                }

                if(!typeof(IService).IsAssignableFrom(interfaceType))
                {
                    continue;
                }

                RegisterType(interfaceType, entry);
            }
        }
        
        private void RegisterType(Type type, ServiceEntry entry)
        {
            if(servicesByType.TryGetValue(type, out ServiceEntry existingEntry))
            {
                if(existingEntry == entry)
                {
                    return;
                }

                throw new InvalidOperationException($"Service type '{type.Name}' is already registered in scope '{Name}'.");
            }

            entry.Scope = this;
            entry.ScopeName = Name;
            servicesByType.Add(type, entry);
        }

        private ServiceEntry GetOrCreateEntry(IService service)
        {
            foreach(ServiceEntry entry in services)
            {
                if(ReferenceEquals(entry.Instance, service))
                {
                    throw new InvalidOperationException($"Service instance '{service.GetType().Name}' is already registered in scope '{Name}'.");
                }
            }

            var newEntry = new ServiceEntry(service);
            services.Add(newEntry);
            return newEntry;
        }

        private void EnsureCanRegister()
        {
            if(State != ServiceScopeState.Registered)
            {
                throw new InvalidOperationException($"Services can only be registered before scope '{Name}' is initialized.");
            }
        }

        private void EnsureCanInitialize()
        {
            if(State != ServiceScopeState.Registered)
            {
                throw new InvalidOperationException($"Service scope '{Name}' cannot initialize from state '{State}'.");
            }
        }

        private void EnsureCanAttachScope(IServiceScope child)
        {
            if(child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }
            
            if(State == ServiceScopeState.Stopping ||
               State == ServiceScopeState.Stopped ||
               State == ServiceScopeState.ShuttingDown ||
               State == ServiceScopeState.Shutdown)
            {
                throw new InvalidOperationException($"Cannot create a child scope from scope '{Name}' while it is '{State}'.");
            }
            
            if(children.Contains(child))
            {
                throw new InvalidOperationException($"Child scope is already attached to scope '{Name}'.");
            }

            if(child.Parent != null)
            {
                throw new InvalidOperationException($"Child scope is already attached to another parent scope.");
            }

            if(child.State != ServiceScopeState.Registered)
            {
                throw new InvalidOperationException($"Child scope must be registered before it can be attached to scope '{Name}'. Current state is '{child.State}'.");
            }
        }

        private void EnsureCanResolveService()
        {
            if(State == ServiceScopeState.ShuttingDown || State == ServiceScopeState.Shutdown)
            {
                throw new InvalidOperationException($"Service scope '{Name}' cannot resolve services from state '{State}'.");
            }
        }

        private static string BuildCycleMessage(Stack<ServiceEntry> stack, ServiceEntry repeated)
        {
            var names = new List<string>();
            foreach(ServiceEntry entry in stack)
            {
                names.Add(entry.Instance.GetType().Name);
                if(entry == repeated)
                {
                    break;
                }
            }

            names.Reverse();
            names.Add(repeated.Instance.GetType().Name);
            return $"Service initialization cycle detected: {string.Join(" -> ", names)}";
        }

        private sealed class ServiceInitializer : IServiceInitializer
        {
            private readonly ServiceScope scope;
            private bool isActive = true;

            public ServiceInitializer(ServiceScope scope)
            {
                this.scope = scope;
            }

            public Task<T> GetServiceAsync<T>() where T : class, IService
            {
                if(!isActive)
                {
                    throw new InvalidOperationException(
                        $"Service initializer for scope '{scope.Name}' can only be used during InitializeAsync.");
                }

                return scope.ResolveServiceForInitializationAsync<T>();
            }

            public void Invalidate()
            {
                isActive = false;
            }
        }
    }
}
