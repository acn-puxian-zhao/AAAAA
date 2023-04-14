angular.module('app.taskptpedit', ['ui.bootstrap'])

    .controller('taskptpeditCtrl',
    ['$scope', '$filter', 'id', 'siteuseid', 'customerno', 'promisedate', 'status', '$uibModalInstance', 'baseDataProxy', 'taskProxy',
            function ($scope, $filter, id, siteuseid, customerno, promisedate, status, $uibModalInstance, baseDataProxy, taskProxy) {

                $scope.setTtitle = function () {
                    $scope.title = " PTP - " + status ;
                };
                $scope.setTtitle();

                $scope.siteuseid = siteuseid;
                $scope.customerno = customerno;
                $scope.promisedate = $filter('date')(promisedate, "yyyy-MM-dd");

                $scope.popup = {
                    opened: false
                };

                $scope.open = function () {
                    $scope.popup.opened = true;
                };

                var result = []
                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    taskProxy.saveTaskPTP(id, status, $scope.taskContent, function () {
                        result.push('submit');
                        $uibModalInstance.close(result);
                    })
                };

            }])
