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
using Npgsql;
using PriendWeb.Interaction.Membership;
using PriendWeb.Interaction.Membership.Web;
using PriendWeb.Interaction.Data;
using PriendWeb.Interaction.Calendar;
using PriendWeb.Interaction.Home;

namespace PriendWeb
{
    public class Startup
    {
        private Dictionary<string, IResponse> WebSocketRoutingTable { get; } = null;
        private NpgsqlConnectionManager NpgConnections { get; } = null;
        internal static string MailApiKey = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            if (MailApiKey == null)
            {
                MailApiKey = configuration.GetValue<string>("MailApiKey");
            }
            NpgConnections = new NpgsqlConnectionManager(configuration.GetValue<string>("ConnectionString"), 3, true);

            IResponse[] responses =
            {
                new EchoResponse(),

                new EvaluationResponse(),
                new LoginResponse(),
                new RegisterResponse(),
                new ResetPasswordResponse(),
                new VerificationResponse(),
                new ResetPasswordWebResponse(),
                new ResetPasswordConfirmResponse(),

                new SpeciesListResponse(),

                new EntityListResponse(),
                new CreateGroupReponse(),
                new JoinGroupResponse(),
                new RegisterAnimalResponse(),
                new EditAnimalResponse(),
                new DeleteAnimalResponse(),
                
                new CommitWeightResponse(),
                new InsertMemoResponse(),
                new UpdateMemoResponse(),
                new DeleteMemoResponse(),
                new MemoListResponse(),
            };

            WebSocketRoutingTable = new Dictionary<string, IResponse>();
            foreach (var response in responses)
            {
                WebSocketRoutingTable.Add(response.Path, response);
            }
        }

        ~Startup()
        {
            NpgConnections.Dispose();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this Wmethod to configure the HTTP request pipeline.
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
                            var sqlConnection = NpgConnections.WaitForConnection();

                            await response.Response(context, conn, sqlConnection);
                            await conn.ReceiveAsync();

                            NpgConnections.TryReturnConnection(sqlConnection);
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
