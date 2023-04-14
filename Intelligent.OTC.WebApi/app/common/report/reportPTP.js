angular.module('app.reportPTP', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/reportPTP', {
                templateUrl: 'app/common/report/reportPTP.tpl.html',
                controller: 'reportPTPCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('reportPTPCtrl',
    ['$scope', '$interval', 'reportProxy', 'modalService',
        function ($scope, $interval, reportProxy, modalService) {
            $scope.$parent.helloAngular = "OTC - PTP Report";

            $scope.reportPTPList1 = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'region', displayName: 'Region', width: '80' },
                    { field: 'customerBrokenCount', displayName: 'Broken Count', width: '100' },
                    { field: 'customerPTPCount', displayName: 'PTP Count', width: '100' },
                    { field: 'countRate', displayName: 'Count Rate', width: '100', cellTemplate: '<span style="line-height:30px;vertical-align:middle;display:block; margin-left:10px;">{{row.entity.countRate}} %</span>' },
                    { field: 'currency', displayName: 'Currency', width: '80' },
                    { field: 'brokenAmount', displayName: 'Broken Amount', width: '100' },
                    { field: 'ptpAmount', displayName: 'PTP Amount', width: '100' },
                    { field: 'amountRate', displayName: 'Amount Rate', width: '100', cellTemplate: '<span style="line-height:30px;vertical-align:middle;display:block; margin-left:10px;">{{row.entity.amountRate}} %</span>' },
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.reportPTPList2 = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'region', displayName: 'Region', width: '80' },
                    { field: 'customerPTPCount', displayName: 'PTP Count', width: '100' },
                    { field: 'customerODCount', displayName: 'OD Count', width: '100' },
                    { field: 'countRate', displayName: 'Count Rate', width: '100', cellTemplate: '<span style="line-height:30px;vertical-align:middle;display:block; margin-left:10px;">{{row.entity.countRate}} %</span>' },
                    { field: 'currency', displayName: 'Currency', width: '80' },
                    { field: 'ptpAmount', displayName: 'PTP Amount', width: '100' },
                    { field: 'odAmount', displayName: 'OD Amount', width: '100' },
                    { field: 'amountRate', displayName: 'Amount Rate', width: '100', cellTemplate: '<span style="line-height:30px;vertical-align:middle;display:block; margin-left:10px">{{row.entity.amountRate}} %</span>' },
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            reportProxy.getPTPStatistics(function (result) {
                var data1 = [];
                var data2 = [];
                for (var i = 0; i < result.length; i++) {
                    if (result[i].category == "Confirm") {
                        data2.push(result[i]);
                    } else {
                        data1.push(result[i]);
                    }
                }
                $scope.reportPTPList1.data = data1;
                $scope.reportPTPList2.data = data2;
            }, function (error) {
                alert(error);
            });

            $scope.category = 0;
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
                    { field: 'customerName', displayName: 'CustomerName', width: '150' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '80' },
                    { field: 'invoiceType', displayName: 'Class', width: '100' },
                    { field: 'invoiceNum', displayName: 'Invoice Num', width: '120' },
                    { field: 'invoiceDate', displayName: 'Invoice Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'dueDate', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'funcCurrency', displayName: 'Func Curr Code', width: '120' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'dueDays', displayName: 'Due Days', width: '80' },
                    { field: 'odAmount', displayName: 'Amt Remaining', width: '80' },
                    { field: 'agingBucket', displayName: 'Aging Bucket', width: '80' },
                    { field: 'creditTrem', displayName: 'CreditTrem', width: '100' },
                    { field: 'ebName', displayName: 'Ebname', width: '120' },
                    { field: 'lsrNameHist', displayName: 'LSR', width: '120' },
                    { field: 'fsrNameHist', displayName: 'Sales', width: '120' },
                    { field: 'organization', displayName: 'Organization', width: '100' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '100' },
                    { field: 'soNum', displayName: 'Sales Order', width: '120' },
                    { field: 'poNum', displayName: 'Cpo', width: '120' },
                    { field: 'ptpDate', displayName: '承诺付款日', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'overdueReason', displayName: '逾期原因', width: '150' },
                    { field: 'remark', displayName: '备注', width: '150' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.changeCategory = function (category) {
                $scope.category = category;
                $scope.currentPage = 1;
                $scope.pageChanged();
            }

            //单页容量变化
            $scope.pageSizeChanged = function (selectedLevelId) {
                reportProxy.getPTPDetails($scope.currentPage, selectedLevelId, $scope.category, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                })
            };

            //翻页
            $scope.pageChanged = function () {
                reportProxy.getPTPDetails($scope.currentPage, $scope.itemsperpage, $scope.category, function (result) {
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                }, function (error) {
                    alert(error);
                });
            };

            $scope.pageChanged();

            $scope.exportReport = function () {
                reportProxy.downloadPTPReport(function (path) {
                    window.location = path;
                    alert("Export Successful!");
                }, function (res) {
                    alert(res);
                });
            };

        }]);