namespace Orchard.Environment.Shell {
    public interface IOrchardShellHost {
        void BeginRequest(ShellSettings shellSettings);
        void EndRequest(ShellSettings shellSettings);
    }
}