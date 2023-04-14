angular.module('app.masterdata.paymentbank', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        //        resolve:
        //        {
        //            //首次加载第一页
        //            bdCusclass: ['baseDataProxy', function (baseDataProxy) {
        //                return baseDataProxy.SysTypeDetail("006");
        //            } ]
        //        }
    } ])

    .controller('paymentbankEditCtrl', ['$scope', 'custInfo', '$uibModalInstance', 'num', 'flg', 'customerPaymentbankProxy', 'legal',
     function ($scope, custInfo, $uibModalInstance, num, flg, customerPaymentbankProxy, legal) {

         $scope.legallist = legal;
         $scope.flglist = flg;
         if (custInfo.flg == null) {
             custInfo.flg = flg[1].detailValue;
         } else {
             custInfo.flg = custInfo.flg;
         }
         if (custInfo.legalEntity == null) {
             custInfo.legalEntity = legal[0].legalEntity;
         } else {
             custInfo.legalEntity = custInfo.legalEntity;
         }
         custInfo.customerNum = num;
         $scope.cust = custInfo;


         //************************judge textbox is readonly****************e
         $scope.closeCust = function () {
             $uibModalInstance.close();
         };

         $scope.updateCommon = function () {
             customerPaymentbankProxy.updatePayment($scope.cust, function () { $uibModalInstance.close(); },
                 function (res) {
                     alert(res);
                 });
         };

     } ]);