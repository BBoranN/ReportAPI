namespace ReportApi.Models
{
    public class User
    {
        public int? Id { get; set; }

        public string? name { get; set; }

        public string? password { get; set; }

        public string? token { get; set; }

        public string? role { get; set; }
    }
}
