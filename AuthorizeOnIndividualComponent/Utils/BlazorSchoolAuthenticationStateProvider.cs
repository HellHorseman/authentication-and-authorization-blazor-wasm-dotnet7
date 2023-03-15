﻿using AuthorizeOnIndividualComponent.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AuthorizeOnIndividualComponent.Utils;

public class BlazorSchoolAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly BlazorSchoolUserService _blazorSchoolUserService;

    public User? CurrentUser { get; set; } = new();

    public BlazorSchoolAuthenticationStateProvider(BlazorSchoolUserService blazorSchoolUserService)
    {
        AuthenticationStateChanged += OnAuthenticationStateChangedAsync;
        _blazorSchoolUserService = blazorSchoolUserService;
    }

    private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
    {
        var authenticationState = await task;

        if (authenticationState is not null)
        {
            CurrentUser = User.FromClaimsPrincipal(authenticationState.User);
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();
        var user = _blazorSchoolUserService.FetchUserFromBrowser();

        if (user is not null)
        {
            var authenticatedUser = await _blazorSchoolUserService.SendAuthenticateRequestAsync(user.Username, user.Password);
            CurrentUser = authenticatedUser;

            if (authenticatedUser is not null)
            {
                principal = authenticatedUser.ToClaimsPrincipal();
            }
        }

        return new(principal);
    }

    public async Task LoginAsync(string username, string password)
    {
        var principal = new ClaimsPrincipal();
        var user = await _blazorSchoolUserService.SendAuthenticateRequestAsync(username, password);
        CurrentUser = user;

        if (user is not null)
        {
            principal = user.ToClaimsPrincipal();
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void Logout()
    {
        _blazorSchoolUserService.ClearBrowserUserData();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
    }

    public void Dispose() => AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
}
