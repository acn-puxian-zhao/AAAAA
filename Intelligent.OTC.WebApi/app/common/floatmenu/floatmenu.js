angular.module('app.common.floatmenu', [])

    .controller('floatmenuListCtrl', ['$scope', 'commonProxy', '$location', function ($scope, commonProxy, $location) {

        $scope.workflownum0 = "0";
        $scope.workflownum1 = "0";
        $scope.workflownum2 = "0";
        $scope.workflownum3 = "0";
        $scope.workflownum4 = "0";
        $scope.workflownum5 = "0";
        $scope.workflownum6 = "0";
        $scope.workflownum7 = "0";

        $scope.caimg = "~/../Content/images/All_task_default.png";
        $scope.mailimg = "~/../Content/images/My_mailbox_default.png";
        $scope.followupimg = "~/../Content/images/All_task_default.png";
        $scope.taskimg = "~/../Content/images/All_task_default.png";
        $scope.soaimg = "~/../Content/images/SOA_default.png";
        $scope.contactcustomerimg = "~/../Content/images/Contact_customer_default.png";
        $scope.dunningreminderimg = "~/../Content/images/Dunning_Reminder_default.png";
        $scope.disputetrackingimg = "~/../Content/images/Dispute_tracking_default.png";
        $scope.breakptpimg = "~/../Content/images/Break_PTP_default.png";
        $scope.holdcustomerimg = "~/../Content/images/Hold_customer_default.png";

        $scope.cacolor = "#3c3c3c";
        $scope.mailcolor = "#3c3c3c";
        $scope.followupcolor = "#3c3c3c";
        $scope.taskcolor = "#3c3c3c";
        $scope.soacolor = "#3c3c3c";
        $scope.contactcustomercolor = "#3c3c3c";
        $scope.dunningremindercolor = "#3c3c3c";
        $scope.disputetrackingcolor = "#3c3c3c";
        $scope.breakptpcolor = "#3c3c3c";
        $scope.holdcustomercolor = "#3c3c3c";

        var workFlowPendingNumList = [];

        $scope.toPage = function (items) {
            //$scope.imgChange(items);
            if (items == "ca") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    window.location.reload();
                } else {
                    $location.path("/myinvoices");
                }
                //alert(items);
            } else if (items == "Task") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    $scope.callback();
                } else {
                    $location.path("/task");
                }
            } else if (items == "FollowUp") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    $scope.callback();
                } else {
                    $location.path("/followup");
                }
            } else if (items == "Soa") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    $scope.callback();
                } else {
                    $location.path("/collectorSoa");
                }
            } else if (items == "contactcustomer") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    $scope.callback();
                } else {
                    $location.path("/collectorSoa");
                }
            } else if (items == "dunning") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    $scope.callback();
                } else {
                    $location.path("/disputetracking");
                }
            } else if (items == "disputetracking") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    window.location.reload();
                } else {
                    $location.path("/disputetracking");
                }
            } else if (items == "breakptp") {
                $location.path("/breakPTP");
            } else if (items == "holdcustomer") {
                $location.path("/holdCustomer");
            } else if (items == "mail") {
                if (window.location.hash.toString().indexOf(items) >= 0) {
                    window.location.reload();
                } else {
                    $location.path("/maillist");
                }
            } 
        }

        $scope.$on('FLOAT_MENU_REFRESH', function (e) {
            if (e.currentScope && e.currentScope.params && e.currentScope.params[0] != $scope.params[0]) {
                return;
            }

            commonProxy.search(function (list) {
                workFlowPendingNumList = list;

                for (var i = 0; i < workFlowPendingNumList.length; i++) {
                    var key = workFlowPendingNumList[i].key;
                    var value = workFlowPendingNumList[i].value;

                    if (key == 0) {
                        //CA
                        $scope.workflownum0 = value;
                    } else if (key == 1) {
                        //SOA
                        $scope.workflownum1 = value;
                    } else if (key == 2) {
                        //Contact Customer
                        $scope.workflownum2 = value;
                    } else if (key == 3) {
                        //Dunning Reminder
                        $scope.workflownum3 = value;
                    } else if (key == 4) {
                        //Dispute Tracking
                        $scope.workflownum4 = value;
                    } else if (key == 5) {
                        //Break PTP
                        $scope.workflownum5 = value;
                    } else if (key == 6) {
                        //Hold Customer
                        $scope.workflownum6 = value;
                    } else if (key == 7) {
                        //MAIL View
                        $scope.workflownum7 = value;
                    }
                }
            });
        });

        var path = $location.path();
        if (path.indexOf("myinvoices") >= 0) {
            $scope.caimg = "~/../Content/images/All_task_actived.png";
            $scope.cacolor = "#0072c6";
        } else if (path.indexOf("followup") >= 0) {
            $scope.followupimg = "~/../Content/images/All_task_actived.png";
            $scope.followupcolor = "#0072c6";
        } else if (path.indexOf("task") >= 0) {
            $scope.taskimg = "~/../Content/images/All_task_actived.png";
            $scope.taskcolor = "#0072c6";
        } else if (path.indexOf("collectorSoa") >= 0) {
            $scope.soaimg = "~/../Content/images/SOA_default.png";
            $scope.soacolor = "#0072c6";
        } else if (path.indexOf("contactcustomer") >= 0) {
            $scope.contactcustomerimg = "~/../Content/images/Contact_customer_actived.png";
            $scope.contactcustomercolor = "#0072c6";
        } else if (path.indexOf("dunning") >= 0) {
            $scope.dunningreminderimg = "~/../Content/images/Dunning_Reminder_actived.png";
            $scope.dunningremindercolor = "#0072c6";
        } else if (path.indexOf("disputetracking") >= 0) {
            $scope.disputetrackingimg = "~/../Content/images/Dispute_tracking_actived.png";
            $scope.disputetrackingcolor = "#0072c6";
        } else if (path.indexOf("breakPTP") >= 0) {
            $scope.breakptpimg = "~/../Content/images/Break_PTP_actived.png";
            $scope.breakptpcolor = "#0072c6";
        } else if (path.indexOf("holdCustomer") >= 0) {
            $scope.holdcustomerimg = "~/../Content/images/Hold_customer_actived.png";
            $scope.holdcustomercolor = "#0072c6";
        } else if (path.indexOf("maillist") >= 0) {
            $scope.mailimg = "~/../Content/images/My_mailbox_actived.png";
            $scope.mailcolor = "#0072c6";
        }

    } ]);