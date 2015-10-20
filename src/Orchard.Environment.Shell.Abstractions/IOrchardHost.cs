namespace Orchard.Environment.Shell {
    public interface IOrchardHost {
        void Initialize();

        ShellContext GetShellContext(ShellSettings shellSettings);

        ShellContext CreateShellContext(ShellSettings settings);
    }
}
