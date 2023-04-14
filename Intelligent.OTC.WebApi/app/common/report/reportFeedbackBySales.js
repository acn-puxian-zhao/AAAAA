angular.module('app.reportFeedbackBySales', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/reportFeedbackBySales', {
                templateUrl: 'app/common/report/reportFeedbackBySales.tpl.html',
                controller: 'reportFeedbackBySalesCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('reportFeedbackBySalesCtrl',
    ['$scope', '$interval', 'reportProxy', 'modalService',
        function ($scope, $interval, reportProxy, modalService) {
            $scope.$parent.helloAngular = "OTC - Feedback Report";

            $scope.reportFeedbackBySalesList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'region', displayName: 'Region', width: '120' },
                    { field: 'sales', displayName: 'Sales', width: '200' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'balanceAmt', displayName: 'BalanceAmt', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'branchmanager', displayName: 'Branchmanager', width: '400' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            reportProxy.getFeedbackStatisticsBySales(function (result) {
                $scope.reportFeedbackBySalesList.data = result;
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
                    { field: 'region', displayName: 'Region', width: '100' },
                    { field: 'sales', displayName: 'Sales', width: '200' },
                    { field: 'ebName', displayName: 'EbName', width: '200' },
                    { field: 'creditTerm', displayName: 'CreditTerm', width: '120' },
                    { field: 'customerName', displayName: 'CustomerName', width: '200' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '120' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '120' },
                    { field: 'invoiceNum', displayName: 'InvoiceNum', width: '120' },
                    { field: 'invoiceDate', displayName: 'InvoiceDate', width: '120' },
                    { field: 'dueDate', displayName: 'DueDate', width: '120' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'balanceAmount', displayName: 'BalanceAmount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'ptpDate', displayName: 'PtpDate', width: '120' },
                    { field: 'overDueReason', displayName: 'OverDueReason', width: '200' },
                    { field: 'comments', displayName: 'Comments', width: '300' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            //单页容量变化
            $scope.pageSizeChanged = function (selectedLevelId) {
                reportProxy.getFeedbackDetailsBySales($scope.currentPage, selectedLevelId, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                })
            };

            //翻页
            $scope.pageChanged = function () {
                reportProxy.getFeedbackDetailsBySales($scope.currentPage, $scope.itemsperpage, function (result) {
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                }, function (error) {
                    alert(error);
                });
            };

            $scope.pageChanged();

            $scope.exportReport = function () {
                reportProxy.downloadFeedbackReportBySales(function (path) {
                    window.location = path;
                    alert("Export Successful!");
                }, function (res) {
                    alert(res);
                });
            };

        }]);