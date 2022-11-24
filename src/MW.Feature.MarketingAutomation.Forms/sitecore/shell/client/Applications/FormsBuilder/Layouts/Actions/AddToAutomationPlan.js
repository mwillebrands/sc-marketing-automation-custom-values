(function (speak) {
    var parentApp = window.parent.Sitecore.Speak.app.findApplication('EditActionSubAppRenderer');
    var FormDesignBorder = window.parent.Sitecore.Speak.app.findComponent('FormDesignBoard');

    var parameterNames = {
        automationPlan: "automationPlanId",
        token: "fieldsTokens",
        email: "contactIdentifierFieldId",
        source: "contactIdentifierSource",
    };


    speak.pageCode(["underscore", "/-/speak/v1/formsbuilder/layouts/actions/formfieldsutils.js"],
        function (_, formFieldsUtils) {
            return {
                initialized: function () {
                    this.on({
                        "loaded": this.loadDone 
                    },
                        this);

                    this.Parameters = {};

                    this.Fields = formFieldsUtils.getInputFields(FormDesignBorder);

                    this.AutomationPlansList = this.getFormComponentByBindingName(parameterNames.automationPlan);
                    this.EmailsList = this.getFormComponentByBindingName(parameterNames.email);
                    this.SourceField = this.getFormComponentByBindingName(parameterNames.source);

                    this.setupFormControls();
                    this.attachHandlers();
                    if (parentApp) {
                        parentApp.loadDone(this, this.HeaderTitle.Text, this.HeaderSubtitle.Text);
                    }
                },

                attachHandlers: function() {
                    this.AutomationPlansList.on("change:SelectedItem", this.validateRequiredFields, this);
                    this.EmailsList.on("change:SelectedItem", this.validateRequiredFields, this);
                    this.SourceField.on("change:Value", this.validateRequiredFields, this);

                    this.AutomationPlanDataSource.on("change:DynamicData", this.automationPlansChanged, this);
                    this.AutomationPlanDataSource.on("error", this.messagesError, this);
                },

                setupFormControls: function() {
                    this.Form.children.forEach(function (control) {
                        if (control.deps && control.deps.indexOf("bclSelection") !== -1) {
                            control.IsSelectionRequired = false;
                        }
                    });
                },

                validateRequiredFields: function () {
                    var isMessageSpecified = this.AutomationPlansList.SelectedValue && this.AutomationPlansList.SelectedValue.length;
                    var isEmailFieldSpecified =
                        this.EmailsList.SelectedValue && this.EmailsList.SelectedValue.length;
                    
                    var isSelectable = isMessageSpecified && isEmailFieldSpecified && this.SourceField.Value;
                    parentApp.setSelectability(this, isSelectable);
                },

                getFormComponentByBindingName: function(parameterName) {
                    var componentName = this.Form.bindingConfigObject[parameterName].split(".")[0];
                    return this.Form[componentName];
                },

                setDynamicData: function (formPropKey, items) {
                    var component = this.getFormComponentByBindingName(formPropKey);
                    var selectedItemId = this.Parameters[formPropKey];
                    items = items.slice(0);
                    items.unshift({ Name: "", Id: "" });


                    if (selectedItemId &&
                        !_.find(items, function (item) { return item.Id === selectedItemId })) {
                        items.splice(1, 0, {
                            Id: selectedItemId,
                            Name: selectedItemId +
                                " - " +
                                (this.ValueNotInListText.Text || "value not in the selection list")
                        });

                        component.reset(items);
                        $(component.el).find('option').eq(1).css("font-style", "italic");
                    } else {
                        component.reset(items);
                        component.SelectedValue = selectedItemId;
                    }
                },

                messagesError: function (result) {
                    this.MessageBar.add(errorMessage);
                    this.showMessage(
                        result.response.status === 401 || result.response.status === 403
                            ? this.UnauthorizedErrorMessage.Text
                            : this.GenericErrorMessage.Text
                    );
                    this.setDynamicData(parameterNames.automationPlan, []);
                },

                showMessage: function (text, type) {
                    var message = {
                        Type: type || "error",
                        Text: text,
                        IsClosable: true,
                        IsTemporary: false
                    };
                    this.MessageBar.add(message);
                },

                automationPlansChanged: function (items) {
                   items = items.map(function(item) {
                        return {
                            Id: item.id,
                            Name: item.name
                        }
                    });
                    this.setDynamicData(parameterNames.automationPlan, items);
                },

                convertTokensToArray: function (tokensObj) {
                    var tokensArray = [];
                    for (var key in tokensObj) {
                        tokensArray.push({ name: key, id: tokensObj[key] });
                    }
                    return tokensArray;
                },

                convertTokensToObject: function (tokensArray) {
                    var tokensObject = {};
					for (var index in tokensArray) {
						var token = tokensArray[index];
                        tokensObject[token.name] = token.id;
                    }
                    return tokensObject;
                },

                loadDone: function (parameters) {
                    this.Parameters = parameters || {};
                    this.Form.setFormData(this.Parameters);
                    this.setDynamicData(parameterNames.email, this.Fields);

                    var tokens = this.Parameters[parameterNames.token] || [];
                    if (typeof tokens === 'object') {
                        tokens = this.convertTokensToArray(tokens);
                    }

                    this.CustomTokensForm.reset(tokens);
                },

                getData: function () {
                    this.Parameters[parameterNames.automationPlan] = this.AutomationPlansList.SelectedValue;
                    this.Parameters[parameterNames.token] = this.convertTokensToObject(this.CustomTokensForm.serializeTokens() || []);
                    this.Parameters[parameterNames.email] = this.EmailsList.SelectedValue;
                    this.Parameters[parameterNames.source] = this.SourceField.Value;
                    return this.Parameters;
                }
            };
        });
})(Sitecore.Speak);
