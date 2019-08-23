namespace Stunts.Emit.Static
{
    internal class StuntTypeName
    {
        int hashCode;

        public StuntTypeName(string @namespace, string name)
        {
            Namespace = @namespace;
            Name = name;
            hashCode = new HashCode().Add(@namespace).Add(name).ToHashCode();
        }

        public string FullName => Namespace + "." + Name;

        public string Name { get; }

        public string Namespace { get; }

        public override string ToString() => FullName;

        public override int GetHashCode() => hashCode;

        public override bool Equals(object obj)
            => obj is StuntTypeName other && other.Namespace == this.Namespace && other.Name == this.Name;
    }
}
