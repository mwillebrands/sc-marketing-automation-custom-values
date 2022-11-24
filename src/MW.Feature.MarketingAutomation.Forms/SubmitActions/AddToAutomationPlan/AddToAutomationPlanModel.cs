using System;
using System.Collections.Generic;

namespace MW.Feature.MarketingAutomation.Forms.SubmitActions.AddToAutomationPlan
{
    public class AddToAutomationPlanModel
    {
        public Guid ContactIdentifierFieldId { get; set; }
        public Guid AutomationPlanId { get; set; }
        public IDictionary<string, string> FieldsTokens { get; set; }
        public string ContactIdentifierSource { get; set; }
    }
}
