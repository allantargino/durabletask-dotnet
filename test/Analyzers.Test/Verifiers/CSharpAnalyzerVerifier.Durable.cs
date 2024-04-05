// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.DurableTask.Analyzers.Test.Verifiers;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyDurableTaskAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test()
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };

        var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Microsoft.Azure.Functions.Worker.FunctionAttribute).Assembly.Location),             // Microsoft.Azure.Functions.Worker.Extensions.Abstractions
                MetadataReference.CreateFromFile(typeof(Microsoft.Azure.Functions.Worker.OrchestrationTriggerAttribute).Assembly.Location), // Microsoft.Azure.Functions.Worker.Extensions.DurableTask
                MetadataReference.CreateFromFile(typeof(Microsoft.DurableTask.TaskOrchestrationContext).Assembly.Location),                 // Microsoft.DurableTask.Abstractions
            };

        test.TestState.AdditionalReferences.AddRange(references);

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(CancellationToken.None);
    }
}
