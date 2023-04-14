angular.module('app.common.exportContactor', [])

    .controller('exportContactorCtrl', ['$scope', 'APPSETTING', '$uibModalInstance', 'contactProxy', 'legalList',
        function ($scope, APPSETTING, $uibModalInstance, contactProxy, legalList) {

            $scope.legalEntityList = legalList;
           
            $scope.reset = function () {
                $scope.customerNum = "";
                $scope.name = "";
                $scope.siteUseId = "";
                $scope.legalEntity = "";
            }

            $scope.exportExcel = function () {
                contactProxy.exportByCondition($scope.customerNum, $scope.name, $scope.siteUseId, $scope.legalEntity,
                    function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                    function (res) {
                        alert(res);
                    });
            }

            $scope.close = function () {
                $uibModalInstance.close();
            };

            $scope.reset();
        }
]);
