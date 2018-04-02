
namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.ConnectorEx;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json.Linq;

    [Serializable]
    internal class ReceiveAttachmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
           /* */
            await context.PostAsync("Ha decidido reportar una nueva denuncia. Agradecemos su apoyo. Comience por agregar evidencia de los hechos");

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

            _channelID = message.ChannelId;

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

                    await this.preguntaCorreo(context, argument); 
 
                   
                }
            }
            else
            {
             
                await context.PostAsync($"Agregue evidencia adjuntando imagenes, videos o documentos");
 
                 
            }

           // context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task preguntaCorreo(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            if (denunciaSession.correo == null)
            {
                await context.PostAsync($"Ahora comenzaremos con los datos de la denuncia");
                denunciaSession.correoPreguntado = true;
                   PromptDialog.Text(
                    context: context,
                resume: ResumeGetCorreo,
                prompt: "Cual es su correo electronico?",
                retry: "Por favor digame su correo electronico de nuevo"
            );
            }else
            {

                await this.preguntaDescripcion(context, argument); 
             

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
            }else
            {
                await this.preguntaDireccion(context, argument ); 
                //context.Wait(this.preguntaDireccion);

            }
        }
        private String _channelID; 

        public virtual async Task preguntaDireccion(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {


            var msg = await argument;

            // Here we prepare the message on Facebook that will ask for Location
            if (_channelID  == "facebook")
            {
                var reply = context.MakeMessage();
                reply.ChannelData = new FacebookMessage
                (
                    text: "Por favor comparta su ubicacion mediante Facebook",
                    quickReplies: new List<FacebookQuickReply>
                    {
                    // If content_type is location, title and payload are not used
                    // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
                    // for more information.
                    new FacebookQuickReply(
                        contentType: FacebookQuickReply.ContentTypes.Location,
                        title: default(string),
                        payload: default(string)
                    )
                    }
                );
                await context.PostAsync(reply);

                // LocationReceivedAsync will be the place where we handle the result
                context.Wait(LocationReceivedAsync);
            }
            else
            {
                if (denunciaSession.direccion == null)
                {
                    denunciaSession.direccionPreguntada = true;
                    PromptDialog.Text(
                    context: context,
                        resume: ResumeGetDireccion,
                    prompt: "Cual es la direccion aproximada de los hechos?",
                        retry: "Cual es la direccion aproximada de los hechos?"
                );
                }
                else
                {

                    //   await context.PostAsync($"Gracias por su apoyo");
                    context.EndConversation("Gracias por su apoyo, su Folio es 01-00000044-5DC67D y contraseña: 213B62");
                }
            }
          
        }

        public async Task LocationReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;
            var location = msg.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();

            // Printing message main content about location
            //await context.PostAsync($"Ubicacion recibida: { Newtonsoft.Json.JsonConvert.SerializeObject(msg.Entities) }");

            denunciaSession.direccion = Newtonsoft.Json.JsonConvert.SerializeObject(msg.Entities);
            // The result can be used then to do what you want, here in this sample it outputs a message with a link to Bing Maps centered on the position
            var geo = (location.Geo as JObject)?.ToObject<GeoCoordinates>();
            if (geo != null)
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(new HeroCard
                {
                    Title = "Ubicacion de los hechos",
                    Buttons = new List<CardAction> {
                            new CardAction
                            {
                                Title = "Donde ocurrieron los hechos",

                                Type = ActionTypes.OpenUrl,
                            Value = $"https://bing.com/maps/default.aspx?cp={geo.Latitude}~{geo.Longitude}&lvl=16&dir=0&sty=c&sp=point.{geo.Latitude}_{geo.Longitude}_You%20are%20here&ignoreoptin=1"

                        }
                        }

                }.ToAttachment());

                await context.PostAsync(reply);
                context.Done(location);
                await context.PostAsync("Gracias por su apoyo, su Folio es 01 - 00000044 - 5DC67D y contraseña: 213B62, se enviara una copia a " + this.denunciaSession.correo);
        

            }
            else
            {
                await context.PostAsync("No encontramos la ubicacion!");
                context.Done(default(Place));
            }
        }

        public virtual async Task ResumeGetDireccion(IDialogContext context, IAwaitable<string> val)
        {

            var message = await val;

            this.denunciaSession.direccion =message.ToString();

            await context.PostAsync("Gracias por su apoyo, su Folio es 01 - 00000044 - 5DC67D y contraseña: 213B62, se enviara una copia a " + this.denunciaSession.correo);
            context.EndConversation("Gracias por su apoyo, su Folio es 01-00000044-5DC67D y contraseña: 213B62, se enviara una copia a " + this.denunciaSession.correo);
       
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




        public virtual async Task ResumeGetCorreo(IDialogContext context, IAwaitable<string> val)  
        {  
            var message = await val;

            this.denunciaSession.correo = message.ToString();
              await context.PostAsync("Correo eletronico guardado");
            //context.Wait(this.preguntaDescripcion);
            await this.preguntaDescripcion(context, null); 
 
                

        }


        public virtual async Task ResumeGetDescripcion(IDialogContext context, IAwaitable<string> val)
        {
            var message = await val;
            this.denunciaSession.descripcion = message.ToString();
            await context.PostAsync("Descripcion guardada");
         //   context.Wait(this.preguntaDireccion);

            await this.preguntaDireccion(context, null); 
 
        }
    }
}
