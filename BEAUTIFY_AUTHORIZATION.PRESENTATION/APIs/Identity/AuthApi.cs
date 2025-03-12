using System.Security.Claims;
using System.Text.Json;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace BEAUTIFY_AUTHORIZATION.PRESENTATION.APIs.Identity;
using CommandV1 = Command;
using QueryV1 = Query;

public class AuthApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/auth";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group1 = app.NewVersionedApi("Authentication")
            .MapGroup(BaseUrl).HasApiVersion(1);

        group1.MapPost("login_google", LoginGoogleV1);
        group1.MapPost("login_google_test", LoginGoogleTest);
        group1.MapGet("logout_google", LogoutGoogleV1).RequireAuthorization();
        group1.MapPost("login", LoginV1);
        group1.MapPost("register", RegisterV1)
            .WithName("Register")
            .WithSummary("Registers a new user.")
            .WithDescription("After Registration, User will receive a verify code through the email." +
                             " After that use Verify Code with type = 0 for verify account.")
            .WithOpenApi(operation => new(operation)
                {
                    RequestBody = new OpenApiRequestBody()
                    {
                        Content =
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Example = new OpenApiString(JsonSerializer.Serialize(new CommandV1.RegisterCommand(
                                    "dung@gmail.com",
                                    "123456789",
                                    "Dung",
                                    "Cao",
                                    "+84983460123",
                                    new DateOnly(2002, 11, 29),
                                    "Vietnam",
                                    "Hanoi",
                                    "Cau Giay",
                                    "123"
                                )))
                            }
                        }
                    }
                }
            );
        group1.MapPost("refresh_token", RefreshTokenV1);
        group1.MapPost("forgot_password", ForgotPasswordV1);
        group1.MapPost("verify_code", VerifyCodeV1);
        group1.MapPost("change_password", ChangePasswordV1).RequireAuthorization();
        group1.MapGet("logout", LogoutV1).RequireAuthorization();
    }


    private static async Task<IResult> LoginGoogleTest(ISender sender, QueryV1.LoginGoolgeTest login)
    {
        var result = await sender.Send(login);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> LoginGoogleV1(ISender sender, QueryV1.LoginGoogle login)
    {
        var result = await sender.Send(login);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> LogoutGoogleV1(ISender sender)
    {
        var result = await sender.Send(new QueryV1.LogoutGoogle());

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> LoginV1(ISender sender, [FromBody] QueryV1.Login login)
    {
        var result = await sender.Send(login);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> RefreshTokenV1(HttpContext context, ISender sender,
        [FromBody] QueryV1.Token query)
    {
        //var accessToken = await context.GetTokenAsync("access_token");
        var result = await sender.Send(query);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> LogoutV1(ISender sender, HttpContext context, IJwtTokenService jwtTokenService)
    {
        var accessToken = await context.GetTokenAsync("access_token");
        var (claimPrincipal, _) = jwtTokenService.GetPrincipalFromExpiredToken(accessToken!);
        var email = claimPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;

        var result = await sender.Send(new CommandV1.LogoutCommand(email));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> ForgotPasswordV1(ISender sender,
        [FromBody] CommandV1.ForgotPasswordCommand command)
    {
        var result = await sender.Send(command);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> VerifyCodeV1(ISender sender, [FromBody] CommandV1.VerifyCodeCommand command)
    {
        var result = await sender.Send(command);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> ChangePasswordV1(ISender sender, HttpContext context,
        IJwtTokenService jwtTokenService, [FromBody] CommandV1.ChangePasswordCommandBody command)
    {
        var accessToken = await context.GetTokenAsync("access_token");
        var (claimPrincipal, _) = jwtTokenService.GetPrincipalFromExpiredToken(accessToken!);
        var email = claimPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
        var result = await sender.Send(new CommandV1.ChangePasswordCommand(email, command.NewPassword));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> RegisterV1(ISender sender, [FromBody] CommandV1.RegisterCommand command)
    {
        var result = await sender.Send(command);

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}