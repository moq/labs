namespace Stunts.Tasks
{
    static class IBuildConfigurationExtensions
    {
        public static ProjectReader GetProjectReader(this IBuildConfiguration configuration)
            => ProjectReader.GetProjectReader(configuration);

        public static BuildWorkspace GetWorkspace(this IBuildConfiguration configuration)
            => BuildWorkspace.GetWorkspace(configuration);
    }
}
