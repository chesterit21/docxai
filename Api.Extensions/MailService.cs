using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;

namespace Api.Extensions
{
    public class MailServiceBuilder(bool ignoreSemicolon = false)
    {
        bool built = false;

        private string subject, body;
        private bool isHtml = false;

        HashSet<string> to = new();
        HashSet<string> cc = new();
        HashSet<string> bcc = new();
        List<KeyValuePair<string, byte[]>> attachments = new();

        public MailServiceBuilder Subject(string subject)
        {
            this.subject = subject;
            return this;
        }

        public MailServiceBuilder Body(string body, bool isHtml)
        {
            this.body = body;
            this.isHtml = isHtml;
            return this;
        }

        public MailServiceBuilder AddAttachment(string fileName, byte[] fileData)
        {
            attachments.Add(new KeyValuePair<string, byte[]>(fileName, fileData));
            return this;
        }

        public MailServiceBuilder AddRecipient(params string[] email)
        {
            if (email == null || email.Length == 0)
                return this;

            foreach (var mail in email)
            {
                if (mail.Contains(';'))
                {
                    foreach (var mail2 in mail.Split(';'))
                    {
                        to.Add(mail2);
                    }
                }
                else
                    to.Add(mail);
            }

            return this;
        }

        public MailServiceBuilder AddCc(params string[] email)
        {
            if (email == null || email.Length == 0)
                return this;

            if (!ignoreSemicolon)
            {
                foreach (var mail in email)
                {
                    if (string.IsNullOrEmpty(mail))
                        continue;

                    if (mail.Contains(';'))
                    {
                        foreach (var mail2 in mail.Split(';'))
                        {
                            cc.Add(mail2);
                        }
                    }
                    else
                        cc.Add(mail);
                }
            }

            return this;
        }

        public MailServiceBuilder AddBcc(params string[] email)
        {
            if (email == null || email.Length == 0)
                return this;

            if (!ignoreSemicolon)
            {
                foreach (var mail in email)
                {
                    if (mail.Contains(';'))
                    {
                        foreach (var mail2 in mail.Split(';'))
                        {
                            bcc.Add(mail);
                        }
                    }
                    else
                        bcc.Add(mail);
                }
            }

            return this;
        }

        public MailServiceBuilder Build()
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException("Subject was not supplied");

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException("Body was not supplied");

            to.RemoveWhere(x => string.IsNullOrWhiteSpace(x));
            if (to == null || to.Count == 0)
                throw new ArgumentNullException("Recipient was not supplied");

            built = true;
            return this;
        }

        public string Send()
        {
            if (!built)
                throw new ArgumentNullException("Build method was not called");

            JsonElement doc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));

            var mailName = doc.GetElement("MailServer.Name").GetString();
            var mailUser = doc.GetElement("MailServer.User").GetString();
            var mailSmtp = doc.GetElement("MailServer.Smtp").GetString();
            var mailPort = doc.GetElement("MailServer.Port").GetInt16();
            var mailPass = doc.GetElement("MailServer.Password").GetString();

            var message = new MimeMessage();

            foreach (var email in to)
            {
                message.To.Add(new MailboxAddress("", email.Trim()));
            }

            if (cc?.Count > 0)
            {
                foreach (var email in cc)
                {
                    message.Cc.Add(new MailboxAddress("", email.Trim()));
                }
            }

            if (bcc?.Count > 0)
            {
                foreach (var email in bcc)
                {
                    message.Bcc.Add(new MailboxAddress("", email.Trim()));
                }
            }

            message.From.Add(new MailboxAddress(mailName, mailUser));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();

            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
                //message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
            }
            else
            {
                bodyBuilder.TextBody = body;
                //message.Body = new TextPart("plain") { Text = body };
            }

            if (attachments.Count > 0)
            {
                foreach (var attachment in attachments)
                    bodyBuilder.Attachments.Add(attachment.Key, attachment.Value);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                client.Timeout = 10000;
                client.Connect(mailSmtp, mailPort, SecureSocketOptions.Auto);
                client.Authenticate(mailUser, mailPass);
                return client.Send(message);
            }
            catch (Exception ex) 
            {
                return ex.Message;
			}
            finally
            {
                client.Disconnect(true);
            }
        }
    }

}
