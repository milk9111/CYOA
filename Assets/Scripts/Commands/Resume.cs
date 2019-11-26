namespace Commands
{
    public class Resume : Command
    {
        private NodeManager _nodeManager;

        void Start()
        {
            _nodeManager = FindObjectOfType<NodeManager>();
        }
        
        public override void Execute(string[] args)
        {
            _nodeManager.LoadCurrentNode();
        }
    }
}