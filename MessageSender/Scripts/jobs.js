$(function () {
    console.log("Loading custom jobs script...");
    var jobsHub = $.connection.jobsHub;

    $.connection.hub.logging = true;
    $.connection.hub.start();
    console.log("Finished loading custom jobs script.");

    var Job = function (id) {
        var self = this;
        self.id = id;
        self.processedItems = ko.observable(0);
        self.totalItems = ko.observable(1);
        self.progressPercentage = ko.computed(function () {
            console.log("Inside job " + self.id);
            return ((self.processedItems() / self.totalItems()) * 100).toFixed(2) + "%";
        })

    };

    // Create a Jobs ViewModel
    var JobsModel = function () {
        var self = this;

        self.jobs = ko.observableArray();
    };

    // Model behaviours
    JobsModel.prototype = {
        //Check if the job being updated already exists in the list
        //       If not, add it first
        updateProgress: function (id, processedItems, totalItems) {
            var self = this;

            var updatedJob = ko.utils.arrayFirst(self.jobs(), function (job) {
                return job.id == id;
            });

            if (updatedJob) {
                console.log("Updating job " + id );
                updatedJob.processedItems(processedItems);
                updatedJob.totalItems(totalItems);
                console.log("Updated job " + id + " to " + updatedJob.progressPercentage() + " complete");

            }
            else {
                console.log("Adding job " + id + " to list of running jobs");
                self.jobs.push(new Job(id, processedItems, totalItems));
            }
        }
    };

    var model = new JobsModel();

    jobsHub.client.updateProgress = function (jobId, processedItems, totalItems) {
        // TODO: Maintain a list of jobs
        //       Check if the job being updated already exists in the list
        //       If not, add it first
        //       Update the job's progress in the view
        console.log("Received update from server");
        model.updateProgress(jobId, processedItems, totalItems);
    };

    jobsHub.client.hello = function () {
        console.log("Server says hello");
    };

    ko.applyBindings(model);
});