using System;
using Lanchat.Core.Handlers;

namespace Lanchat.Tests.Mock
{
    public class NodeState : INodeState
    {
        public bool Ready { get; } = true;
        public Guid Id { get; } = Guid.NewGuid();
    }
}