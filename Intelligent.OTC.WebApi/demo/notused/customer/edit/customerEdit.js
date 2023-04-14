angular.module('app.customeredit', [])
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

    .controller('customerEditCtrl', ['$scope', 'custInfo', '$modalInstance', 'bdCusclass', 'baseDataProxy', 'modifyflag', 'bdgroup', 'isHoldStatus',
     function ($scope, custInfo, $modalInstance, bdCusclass, baseDataProxy, modifyflag, bdgroup, isHoldStatus) {

         $scope.holdlist = isHoldStatus;
         if (custInfo.isHoldFlg == null) {
             custInfo.isHoldFlg = isHoldStatus[0].detailValue;
         } else {
             custInfo.isHoldFlg = custInfo.isHoldFlg;
         }
         if (custInfo.excludeFlg == null) {
             custInfo.excludeFlg = isHoldStatus[0].detailValue;
         } else {
             custInfo.excludeFlg = custInfo.excludeFlg;
         }
         $scope.cust = custInfo;
//         custInfo.isHoldFlg = $scope.status;
//         custInfo.excludeFlg = $scope.exflg;
         $scope.bdcusclass = bdCusclass;
         $scope.bdgroup = bdgroup;
//         $scope.statchange = function (isHoldStatus) {
//             //  alert(flg);
//             custInfo.isHoldFlg = isHoldStatus;
//         }
//         $scope.flgchange = function (excludeFlg) {
//             //  alert(flg);
//             custInfo.excludeFlg = excludeFlg;
//         }
         //************************judge textbox is readonly****************s
         if (modifyflag == 1) {
             $scope.isreadonly = false;
             $scope.isShow = false;
         } else if (modifyflag == 2) {
             $scope.isreadonly = true;
             $scope.isShow = true;
         }
         //************************judge textbox is readonly****************e
         $scope.closeCust = function () {
             $modalInstance.close();
         };

         $scope.onSave = function () {
             $modalInstance.close();
         };

         $scope.onUpdate = function () {
             $modalInstance.close();
         };

     } ]);