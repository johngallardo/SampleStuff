(function () {
    'use strict'; 
    var taskInstance = Windows.UI.WebUI.WebUIBackgroundTaskInstance.current;
    var connection = taskInstance.triggerDetails.appServiceConnection;

    function done() {
        AppServiceThreadBroker.ThreadBroker.postConnectionDoneAsync(connection).then(function () {
            close();
        });
    }

    function run() {
        taskInstance.addEventListener("canceled", done);
        AppServiceThreadBroker.ThreadBroker.postConnectionArrivedAsync(connection);
    }

    run();
})();