namespace WebApplication1.Models
{
    public class ChangePasswordViewModel
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
