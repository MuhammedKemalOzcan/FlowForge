namespace FlowForge.Application.Dtos
{
    public class ApiKeyCreationResultDto
    {
        public Guid ApiKeyId { get; set; }
        public string Name { get; set; }
        public string PlainTextKey { get; set; }
    }
}