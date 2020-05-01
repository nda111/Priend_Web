using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PriendWeb.Interaction;

namespace PriendWeb
{
    public class Startup
    {
        private Dictionary<string, IResponse> WebSocketRoutingTable { get; } = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            IResponse[] responses =
            {
                new EchoResponse(),
            };

            WebSocketRoutingTable = new Dictionary<string, IResponse>();
            foreach (var response in responses)
            {
                // Check path duplication
                if (!WebSocketRoutingTable.TryAdd(response.Path, response))
                {
                    var responseType = WebSocketRoutingTable[response.Path].GetType();
                    throw new ArgumentException($"'{response.Path}'는 '{responseType.Name}'에 할당된 경로입니다.");
                }
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (WebSocketRoutingTable.ContainsKey(context.Request.Path))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        IResponse response = WebSocketRoutingTable[context.Request.Path];
                        var conn = new WebSocketConnection(await context.WebSockets.AcceptWebSocketAsync());

                        while (!conn.WebSocket.CloseStatus.HasValue)
                        {
                            await response.Response(context, conn);
                            await conn.ReceiveAsync();
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
