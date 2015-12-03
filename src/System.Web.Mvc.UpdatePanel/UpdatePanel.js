/// <reference path="../updatepaneldemo/scripts/jquery-2.1.4.intellisense.js" />
/// <reference path="jquery-1.10.2.js" />
// ReSharper disable InconsistentNaming

//register namespace window.UpdatePanels, where it stores instances of initialized updatePanels
(function (UpdatePanels) { //Init

    //Enum: LoadingModes
    UpdatePanels.LoadingModes = {
        RequestOnDocumentReady: 0,  //Autorequested after document loaded
        RenderWithParent: 1,    //Preloaded with Parent
        ManualOnly: 2   //Loads only manually when developer triggers it
    };
    //UpdatePanel class
    UpdatePanels.UpdatePanel =
        function UpdatePanel(id, callbackUrl, settings) {
        var $scope = this;

        //Page-wide unique generated ID for this UpdatePanel (supposed to be hierarchical)
        $scope.id = id;

        //Callback url, that should be equal to original URL of page with appended parameter _updatePanel={id}
        $scope.callbackUrl = callbackUrl;

        //UpdatePanel settings came from server size
        $scope.settings = settings;

        //function initialize ajax request to server using callback url. Can be overriden by user
        //by default it's just get request
        $scope.ajaxRequest = function () {
            return $.ajax($scope.callbackUrl);
        };

        //function to make value safe for using as HTML DOC element Id, to prevent breaking Jquery selector
        function sanitizeId(value) {
            if (value) return value.replace(/([ #;?%&,.+*~\':"!^$[\]()=>|\/@])/g, '\\$1');
            return value;
        }
        var jselectorByData = "[data-updatePanel='" + sanitizeId(id) + "']";

        //Returns jquery of UpdatePanel element
        $scope.getElement = function () {
            return $(".updatePanel" + jselectorByData);
        };

        //Return jquery of div displayed as content while UpdatePanel is loading
        $scope.getElementLoading = function () {
            return $(".updatePanel-loading" + jselectorByData);
        };

        //Returns jquery of UpdatePanel content placeholder
        $scope.getElementContent = function () {
            return $(".updatePanel-content" + jselectorByData);
        };
        $scope.getElements = function () {
            return $(jselectorByData);
        };

        //Try to add this instance to data dictionary of elements
        $scope.getElements().data('updatePanelInstance', $scope);

        //Starts reloading of UpdatePanel content
        $scope.load = function () {
            var eventSource = $scope.getElements(); //Element on which events will be triggered
            var elementLoading = $scope.getElementLoading();
            var elementContent = $scope.getElementContent();

            //Before starting reloading it triggers event
            eventSource.trigger("updatePanel.load.starting", [$scope]);

            //hide content and show loading
            elementContent.hide();
            elementLoading.show();

            var res = $scope.ajaxRequest()
                .success(function (d, r) {
                    eventSource.trigger("updatePanel.load.rendering", [$scope, d, r]);
                    elementLoading.hide();
                    elementContent.show();
                    elementContent.html(d);
                    eventSource.trigger("updatePanel.load.rendered", [$scope, d, r]);
                })
                .error(function (d, r) {
                    var args = { errorToDisplay: d };
                    eventSource.trigger("updatePanel.load.error.rendering", [$scope, d, r, args]);

                    console.error("Update panel received error from server", [$scope, d, r]);
                    elementLoading.hide();
                    elementContent.show();

                    //Display error in content
                    elementContent.html(args.errorToDisplay);

                    eventSource.trigger("updatePanel.load.error.rendered", [$scope, d, r]);
                });

            //After loading initiated triggering event
            eventSource.trigger("updatePanel.load.started", [$scope, res]);
            return res;
        };

        //Initialize state of LoadingPanel/Content containers depending on settings
        $scope.OnInit = function () {
            if ($scope.settings.LoadMode === UpdatePanels.LoadingModes.RenderWithParent) {
                $scope.getElementLoading().hide();
                $scope.getElementContent().show();
            }
            else if ($scope.settings.LoadMode === UpdatePanels.LoadingModes.ManualOnly) {
                $scope.getElementLoading().hide();
                $scope.getElementContent().hide();
            }

            if ($scope.settings.LoadMode === UpdatePanels.LoadingModes.RequestOnDocumentReady) {
                $scope.load();
            }

        };

        $(function () {
            $scope.OnInit();
        });

        
    };

    //Function that create and register UpdatePanel
    UpdatePanels.CreateUpdatePanel = function (id, callbackUrl, settings) {
        var updatePanel = new UpdatePanels.UpdatePanel(id, callbackUrl, settings);

        //Register created update panel in a dictionary of Instances
        UpdatePanels.Instances = UpdatePanels.Instances || {};
        UpdatePanels.Instances[id] = updatePanel;
        return updatePanel;
    };

})(  window.UpdatePanels = window.UpdatePanels || {}, window);

// ReSharper restore InconsistentNaming
