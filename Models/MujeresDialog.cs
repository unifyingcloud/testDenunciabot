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
    public class MujeresQuery
    {
        [Prompt("Por favor digame su {&} (Nombre del denunciante)")]
        public string Nombre { get; set; }
       
        [Prompt("Cuando ocurrio la {&}?")]
        public DateTime fechaDelIncidente { get; set; }


        [Prompt("Por favor digame el {&}")]
        public string NombreDeLaPersonaDenunciada { get; set; }

        [Prompt("Seleccione el estado {||}")]
        public _Estado Estado{ get; set; }


        public enum  _Estado { Chihuahua,
Sonora,
Coahuila,
Durango,
Oaxaca,
Tamaulipas,
Jalisco,
Zacatecas,
BajaCaliforniaSur,
Chiapas,
Veracruz,
BajaCalifornia,
NuevoLeon,
Guerrero,
SanLuisPotosi,
Michoacán,
Sinaloa,
Campeche,
QuintanaRoo,
Yucatán,
Puebla,
Guanajuato,
Nayarit,
Tabasco,
México,
Hidalgo,
Querétaro,
Colima,
Aguascalientes,
Morelos,
Tlaxcala,
CiudadDeMexico }


        [Prompt("Por favor digame su {&}")]
        public string correoElectronico { get; set; }


      /*  
      

        [Prompt("Por favor envie una imagen del incidente {&}")]
        public Attachment imagen { get; set; }*/


    }


    [Serializable]
    public class MujeresDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Agradecemos su apoyo, su informacion sera usada de forma confidencial.");

            var mujeresFormDialog = FormDialog.FromForm(this.BuildMujeresForm, FormOptions.PromptInStart);

         context.Call(mujeresFormDialog, this.ResumeAfterMujeresFormDialog);
        }

     private IForm<MujeresQuery> BuildMujeresForm()
        {
             OnCompletionAsyncDelegate<MujeresQuery> processMujeresSearch = async (context, state) =>
            {
                await context.PostAsync($"Gracias por introducir su informacion");
            }; 

               return new FormBuilder<MujeresQuery>()
                .Field(nameof(MujeresQuery.Nombre))
              .Field(nameof(MujeresQuery.correoElectronico))
                .Field(nameof(MujeresQuery.fechaDelIncidente))
                .Field(nameof(MujeresQuery.NombreDeLaPersonaDenunciada))
                .AddRemainingFields()
                .OnCompletion(processMujeresSearch)
                .Build();
 

} 

       private async Task ResumeAfterMujeresFormDialog(IDialogContext context, IAwaitable<MujeresQuery> result)
        {
            try
            {
                
           //     await context.PostAsync($"Hemos registrado su denuncia.");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Denunica bot",
                    Subtitle = "Su colaboracion, es importante",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "https://app.cedac.pgr.gob.mx/ATENCIONPGR/img/LogoAtenci%C3%B3nPGR-02.jpg" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Mas informacion",
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