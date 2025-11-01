using ModularPipelines.Attributes;
using ModularPipelines.Context;

namespace RevitApi.Pipeline.Attributes;

public sealed class SkipIfHasGitHubToken : MandatoryRunConditionAttribute
{
    public override Task<bool> Condition(IPipelineHookContext context)
    {
        var token = context.Environment.EnvironmentVariables.GetEnvironmentVariable("GITHUB_TOKEN");

        return Task.FromResult(string.IsNullOrEmpty(token));
    }
}