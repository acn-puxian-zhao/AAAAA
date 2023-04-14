angular.module('app.masterdata.paymentbank', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/admin/paymentbank', {
            templateUrl: 'app/masterdata/paymentbank/paymentbank-list.tpl.html',

            controller: 'customerPaymentBank',
            resolve: {
                //首次加载第一页
                statuslist: ['baseDataProxy', function (baseDataProxy) {
                    return baseDataProxy.SysTypeDetail("018");
                } ],
                customerPaymentBankList: ['customerPaymentbankProxy', function (customerPaymentbankProxy) {
                    return customerPaymentbankProxy.forCustomer();
                } ],
                workFlowPendingNumList: ['commonProxy', function (commonProxy) {
                    return commonProxy.search();
                } ]
            }
        });
    } ])

    .controller('customerPaymentBank',
      ['$scope', 'customerPaymentBankList', 'statuslist', 'baseDataProxy', 'modalService', 'customerPaymentbankProxy', 'CustomerGroupCfgProxy', 'workFlowPendingNumList',
       function ($scope, customerPaymentBankList, statuslist, baseDataProxy, modalService, customerPaymentbankProxy, CustomerGroupCfgProxy, workFlowPendingNumList) {

           //*****************************************floatMenu***************************s
           $scope.workflownum0 = "0";
           $scope.workflownum1 = "0";
           $scope.workflownum2 = "0";
           $scope.workflownum3 = "0";
           $scope.workflownum4 = "0";
           $scope.workflownum5 = "0";
           $scope.workflownum6 = "0";
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
                   //Break PTP
                   $scope.workflownum4 = value;
               } else if (key == 5) {
                   //Dispute Tracking
                   $scope.workflownum5 = value;
               } else if (key == 6) {
                   //Hold Customer
                   $scope.workflownum6 = value;
               }
           }
           //*****************************************floatMenu***************************e

           $scope.list = customerPaymentBankList;

           $scope.statuslist = statuslist;

           $scope.paymentBankList = {
               data: 'list',
               multiSelect: false,
               enableFullRowSelection: true,
               columnDefs: [
                            { field: 'deal', displayName: 'Deal' },
                            { field: 'customerNum', displayName: 'Customer Num' },
                            { field: 'bankAccountName', displayName: 'Bank Account Name' },
                            { field: 'bankName', displayName: 'Bank Name' },
                            { field: 'bankAccount', displayName: 'Bank Account' },
                            { field: 'createPersonId', displayName: 'Create Person Id' },
                            { field: 'flg', displayName: 'Status',
                                cellTemplate: '<div hello="{valueMember: \'flg\', basedata: \'grid.appScope.statuslist\'}"></div>'
                            },
                            { field: 'description', displayName: 'Description' }
                           ],
               onRegisterApi: function (gridApi) {
                   //set gridApi on scope
                   $scope.gridApi = gridApi;
                   gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                       //                        var num = row.entity.customerNum;
                       //                        var site = row.entity.siteCode;
                       customerPaymentbankProxy.forCustomer(function (paymentbanklist) {
                           $scope.list = paymentbanklist;
                       });
                   });
               }
           };

           $scope.$on('new', function (e) {
               $scope.Newpaymentbank();
           });

           $scope.Newpaymentbank = function () {

               var row = null;
               var modalDefaults = {
                   templateUrl: 'app/masterdata/paymentbank/edit/paymentbank-edit.tpl.html',
                   controller: 'paymentbankEditCtrl',
                   resolve: {
                       custInfo: function () {
                           return customerPaymentbankProxy.queryObject({ type: "new" });
                       }
                       //                       custInfo: function () { return row; },
                       //                        bdCusclass: function () { return $scope.bdCusclass; },
                       //                        modifyflag: function () { return 1; },
                       //                        bdlegallist: function () { return $scope.legallist; }
                       //                        bdgroup: function () {
                       //                            return CustomerGroupCfgProxy.search("");
                       //                        }
                   }
               };


               modalService.showModal(modalDefaults, {}).then(function (result) {
                   customerPaymentbankProxy.forCustomer(function (paymentbanklist) {
                       $scope.list = paymentbanklist;
                   });
                   //$scope.searchcustomer();
               });
           };

           $scope.$on('edit', function (e) {
               $scope.Edit();
           });
           $scope.Edit = function () {
               if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                   var row = $scope.gridApi.selection.getSelectedRows()[0];
                   var modalDefaults = {
                       templateUrl: 'app/masterdata/paymentbank/edit/paymentbank-edit.tpl.html',
                       controller: 'paymentbankEditCtrl',
                       resolve: {
                           custInfo: function () { return row; }
                           //                           bdCusclass: function () { return $scope.bdCusclass; },
                           //                           modifyflag: function () { return 2; },
                           //                           bdlegallist: function () {
                           //                               return $scope.legallist;
                           //                           }
                           //                           bdgroup: function () {
                           //                               return CustomerGroupCfgProxy.search("");
                           //                           }
                       }
                   }
               } else {
                   alert("请选择");
               };
           };

           $scope.$on('delete', function (e) {
               $scope.Del();
           });

           $scope.Del = function () {
               var entity = $scope.gridApi.selection.getSelectedRows()[0];

               entity.$remove(function () {
                   customerPaymentbankProxy.forCustomer(function (paymentbanklist) {
                       $scope.list = paymentbanklist;
                   });

               }, function () {
                   alert("Delete Error");
               });
           }

       } ])