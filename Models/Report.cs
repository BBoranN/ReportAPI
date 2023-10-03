namespace ReportApi.Models
{
    public class Report
    {
        public int? Id { get; set; }

        public Guid? UserId { get; set; }
        public string reportTitle { get; set; }
        public string reportDescription { get; set; }

        public double x { get; set; }

        public double y { get; set; }
    }


}
