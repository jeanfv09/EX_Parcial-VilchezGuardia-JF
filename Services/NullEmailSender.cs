using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EX_Parcial_VilchezGuardia_JF.Services
{
    // Simple no-op email sender used for development/testing when no SMTP is configured.
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Intentionally do nothing. In production, replace with a real email sender.
            return Task.CompletedTask;
        }
    }
}
