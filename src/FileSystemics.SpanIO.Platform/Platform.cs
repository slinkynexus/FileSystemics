using FileSystemics.IO.PlatformHosts.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Selects custom platform hosts for SpanIO APIs via native dispatch rebinding.
/// </summary>
public static class Platform {
    private static readonly IPlatformHost s_actual = NativePlatformHostFacade.Instance;
    private static IPlatformHost? s_currentOverride;

    /// <summary>
    /// The platform host resolved at startup for the current operating system.
    /// </summary>
    public static IPlatformHost Actual => s_actual;

    /// <summary>
    /// The platform host used when a custom backend is active; otherwise matches <see cref="Actual"/>.
    /// </summary>
    public static IPlatformHost Current => s_currentOverride ?? s_actual;

    static Platform() {
        IPlatformHost? registered = PlatformRegistry.TryResolve();
        if (registered is not null) {
            s_currentOverride = registered;
            CustomPlatformBinder.Bind(registered);
        }
    }

    /// <summary>
    /// Registers a named platform host factory for <c>FILESYSTEMICS_PLATFORM</c> resolution.
    /// </summary>
    public static void Register(string id, Func<IPlatformHost> factory) {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factory);
        PlatformRegistry.Register(id, factory);
    }

    /// <summary>
    /// Temporarily replaces the active custom platform host and restores native bindings on dispose.
    /// </summary>
    public static IDisposable Use(IPlatformHost host) {
        ArgumentNullException.ThrowIfNull(host);
        return new Scope(host);
    }

    private sealed class Scope : IDisposable {
        private readonly IPlatformHost _host;
        private bool _disposed;

        internal Scope(IPlatformHost host) {
            _host = host;
            s_currentOverride = host;
            CustomPlatformBinder.Bind(host);
        }

        /// <inheritdoc/>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            CustomPlatformBinder.Unbind();
            if (ReferenceEquals(s_currentOverride, _host)) {
                s_currentOverride = null;
            }
        }
    }
}
