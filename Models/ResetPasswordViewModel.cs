namespace WebApplication1.Models
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public ResetPasswordViewModel()
        {
            Email = string.Empty;
            Token = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}

