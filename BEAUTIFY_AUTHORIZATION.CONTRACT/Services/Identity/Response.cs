﻿namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;

public static class Response
{
    public class Authenticated
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
