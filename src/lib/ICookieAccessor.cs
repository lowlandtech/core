namespace LowlandTech.Core;

/// <summary>
/// Minimal cookie read/write abstraction used by frontend services (<c>TokenStore</c>)
/// that must hydrate persistent state across request boundaries. Introduced so the
/// frontend can target MAUI Blazor Hybrid (no <c>HttpContext</c>, no Kestrel) without
/// dragging in <c>Microsoft.AspNetCore.Http.IHttpContextAccessor</c> and the rest of
/// the AspNetCore framework reference.
///
/// <list type="bullet">
///   <item><b>Host (ASP.NET Core SSR)</b> — provides an implementation that wraps
///     <c>IHttpContextAccessor</c> and reads/writes cookies on the current response.</item>
///   <item><b>Host (MAUI Blazor Hybrid)</b> — provides a <c>SecureStorage</c>-backed
///     or in-memory implementation (there are no cookies in a WebView that isn't
///     serving through Kestrel).</item>
///   <item><b>Host (unit / integration tests)</b> — uses
///     <see cref="NullCookieAccessor"/> for the default-no-op behavior.</item>
/// </list>
/// </summary>
public interface ICookieAccessor
{
    /// <summary>Returns the cookie value for <paramref name="name"/>, or null when absent.</summary>
    string? Get(string name);

    /// <summary>Persists <paramref name="value"/> under <paramref name="name"/> with
    /// HttpOnly / SameSite=Lax / Path=/ semantics where the backing transport supports
    /// them. Non-cookie hosts (MAUI) may ignore those flags and just persist the value.</summary>
    void Set(string name, string value);

    /// <summary>Removes the cookie (or stored value) keyed by <paramref name="name"/>.</summary>
    void Delete(string name);
}

/// <summary>
/// No-op accessor — safe default when no host-provided implementation is registered.
/// Frontends still function (in-memory tokens survive a single Blazor circuit), but
/// state won't persist across request boundaries.
/// </summary>
public sealed class NullCookieAccessor : ICookieAccessor
{
    public string? Get(string name) => null;
    public void Set(string name, string value) { }
    public void Delete(string name) { }
}
