﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell.EditorServices.Hosting;
using Microsoft.PowerShell.EditorServices.Services;
using Microsoft.PowerShell.EditorServices.Services.Extension;
using Microsoft.PowerShell.EditorServices.Services.PowerShell;
using Microsoft.PowerShell.EditorServices.Services.PowerShell.Debugging;
using Microsoft.PowerShell.EditorServices.Services.PowerShell.Host;
using Microsoft.PowerShell.EditorServices.Services.PowerShell.Runspace;
using Microsoft.PowerShell.EditorServices.Services.Template;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Microsoft.PowerShell.EditorServices.Server
{
    internal static class PsesServiceCollectionExtensions
    {
        public static IServiceCollection AddPsesLanguageServices(
            this IServiceCollection collection,
            HostStartupInfo hostStartupInfo)
        {
            return collection
                .AddSingleton<HostStartupInfo>(hostStartupInfo)
                .AddSingleton<WorkspaceService>()
                .AddSingleton<SymbolsService>()
                .AddSingleton<EditorServicesConsolePSHost>()
                .AddSingleton<IRunspaceContext>(
                    (provider) => provider.GetService<EditorServicesConsolePSHost>())
                .AddSingleton<PowerShellExecutionService>(
                    (provider) => provider.GetService<EditorServicesConsolePSHost>().ExecutionService)
                .AddSingleton<ConfigurationService>()
                .AddSingleton<IPowerShellDebugContext>(
                    (provider) => provider.GetService<EditorServicesConsolePSHost>().DebugContext)
                .AddSingleton<TemplateService>()
                .AddSingleton<EditorOperationsService>()
                .AddSingleton<RemoteFileManagerService>()
                .AddSingleton(async (provider) =>
                    {
                        var extensionService = new ExtensionService(
                            provider.GetService<PowerShellExecutionService>(),
                            provider.GetService<OmniSharp.Extensions.LanguageServer.Protocol.Server.ILanguageServerFacade>());
                        await extensionService.InitializeAsync(
                            serviceProvider: provider,
                            editorOperations: provider.GetService<EditorOperationsService>()).ConfigureAwait(false);

                        return extensionService;
                    })
                .AddSingleton<AnalysisService>();
        }

        public static IServiceCollection AddPsesDebugServices(
            this IServiceCollection collection,
            IServiceProvider languageServiceProvider,
            PsesDebugServer psesDebugServer,
            bool useTempSession)
        {
            return collection.AddSingleton(languageServiceProvider.GetService<PowerShellExecutionService>())
                .AddSingleton(languageServiceProvider.GetService<WorkspaceService>())
                .AddSingleton(languageServiceProvider.GetService<RemoteFileManagerService>())
                .AddSingleton<PsesDebugServer>(psesDebugServer)
                .AddSingleton<DebugService>()
                .AddSingleton<BreakpointService>()
                .AddSingleton<DebugStateService>(new DebugStateService
                {
                     OwnsEditorSession = useTempSession
                })
                .AddSingleton<DebugEventHandlerService>();
        }
    }
}
