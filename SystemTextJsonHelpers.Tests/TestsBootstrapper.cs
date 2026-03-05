using SystemTextJsonHelpers;

[TestClass]
public class TestsBootstrapper
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        // Bootstrapping code that runs once before ANY tests execute.
        SystemTextJsonDefaults.ConfigureRelaxedWebDefaults();
    }
}