using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.FormBot
{
    public enum OpcionesDenuncias
    {
        DelitoElectoral, DelitoDeTortura,ContraLasMujeres
    };
 

    [Serializable]
    public class Denuncias
    {
        [Prompt("Por favor selecciona el delito que deseas denunciar: {||}"), Describe("Denuncia", null, "Mensaje", "Elige un delito:", "Los delitos deben ser denunciados.")]
        public OpcionesDenuncias? Denuncia;
        [Prompt("Por favor envianos una imagen: {||}"), Describe("Imagen", null, "Imagen", "Elige un delito:", "Los delitos deben ser denunciados.")]
        public System.Drawing.Image imagen;

       

        public static IForm<Denuncias> BuildForm()
        {
            OnCompletionAsyncDelegate<Denuncias> procesaDenuncia = async (context, state) =>
            {
                await context.PostAsync("Gracias por tu informacion, es importante reportar delitos");
            };

            return new FormBuilder<Denuncias>()
                    .Message("Bienvenido al sistema privado inteligente de denuncias")
                .Message("Gracias por ayudar a eliminar futuros delitos")
                  .OnCompletion(procesaDenuncia)
                    .Build();
        }
    };
}