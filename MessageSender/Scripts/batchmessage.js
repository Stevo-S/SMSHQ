$(function () {
    // Load the associated services into a dropdown list on the BatchMessages Create View
    // when a short code is selected
    function batchMessageViewModel() {
        var self = this;
        self.selectedShortCode = ko.observable();
        self.shortCodes = ko.observableArray(ko.utils.parseJson($('#fromDatabase').attr('data-shortCodes')));
        self.availableServices = ko.utils.parseJson($('#fromDatabase').attr('data-availableServices'));
        self.associatedServices = ko.computed(function () {
            if (self.selectedShortCode()) {
                var services = ko.utils.arrayFilter(self.availableServices, function (service) {
                    return service.Name.startsWith(self.selectedShortCode());
                });

                services = ko.utils.arrayMap(services, function (item) {
                    return { ServiceId: item.ServiceId, Name: item.Name + ' - ' + item.ServiceId };
                });

                return services;
            }
            else {
                return [];
            }
        });

        // Count and display the number of SMS messages that will be required
        // to send all the content in the MessageContent TextArea
        // There is also use of #MessageContentCopy which is a hidden input field
        // that is used to store the message content and make sure it is available
        // if a user clicks the 'back' button after clicking submit so that they do
        // not have to re-input the entire message all-over again
        // if they still need the same message from before
        var numberOfCharactersPerSms = 160;
        var previousMessage = $('#MessageContentCopy').val();
        previousMessage = previousMessage == null ? '' : previousMessage;
        console.log('Message: ' + previousMessage);
        self.messageContent = ko.observable(previousMessage);
        self.characterCount = ko.computed(function () {
            var currentMessage = self.messageContent();
            $('#MessageContentCopy').val(currentMessage);
            return currentMessage.length;
        });
        self.smsCount = ko.computed(function () {
            var count = Math.ceil(self.characterCount() / numberOfCharactersPerSms);
            return isNaN(count) || count == 0 ? 1 : count;
        });
        self.charactersLeft = ko.computed(function () {
            var leftCount = numberOfCharactersPerSms - (self.characterCount() % numberOfCharactersPerSms);
            return (leftCount == numberOfCharactersPerSms) && (self.characterCount() > 0) ? 0 : leftCount;
        });
    };

    ko.applyBindings(new batchMessageViewModel());
});