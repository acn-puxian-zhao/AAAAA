angular.module('app.changeinvoicestatus', [])
    .config(['$routeProvider', function ($routeProvider) {

    }])

    .controller('changeInvoiceStatusCtrl',
    ['$scope', 'baseDataProxy', 'status', 'invNums','disputeId', 'commonProxy', '$uibModalInstance',
        function ($scope, baseDataProxy, status, invNums, disputeId, commonProxy, $uibModalInstance) {

           var result = "";
           $scope.invNums = invNums;
           $scope.disputeId = disputeId;
           baseDataProxy.SysTypeDetail('029', function (list) {
               var list2 = [];
               list.forEach(function (val, index, arr) {
                   var nNum = arr[index].detailValue;
                   if (nNum == '007' || nNum == '010' || nNum == '012' || nNum == '013') {
                       list2.push(arr[index]);
                    }
               })
               $scope.invstatuslist = list2;
           });

           //status binding
           //commonProxy.GetChangeableInvoiceStatus(function (list) {
           //    $scope.invstatuslist = list;
           //});

           $scope.cancel = function () {
               result = "cancel";
               $uibModalInstance.close(result);
           };
           
           $scope.submit = function () {
               result = "submit";
               //UpdateInvoicesStatus(string status, List < string > invNums)
               commonProxy.updateInvoiceStatus($scope.disputeId,$scope.invstatusValue, $scope.invNums, function (res) {
                   $uibModalInstance.close(result);
               }, function (err) {
                   alert(err);
               });
           }
    }])