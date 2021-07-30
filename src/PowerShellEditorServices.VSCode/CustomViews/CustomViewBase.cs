// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.PowerShell.EditorServices.Extensions.Services;

namespace Microsoft.PowerShell.EditorServices.VSCode.CustomViews
{
    internal abstract class CustomViewBase : ICustomView
    {
        protected ILanguageServerService languageServer;

        public Guid Id { get; private set; }

        public string Title { get; private set; }

        protected CustomViewType ViewType { get; private set; }

        public CustomViewBase(
            string viewTitle,
            CustomViewType viewType,
            ILanguageServerService languageServer)
        {
            this.Id = Guid.NewGuid();
            this.Title = viewTitle;
            this.ViewType = viewType;
            this.languageServer = languageServer;
        }

        internal async Task CreateAsync()
        {
            await languageServer.SendRequestAsync(
                NewCustomViewRequest.Method,
                new NewCustomViewRequest
                {
                    Id = this.Id,
                    Title = this.Title,
                    ViewType = this.ViewType,
                }
            ).ConfigureAwait(false);
        }

        public async Task ShowAsync(ViewColumn viewColumn)
        {
            await languageServer.SendRequestAsync(
                ShowCustomViewRequest.Method,
                new ShowCustomViewRequest
                {
                    Id = this.Id,
                    ViewColumn = viewColumn
                }
            ).ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            await languageServer.SendRequestAsync(
                CloseCustomViewRequest.Method,
                new CloseCustomViewRequest
                {
                    Id = this.Id,
                }
            ).ConfigureAwait(false);
        }
    }
}
