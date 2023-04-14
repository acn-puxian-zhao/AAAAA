angular.module('app.changestatus', [])
    .config(['$routeProvider', function ($routeProvider) {

    }])

    .controller('changeStatusCtrl',
    ['$scope', 'baseDataProxy', 'title', 'id', 'type', 'index', 'commonProxy', '$uibModalInstance', 'mailId','actionOwnerDepartmentCode','disputeReasonCode',
        function ($scope, baseDataProxy, title, id, type, index, commonProxy, $uibModalInstance, mailId, actionOwnerDepartmentCode, disputeReasonCode) {

           var result = "";
           //1:dispute change status  2:break ptp change status
           var statusFlg = "1";
           $scope.title = title;

           if (type == "026") {
               statusFlg = "1"; //1:dispute change status
           }
           else if(type == "027")
           {
               statusFlg = "2"; //break PTP
           }
           else if (type == "028") {
               statusFlg = "3"; //hold Customer
           }
           else {
               $scope.title = "Change Status";
               statusFlg = "0"; //0:unknown
           }

            $scope.statusValue = index;
            $scope.actionownerdept = actionOwnerDepartmentCode;
            $scope.disputereason = disputeReasonCode;

           //status binding
           baseDataProxy.SysTypeDetail(type, function (list) {
               $scope.statuslist = list;
           });

           baseDataProxy.SysTypeDetail("038", function (list) {
               $scope.actionOwnerDeptList = list;
           });

           baseDataProxy.SysTypeDetail("025", function (list) {
               $scope.disputereasonList = list;
           });
           

           $scope.cancel = function () {
               result = "cancel";
               $uibModalInstance.close(result);
           };
           
           $scope.submit = function () {
               if(type=="027" || type=="028")
               {
                   if ($scope.statusValue == "")
                   {
                       alert("please select a status!");
                       return;
                   }
               }
               result = "submit";
               if (mailId == null) { mailId = "";}
               commonProxy.updateStatus(id, $scope.statusValue, statusFlg, mailId, $scope.actionownerdept,$scope.disputereason, function (res) {
                   if (res != "success") {
                       alert(res);
                   } else{
                       $uibModalInstance.close(result);
                   }
               }, function (err) {
                   alert(err);
               });
           }
    }])