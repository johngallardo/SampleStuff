(function () {
    'use strict'; 
    var taskInstance = Windows.UI.WebUI.WebUIBackgroundTaskInstance.current;

    function done() {
        close();
    }

    function run() {
        taskInstance.addEventListener("canceled", done);
        var connection = taskInstance.triggerDetails.appServiceConnection;
        AppServiceThreadBroker.ThreadBroker.signalNewConnectionArrived(connection);
    }

    run();
})();