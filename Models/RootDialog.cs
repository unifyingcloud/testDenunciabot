namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
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
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("ayuda") || message.Text.ToLower().Contains("soporte") || message.Text.ToLower().Contains("problema"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                this.ShowOptions(context);
            }
        }

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
                        context.Call(new DenunciaDialog(), this.ResumeAfterStepOneDialog);
                        context.Call(new ReceiveAttachmentDialog(), this.ResumeAfterOptionDialog);
 
                       // context.Call(new MyLocationDialog("1"), this.ResumeAfterOptionDialog);
 
                        break;

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


        private async Task ResumeAfterStepOneDialog(IDialogContext context, IAwaitable<object> result)
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
