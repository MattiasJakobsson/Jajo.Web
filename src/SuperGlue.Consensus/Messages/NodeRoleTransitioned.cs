namespace SuperGlue.Consensus.Messages
{
    public class NodeRoleTransitioned
    {
        public NodeRoleTransitioned(NodeRole newRole)
        {
            NewRole = newRole;
        }

        public NodeRole NewRole { get; }
    }
}