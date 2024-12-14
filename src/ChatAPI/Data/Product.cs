public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Rid { get; set; }
    public string Self { get; set; }
    public string Etag { get; set; }
    public string Attachments { get; set; }
    public long Timestamp { get; set; }
    public List<string> Paths { get; set; }
}