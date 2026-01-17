namespace JobPlatform.Application.Common.Models;

public sealed class EmailSettings
{
    public string From { get; set; } = "";
    public string Password { get; set; } = "";
    public string Smtp { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public int DailyLimitPerUser { get; set; } = 20;
}