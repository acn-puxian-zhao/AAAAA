angular.module('app.masterdata.delConfirm', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
    }])

    .controller('delConfirmCtrl', ['$scope', 'gridApi', '$uibModalInstance', 'customerProxy',
        function ($scope, gridApi, $uibModalInstance, customerProxy) {
            $scope.Del = function () {
                var entity = gridApi.selection.getSelectedRows()[0];
                customerProxy.delCustomer(entity, function (res) {
                    alert(res);
                    $uibModalInstance.close();
                }, function (res) {
                    alert(res);
                });
            };

            $scope.DelFalse = function () {
                var entity = gridApi.selection.getSelectedRows()[0];
                entity.removeFlg = 0;
                customerProxy.saveCustomer(entity, function (res) {
                    alert(res);
                    $uibModalInstance.close();
                }, function (res) {
                    alert(res);
                });
            };

            $scope.closeCust = function () {
                $uibModalInstance.close();
            };
        }]);