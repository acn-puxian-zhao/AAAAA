angular.module('app.customerExpirationDate', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

    }])

    .controller('customerExpirationDateCTL', ['$scope', 'customerCode', 'siteUseId', 'collectorSoaProxy', '$uibModalInstance', 
        function ($scope, customerCode, siteUseId, collectorSoaProxy, $uibModalInstance,$sce) {
            
            $scope.hislist = {
                data: 'resultHis',
                columnDefs: [
                    { field: 'userId', displayName: 'UserId', width: '100' },
                    { field: 'changeDate', displayName: 'Change Date', width: '140', cellFilter: 'date:\'yyyy-MM-dd hh:mm:ss\'' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '100' },
                    { field: 'oldCommentExpirationDate', displayName: 'Old Expiration Date', width: '180', cellClass: 'center', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'newCommentExpirationDate', displayName: 'New Expiration Date', width: '180', cellClass: 'center', cellFilter: 'date:\'yyyy-MM-dd\'' }
                ]
            }
            
            collectorSoaProxy.query({ CustomerCode: customerCode, siteUseId: siteUseId }, function (list) {
                $scope.resultHis = list;
            }    )         

            $scope.closeCommDateHis = function () {
                    $uibModalInstance.close();
                };
        }]);