namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string ElectoralDenunciaOption = "Nueva denuncia";


        private const string ElectoralBusquedaOption = "Verificar denuncia";

        private const string ElectoralAyudaOption = "Preguntar a Fepade";

  

        public async Task StartAsync(IDialogContext context)
        {
            try{
            context.Wait(this.MessageReceivedAsync);
            }
            catch(Exception ex)
            {
                await context.PostAsync("Gracias por su mensaje, lo atenderemos mas tarde " + ex.Message );


            }
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.PostAsync("Gracias por su mensaje");

            var message = await result;

            if (message.Text.ToLower().Contains("ayuda") || message.Text.ToLower().Contains("soporte") || message.Text.ToLower().Contains("problema"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
           
            else if (message.Attachments.Any() )
            {

                await context.PostAsync("su mensaje contiene datos adjuntos");

            }
            else


            {
                this.ShowOptions(context);
            }
        }



    /*


                 /*   var attachment = message.Attachments.First();
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
         */

        private void ShowOptions(IDialogContext context)
        {
            try
            {
                PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { ElectoralBusquedaOption, ElectoralDenunciaOption,ElectoralAyudaOption }, "¿Qué desea hacer?", "Seleccione una opcion valida", 3);
            }
                catch            
            {
                }
            }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case ElectoralBusquedaOption:
                        context.Call(new BuscarDenunciaDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case ElectoralDenunciaOption:
                        {
                            context.Call(new DenunciaDialog(), this.ResumeAfterStepOneDialog);
                            break;
                        } 
                     
  

                    case ElectoralAyudaOption:
                     context.Call(new AyudaDenunciaDialog(), this.ResumeAfterOptionDialog);
                        break;

                }
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Por favor selecciona o escribe una opcion!. Escribe ayuda o soporte y tu mensaje para registrar tu solicitud de otra manera." +ex.Message);

                context.Wait(this.MessageReceivedAsync);
            }
        }



       

        private async Task ResumeAfterStepOneDialog(IDialogContext context, IAwaitable<object > result)
        {
            try
            {
                var message = await result;


               
                 

            }
            catch (Exception ex)
            {
                await context.PostAsync($"Algo ha fallado: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Gracias por contactar al equipo de soporte. Su numero de incidencia es {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Algo ha fallado: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

    
    }
     


}
