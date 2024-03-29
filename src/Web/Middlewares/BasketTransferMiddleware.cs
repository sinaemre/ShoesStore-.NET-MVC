﻿using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Web.Middlewares
{
    public class BasketTransferMiddleware
    {
        private readonly RequestDelegate _next;

        public BasketTransferMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IBasketService basketService)
        {
            if (context.User.Identity.IsAuthenticated && context.Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
            {
                var anonId = context.Request.Cookies[Constants.BASKET_COOKIENAME];
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                await basketService.TransferBasketAsync(anonId, userId);
                context.Response.Cookies.Delete(Constants.BASKET_COOKIENAME);
            }

            await _next(context);
        }
    }
}
