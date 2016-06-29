(function() {
  'use strict';
  var app = WinJS.Application;
  var activation = Windows.ApplicationModel.Activation;
  var broker = AppServiceThreadBroker.ThreadBroker;
  var listeningToBroker = false;

  function appServiceRequestReceived(data) {
      requestReceivedDiv.innerText = "Request Received, body = " + data.request.message.X1;
  }

  function activate() {

      if (!listeningToBroker) {
          broker.addEventListener("connectionarrived", function (data) {
              var connection = data.detail[0];
              connection.addEventListener("requestreceived", appServiceRequestReceived);
              statusDiv.innerText = "Connection Arrived.";
          });

          broker.addEventListener("connectionarrived", function (data) {
              console.debug("connection arrived");
          });
          listeningToBroker = true;
      }
  }

  app.onactivated = function (args) {
    if (args.detail.kind === activation.ActivationKind.launch) {
      if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
      } else {
        // TODO: This application has been reactivated from suspension.
        // Restore application state here.
      }
      args.setPromise(WinJS.UI.processAll().then(function () {
          activate();
      }));
    }
  };
  app.oncheckpoint = function (args) {
    // TODO: This application is about to be suspended. Save any state that needs to persist across suspensions here.
    // You might use the WinJS.Application.sessionState object, which is automatically saved and restored across suspension.
    // If you need to complete an asynchronous operation before your application is suspended, call args.setPromise().
  };
  app.start();
}());
