using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MW.Feature.MarketingAutomation.Forms.SubmitActions.SendMail;
using Sitecore.Data;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.XConnect.Web;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;
using Sitecore.Xdb.MarketingAutomation.Core.Requests;
using Sitecore.Xdb.MarketingAutomation.OperationsClient;

namespace MW.Feature.MarketingAutomation.Forms.SubmitActions.AddToAutomationPlan
{
    public class AddToAutomationPlanAction : SubmitActionBase<AddToAutomationPlanModel>
    {
        private readonly IAutomationOperationsClient _automationOperationsClient;
        private readonly SendMailAction _sendMailAction;
        private readonly XConnectRetry _xconnectRetry;

        public AddToAutomationPlanAction(ISubmitActionData submitActionData) : this(submitActionData,
            ServiceLocator.ServiceProvider.GetService<IAutomationOperationsClient>(),
            new SendMailAction(submitActionData),
            ServiceLocator.ServiceProvider.GetService<XConnectRetry>())
        {
        }

        public AddToAutomationPlanAction(ISubmitActionData submitActionData,
            IAutomationOperationsClient automationOperationsClient,
            SendMailAction sendMailAction,
            XConnectRetry xconnectRetry) : base(submitActionData)
        {
            _automationOperationsClient = automationOperationsClient;
            _sendMailAction = sendMailAction;
            _xconnectRetry = xconnectRetry;
        }

        protected override bool Execute(AddToAutomationPlanModel data, FormSubmitContext formSubmitContext)
        {
            var tokens = _sendMailAction.GetTokens(data.FieldsTokens, formSubmitContext);
            tokens.Add("language", Sitecore.Context.Language?.Name);

            var identifierField = formSubmitContext.Fields.FirstOrDefault(x => new ID(x.ItemId) == new ID(data.ContactIdentifierFieldId));
            var identifier = identifierField?.GetType().GetProperty("Value")?.GetValue(identifierField)?.ToString();

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("No identifier found");
            }

            var contact = GetOrCreateContact(new ContactIdentifier(data.ContactIdentifierSource, identifier, ContactIdentifierType.Known));
            if (contact?.Id == null)
            {
                throw new ArgumentException("Could not get or create contact");
            }

            var enrollmentRequest = new EnrollmentRequest(contact.Id.Value, data.AutomationPlanId);
            foreach (var customValue in tokens)
            {
                enrollmentRequest.CustomValues.Add(customValue.Key, customValue.Value?.ToString() ?? string.Empty);
            }
            _automationOperationsClient.EnrollInPlanDirect(new EnrollmentRequest[1] { enrollmentRequest });

            return true;
        }

        private Contact GetOrCreateContact(ContactIdentifier identifier)
        {
            var contact = GetContact(identifier);
            if (contact != null)
            {
                return contact;
            }

            return CreateContact(identifier);
        }

        private Contact CreateContact(ContactIdentifier identifier)
        {
            Contact contact = null;
            _xconnectRetry.RequestWithRetry(context =>
            {
                contact = new Contact(identifier);
                context.AddContact(contact);
                context.Submit();
            });
            return contact;
        }

        private Contact GetContact(ContactIdentifier identifier)
        {
            var expandOptions = new ContactExpandOptions(PersonalInformation.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ConsentInformation.DefaultFacetKey);
            Contact contact = null;
            _xconnectRetry.RequestWithRetry(context =>
            {
                contact = context.Get(new IdentifiedContactReference(identifier.Source, identifier.Identifier), new ContactExecutionOptions(expandOptions));
            });
            return contact;
        }
    }
}
