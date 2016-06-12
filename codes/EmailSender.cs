namespace Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public class SmtpEmailSender
    {
        public static int Main(string[] args)
        {
            try
            {
                var mailData = args.Where(x => x.Contains(':')).ToDictionary(x => x.Substring(0, x.IndexOf(':')).ToLower(), x => x.Substring(x.IndexOf(':') + 1));

                SendMail(mailData);

                return 0;
            }
            catch  (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("this.exe to:sb subject:subjectContent [cc:sb body:filePath attach:file1;file2... image:img1;img2...](IsBodyHtml=true,SubjectEncoding=UTF-8)");

                return -1;
            }
        }

        public static void SendMail(Dictionary<string, string> mailData)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("yajxu@microsoft.com", null, Encoding.UTF8),
                Subject = mailData["subject"],
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            var tos = mailData["to"].Split(new []{';', ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            tos.ForEach(x =>  mailMessage.To.Add(x.Contains("@") ? x : x + "@microsoft.com"));

            if (mailData.ContainsKey("cc"))
            {
                var ccs = mailData["cc"].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                ccs.ForEach(x => mailMessage.CC.Add(x.Contains("@") ? x : x + "@microsoft.com"));
            }

            if (mailData.ContainsKey("attach"))
            {
                var attaches = mailData["attach"].Split(";,".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var attach in attaches)
                {
                    var attachment = new Attachment(attach, "application/octet-stream");
                    attachment.ContentDisposition.CreationDate = File.GetCreationTime(attach);
                    attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(attach);
                    attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(attach);
                    mailMessage.Attachments.Add(attachment);
                }
            }

            var body = mailData.ContainsKey("body") ? File.ReadAllText(mailData["body"]) : "<html><head><meta http-equiv=Content-Type content=\"text/html; charset=utf-8\"/></head><body/>";
            if (mailData.ContainsKey("image"))
            {
                var image = mailData["image"].Split(";,".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                body += string.Join(string.Empty, image.Select(x => string.Format("<img src=\"cid:{0}\">", Path.GetFileNameWithoutExtension(x))));
                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                foreach (var img in image)
                {
                    var linkedResource = new LinkedResource(img, "image/" + img.Substring(img.LastIndexOf('.') + 1))
                    {
                        ContentId = Path.GetFileNameWithoutExtension(img)
                    };
                    alternateView.LinkedResources.Add(linkedResource);
                }
                mailMessage.AlternateViews.Add(alternateView);
            }

            mailMessage.Body = body;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            var smtpClient = new SmtpClient("smtphost.redmond.corp.microsoft.com")
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential("yajxu@microsoft.com", "xyj0720("),
                EnableSsl = false
            };
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Mail failed ,Details :{0}", ex);
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
    }
}
