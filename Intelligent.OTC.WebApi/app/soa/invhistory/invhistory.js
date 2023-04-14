angular.module('app.invhistory', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

    } ])

.controller('invHisCL', ['$scope', 'inNum', 'collectorSoaProxy', '$uibModalInstance', 'contactCustomerProxy', 'modalService', 'mailProxy', '$sce', 
    function ($scope, inNum, collectorSoaProxy, $uibModalInstance, contactCustomerProxy, modalService, mailProxy, $sce) {


        $scope.invhistory = {
            data:'invhistorylist',
            columnDefs: [
                    { field: 'logDate', displayName: 'Contact Date', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'logAction', displayName: 'Operate' },
                    { field: 'logPerson', displayName: 'Operater' },
                    { field: 'oldStatus', displayName: 'Old Status' },
                    { field: 'newStatus', displayName: 'New Status' },
                    { field: 'oldTrack', displayName: 'Old Track' },
                    { field: 'newTrack', displayName: 'New Track' },
                    { field: 'discription', displayName: 'Comments' },
                    { field: 'tmp', displayName: 'Related email',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                                      '<a ng-click=" grid.appScope.getDetail(row.entity)"> {{row.entity.relatedEmail}} </a></div>'
                    }
                    ]
        }
        collectorSoaProxy.query({ InvNum: inNum }, function (list) {
            $scope.invhistorylist = list;

        })


        //********************contactList********************<a> ng-click********************start
        //contactList Detail
        $scope.getDetail = function (row) {
            //6:breakPPT 8:holdCustomer 7:changeStatus 9:unhold
            if (row.logType == "1" || row.logType == "3" || row.logType == "4" || row.logType == "5" || row.logType == "6" ||
                row.logType == "7" || row.logType == "8" || row.logType == "9") {
                if (row.proofId) {
                    mailProxy.queryObject({ messageId: row.proofId }, function (mailInstance) {
                        //mailType
                        mailInstance["title"] = "Mail View";
                        mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);

                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return mailInstance.customerNum; },
                                instance: function () { return mailInstance },
                                mailDefaults: function () {
                                    return {
                                        mailType: 'VI'
                                    };
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {

                        });
                    }); //mailProxy
                } else { alert('Log Id is null') }
            } //if mail end
            else if (row.logType == "2") {

                if (row.proofId) {
                    contactCustomerProxy.queryObject({ contactId: row.proofId }, function (callInstance) {
                        callInstance["contacterId"] = callInstance.contacterId;
                        callInstance["title"] = "Call View";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                            controller: 'contactCallCtrl',
                            size: 'lg',
                            resolve: {
                                callInstance: function () { return callInstance; },
                                custnum: function () { return ""; },
                                invoiceIds: function () { return ""; }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                        });
                    }); //contactCustomerProxy
                } //if contactId
                else
                { alert("Log Id is null"); }
            } //if call end
            else
            { alert("not map the Log"); }
        } //contactList Detail end


        //********************contactList******************<a> ng-click********************end



        $scope.close = function () {
            $uibModalInstance.close();
        };
    } ]);