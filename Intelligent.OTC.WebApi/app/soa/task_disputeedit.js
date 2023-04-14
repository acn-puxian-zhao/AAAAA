angular.module('app.taskdisputeedit', ['ui.bootstrap'])

    .controller('taskdisputeeditCtrl',
        ['$scope', '$filter', 'id', 'siteuseid', 'customerno', 'issuereason', 'status', '$uibModalInstance', 'baseDataProxy', 'taskProxy',
            function ($scope, $filter, id, siteuseid, customerno, issuereason, status, $uibModalInstance, baseDataProxy, taskProxy) {

                $scope.setTtitle = function () {
                    $scope.title = " Dispute - " + status;
                };
                $scope.setTtitle();

                $scope.siteuseid = siteuseid;
                $scope.customerno = customerno;
                $scope.issuereason = issuereason;

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
                    taskProxy.saveTaskDispute(id, status, $scope.taskContent, function () {
                        result.push('submit');
                        $uibModalInstance.close(result);
                    });
                };

            }]);
