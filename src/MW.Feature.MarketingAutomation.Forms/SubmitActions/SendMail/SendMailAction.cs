using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Cd.Actions.FormFields;
using Sitecore.EmailCampaign.Cd.Services;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Contacts;

namespace MW.Feature.MarketingAutomation.Forms.SubmitActions.SendMail
{
    //We're extending the default SendEmail class so we can publicly expose the "ExtractTokens" method and use that
    public class SendMailAction : Sitecore.EmailCampaign.Cd.Actions.SendEmail
    {

        public SendMailAction(ISubmitActionData submitActionData) : this(
            ServiceLocator.ServiceProvider.GetService<IClientApiService>(), submitActionData,
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<PipelineHelper>(),
            ServiceLocator.ServiceProvider.GetService<IContactService>(),
            ServiceLocator.ServiceProvider.GetService<IFormsFieldValueResolver>())
        {
        }

        public SendMailAction(IClientApiService clientApiService,
            ISubmitActionData submitActionData, 
            ILogger logger, 
            PipelineHelper pipelineHelper, 
            IContactService contactService, 
            IFormsFieldValueResolver fieldValueResolver) : base(clientApiService, submitActionData, logger, pipelineHelper, contactService, fieldValueResolver)
        {
            Condition.Requires<IClientApiService>(clientApiService, "clientApiService").IsNotNull<IClientApiService>();
            Condition.Requires<ILogger>(logger, "logger").IsNotNull<ILogger>();
            Condition.Requires<PipelineHelper>(pipelineHelper, "pipelineHelper").IsNotNull<PipelineHelper>();
            Condition.Requires<IContactService>(contactService, "contactService").IsNotNull<IContactService>();
            Condition.Requires<IFormsFieldValueResolver>(fieldValueResolver, "fieldValueResolver").IsNotNull<IFormsFieldValueResolver>();

        }

        public IDictionary<string, object> GetTokens(IDictionary<string, string> fieldTokens,
            FormSubmitContext formSubmitContext)
        {
            return ExtractTokens(fieldTokens, formSubmitContext);
        }
    }
}
