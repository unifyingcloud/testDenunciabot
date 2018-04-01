
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Script.Serialization;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class BuscardenunciaQuery
    {
      
        [Prompt("Por favor digame el {&} que desea consultar")]
        public string folio { get; set; }
 

        [Prompt("Por favor digame la contraseña")]
        public string Contrasenia { get; set; }

    }


    [Serializable]
    public class BuscarDenunciaDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
           // await context.PostAsync("Realizaremos su busqueda al confirmar su correo electrónico.");

            var DenunciaDialog = FormDialog.FromForm(this.BuildForm, FormOptions.PromptInStart);

         context.Call(DenunciaDialog, this.ResumeAfterFormDialog);
        }

        private IForm<BuscardenunciaQuery> BuildForm()
        {
            OnCompletionAsyncDelegate<BuscardenunciaQuery> processSearch = async (context, state) =>
           {

           
                await context.PostAsync($"Gracias por introducir su informacion");
           };

            return new FormBuilder<BuscardenunciaQuery>()
             .AddRemainingFields()
             .OnCompletion(processSearch)
             .Build();

        }

       private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<BuscardenunciaQuery> result)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var res = await result;
            try
            {
                 
                try
                {

                String auth_URL = "https://fepade-web.azurewebsites.net/api/v2/seguridad/login";
                
                    var authRequest = System.Net.WebRequest.Create(auth_URL);
                if (authRequest != null)
                    {
                    authRequest.Method = "POST";
                    authRequest.Timeout = 12000;
                    authRequest.ContentType = "application/json";
                    Stream dataStream = authRequest.GetRequestStream ();  
                   
                        byte[] data = Encoding.ASCII.GetBytes(@"{ ""nombreUsuario"":""admin001"",""passwordUsuario"":""admin001"",""tokenUsuario"":""admin001"",""esUsuarioAnonimo"":false,""rol"":""admin""}");
                        dataStream.Write (data, 0, data.Length);  
                    
                    }
                 

                HttpWebResponse response = (HttpWebResponse)authRequest.GetResponse();
                String r;
                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    r = rdr.ReadToEnd();
                }
                    var authObj = ser.Deserialize<Dictionary<string, string>>(r);


                String WEBSERVICE_URL = "https://fepade-web.azurewebsites.net/api/v2/pde/denuncia?folioDenuncia="  + res.folio +  "&password="  + res.Contrasenia +  "&esFepadeTel=false";
              
                    var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                    if (webRequest != null)
                    {
                        webRequest.Method = "GET";
                        webRequest.Timeout = 12000;
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("Authorization", authObj["tokenUsuario"]);
                       // webRequest.
                        using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                            {
                                var jsonResponse = sr.ReadToEnd();
                             

                           
                               

                                var JSONObj = ser.Deserialize<Dictionary<string, string>>(jsonResponse);

                                await context.PostAsync("Denuncia encontrada, esta es la descripcion de los hechos");
                                await context.PostAsync(JSONObj["DescripcionHechos"]);


                              /*  foreach(var JsonVal in JSONObj)
                                {
                                    await context.PostAsync(JsonVal.Key + ", " + JsonVal.Value );

                                }*/
                              
                               
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await context.PostAsync("No hemos encontrado su denuncia, por favor intente de nuevo");
                    await context.PostAsync(ex.Message);
                }

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();
                HeroCard heroCard = new HeroCard()
                {
                    Title = "Ver estado de la denuncia",
                    Subtitle = "Gracias por su apoyo",
                    Images = new List<CardImage>()
                        {
                        new CardImage() { Url = "http://despertardeoaxaca.com/wp-content/uploads/2015/11/PGR-LOGO-770x470.png" }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "FEPADE",
                                Type = ActionTypes.OpenUrl,
                            Value = $"https://www.fepadenet.gob.mx/folio.aspx?numfolio=00002420"
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
