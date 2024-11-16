namespace WebApplication1.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
       


        // Propriété pour le chemin de la photo de profil
        public string ProfilePicture { get; set; }

        public ProfileViewModel()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            ProfilePicture = string.Empty;
        }
    }
}
