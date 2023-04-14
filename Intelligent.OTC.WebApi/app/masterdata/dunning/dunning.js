angular.module('app.masterdata.dunning', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
    }])

    .controller('dunningEditCtrl', ['$scope', 'config', '$uibModalInstance', 'dunningProxy', 'num', 'siteUseId' ,'legal', 'typeDetailList',
        function ($scope, config, $uibModalInstance, dunningProxy, num, siteUseId ,legal, typeDetailList) {
         $scope.legallist = legal;
         $scope.typeDetailList = typeDetailList;
         if (config.legalEntity == null) {
             config.legalEntity = legal[0].legalEntity;
         }
         config.customerNum = num;
         config.siteUseId = siteUseId;
         $scope.dunning = config;


         //************************judge textbox is readonly****************e
         $scope.closeCust = function () {
             $uibModalInstance.close();
         };

         $scope.updateConfig = function () {
             dunningProxy.saveDunConfig($scope.dunning, function () { $uibModalInstance.close(); },
                 function (res) {
                     alert(res);
                 });
         };

     }]);