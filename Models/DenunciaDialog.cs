
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Denuncia
    {


        [Prompt("Por favor confirme su correo electronico")]
        public string correo { get; set; }


        [Prompt("Por digame el nombre de la {&} que desea denunciar")]
        public string persona { get; set; }

        [Prompt("Por favor digame la descripcion de los hechos")]
        public string descripcion { get; set; }

        [Prompt("Por favor digame la dirección más aproximada de dónde fueron los hechos")]
        public string direccion { get; set; }

        [Prompt("Por adjunte evidencia")]
        public Attachment adjunto{ get; set; }
      
        
       
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

    [Serializable]
    public class DenunciaDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Ha decidido reportar una nueva denuncia.");

            var DenunciaDialog = FormDialog.FromForm(this.BuildForm, FormOptions.PromptInStart);

         context.Call(DenunciaDialog, this.ResumeAfterFormDialog);
        }

     private IForm<Denuncia> BuildForm()
        {
             OnCompletionAsyncDelegate<Denuncia> processSearch = async (context, state) =>
            {
                await context.PostAsync($"Gracias por introducir su informacion");
            }; 

               return new FormBuilder<Denuncia>()
                .AddRemainingFields()
                .OnCompletion(processSearch)
                .Build();
 

} 

       private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<Denuncia> result)
        {
            try
            {
              
           //     await context.PostAsync($"Hemos registrado su denuncia.");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Denuncia FEPADE",
                    Subtitle = "Hemos generado su denuncia",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "https://app.cedac.pgr.gob.mx/ATENCIONPGR/img/LogoAtenci%C3%B3nPGR-02.jpg" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Gracias por contribuir",
                                Type = ActionTypes.OpenUrl,
                            Value = $"https://www.gob.mx/pgr"
                            }
                        }
                };

                resultMessage.Attachments.Add(heroCard.ToAttachment());

                await context.PostAsync(resultMessage);
            }
            catch (Exception ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Operacion cancelada";
                }
                else
                {
                    reply = $"Error: {ex.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }
 
   /*     private async Task<IEnumerable<Hotel>> GetMujeresAsync(MujeresQuery searchQuery)
        {
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.Destination} Hotel {i}",
                    Location = searchQuery.Destination,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;
        }*/
    }
}
