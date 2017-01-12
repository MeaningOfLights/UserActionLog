using System;
using Microsoft.Exchange.WebServices.Data;
using System.Configuration;

namespace JeremyThompsonLabs
{
    public class EWSEmail
    {

        #region "Private Member Variables"

        private ExchangeService exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
        private string fromEmail = string.Empty;

        #endregion

        #region "Constructor"

        public EWSEmail()
        {
            try
            {
                exchangeService.Url = new Uri(ConfigurationManager.AppSettings["ExchangeServiceURL"].ToString());
                fromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();

#if DEBUG
                //Edit these to match your Exchange server...
                string logonID = Environment.UserName.ToLower().TrimEnd('d');
                string password = "password";
                exchangeService.Credentials = new WebCredentials(logonID, password, "XYZ");
#else
			exchangeService.UseDefaultCredentials = true;
#endif

            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw;
            }
        }

        #endregion

        #region "Methods"
        
        public void SendEmail(string emailTo, string emailCc, string emailBcc, string emailSubject, string emailBody, string[] emailFileAttachments, bool emailFromSharedMailbox = false, bool saveToDraftAndDontSend = false)
        {
            EmailMessage message = default(EmailMessage);
            message = new EmailMessage(exchangeService);

            emailTo = emailTo.TrimEnd(';', ',');
            string[] emailArr = emailTo.Split(';', ',');
            if (emailTo.Length > 0)
                message.ToRecipients.AddRange(emailArr);

            emailCc = emailCc.TrimEnd( ';', ',' );
            emailArr = emailCc.Split(';', ',' );
            if (emailCc.Length > 0)
                message.CcRecipients.AddRange(emailArr);

            emailBcc = emailBcc.TrimEnd(';', ',');
            emailArr = emailBcc.Split(';', ',');
            if (emailBcc.Length > 0)
                message.BccRecipients.AddRange(emailArr);

#if DEBUG
            emailSubject = "IGNORE - TESTING ONLY - " + emailSubject;
#endif
            message.Subject = emailSubject;

            if ((emailFileAttachments == null) == false)
            {
                foreach (string fileAttachment in emailFileAttachments)
                {
                    if (string.IsNullOrEmpty(fileAttachment) == false)
                        message.Attachments.AddFileAttachment(fileAttachment);
                }
            }

            message.Sensitivity = Sensitivity.Private;
            message.Body = new MessageBody();
            message.Body.BodyType = BodyType.HTML;
            message.Body.Text = emailBody;

            //Save the email message to the Drafts folder (where it can be retrieved, updated, and sent at a later time). 
            if (saveToDraftAndDontSend)
            {
                message.Save(WellKnownFolderName.Drafts);
            }
            else
            {
                try
                {
                    message.Send();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("FAILED TO SEND EMAIL - PLEASE CONTACT " + fromEmail + "!", "Email sending failed", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                }

            }
        }

        #endregion
    }
}
