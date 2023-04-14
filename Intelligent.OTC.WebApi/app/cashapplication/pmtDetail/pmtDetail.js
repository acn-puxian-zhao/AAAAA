angular.module('app.pmtDetail', ['ui.bootstrap'])

    .controller('pmtDetailCtrl', 
    // 'siteuseid','legalentity',
        ['$scope', '$uibModalInstance', 'caHisDataProxy','entity',
            //siteuseid,legalentity,
            function ($scope, $uibModalInstance, caHisDataProxy, entity) {


                $scope.PmtDetailHisDataList = {
                    showGridFooter: true,
                    enableFullRowSelection: true, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                    //enableRowHeaderSelection: true, //是否显示选中checkbox框 ,default为true
                    enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                    enableSelectionBatchEvent: true, //default为true
                    multiSelect: false,// 是否可以选择多个,默认为true;
                    noUnselect: false,//default为false,选中后是否可以取消选中
                    enableFiltering: true,
                    data: 'PmtDetailList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'hasbs', name: 'BS', displayName: 'BS', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasbs>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                        { field: 'hasinv', name: 'Detail', displayName: 'Detail', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasinv>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                        { field: 'hasMatched', name: 'Matched', displayName: 'Matched', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasMatched>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '50' },
                        { field: 'valueDate', displayName: 'Value Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                        { field: 'receiveDate', displayName: 'Receive Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                        { field: 'customerNum', displayName: 'Customer Number', width: '80', cellClass: 'center' },
                        { field: 'customerName', displayName: 'Customer Name', width: '140' },
                        { field: 'currency', displayName: 'Currency', width: '40', cellClass: 'center' },
                        { field: 'transactionAmount', displayName: 'Transaction Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'amount', displayName: 'Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'groupNo', displayName: 'Group No', width: '120', cellClass: 'center' },
                        { field: 'creatE_USER', displayName: 'Create User', width: '100' },
                        { field: 'creatE_DATE', displayName: 'Create Time', width: '140', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.PmtDetailGridApi = gridApi;
                        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                            caHisDataProxy.getPmtDetailHisDataBsById(row.entity.id, function (result) {
                                $scope.PmtDetailBsList = result;
                            });
                            caHisDataProxy.getPmtDetailHisDataDetailById(row.entity.id, function (result) {
                                $scope.PmtDetailDetailList = result;
                            });
                        });
                    }
                };
                $scope.PmtDetailBSHisDataList = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    noUnselect: false,
                    data: 'PmtDetailBsList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'transactionNumber', displayName: 'Transaction Inc', width: '130' },
                        { field: 'valueDate', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '80', cellClass: 'center' },
                        { field: 'amount', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'reF1', displayName: 'Description', width: '200' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.PmtDetailBSGridApi = gridApi;
                    }
                };

                $scope.PmtDetailDetailHisDataList = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    noUnselect: false,
                    data: 'PmtDetailDetailList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'invIsClosed', name: 'invIsClosed', displayName: 'IS Closed', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.invIsClosed>0" ><img style="height:12px;width:12px" src="~/../Content/images/259.png"></span>' },
                        { field: 'siteUseId', displayName: 'SiteUseId', width: '100', cellClass: 'center' },
                        { field: 'invoiceNum', displayName: 'Invoice No', width: '120' },
                        { field: 'invoiceDate', displayName: 'Invoice Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'dueDate', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '80', cellClass: 'center' },
                        { field: 'amount', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.PmtDetailDetailGridApi = gridApi;
                    }
                };
                
                $scope.PmtDetailPageChanged = function () {
                    caHisDataProxy.getCaPmtDetailListByBsId(entity.id, function (result) {
                            $scope.PmtDetailTotalItems = result.count;
                            $scope.PmtDetailList = result.pmt;
                            if (result.count > 0) {
                                $scope.PmtDetailBsList = result.pmt[0].pmtBs;
                                $scope.PmtDetailDetailList = result.pmt[0].pmtDetail;
                            }
                            else {
                                $scope.PmtDetailBsList = [];
                                $scope.PmtDetailDetailList = [];
                            }
                        }, function (error) {
                            alert(error);
                        });
                };

                $scope.PmtDetailPageChanged();

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    var result = [];
                    result.push('submit');

                    var row = $scope.PmtDetailGridApi.selection.getSelectedRows()[0];

                    if (!row) {
                        return;
                    }

                    caHisDataProxy.changePMTBsId(entity.id, row.id, function () {
                        alert("Operation success!");
                        $uibModalInstance.close(result);
                    }, function (error) {
                        alert(error);
                    });
                }

            }])
