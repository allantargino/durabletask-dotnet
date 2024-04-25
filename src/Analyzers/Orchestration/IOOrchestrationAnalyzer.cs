// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using static Microsoft.DurableTask.Analyzers.Orchestration.IOOrchestrationAnalyzer;

namespace Microsoft.DurableTask.Analyzers.Orchestration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IOOrchestrationAnalyzer : OrchestrationAnalyzer<IOOrchestrationVisitor>
{
    /// <summary>
    /// Diagnostic ID supported for the analyzer.
    /// </summary>
    public const string DiagnosticId = "DURABLE0005";

    static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "TODO",
        "TODO",
        AnalyzersCategories.Orchestration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public sealed class IOOrchestrationVisitor : MethodProbeOrchestrationVisitor
    {
        ImmutableArray<INamedTypeSymbol> bannedTypes;

        /// <inheritdoc/>
        public override bool Initialize()
        {
            List<INamedTypeSymbol> candidateSymbols = [
                this.KnownTypeSymbols.HttpClient,
                this.KnownTypeSymbols.BlobServiceClient,
                this.KnownTypeSymbols.BlobContainerClient,
                this.KnownTypeSymbols.BlobClient,
                this.KnownTypeSymbols.QueueServiceClient,
                this.KnownTypeSymbols.QueueClient,
                this.KnownTypeSymbols.TableServiceClient,
                this.KnownTypeSymbols.TableClient,
                this.KnownTypeSymbols.CosmosClient,
                this.KnownTypeSymbols.SqlConnection,
                ];

            // filter out null values, since some of them may not be available during compilation:
            this.bannedTypes = candidateSymbols.Where(s => s is not null).ToImmutableArray();

            return this.bannedTypes.Length > 0;
        }

        /// <inheritdoc/>
        protected override void VisitMethod(SemanticModel semanticModel, SyntaxNode methodSyntax, IMethodSymbol methodSymbol, string orchestrationName, Action<Diagnostic> reportDiagnostic)
        {
            IOperation? methodOperation = semanticModel.GetOperation(methodSyntax);
            if (methodOperation is null)
            {
                return;
            }

            foreach (IOperation operation in methodOperation.Descendants())
            {
                if (operation.Type is not null)
                {
                    if (this.bannedTypes.Contains(operation.Type, SymbolEqualityComparer.Default))
                    {
                        string typeName = operation.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                        reportDiagnostic(RoslynExtensions.BuildDiagnostic(Rule, operation, methodSymbol.Name, typeName, orchestrationName));
                    }
                }
            }
        }
    }
}
