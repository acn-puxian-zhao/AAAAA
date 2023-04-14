angular.module('app.reportUnApply', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/reportUnApply', {
                templateUrl: 'app/common/report/reportUnApply.tpl.html',
                controller: 'reportUnApplyCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('reportUnApplyCtrl',
    ['$scope', '$interval', 'reportProxy', 'modalService',
        function ($scope, $interval, reportProxy, modalService) {
            $scope.$parent.helloAngular = "OTC - UnApply Report";

            $scope.reportUnApplyList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'region', displayName: 'Region', width: '120' },
                    { field: 'totalAR', displayName: 'TotalAR', width: '120' },
                    { field: 'type', displayName: 'Type', width: '300' },
                    { field: 'currency', displayName: 'Currency', width: '120' },
                    { field: 'amount', displayName: 'Amount', width: '120' },
                    { field: 'rate', displayName: 'Rate', width: '120' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            reportProxy.getUnApplyStatistics(function (result) {
                $scope.reportUnApplyList.data = result;
            }, function (error) {
                alert(error);
            });


            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页     
            $scope.startIndex = 0;

            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex}}</span>' },
                    { field: 'collector', displayName: 'Collector', width: '150' },
                    { field: 'region', displayName: 'Region', width: '150' },
                    { field: 'customerName', displayName: 'CustomerName', width: '150' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '80' },
                    { field: 'class', displayName: 'Class', width: '100' },
                    { field: 'invoiceNum', displayName: 'Invoice Num', width: '120' },
                    { field: 'invoiceDate', displayName: 'Invoice Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'dueDate', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'funcCurrCode', displayName: 'Func Curr Code', width: '120' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'dueDays', displayName: 'Due Days', width: '80' },
                    { field: 'invoiceAmount', displayName: 'Amt Remaining', width: '80' },
                    { field: 'agingBucket', displayName: 'Aging Bucket', width: '80' },
                    { field: 'creditTrem', displayName: 'CreditTrem', width: '100' },
                    { field: 'ebName', displayName: 'Ebname', width: '120' },
                    { field: 'lsr', displayName: 'LSR', width: '120' },
                    { field: 'sales', displayName: 'Sales', width: '120' },
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '100' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '100' },
                    { field: 'soNum', displayName: 'Sales Order', width: '120' },
                    { field: 'poNum', displayName: 'Cpo', width: '120' },
                    { field: 'ptpDate', displayName: '承诺付款日', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'overdueReason', displayName: '逾期原因', width: '150' },
                    { field: 'comments', displayName: '备注', width: '150' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            //单页容量变化
            $scope.pageSizeChanged = function (selectedLevelId) {
                reportProxy.getUnApplyDetails($scope.currentPage, selectedLevelId, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                })
            };

            //翻页
            $scope.pageChanged = function () {
                reportProxy.getUnApplyDetails($scope.currentPage, $scope.itemsperpage, function (result) {
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                }, function (error) {
                    alert(error);
                });
            };

            $scope.pageChanged();

            $scope.exportReport = function () {
                reportProxy.downloadUnApplyReport(function (path) {
                    window.location = path;
                    alert("Export Successful!");
                }, function (res) {
                    alert(res);
                });
            };

        }]);