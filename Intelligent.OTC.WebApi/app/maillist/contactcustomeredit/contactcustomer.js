angular.module('app.contactcustomeredit.contactcustomer', [])

    .controller('contactCustomerEditCtrl',
    ['$scope', 'messageId', 'customerProxy', '$uibModalInstance', 'modalService', 'mailProxy', 'permissionProxy','customers',
    function ($scope, messageId, customerProxy, $uibModalInstance, modalService, mailProxy, permissionProxy, customers) {
        $scope.messageId = messageId;
        $scope.customers = customers;

        permissionProxy.getCurrentUser("dummy", function (user) {
            document.getElementById("inputOperator").innerHTML = user.eid;
        });

        $scope.customerList = {
            multiSelect: false,
            enableFullRowSelection: true,
            columnDefs: [
                 { field: 'customerNum', displayName: 'Customer NO.' },
                 { field: 'customerName', displayName: 'Customer Name' },
                 { field: 'siteUseId', displayName: 'Site Use Id' }
                 //,                 { field: 'collector', displayName: 'Collector' }
            ],

            onRegisterApi: function (gridApi) {
                //set gridApi on scope
                $scope.gridApi = gridApi;
            }
        };

        //Do search
        var filter = "customers=";
        $scope.searchCollection = function () {
            var filterStr = '';
            if ($scope.custCode) {
                if (filterStr != "") {
                    filterStr += "and (contains(CustomerNum,'" + $scope.custCode + "'))";
                } else {
                    filterStr += "&$filter=(contains(CustomerNum,'" + $scope.custCode + "'))";
                }
            }

            if ($scope.custName) {
                if (filterStr != "") {
                    filterStr += "and (contains(CustomerName,'" + $scope.custName + "'))";
                } else {
                    filterStr += "&$filter=(contains(CustomerName,'" + $scope.custName + "'))";
                }
            }
            if ($scope.siteUseId) {
                if (filterStr != "") {
                    filterStr += "and (contains(siteUseId,'" + $scope.siteUseId + "'))";
                } else {
                    filterStr += "&$filter=(contains(siteUseId,'" + $scope.siteUseId + "'))";
                }
            }
            //if ($scope.operator) {
            //    if (filterStr != "") {
            //        filterStr += "and (contains(Collector,'" + $scope.operator + "'))";
            //    } else {
            //        filterStr += "&$filter=(contains(Collector,'" + $scope.operator + "'))";
            //    }
            //}

            //filter = "messageId=" + $scope.messageId + filterStr;
            filter = "customers=" + filterStr;
            customerProxy.searchcustomer(filter, function (list) {
                $scope.customerList.data = list;
            }, function (error) {
                alert(error);
            });
        };

        //reset Search conditions
        $scope.resetSearch = function () {
            filter = "";
            $scope.custCode = "";
            $scope.custName = "";
            $scope.siteUseId = "";
        }

        $scope.close = function () {
            $uibModalInstance.close();
        };

        $scope.submit = function () {
            var cusNum = [];
            var result = {};

            angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                cusNum.push(rowItem.customerNum);
                //siteUseId.push(rowItem.siteUseId);
                result.custnum = rowItem.customerNum;
                result.siteUseId = rowItem.siteUseId;
            }
            );

            if (cusNum.length == 0) {
                alert("Please choose customer .");
                return;
            }
            
            //$modalInstance.close(cusNum);
            else {
                mailProxy.assignCustomer($scope.messageId, result.custnum, result.siteUseId, function (res) {
                    //alert('success!');
                    //$modalInstance.close();
                }, function (error) {
                    alert(error);
                })
            }
            $uibModalInstance.close(result);

            //mailProxy.assignCustomer($scope.messageId, cusNum, function (res) {
            //    //alert('success!');
            //    $uibModalInstance.close();
            //}, function (error) {
            //    alert(error);
            //})
        }


        //customerProxy.searchcustomer(filter, function (list) {
        //    $scope.customerList.data = list;
        //}, function (error) {
        //    alert(error);
        //});
    }]);