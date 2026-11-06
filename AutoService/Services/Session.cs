using AutoService.Models;

namespace AutoService.Services;

public static class Session
{
    public static Employee? User { get; set; }
    public static bool IsAdmin => User?.Role == Roles.Admin;
}
