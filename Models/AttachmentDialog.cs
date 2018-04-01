
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
            await context.PostAsync("Ha decidido reportar una nueva denuncia. Comience por enviar su evidencia");

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

                     
                    await context.PostAsync($"Hemos guardado su evidencia en formato {attachment.ContentType}  y de {contentLenghtBytes} bytes");

                    await preguntaCorreo(context, argument);
                    await preguntaDescripcion(context, argument);
                
                }
            }
            else
            {
                await preguntaCorreo(context, argument);
                await preguntaDescripcion(context, argument);
                 
            }

            context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task preguntaCorreo(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            if (denunciaSession.correo == null)
            {
                denunciaSession.correoPreguntado = true;
                PromptDialog.Text(
                context: context,
                resume: ResumeGetCorreo,
                prompt: "Cual es su correo electronico?",
                retry: "Por favor digame su correo electronico de nuevo"
            );
            }
        }

        public virtual async Task preguntaDescripcion(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            if (denunciaSession.descripcion == null)
            {
                denunciaSession.descripcionPreguntada = true;
                PromptDialog.Text(
                context: context,
                resume: ResumeGetDescripcion,
                prompt: "Cual es su descripcion de los hechos?",
                retry: "Escriba su descripcion de los hechos?"
            );
            }
        }



        public virtual async Task ResumeGetCorreo(IDialogContext context, IAwaitable<string> val)  
        {  
        
            this.denunciaSession.correo = val.ToString();
              await context.PostAsync("Correo eletronico guardado");
        }


        public virtual async Task ResumeGetDescripcion(IDialogContext context, IAwaitable<string> val)
        {

            this.denunciaSession.correo = val.ToString();
            await context.PostAsync("Descripcion guardada");
        }
    }
}
