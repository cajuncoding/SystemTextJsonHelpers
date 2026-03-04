using SystemTextJsonHelpers;

[TestClass]
public class TestBootstrap
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        // Bootstrapping code that runs once before ANY tests execute.
        SystemTextJsonDefaults.ConfigureRelaxedWebDefaults();
    }
}