// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis;

namespace Microsoft.DurableTask.Analyzers;

/// <summary>
/// Provides a set of well-known types that are used by the analyzers.
/// Inspired by KnownTypeSymbols class in
/// <see href="https://github.com/dotnet/runtime/blob/2a846acb1a92e811427babe3ff3f047f98c5df02/src/libraries/System.Text.Json/gen/Helpers/KnownTypeSymbols.cs">System.Text.Json.SourceGeneration</see> source code.
/// Lazy initialization is used to avoid the the initialization of all types during class construction, since not all symbols are used by all analyzers.
/// </summary>
public sealed class KnownTypeSymbols(Compilation compilation)
{
    readonly Compilation compilation = compilation;

    INamedTypeSymbol? functionOrchestrationAttribute;
    INamedTypeSymbol? functionNameAttribute;
    INamedTypeSymbol? taskOrchestratorInterface;
    INamedTypeSymbol? taskOrchestratorBaseClass;
    INamedTypeSymbol? durableTaskRegistry;
    INamedTypeSymbol? taskOrchestrationContext;
    INamedTypeSymbol? durableClientAttribute;
    INamedTypeSymbol? durableTaskClient;
    INamedTypeSymbol? entityTriggerAttribute;
    INamedTypeSymbol? taskEntityDispatcher;
    INamedTypeSymbol? guid;
    INamedTypeSymbol? thread;
    INamedTypeSymbol? task;
    INamedTypeSymbol? taskT;
    INamedTypeSymbol? taskFactory;
    INamedTypeSymbol? taskContinuationOptions;
    INamedTypeSymbol? taskFactoryT;
    INamedTypeSymbol? cancellationToken;
    INamedTypeSymbol? environment;
    INamedTypeSymbol? httpClient;
    INamedTypeSymbol? blobServiceClient;
    INamedTypeSymbol? blobContainerClient;
    INamedTypeSymbol? blobClient;
    INamedTypeSymbol? queueServiceClient;
    INamedTypeSymbol? queueClient;
    INamedTypeSymbol? tableServiceClient;
    INamedTypeSymbol? tableClient;
    INamedTypeSymbol? cosmosClient;
    INamedTypeSymbol? sqlConnection;

    /// <summary>
    /// Gets an OrchestrationTriggerAttribute type symbol.
    /// </summary>
    public INamedTypeSymbol? FunctionOrchestrationAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.OrchestrationTriggerAttribute", ref this.functionOrchestrationAttribute);

    /// <summary>
    /// Gets a FunctionNameAttribute type symbol.
    /// </summary>
    public INamedTypeSymbol? FunctionNameAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.FunctionAttribute", ref this.functionNameAttribute);

    /// <summary>
    /// Gets an ITaskOrchestrator type symbol.
    /// </summary>
    public INamedTypeSymbol? TaskOrchestratorInterface => this.GetOrResolveFullyQualifiedType("Microsoft.DurableTask.ITaskOrchestrator", ref this.taskOrchestratorInterface);

    /// <summary>
    /// Gets a TaskOrchestrator type symbol.
    /// </summary>
    public INamedTypeSymbol? TaskOrchestratorBaseClass => this.GetOrResolveFullyQualifiedType("Microsoft.DurableTask.TaskOrchestrator`2", ref this.taskOrchestratorBaseClass);

    /// <summary>
    /// Gets a DurableTaskRegistry type symbol.
    /// </summary>
    public INamedTypeSymbol? DurableTaskRegistry => this.GetOrResolveFullyQualifiedType("Microsoft.DurableTask.DurableTaskRegistry", ref this.durableTaskRegistry);

    /// <summary>
    /// Gets a TaskOrchestrationContext type symbol.
    /// </summary>
    public INamedTypeSymbol? TaskOrchestrationContext => this.GetOrResolveFullyQualifiedType("Microsoft.DurableTask.TaskOrchestrationContext", ref this.taskOrchestrationContext);

    /// <summary>
    /// Gets a DurableClientAttribute type symbol.
    /// </summary>
    public INamedTypeSymbol? DurableClientAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.DurableClientAttribute", ref this.durableClientAttribute);

    /// <summary>
    /// Gets a DurableTaskClient type symbol.
    /// </summary>
    public INamedTypeSymbol? DurableTaskClient => this.GetOrResolveFullyQualifiedType("Microsoft.DurableTask.Client.DurableTaskClient", ref this.durableTaskClient);

    /// <summary>
    /// Gets an EntityTriggerAttribute type symbol.
    /// </summary>
    public INamedTypeSymbol? EntityTriggerAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.EntityTriggerAttribute", ref this.entityTriggerAttribute);

    /// <summary>
    /// Gets a TaskEntityDispatcher type symbol.
    /// </summary>
    public INamedTypeSymbol? TaskEntityDispatcher => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.TaskEntityDispatcher", ref this.taskEntityDispatcher);

    /// <summary>
    /// Gets a Guid type symbol.
    /// </summary>
    public INamedTypeSymbol? GuidType => this.GetOrResolveFullyQualifiedType(typeof(Guid).FullName, ref this.guid);

    /// <summary>
    /// Gets a Thread type symbol.
    /// </summary>
    public INamedTypeSymbol? Thread => this.GetOrResolveFullyQualifiedType(typeof(Thread).FullName, ref this.thread);

    /// <summary>
    /// Gets a Task type symbol.
    /// </summary>
    public INamedTypeSymbol? Task => this.GetOrResolveFullyQualifiedType(typeof(Task).FullName, ref this.task);

    /// <summary>
    /// Gets a Task&lt;T&gt; type symbol.
    /// </summary>
    public INamedTypeSymbol? TaskT => this.GetOrResolveFullyQualifiedType(typeof(Task<>).FullName, ref this.taskT);

    public INamedTypeSymbol? TaskFactory => this.GetOrResolveFullyQualifiedType(typeof(TaskFactory).FullName, ref this.taskFactory);

    public INamedTypeSymbol? TaskFactoryT => this.GetOrResolveFullyQualifiedType(typeof(TaskFactory<>).FullName, ref this.taskFactoryT);

    public INamedTypeSymbol? TaskContinuationOptions => this.GetOrResolveFullyQualifiedType(typeof(TaskContinuationOptions).FullName, ref this.taskContinuationOptions);

    public INamedTypeSymbol? CancellationToken => this.GetOrResolveFullyQualifiedType(typeof(CancellationToken).FullName, ref this.cancellationToken);

    public INamedTypeSymbol? Environment => this.GetOrResolveFullyQualifiedType("System.Environment", ref this.environment);

    public INamedTypeSymbol? HttpClient => this.GetOrResolveFullyQualifiedType("System.Net.Http.HttpClient", ref this.httpClient);

    public INamedTypeSymbol? BlobServiceClient => this.GetOrResolveFullyQualifiedType("Azure.Storage.Blobs.BlobServiceClient", ref this.blobServiceClient);

    public INamedTypeSymbol? BlobContainerClient => this.GetOrResolveFullyQualifiedType("Azure.Storage.Blobs.BlobContainerClient", ref this.blobContainerClient);

    public INamedTypeSymbol? BlobClient => this.GetOrResolveFullyQualifiedType("Azure.Storage.Blobs.BlobClient", ref this.blobClient);

    public INamedTypeSymbol? QueueServiceClient => this.GetOrResolveFullyQualifiedType("Azure.Storage.Queues.QueueServiceClient", ref this.queueServiceClient);

    public INamedTypeSymbol? QueueClient => this.GetOrResolveFullyQualifiedType("Azure.Storage.Queues.QueueClient", ref this.queueClient);

    public INamedTypeSymbol? TableServiceClient => this.GetOrResolveFullyQualifiedType("Azure.Data.Tables.TableServiceClient", ref this.tableServiceClient);

    public INamedTypeSymbol? TableClient => this.GetOrResolveFullyQualifiedType("Azure.Data.Tables.TableClient", ref this.tableClient);

    public INamedTypeSymbol? CosmosClient => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Cosmos.CosmosClient", ref this.cosmosClient);

    public INamedTypeSymbol? SqlConnection => this.GetOrResolveFullyQualifiedType("Microsoft.Data.SqlClient.SqlConnection", ref this.sqlConnection);

    INamedTypeSymbol? GetOrResolveFullyQualifiedType(string fullyQualifiedName, ref INamedTypeSymbol? field)
    {
        if (field != null)
        {
            return field;
        }

        return field = this.compilation.GetTypeByMetadataName(fullyQualifiedName);
    }
}
