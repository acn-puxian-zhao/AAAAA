angular.module('app.multipleResult', ['ui.bootstrap'])

    .controller('multipleResultCtrl', 
    // 'siteuseid','legalentity',
        ['$scope', '$uibModalInstance', 'caHisDataProxy','entity',
            //siteuseid,legalentity,
            function ($scope, $uibModalInstance, caHisDataProxy, entity) {

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                // recon grid start
                $scope.reconDataList = {
                    multiSelect: true,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    //enableCellEditOnFocus: true,
                    data: 'reconList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'center' },
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'transactioN_CURRENCY', displayName: 'Transaction Currency', width: '100', cellClass: 'center' },
                        { field: 'customeR_NUM', displayName: 'Customer Number', width: '120', cellClass: 'center' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '120' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'grouP_NO', displayName: 'GroupNo', width: '120' },
                        { field: 'grouP_TYPE', displayName: 'GroupType', width: '100' },
                        { field: 'invoicE_NUM', displayName: 'Invoice Number', width: '120' },
                        { field: 'invoicE_DUEDATE', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'invoicE_CURRENCY', displayName: 'Invoice Currency', width: '80', cellClass: 'center' },
                        { field: 'invoicE_AMOUNT', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'invoicE_SITEUSEID', displayName: 'SiteUseID', width: '120', cellClass: 'center' },
                        { field: 'ebname', displayName: 'EBName', width: '120' },
                        { field: 'hasVAT', displayName: 'Has VAT', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasVAT>0"><img src="~/../Content/images/Execute.png"></span>' },
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.reconGridApi = gridApi;
                    }
                };

                $scope.reconGridInit = function (id) {
                    caHisDataProxy.getReconDetailsMultipleResult(id, function (result) {
                        if (null != result.dataRows && result.dataRows.length > 0) {
                            $scope.reconList = result.dataRows;
                        } else {
                            $scope.reconList = [];
                        }
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.reconGridInit(entity.id);
                // recon grid end

                $scope.submit = function () {
                    if (confirm("Continue to use this recon group?")) {
                        caHisDataProxy.changeToMatch(entity.id, function (result) {
                            result = "submit";
                            $uibModalInstance.close(result);
                        }, function (error) {
                            alert(error);
                        });
                    }
                };

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };
            }])
