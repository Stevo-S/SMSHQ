$(function () {
    console.log("Successfully loaded the script 'batchmessage.js'");

    function batchMessageViewModel() {
        var self = this;
        self.selectedShortCode = ko.observable();
        self.shortCodes = ko.observableArray(ko.utils.parseJson($('#fromDatabase').attr('data-shortCodes')));
        self.availableServices = ko.utils.parseJson($('#fromDatabase').attr('data-availableServices'));
        self.associatedServices = ko.computed(function () {
            console.log('updating services dropdown')
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
            //return ['abc', '123'];
        });
        //this.availableServices = ko.observableArray(ko.utils.parseJson($('#fromDatabase').attr('data-availableServices')));
    };

    ko.applyBindings(new batchMessageViewModel());
});