namespace lumires.Core.Abstractions.Data;

/// <summary>
///     Marker interface for query classes in vertical slices.
///     Classes implementing this will be automatically registered as Scoped services.
/// </summary>
public interface IDataAccess;


/// <summary>
///     Marker interface for resolver classes in vertical slices.
///     Classes implementing this will be automatically registered as Scoped services.
/// </summary>
public interface IResolver;