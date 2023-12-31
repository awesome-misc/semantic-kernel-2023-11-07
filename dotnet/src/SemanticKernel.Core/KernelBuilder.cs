﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TemplateEngine;

namespace Microsoft.SemanticKernel;

/// <summary>
/// A builder for Semantic Kernel.
/// </summary>
public sealed class KernelBuilder
{
    private Func<ISemanticTextMemory> _memoryFactory = () => NullMemory.Instance;
    private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
    private Func<IMemoryStore>? _memoryStorageFactory = null;
    private IDelegatingHandlerFactory _httpHandlerFactory = NullHttpHandlerFactory.Instance;
    [Obsolete("Use IPromptTemplateFactory instead. This will be removed in a future release.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private IPromptTemplateEngine? _promptTemplateEngine;
    private readonly AIServiceCollection _aiServices = new();
    private IAIServiceSelector? _serviceSelector;

    /// <summary>
    /// Create a new kernel instance
    /// </summary>
    /// <returns>New kernel instance</returns>
    public static IKernel Create()
    {
        var builder = new KernelBuilder();
        return builder.Build();
    }

    /// <summary>
    /// Build a new kernel instance using the settings passed so far.
    /// </summary>
    /// <returns>Kernel instance</returns>
    public IKernel Build()
    {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS0618 // Type or member is obsolete
        var instance = new Kernel(
            new FunctionCollection(),
            this._aiServices.Build(),
            this._promptTemplateEngine,
            this._memoryFactory.Invoke(),
            this._httpHandlerFactory,
            this._loggerFactory,
            this._serviceSelector
        );
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS8604 // Possible null reference argument.

        // TODO: decouple this from 'UseMemory' kernel extension
        if (this._memoryStorageFactory != null)
        {
#pragma warning disable CS0618 // This will be removed in a future release.
            instance.UseMemory(this._memoryStorageFactory.Invoke());
#pragma warning restore CS0618 // This will be removed in a future release.
        }

        return instance;
    }

    /// <summary>
    /// Add a logger to the kernel to be built.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    /// <returns>Updated kernel builder including the logger.</returns>
    public KernelBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        Verify.NotNull(loggerFactory);
        this._loggerFactory = loggerFactory;
        return this;
    }

    /// <summary>
    /// Add a semantic text memory entity to the kernel to be built.
    /// </summary>
    /// <param name="memory">Semantic text memory entity to add.</param>
    /// <returns>Updated kernel builder including the semantic text memory entity.</returns>
    [Obsolete("Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithMemory(ISemanticTextMemory memory)
    {
        Verify.NotNull(memory);
        this._memoryFactory = () => memory;
        return this;
    }

    /// <summary>
    /// Add a semantic text memory store factory.
    /// </summary>
    /// <param name="factory">The store factory.</param>
    /// <returns>Updated kernel builder including the semantic text memory entity.</returns>
    [Obsolete("Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithMemory<TStore>(Func<ILoggerFactory, TStore> factory) where TStore : ISemanticTextMemory
    {
        Verify.NotNull(factory);
        this._memoryFactory = () => factory(this._loggerFactory);
        return this;
    }

    /// <summary>
    /// Add memory storage to the kernel to be built.
    /// </summary>
    /// <param name="storage">Storage to add.</param>
    /// <returns>Updated kernel builder including the memory storage.</returns>
    [Obsolete("Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithMemoryStorage(IMemoryStore storage)
    {
        Verify.NotNull(storage);
        this._memoryStorageFactory = () => storage;
        return this;
    }

    /// <summary>
    /// Add memory storage factory to the kernel.
    /// </summary>
    /// <param name="factory">The storage factory.</param>
    /// <returns>Updated kernel builder including the memory storage.</returns>
    [Obsolete("Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithMemoryStorage<TStore>(Func<ILoggerFactory, TStore> factory) where TStore : IMemoryStore
    {
        Verify.NotNull(factory);
        this._memoryStorageFactory = () => factory(this._loggerFactory);
        return this;
    }

    /// <summary>
    /// Add memory storage factory to the kernel.
    /// </summary>
    /// <param name="factory">The storage factory.</param>
    /// <returns>Updated kernel builder including the memory storage.</returns>
    [Obsolete("Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithMemoryStorage<TStore>(Func<ILoggerFactory, IDelegatingHandlerFactory, TStore> factory) where TStore : IMemoryStore
    {
        Verify.NotNull(factory);
        this._memoryStorageFactory = () => factory(this._loggerFactory, this._httpHandlerFactory);
        return this;
    }

    /// <summary>
    /// Add prompt template engine to the kernel to be built.
    /// </summary>
    /// <param name="promptTemplateEngine">Prompt template engine to add.</param>
    /// <returns>Updated kernel builder including the prompt template engine.</returns>
    [Obsolete("Use IPromptTemplateFactory instead. This will be removed in a future release.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public KernelBuilder WithPromptTemplateEngine(IPromptTemplateEngine promptTemplateEngine)
    {
        Verify.NotNull(promptTemplateEngine);
        this._promptTemplateEngine = promptTemplateEngine;
        return this;
    }

    /// <summary>
    /// Add a http handler factory to the kernel to be built.
    /// </summary>
    /// <param name="httpHandlerFactory">Http handler factory to add.</param>
    /// <returns>Updated kernel builder including the http handler factory.</returns>
    public KernelBuilder WithHttpHandlerFactory(IDelegatingHandlerFactory httpHandlerFactory)
    {
        Verify.NotNull(httpHandlerFactory);
        this._httpHandlerFactory = httpHandlerFactory;
        return this;
    }

    /// <summary>
    /// Add a retry handler factory to the kernel to be built.
    /// </summary>
    /// <param name="httpHandlerFactory">Retry handler factory to add.</param>
    /// <returns>Updated kernel builder including the retry handler factory.</returns>
    [Obsolete("This method is deprecated, use WithHttpHandlerFactory instead")]
    public KernelBuilder WithRetryHandlerFactory(IDelegatingHandlerFactory httpHandlerFactory)
    {
        return this.WithHttpHandlerFactory(httpHandlerFactory);
    }

    /// <summary>
    /// Adds a <typeparamref name="TService"/> instance to the services collection
    /// </summary>
    /// <param name="instance">The <typeparamref name="TService"/> instance.</param>
    public KernelBuilder WithDefaultAIService<TService>(TService instance) where TService : IAIService
    {
        this._aiServices.SetService<TService>(instance);
        return this;
    }

    /// <summary>
    /// Adds a <typeparamref name="TService"/> factory method to the services collection
    /// </summary>
    /// <param name="factory">The factory method that creates the AI service instances of type <typeparamref name="TService"/>.</param>
    public KernelBuilder WithDefaultAIService<TService>(Func<ILoggerFactory, TService> factory) where TService : IAIService
    {
        this._aiServices.SetService<TService>(() => factory(this._loggerFactory));
        return this;
    }

    /// <summary>
    /// Adds a <typeparamref name="TService"/> instance to the services collection
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <param name="instance">The <typeparamref name="TService"/> instance.</param>
    /// <param name="setAsDefault">Optional: set as the default AI service for type <typeparamref name="TService"/></param>
    public KernelBuilder WithAIService<TService>(
        string? serviceId,
        TService instance,
        bool setAsDefault = false) where TService : IAIService
    {
        this._aiServices.SetService<TService>(serviceId, instance, setAsDefault);
        return this;
    }

    /// <summary>
    /// Adds a <typeparamref name="TService"/> factory method to the services collection
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <param name="factory">The factory method that creates the AI service instances of type <typeparamref name="TService"/>.</param>
    /// <param name="setAsDefault">Optional: set as the default AI service for type <typeparamref name="TService"/></param>
    public KernelBuilder WithAIService<TService>(
        string? serviceId,
        Func<ILoggerFactory, TService> factory,
        bool setAsDefault = false) where TService : IAIService
    {
        this._aiServices.SetService<TService>(serviceId, () => factory(this._loggerFactory), setAsDefault);
        return this;
    }

    /// <summary>
    /// Adds a <typeparamref name="TService"/> factory method to the services collection
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <param name="factory">The factory method that creates the AI service instances of type <typeparamref name="TService"/>.</param>
    /// <param name="setAsDefault">Optional: set as the default AI service for type <typeparamref name="TService"/></param>
    public KernelBuilder WithAIService<TService>(
        string? serviceId,
        Func<ILoggerFactory, IDelegatingHandlerFactory, TService> factory,
        bool setAsDefault = false) where TService : IAIService
    {
        this._aiServices.SetService<TService>(serviceId, () => factory(this._loggerFactory, this._httpHandlerFactory), setAsDefault);
        return this;
    }

    /// <summary>
    /// Adds a <cref name="IAIServiceSelector"/> to the builder
    /// </summary>
    public KernelBuilder WithAIServiceSelector(IAIServiceSelector serviceSelector)
    {
        this._serviceSelector = serviceSelector;
        return this;
    }
}
