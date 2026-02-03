namespace LivasModLoader.Models;

public class ServerPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseBackendBanList { get; set; }
    public string StartingMap { get; set; } = string.Empty;
    public int GamePort { get; set; } = 7777;
    public int RconPort { get; set; } = 9001;
    public int A2SPort { get; set; } = 7071;
    public int PingPort { get; set; } = 3075;
}
