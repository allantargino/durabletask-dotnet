﻿//  ----------------------------------------------------------------------------------
//  Copyright Microsoft Corporation
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ----------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static DurableTask.Protobuf.TaskHubClientService;
using P = DurableTask.Protobuf;

namespace DurableTask.Grpc;

public class TaskHubGrpcClient : TaskHubClient, IAsyncDisposable
{
    readonly GrpcChannel workerGrpcChannel;
    readonly TaskHubClientServiceClient workerClient;
    readonly IDataConverter dataConverter;
    readonly ILogger logger;

    bool isDisposed;

    TaskHubGrpcClient(Builder builder)
    {
        this.workerGrpcChannel = GrpcChannel.ForAddress(builder.address);
        this.workerClient = new TaskHubClientServiceClient(this.workerGrpcChannel);
        this.dataConverter = builder.dataConverter;
        this.logger = SdkUtils.GetLogger(builder.loggerFactory);
    }

    public static TaskHubGrpcClient Create() => CreateBuilder().Build();

    public static Builder CreateBuilder() => new();

    public async ValueTask DisposeAsync()
    {
        if (!this.isDisposed)
        {
            await this.workerGrpcChannel.ShutdownAsync();
            this.workerGrpcChannel.Dispose();

            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }
    }

    /// <inheritdoc/>
    public override async Task<string> ScheduleNewOrchestrationInstanceAsync(
        TaskName orchestratorName,
        string? instanceId = null,
        object? input = null,
        DateTimeOffset? startTime = null)
    {
        var request = new P.CreateInstanceRequest
        {
            Name = orchestratorName.Name,
            Version = orchestratorName.Version,
            InstanceId = instanceId ?? Guid.NewGuid().ToString("N"),
            Input = this.dataConverter.Serialize(input),
        };

        if (startTime.HasValue)
        {
            // Convert timestamps to UTC if not already UTC
            request.ScheduledStartTimestamp = Timestamp.FromDateTimeOffset(startTime.Value.ToUniversalTime());
        }

        P.CreateInstanceResponse? result = await this.workerClient.StartInstanceAsync(request);
        return result.InstanceId;
    }

    /// <inheritdoc/>
    public override async Task RaiseEventAsync(string instanceId, string eventName, object? eventPayload)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            throw new ArgumentNullException(nameof(instanceId));
        }

        if (string.IsNullOrEmpty(eventName))
        {
            throw new ArgumentNullException(nameof(eventName));
        }

        P.RaiseEventRequest request = new()
        {
            InstanceId = instanceId,
            Name = eventName,
            Input = this.dataConverter.Serialize(eventPayload),
        };

        await this.workerClient.RaiseEventAsync(request);
    }

    /// <inheritdoc/>
    public override async Task<OrchestrationMetadata> WaitForInstanceStartAsync(
        string instanceId,
        CancellationToken cancellationToken,
        bool getInputsAndOutputs = false)
    {
        P.GetInstanceRequest request = new()
        {
            InstanceId = instanceId,
            GetInputsAndOutputs = getInputsAndOutputs,
        };

        P.GetInstanceResponse response;
        try
        {
            response = await this.workerClient.WaitForInstanceStartAsync(
                request,
                cancellationToken: cancellationToken);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            throw new OperationCanceledException($"The {nameof(WaitForInstanceStartAsync)} operation was canceled.", e, cancellationToken);
        }

        return new OrchestrationMetadata(response, this.dataConverter);
    }

    /// <inheritdoc/>
    public override async Task<OrchestrationMetadata> WaitForInstanceCompletionAsync(
        string instanceId,
        CancellationToken cancellationToken,
        bool getInputsAndOutputs = false)
    {
        P.GetInstanceRequest request = new()
        {
            InstanceId = instanceId,
            GetInputsAndOutputs = getInputsAndOutputs,
        };

        P.GetInstanceResponse response;
        try
        {
            response = await this.workerClient.WaitForInstanceCompletionAsync(
                request,
                cancellationToken: cancellationToken);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            throw new OperationCanceledException($"The {nameof(WaitForInstanceCompletionAsync)} operation was canceled.", e, cancellationToken);
        }

        return new OrchestrationMetadata(response, this.dataConverter);
    }

    public sealed class Builder
    {
        internal ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
        internal string address = "http://127.0.0.1:4001";
        internal IDataConverter dataConverter = SdkUtils.DefaultDataConverter;

        public Builder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            return this;
        }

        public Builder UseAddress(string address)
        {
            this.address = SdkUtils.ValidateAddress(address);
            return this;
        }

        public Builder UseDataConverter(IDataConverter dataConverter)
        {
            this.dataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
            return this;
        }

        public TaskHubGrpcClient Build() => new(this);
    }
}
