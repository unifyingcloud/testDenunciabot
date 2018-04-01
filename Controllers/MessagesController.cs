namespace MultiDialogsBot
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Resources;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Autofac;
    using Autofac.Builder;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;

    public class DefaultExceptionMessageOverrideModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PostUnhandledExceptionToUser>().Keyed<IPostToBot>(typeof(PostUnhandledExceptionToUser)).InstancePerLifetimeScope();


            RegisterAdapterChain<IPostToBot>(builder, 
                typeof(PersistentDialogTask),
                typeof(ExceptionTranslationDialogTask),
                typeof(SerializeByConversation),
                typeof(SetAmbientThreadCulture),
                typeof(PostUnhandledExceptionToUser),
                typeof(LogPostToBot)
            )
            .InstancePerLifetimeScope();
        }

        public static IRegistrationBuilder<TLimit, SimpleActivatorData, SingleRegistrationStyle> RegisterAdapterChain<TLimit>(ContainerBuilder builder, params Type[] types)
        {
            return
                builder
                .Register(c =>
                {
                // http://stackoverflow.com/questions/23406641/how-to-mix-decorators-in-autofac
                TLimit service = default(TLimit);
                    for (int index = 0; index < types.Length; ++index)
                    {
                    // resolve the keyed adapter, passing the previous service as the inner parameter
                    service = c.ResolveKeyed<TLimit>(types[index], TypedParameter.From(service));
                    }

                    return service;
                })
                .As<TLimit>();
        }
    }  


    public class PostUnhandledExceptionToUser : IPostToBot
    {
        private readonly ResourceManager resources;
        private readonly IPostToBot inner;
        private readonly IBotToUser botToUser;
        private readonly TraceListener trace;

        public PostUnhandledExceptionToUser(IPostToBot inner, IBotToUser botToUser, ResourceManager resources, TraceListener trace)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.resources, nameof(resources), resources);
            SetField.NotNull(out this.trace, nameof(trace), trace);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            catch (Exception error)
            {
                try
                {
                    
                    
                    await this.botToUser.PostAsync("Lo siento, ha ocurrido un error, lo contactaremos mas tarde " + error.Message);
                    
                }
                catch (Exception inner)
                {
                    this.trace.WriteLine(inner);
                }

                throw;
            }
        }
    }


    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }

                else
                {
                    this.HandleSystemMessage(activity);
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch 
            {
               
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;

            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}