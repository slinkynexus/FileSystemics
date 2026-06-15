namespace FileSystemics.IO.PlatformHosts.Internal;

internal static class PlatformRegistry {
    private static readonly Dictionary<string, Func<IPlatformHost>> s_hosts = new(StringComparer.OrdinalIgnoreCase);

    internal static void Register(string id, Func<IPlatformHost> factory) {
        s_hosts[id] = factory;
    }

    internal static IPlatformHost? TryResolve() {
        string? id = Environment.GetEnvironmentVariable("FILESYSTEMICS_PLATFORM");
        if (id is not null && s_hosts.TryGetValue(id, out Func<IPlatformHost>? factory)) {
            return factory();
        }

        return null;
    }
}
