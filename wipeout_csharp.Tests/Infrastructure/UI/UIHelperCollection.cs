using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

[CollectionDefinition("UIHelperState", DisableParallelization = true)]
public class UIHelperCollection
{
    // Intentionally empty; serves only to tag tests that share UIHelper static state.
}