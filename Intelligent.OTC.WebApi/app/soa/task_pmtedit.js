angular.module('app.taskpmtedit', ['ui.bootstrap'])

    .controller('taskpmteditCtrl',
    ['$scope', '$filter', 'id', 'siteuseid', 'customerno', 'invoiceNum', 'balanceAmt', 'status', '$uibModalInstance', 'baseDataProxy', 'taskProxy',
        function ($scope, $filter, id, siteuseid, customerno, invoiceNum, balanceAmt, status, $uibModalInstance, baseDataProxy, taskProxy) {

                $scope.setTtitle = function () {
                    $scope.title = " PMT - " + status;
                };
                $scope.setTtitle();

                $scope.siteuseid = siteuseid;
                $scope.customerno = customerno;
                $scope.balanceAmt = balanceAmt;
                $scope.invoiceNum = invoiceNum;

                $scope.popup = {
                    opened: false
                };

                $scope.open = function () {
                    $scope.popup.opened = true;
                };

                var result = [];
                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    taskProxy.saveTaskPMT(customerno, siteuseid, invoiceNum, status, $scope.taskContent, function () {
                        result.push('submit');
                        $uibModalInstance.close(result);
                    });
                };

            }]);
