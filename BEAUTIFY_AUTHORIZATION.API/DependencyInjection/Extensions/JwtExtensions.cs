using System.Text;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.INFRASTRUCTURE.DependencyInjection.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BEAUTIFY_AUTHORIZATION.API.DependencyInjection.Extensions;
public static class JwtExtensions
{
    public static void AddJwtAuthenticationAPI1(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                JwtOption jwtOption = new JwtOption();
                configuration.GetSection(nameof(JwtOption)).Bind(jwtOption);

                /**
                 * Storing the JWT in the AuthenticationProperties allows you to retrieve it from elsewhere within your application.
                 * public async Task<IActionResult> SomeAction()
                    {
                        // using Microsoft.AspNetCore.Authentication;
                        var accessToken = await HttpContext.GetTokenAsync("access_token");
                        // ...
                    }
                 */
                o.SaveToken = true; // Save token into AuthenticationProperties

                var Key = Encoding.UTF8.GetBytes(jwtOption.SecretKey);
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true, // on production make it true
                    ValidateAudience = true, // on production make it true
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                        }

                        return Task.CompletedTask;
                    }
                };

                //o.EventsType = typeof(CustomJwtBearerEvents);
            })
            .AddGoogle(o =>
            {
                o.ClientId = "298458591622-fqjumsnn3u0j0k7l56haslbb2dbvjrjn.apps.googleusercontent.com";
                o.ClientSecret = "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";
                
            });

        services.AddAuthorization(opts =>
        {
            // opts.AddPolicy(RoleNames.Student, policy => policy.RequireRole("0"));
            // opts.AddPolicy(RoleNames.Mentor, policy => policy.RequireRole("1"));
            // opts.AddPolicy(RoleNames.Admin, policy => policy.RequireRole("2"));
        });
        // services.AddScoped<CustomJwtBearerEvents>();
    }
}