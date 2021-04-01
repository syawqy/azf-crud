// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.Http;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using bl_syauqi.API.DTO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;

namespace bl_syauqi.API
{
    public static class SendEmail
    {
        [FunctionName("PostEmail")]
        public static async Task<IActionResult> PostEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PostEmail")] HttpRequest req,
            //[RequestBodyType(typeof(EmailDTO), "person request")] EmailDTO emailDTO,
            [SendGrid(ApiKey = "sendgrid-key")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            var file = req.Form.Files["file"];
            var datalain = await req.ReadFormAsync();
            EmailDTO emailDTO = JsonConvert.DeserializeObject<EmailDTO>(datalain["data"]);

            var emaildata = new Dictionary<string, string>();
            emaildata.Add("namauser", emailDTO.namauser);
            emaildata.Add("subject", emailDTO.subject);
            emaildata.Add("listdata", "['asdf','test213','ffff']");
            SendGridMessage message = new SendGridMessage()
            {
                TemplateId = "d-d24948c18a0e4215b6fe9649bb67753b",
                Subject = emailDTO.subject,
                //CustomArgs = emaildata,
                From = new EmailAddress() { Email = "syauqi.fuadi@ecomindo.com" , Name = "syauqi fuadi"}
            };
            message.SetTemplateData(emailDTO);
            string[] listto = emailDTO.email.Split(',');
            foreach(string email in listto)
            {
                message.AddTo(email);
            }
            string f64;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var fb = ms.ToArray();
                f64 = Convert.ToBase64String(fb);
            }
            message.AddAttachment(file.FileName, f64);
            await messageCollector.AddAsync(message);

            //message.AddContent("text/plain", $"{order.CustomerName}, your order ({order.OrderId}) is being processed!");
            return new OkObjectResult(message);
        }
    }
}
