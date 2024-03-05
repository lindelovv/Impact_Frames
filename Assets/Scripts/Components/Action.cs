using System;
using System.Linq.Expressions;
using NUnit.Framework.Internal;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

public enum ActionState
{
    Startup,
    Active,
    Recovery,
}

public struct Action : IComponentData
{
    public ActionState CurrentState;
}
