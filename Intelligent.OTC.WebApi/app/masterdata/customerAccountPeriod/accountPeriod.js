angular.module('app.masterdata.customerAccountPeriod', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
    }])

    .controller('accountPeriodCtrl', ['$scope','cont' ,'ap', '$uibModalInstance', 'dunningProxy', 'num', 'legal', 'typeDetailList',
        'customerAccountPeriodProxy',
        function ($scope, cont ,ap, $uibModalInstance, dunningProxy, num, legal, typeDetailList, customerAccountPeriodProxy) {
            //$scope.legallist = legal;
            //$scope.typeDetailList = typeDetailList;
            //alert($scope.typeDetailList[0].legalEntity);
            //if (config.legalEntity == null) {
            //    config.legalEntity = legal[0].legalEntity;
            //}
            //config.customerNum = num;
            if (cont == '') {
                var acp = {};
                acp.customeR_NUM = ap.customerNum;
                acp.siteUseId = ap.siteUseId;
                acp.accountYear = '';
                acp.accountMonth = '';
                acp.reconciliationDay = '';
                $scope.accountPeriod = acp;
            }
            else
            {
                $scope.accountPeriod = cont;
            }
            


            //************************judge textbox is readonly****************e
            $scope.closeCust = function () {
                $uibModalInstance.close();
            };

            $scope.saveOrUpdateAccountPeriod = function () {
                var isAdd = 0;
                if (cont == '') {
                    isAdd = 1;
                }
                customerAccountPeriodProxy.saveOrUpdateAccountPeriod(isAdd,$scope.accountPeriod, function (res) {
                    alert(res);
                    $uibModalInstance.close();
                },
                    function (res) {
                        $uibModalInstance.close();
                    });
            };
        }]);