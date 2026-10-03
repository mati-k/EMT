namespace EMT.Models
{
    /// <summary>
    /// Potential copied into every newly created branch, editable on the configuration screen.
    /// </summary>
    public class DefaultPotential
    {
        public static DefaultPotential Instance { get; } = new DefaultPotential();

        public GroupNodeModel Potential { get; }

        private DefaultPotential()
        {
            Potential = new GroupNodeModel() { Name = "potential" };
            Potential.Nodes.Add(new ValueNodeModel() { Name = "tag", Value = "AAA", Parent = Potential });
        }
    }
}
