angular.module('app.masterdata.custdomain', [])

    .controller('custdomainEditCtrl', ['$scope', 'cont', '$uibModalInstance', 'num', 'contactProxy','legal',
    function ($scope, cont, $uibModalInstance, num, contactProxy, legal) {
        $scope.legallist = legal;

        if (cont.id == null) {
            cont.customerNum = num;
        }
        if (cont.legalEntity == null) {
            cont.legalEntity = "All";
        } else {
            cont.legalEntity = cont.legalEntity;
        }

        $scope.cont = cont;

        $scope.closeContact = function () {
            $uibModalInstance.close();
        };
        $scope.updateCommon = function () {
            if ($scope.cont.legalEntity == null) {
                $scope.cont.legalEntity = "All";
            }
            contactProxy.updateCustDomain($scope.cont, function () { $uibModalInstance.close(); },
                function (res) {
                    alert(res);
                });
        };

    }]);
