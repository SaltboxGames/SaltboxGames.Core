# Services

The service APIs live under `SaltboxGames.Core.Services`.

The service system manages application-level and subsystem-level objects that need explicit initialization, startup, stopping, and shutdown. Services live in scopes. A scope owns service instances, resolves dependencies, and can be attached to a parent scope so local contexts can use broader application services.

Services are intended to be long-lived systems with shared dependencies, explicit lifecycle phases, or meaningful setup and teardown work.

## Runtime Files

- `Runtime/Services/IService.cs`: service lifecycle contract.
- `Runtime/Services/IServiceInitializer.cs`: short-lived async-only initialization resolver.
- `Runtime/Services/IServiceManager.cs`: synchronous ready-service resolver.
- `Runtime/Services/IServiceScope.cs`: scope registration, hierarchy, and lifecycle contract.
- `Runtime/Services/ServiceManager.cs`: convenience wrapper around a root scope.
- `Runtime/Services/ServiceScope.cs`: concrete scope implementation.
- `Runtime/Services/ServiceScopeState.cs`: lifecycle state enum.

## Quick Start

Use `ServiceManager` when one root scope is enough:

```csharp
using System.Threading.Tasks;
using SaltboxGames.Core.Services;

public interface IAudioService : IService
{
    void PlayClick();
}

public sealed class AudioService : IAudioService
{
    public Task InitializeAsync(IServiceInitializer services)
    {
        return Task.CompletedTask;
    }

    public void StartService() { }

    public void StopService() { }

    public Task ShutdownAsync()
    {
        return Task.CompletedTask;
    }

    public void PlayClick() { }
}

ServiceManager services = new ServiceManager();
services.Register<IAudioService>(new AudioService());

await services.InitializeAsync();
await services.Start();

IAudioService audio = services.GetService<IAudioService>();
audio.PlayClick();

await services.ShutdownAsync();
```

The normal sequence for one scope is:

- Create a `ServiceManager` or `ServiceScope`.
- Register every service owned by that scope.
- Call `InitializeAsync()`.
- Call `Start()` or `StartServices()`.
- Resolve ready services with `GetService<T>()` or `TryGetService<T>()`.
- Call `ShutdownAsync()` when the scope is done.

Child scopes are created separately as contexts appear, such as logged-in user state, loaded scenes, gameplay sessions, or other temporary contexts.

## Service Shape

A service implements `IService`:

- `InitializeAsync(IServiceInitializer services)`: perform async setup and resolve dependencies.
- `StartService()`: synchronously begin runtime behavior after initialization.
- `StopService()`: synchronously stop runtime behavior before shutdown.
- `ShutdownAsync()`: release resources after stopping.

Keep constructors cheap. Store configuration in the constructor, but do async work and dependency resolution in `InitializeAsync`.

Use the initializer only during `InitializeAsync`:

```csharp
public sealed class SessionService : IService
{
    private IClockService clock;
    private IAnalyticsService analytics;

    public async Task InitializeAsync(IServiceInitializer services)
    {
        clock = await services.GetServiceAsync<IClockService>();
        analytics = await services.GetServiceAsync<IAnalyticsService>();
    }

    public void StartService()
    {
        analytics.Track("session_started");
    }

    public void StopService() { }

    public Task ShutdownAsync()
    {
        clock = null;
        analytics = null;
        return Task.CompletedTask;
    }
}
```

`IServiceInitializer` intentionally exposes only `GetServiceAsync<T>()`. This prevents initialization code from synchronously resolving a dependency that may not be ready yet. Do not store the initializer; the scope invalidates it after the current `InitializeAsync` call returns.

## Service Candidates

Good service candidates usually have at least one of these properties:

- They are shared by multiple systems through an interface.
- They own resources that need explicit startup or shutdown.
- They have dependencies that must be initialized before runtime use.
- They represent an application, scene, gameplay mode, network, analytics, save, or platform context.
- They need to be replaced in child scopes for tests, tools, scenes, or feature-specific behavior.

Poor service candidates are usually one of these:

- Data objects with no lifecycle.
- Utility classes with only pure functions.
- Short-lived objects created many times.
- Classes that only need constructor injection and never need `InitializeAsync`, `StartService`, `StopService`, or `ShutdownAsync`.
- Unity-owned `MonoBehaviour` registered directly. Use the companion `SaltboxGames.Unity` package for `MonoServices` when Unity's lifecycle should be considered.

## ServiceScope And ServiceManager

`ServiceScope` owns a named set of services and controls their lifecycle.

Key members:

- `Name`: diagnostic scope name used in errors.
- `State`: current scope lifecycle state.
- `Register<TService>(TService service)`: register by declared type, implementation type, and implemented service interfaces.
- `Register(IService service)`: register by implementation type and implemented service interfaces.
- `AttachScope(IServiceScope child)`: attach a child scope for parent fallback lookup.
- `InitializeAsync()`: initialize directly registered services.
- `StartServices()`: start directly registered services.
- `StopServices()`: stop child scopes, then directly registered services.
- `ShutdownAsync()`: shut down child scopes, then directly registered services.
- `GetService<T>()`, `TryGetService<T>()`: resolve ready services.

`ServiceManager` wraps one root `ServiceScope` and exposes equivalent root-scope operations with shorter names: `Start()` and `Stop()` instead of `StartServices()` and `StopServices()`.

Use `ServiceManager` for a simple application root. Use `ServiceScope` directly when you need explicit hierarchy control.

## Registration

Registration is scope-local. A parent and child can register different implementations for the same interface. Lookup from the child returns the child implementation first; lookup from the parent returns the parent implementation.

Within one scope:

- The same service instance can only be registered once.
- Two different service instances cannot register the same concrete service type.
- Two different service instances cannot register the same service interface.
- The base `IService` interface itself is not registered as a lookup key.

One service instance can implement multiple service interfaces. The instance is exposed through each implemented `IService` interface, but lifecycle methods are called once for the instance.

```csharp
public interface IAudioService : IService { }
public interface IMusicService : IService { }

public sealed class AudioService : IAudioService, IMusicService
{
    public Task InitializeAsync(IServiceInitializer services) => Task.CompletedTask;
    public void StartService() { }
    public void StopService() { }
    public Task ShutdownAsync() => Task.CompletedTask;
}

var scope = new ServiceScope("Audio");
scope.Register(new AudioService());
```

You may prefer registering by the public service interface:

```csharp
scope.Register<IInventoryService>(new InventoryService());
```
The implementation type is still registered automatically, so tests and diagnostics can still resolve `InventoryService` when needed.

## Scope Hierarchy

A child scope can resolve services from its parent. This is useful for local contexts that depend on broader application services.

```csharp
ServiceScope app = new ServiceScope("App");
app.Register<IPlatformService>(new PlatformService());
app.Register<ISaveService>(new SaveService());

await app.InitializeAsync();
await app.StartServices();

ServiceScope scene = new ServiceScope("Scene:MainMenu");
app.AttachScope(scene);
scene.Register<IMenuService>(new MenuService());

await scene.InitializeAsync();
await scene.StartServices();

await scene.ShutdownAsync();
```

Attachment rules:

- The child must still be `Registered`.
- The child cannot already have a parent.
- The parent cannot be `Stopping`, `Stopped`, `ShuttingDown`, or `Shutdown`.
- A child can be attached after the parent is initialized or started.

Parent lifecycle does not automatically initialize or start children:

- Parent `InitializeAsync()` does not initialize children.
- Parent `StartServices()` does not start children.
- Parent `StopServices()` stops initialized or started children first, then parent services.
- Parent `ShutdownAsync()` shuts children down before stopping or shutting down parent services.

This makes parent scopes suitable for long-lived app services while child scopes may come and go.

## Lookup

Lookup searches the current scope first, then each parent scope.

Use synchronous lookup only for ready services:

- `GetService<T>()`: returns a ready service or throws if none is registered.
- `TryGetService<T>(out T service)`: returns false only when no matching service is registered.

If a matching service is registered but is still `Registered` or `Initializing`, both lookup methods throw. That is a lifecycle error, not a missing optional dependency.

Synchronous lookup is valid when the resolved service is in one of these states:

- `Initialized`
- `Started`
- `Stopping`
- `Stopped`

Scope lookup fails when the resolving scope is `ShuttingDown` or `Shutdown`.

Async lookup is only available through `IServiceInitializer.GetServiceAsync<T>()` during `InitializeAsync`. It is not exposed by `ServiceManager` or `ServiceScope`.

Child scopes can override parent services:

```csharp
app.Register<IAudioService>(new AppAudioService());
gameplay.Register<IAudioService>(new GameplayAudioService());

IAudioService localAudio = gameplay.GetService<IAudioService>(); // GameplayAudioService
IAudioService appAudio = app.GetService<IAudioService>();         // AppAudioService
```

## Lifecycle Order

A scope moves through these states:

- `Registered`: services can be registered and child scopes can be attached.
- `Initializing`: services are being initialized.
- `Initialized`: services are initialized and ready for synchronous lookup.
- `Started`: services have started.
- `Stopping`: services are stopping.
- `Stopped`: services are stopped but not shut down.
- `ShuttingDown`: services are shutting down.
- `Shutdown`: services have shut down and lookup is no longer allowed.

Initialization begins in registration order. If a service awaits a dependency through `IServiceInitializer.GetServiceAsync<T>()`, that dependency is initialized immediately if needed. The scope records the order services actually finish initialization.

Start uses actual initialization order. Stop and shutdown use the reverse of actual initialization order.

With dependency-first registration:

```csharp
scope.Register<IConfigService>(new ConfigService());
scope.Register<IAssetService>(new AssetService());
scope.Register<IGameplayService>(new GameplayService());
```

The order is:

- Initialize/start `ConfigService`, then `AssetService`, then `GameplayService`.
- Stop/shut down `GameplayService`, then `AssetService`, then `ConfigService`.

If a later-registered dependency is initialized early:

```csharp
scope.Register<IGameplayService>(new GameplayService());
scope.Register<IAssetService>(new AssetService());
```

If `GameplayService.InitializeAsync` awaits `IAssetService`, the actual order is:

- Initialize/start `AssetService`, then `GameplayService`.
- Stop/shut down `GameplayService`, then `AssetService`.

Shutdown only calls `ShutdownAsync()` on services that successfully initialized. A scope can be shut down without being initialized; registered-but-never-initialized services are discarded without lifecycle calls.

If `ShutdownAsync()` is called on a parent scope:
- child scopes shut down
- parent services are stopped
- parent services are shut down

## Dependency Initialization

Use `IServiceInitializer.GetServiceAsync<T>()` for dependencies needed during initialization:

```csharp
public sealed class MusicService : IService
{
    private IAudioService audio;

    public async Task InitializeAsync(IServiceInitializer services)
    {
        audio = await services.GetServiceAsync<IAudioService>();
    }

    public void StartService() { }
    public void StopService() { }
    public Task ShutdownAsync() => Task.CompletedTask;
}
```

Cycles are detected. If `A.InitializeAsync()` awaits `B`, and `B.InitializeAsync()` awaits `A`, initialization throws an `InvalidOperationException` with the detected cycle.

To avoid cycles:

- Keep dependency direction one-way.
- Move shared state into a third service.
- Use events or callbacks after `StartService` instead of resolving both services during `InitializeAsync`.
- Split large services that both own lifecycle and depend on each other.

## Startup And Teardown Pattern

Keep lifecycle ownership in one place so startup and teardown stay paired:

```csharp
public sealed class GameServicesHost
{
    private readonly ServiceManager services = new ServiceManager();

    public async Task StartAsync()
    {
        services.Register<IConfigService>(new ConfigService());
        services.Register<IAudioService>(new AudioService());
        services.Register<ISaveService>(new SaveService());

        await services.InitializeAsync();
        await services.Start();
    }

    public async Task StopAsync()
    {
        await services.ShutdownAsync();
    }
}
```

If startup can fail, clean up through the same owner:

```csharp
public async Task StartAsync()
{
    try
    {
        await services.InitializeAsync();
        await services.Start();
    }
    catch
    {
        await services.ShutdownAsync();
        throw;
    }
}
```

If InitializeAsync() fails, the scope returns to Registered.
Services that successfully initialized before the failure may still
hold resources, so the owning application should usually call
ShutdownAsync() before retrying or discarding the scope.

Calling `Stop()` or `StopServices()` separately is useful when runtime activity should stop while services remain resolvable for final reads or flushes. Calling `ShutdownAsync()` directly is enough for normal teardown though.

## Testing Pattern

Services are straightforward to test when callers depend on interfaces:

```csharp
ServiceScope test = new ServiceScope("Test");
test.Register(new FakeClockService());
test.Register(new FakeInventoryService());

await test.InitializeAsync();
await test.StartServices();

IInventoryService inventory = test.GetService<IInventoryService>();

await test.ShutdownAsync();
```

For integration tests, create a parent scope with shared services and a child scope for the system under test:

```csharp
ServiceScope app = new ServiceScope("TestApp");
app.Register(new FakeClockService());

ServiceScope feature = new ServiceScope("InventoryFeature");
app.AttachScope(feature);
feature.Register(new InventoryService());
```

The feature scope can resolve `IClockService` from the parent while each test controls feature-local services independently.

## Common Errors

`Services can only be registered before scope 'Name' is initialized.`

Register all services for a scope before calling `InitializeAsync()`. If a service is only needed for part of the app, create a child scope for that context.

`Service 'Name' is not registered for scope 'Scope' or any parent scope.`

The requested service type was not registered in the current scope or any parent. Check the requested interface and parent attachment.

`Service 'Name' in scope 'Scope' is not ready. Current state is 'Registered'.`

Synchronous lookup happened before the service was initialized. Use `IServiceInitializer.GetServiceAsync<T>()` inside `InitializeAsync`, or wait until the scope has initialized before calling `GetService<T>()`.

`Service initializer for scope 'Name' can only be used during InitializeAsync.`

Runtime code used an `IServiceInitializer` after its `InitializeAsync` call completed. Cache resolved dependencies during initialization instead.

`Service initialization cycle detected: A -> B -> A`

Two or more services depend on each other during initialization. Break the cycle by introducing a third service, moving one side to `StartService`, or replacing direct resolution with an event/callback after startup.

`Service type 'Name' is already registered in scope 'Scope'.`

Two different services in the same scope expose the same concrete type or service interface. Register only one implementation per service type in a scope, or put the replacement in a child scope.

## Best Practices

- Lookup services by interface unless the implementation type is the intended public API.
- Keep constructors cheap and side-effect free.
- Resolve required dependencies in `InitializeAsync` and store them in fields.
- Do not store `IServiceInitializer`.
- Treat `StartService` as the point where runtime behavior begins.
- Treat `StopService` as the point where runtime behavior stops.
- Treat `ShutdownAsync` as resource release, not normal runtime communication.
- Keep dependency direction one-way.
- Shut down child scopes when their context ends.
- Prefer explicit scope names because they appear in lifecycle error messages.
