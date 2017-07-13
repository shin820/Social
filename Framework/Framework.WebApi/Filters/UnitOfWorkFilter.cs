﻿using Framework.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Framework.WebApi.Filters
{
    public class UnitOfWorkFilter : IActionFilter
    {
        private readonly IUnitOfWorkManager _uowManager;

        public bool AllowMultiple => false;

        public UnitOfWorkFilter(IUnitOfWorkManager uowManager)
        {
            _uowManager = uowManager;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            using (var uow = _uowManager.Begin(new UnitOfWorkOptions { IsTransactional=false}))
            {
                var result = await continuation();
                uow.Complete();
                return result;
            }
        }
    }
}