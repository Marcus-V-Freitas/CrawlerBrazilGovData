using Microsoft.AspNetCore.Builder;

namespace IoC.Middlewares
{
    public static class GeneralMiddlewares
    {
        public static IApplicationBuilder AddMiddlewares(this IApplicationBuilder app)
        {
            //Exceptions
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            //Health Check
            app.UseWebHealthCheck();
            return app;
        }
    }
}