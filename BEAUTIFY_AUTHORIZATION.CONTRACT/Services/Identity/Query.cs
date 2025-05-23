﻿using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using System.Text;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
public static class Query
{
    public record LoginGoogleCommand(string GoogleToken) : IQuery<Response.Authenticated>, ICacheable
    {
        public bool BypassCache => true;

        public string CacheKey { get; }

        public int SlidingExpirationInMinutes => 500;
        public int AbsoluteExpirationInMinutes => 500;
    };

    public record LoginGoolgeTest : IQuery<string>;

    public record LogoutGoogle : IQuery<string>;

    public record Login(string Email, string Password) : IQuery<Response.Authenticated>, ICacheable
    {
        public bool BypassCache => true;

        public string CacheKey
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append($"{nameof(Login)}");
                builder.Append($"-UserAccount:{Email}");
                return builder.ToString();
            }
        }

        public int SlidingExpirationInMinutes => 500;
        public int AbsoluteExpirationInMinutes => 500;
    }

    public record StaffLogin(string Email, string Password) : IQuery<Response.Authenticated>, ICacheable
    {
        public bool BypassCache => true;

        public string CacheKey
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append($"{nameof(StaffLogin)}");
                builder.Append($"-UserAccount:{Email}");
                return builder.ToString();
            }
        }

        public int SlidingExpirationInMinutes => 500;
        public int AbsoluteExpirationInMinutes => 500;
    }

    public record Token(string AccessToken, string RefreshToken) : IQuery<Response.Authenticated>;

    public record LoginForTesting(string Email) : IQuery<string>;
}