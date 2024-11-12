namespace WebApplication1.Models
{
    public class DashboardViewModel
    {
        public required ProfileViewModel Profile { get; set; }
        public List<Article> Articles { get; set; }

  
    }
}
