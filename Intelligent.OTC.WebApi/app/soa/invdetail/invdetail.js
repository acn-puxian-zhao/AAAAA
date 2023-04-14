angular.module('app.invdetail', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

    }])

    .controller('invDetCL', ['$scope', 'inNum', 'invId','flag', 'collectorSoaProxy', '$uibModalInstance', 'modalService', '$sce', 
        function ($scope, inNum, invId, flag, collectorSoaProxy, $uibModalInstance, modalService, $sce) {
            $scope.invoiceNo = inNum;
            $scope.flag = flag;
            $scope.dateHis = {
                data: 'dateHislist',
                columnDefs: [
                    { field: 'userId', displayName: 'userId', width: '100' },
                    { field: 'changeDate', displayName: 'ChangeDate', width: '150', cellClass: 'center', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                    { field: 'oldMemoExpirationDate', displayName: 'Old Expiration Date', width: '200', cellClass: 'center', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'newMemoExpirationDate', displayName: 'New Expiration Date', width: '200', cellClass: 'center', cellFilter: 'date:\'yyyy-MM-dd\''}
                ]
            }

            $scope.invdetail = {
                data: 'invdetaillist',
                columnDefs: [
                    { field: 'customerPO', displayName: '采购订单号', width: '100' },
                    { field: 'invoiceLineNumber', displayName: '行号', width: '50', cellClass: 'center' },
                    { field: 'partNumber', displayName: '产品物料号(Arrow)', width: '150' },
                    { field: 'customerPartNumber', displayName: '产品物料号(Customer)', width: '150' },
                    { field: 'invoiceQty', displayName: '数量', width: '80', cellClass: 'right', cellFilter: 'number:0' },
                    { field: 'unitResales', displayName: '单价', width: '80', cellClass: 'right', cellFilter: 'number:5' },
                    { field: 'nsb', displayName: '合计金额', width: '80', cellClass: 'right', cellFilter: 'number:2' },
                    { field: 'balanceStatus', displayName: '对账状态', width: '80' },
                    { field: 'balanceMemo', displayName: '对账记录', width: '100' }
                ]
            }

            $scope.dateHislist = '';

            if (flag == 'dateHislist') {
                collectorSoaProxy.query({ invId: invId }, function (list) {
                    $scope.dateHislist = list;
                })
            }
            $scope.invdetaillist = '';

            if (flag =='invdetaillist') {
                collectorSoaProxy.query({ InvNumNo: inNum }, function (list) {
                    $scope.invdetaillist = list;

                })
            }

            //********************contactList******************<a> ng-click********************end

            $scope.close = function () {
                $uibModalInstance.close();
            };
        }]);