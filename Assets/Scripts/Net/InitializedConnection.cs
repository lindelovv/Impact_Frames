using Unity.Entities;

// This component is used to mark connections as initialized to avoid
// them being processed multiple times.
public struct InitializedConnection : IComponentData { }
