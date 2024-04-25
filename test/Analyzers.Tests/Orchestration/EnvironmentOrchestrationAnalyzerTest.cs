// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis.Testing;
using Microsoft.DurableTask.Analyzers.Orchestration;

using VerifyCS = Microsoft.DurableTask.Analyzers.Tests.Verifiers.CSharpAnalyzerVerifier<Microsoft.DurableTask.Analyzers.Orchestration.EnvironmentOrchestrationAnalyzer>;

namespace Microsoft.DurableTask.Analyzers.Tests.Orchestration;

public class EnvironmentOrchestrationAnalyzerTest
{
    [Fact]
    public async Task EmptyCodeHasNoDiag()
    {
        string code = @"";

        await VerifyCS.VerifyDurableTaskAnalyzerAsync(code);
    }

    [Fact]
    public async Task Tmp()
    {
        string code = Wrapper.WrapDurableFunctionOrchestration(@"
[Function(""Run"")]
void Method([OrchestrationTrigger] TaskOrchestrationContext context)
{
    {|#0:Environment.GetEnvironmentVariable(""PATH"")|};
    {|#1:Environment.GetEnvironmentVariables()|};
    {|#2:Environment.ExpandEnvironmentVariables(""PATH"")|};
}
");
        string[] methods = [
            "Environment.GetEnvironmentVariable(string)",
            "Environment.GetEnvironmentVariables()",
            "Environment.ExpandEnvironmentVariables(string)",
        ];

        DiagnosticResult[] expected = methods.Select(
            (method, i) => BuildDiagnostic().WithLocation(i).WithArguments("Method", method, "Run")).ToArray();

        //DiagnosticResult[] expected = Enumerable.Range(0, methods.Length).Select(
        //     i => BuildDiagnostic().WithLocation(i).WithArguments("Method", methods[i], "Run")).ToArray();

        await VerifyCS.VerifyDurableTaskAnalyzerAsync(code, expected);
    }

    static DiagnosticResult BuildDiagnostic()
    {
        return VerifyCS.Diagnostic(EnvironmentOrchestrationAnalyzer.DiagnosticId);
    }
}