namespace ReportApi.Models
{
    public class User
    {
        public Guid? Id { get; set; }

        public string? name { get; set; }

        public string? password { get; set; }

        public string? token { get; set; }
    }
}
