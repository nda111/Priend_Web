using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PriendWeb.Interaction
{
    public static class MailSender
    {
        public static EmailAddress From { get; } = new EmailAddress("priendapp@gmail.com", "Priend");

        public static async Task<Response> SendVerificationMailAsync(string apiKey, string to, string name, string hash)
        {
            var client = new SendGridClient(apiKey);
            var message = MailHelper.CreateSingleTemplateEmail(From, new EmailAddress(to),
                templateId: "d-bd57bec8ef924ee4926ad946a2f0c890",
                dynamicTemplateData: new Dictionary<string, string>
                {
                    { "user_name", name },
                    { "uri", $"https://priend.herokuapp.com/account/verification/{hash}"},
                });

            return await client.SendEmailAsync(message);
        }

        public static async Task<Response> SendResetPasswordMailAsync(string apiKey, string to, string name, string hash)
        {
            var client = new SendGridClient(apiKey);
            var message = MailHelper.CreateSingleTemplateEmail(From, new EmailAddress(to),
                templateId: "d-c0d6363214d24d66834198724ea22502",
                dynamicTemplateData: new Dictionary<string, string>
                {
                    { "user_name", name },
                    { "uri", $"https://priend.herokuapp.com/account/reset/{hash}"},
                });

            return await client.SendEmailAsync(message);
        }
    }
}
