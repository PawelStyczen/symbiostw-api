namespace DanceApi.Model;

public class EmailSettings
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
