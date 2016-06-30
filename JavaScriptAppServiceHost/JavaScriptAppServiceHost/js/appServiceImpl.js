(function () {
    'use strict'; 
    var taskInstance = Windows.UI.WebUI.WebUIBackgroundTaskInstance.current;
    var connection = taskInstance.triggerDetails.appServiceConnection;

    function done() {
        AppServiceThreadBroker.ThreadBroker.postConnectionDone(connection).then(function () {
            close();
        });
    }

    function run() {
        taskInstance.addEventListener("canceled", done);
        AppServiceThreadBroker.ThreadBroker.postConnectionArrived(connection);
    }

    run();
})();