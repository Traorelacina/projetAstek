namespace WebApplication1.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        

        public ProfileViewModel()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            
        }
    }
}
