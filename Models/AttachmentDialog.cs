
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    internal class ReceiveAttachmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }
        private Denuncia denunciaSession;
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

            if (denunciaSession ==null)
            {

                denunciaSession = new Denuncia();
            }
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                    if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                         && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com", StringComparison.CurrentCulture))
                    {
                        var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    await context.PostAsync($"Se ha ingresado su dato adjunto tipo {attachment.ContentType}  y de {contentLenghtBytes} bites");
                }
            }
            else
            {
                if (denunciaSession.correo == null)
                {
                    PromptDialog.Text(  
                    context: context,  
                    resume: ResumeGetCorreo,  
                    prompt: "Cual es su correo electronico?",  
                    retry: "Por favor digame su correo electronico de nuevo"  
                );  


                }else if(denunciaSession.descripcion == null)
                {

                    await context.PostAsync("Describa los hechos");


                }


                 
            }

            context.Wait(this.MessageReceivedAsync);
        }
        public virtual async Task ResumeGetCorreo(IDialogContext context, IAwaitable<string> UserEmail)  
        {  
        
            this.denunciaSession.correo = UserEmail.ToString();
              await context.PostAsync("Correo eletronico guardado");
        }
    }
}
