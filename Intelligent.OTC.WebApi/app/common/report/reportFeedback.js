angular.module('app.reportFeedback', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/reportFeedback', {
                templateUrl: 'app/common/report/reportFeedback.tpl.html',
                controller: 'reportFeedbackCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('reportFeedbackCtrl',
        ['$scope', '$filter', '$interval', 'reportProxy', 'modalService',
            function ($scope, $filter, $interval, reportProxy, modalService) {
            $scope.$parent.helloAngular = "OTC - Feedback Report";

            $scope.reportFeedbackList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'region', displayName: 'Region', width: '120' },
                    { field: 'totalCount', displayName: 'SOA Count(Wave1 SOA)', width: '300' },
                    { field: 'feedbackCount', displayName: 'FeedBack Count', width: '200' },
                    { field: 'rate', displayName: 'Rate', width: '120' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            reportProxy.getFeedbackStatistics(function (result) {
                $scope.reportFeedbackList.data = result;
            }, function (error) {
                alert(error);
            });

            $scope.startIndex = 0;
            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页  

            $scope.startIndex_notfeedback = 0;
            $scope.selectedLevel_notfeedback = 20;  //下拉单页容量初始化
            $scope.itemsperpage_notfeedback = 20;
            $scope.currentPage_notfeedback = 1; //当前页
            $scope.maxSize_notfeedback = 10; //分页显示的最大页   

            $scope.startIndex_hasfeedback = 0;
            $scope.selectedLevel_hasfeedback = 20;  //下拉单页容量初始化
            $scope.itemsperpage_hasfeedback = 20;
            $scope.currentPage_hasfeedback = 1; //当前页
            $scope.maxSize_hasfeedback = 10; //分页显示的最大页 

            $scope.startIndex_hist = 0;
            $scope.selectedLevel_hist = 20;  //下拉单页容量初始化
            $scope.itemsperpage_hist = 20;
            $scope.currentPage_hist = 1; //当前页
            $scope.maxSize_hist = 10; //分页显示的最大页 

            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            $scope.startDate = new Date();

            $scope.popup = {
                opened: false
            };

            $scope.open = function () {
                $scope.popup.opened = true;
            };

            $scope.notFeedbackList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex_notfeedback}}</span>' },
                    { field: 'region', displayName: 'Region', width: '50' },
                    { field: 'collector', displayName: 'Collector', width: '100' },
                    { field: 'organization', displayName: 'OrgId', width: '60' },
                    { field: 'customerName', displayName: 'CustomerName', width: '200' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '100'},
                    { field: 'class', displayName: 'Class', width: '60' },
                    { field: 'invoiceNum', displayName: 'InvoiceNum', width: '120' },
                    { field: 'invoiceDate', displayName: 'InvoiceDate', width: '100' },
                    { field: 'dueDate', displayName: 'DueDate', width: '80' },
                    { field: 'funcCurrCode', displayName: 'FuncCurrCode', width: '100' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'daysLateSys', displayName: 'DaysLateSys', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'balanceAmt', displayName: 'BalanceAmt', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'agingBucket', displayName: 'AgingBucket', width: '120' },
                    { field: 'creditTremDescription', displayName: 'CreditTremDescription', width: '130' },
                    { field: 'ebname', displayName: 'Ebname', width: '150' },
                    { field: 'lsrNameHist', displayName: 'LsrNameHist', width: '120' },
                    { field: 'fsrNameHist', displayName: 'FsrNameHist', width: '120' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '100' },
                    { field: 'soNum', displayName: 'SoNum', width: '100' },
                    { field: 'poNum', displayName: 'PoNum', width: '100' },
                    { field: 'ptpDate', displayName: 'PtpDate', width: '100' },
                    { field: 'overdueReason', displayName: 'OverdueReason', width: '150' },
                    { field: 'comments', displayName: 'Comments', width: '150' },
                    { field: 'status', displayName: 'Status', width: '80' },
                    { field: 'closeDate', displayName: 'CloseDate', width: '100' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.hasFeedbackList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex_notfeedback}}</span>' },
                    { field: 'region', displayName: 'Region', width: '50' },
                    { field: 'collector', displayName: 'Collector', width: '100' },
                    { field: 'organization', displayName: 'OrgId', width: '60' },
                    { field: 'customerName', displayName: 'CustomerName', width: '200' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '100' },
                    { field: 'class', displayName: 'Class', width: '60' },
                    { field: 'invoiceNum', displayName: 'InvoiceNum', width: '120' },
                    { field: 'invoiceDate', displayName: 'InvoiceDate', width: '100' },
                    { field: 'dueDate', displayName: 'DueDate', width: '80' },
                    { field: 'funcCurrCode', displayName: 'FuncCurrCode', width: '100' },
                    { field: 'currency', displayName: 'Currency', width: '100' },
                    { field: 'daysLateSys', displayName: 'DaysLateSys', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'balanceAmt', displayName: 'BalanceAmt', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'agingBucket', displayName: 'AgingBucket', width: '120' },
                    { field: 'creditTremDescription', displayName: 'CreditTremDescription', width: '130' },
                    { field: 'ebname', displayName: 'Ebname', width: '150' },
                    { field: 'lsrNameHist', displayName: 'LsrNameHist', width: '120' },
                    { field: 'fsrNameHist', displayName: 'FsrNameHist', width: '120' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '100' },
                    { field: 'soNum', displayName: 'SoNum', width: '100' },
                    { field: 'poNum', displayName: 'PoNum', width: '100' },
                    { field: 'ptpDate', displayName: 'PtpDate', width: '100' },
                    { field: 'overdueReason', displayName: 'OverdueReason', width: '150' },
                    { field: 'comments', displayName: 'Comments', width: '150' },
                    { field: 'status', displayName: 'Status', width: '80' },
                    { field: 'closeDate', displayName: 'CloseDate', width: '100' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex}}</span>' },
                    { field: 'region', displayName: 'Region', width: '70' },
                    { field: 'collector', displayName: 'Collector', width: '100' },
                    { field: 'organization', displayName: 'Organization', width: '80' },
                    { field: 'customerName', displayName: 'CustomerName', width: '150' },
                    { field: 'creditTerm', displayName: 'CreditTerm', width: '120' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '100' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '100' },
                    { field: 'class', displayName: 'Class', width: '80' },
                    { field: 'invoiceNum', displayName: 'InvoiceNum', width: '100' },
                    { field: 'invoiceDate', displayName: 'InvoiceDate', width: '80' },
                    { field: 'dueDate', displayName: 'DueDate', width: '80' },
                    { field: 'funcCurrCode', displayName: 'FuncCurrCode', width: '80' },
                    { field: 'currency', displayName: 'Currency', width: '70' },
                    { field: 'dueDays', displayName: 'DueDays', width: '80' },
                    { field: 'invoiceAmount', displayName: 'InvoiceAmount', width: '120' },
                    { field: 'agingBucket', displayName: 'AgingBucket', width: '120' },
                    { field: 'creditTremDesc', displayName: 'CreditTremDesc', width: '130' },
                    { field: 'ebName', displayName: 'EbName', width: '120' },
                    { field: 'cs', displayName: 'CS', width: '120' },
                    { field: 'sales', displayName: 'Sales', width: '120' },
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '100' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '150' },
                    { field: 'soNum', displayName: 'SONum', width: '150' },
                    { field: 'poNum', displayName: 'PONum', width: '150' },
                    { field: 'ptpDate', displayName: 'PtpDate', width: '120' },
                    { field: 'overdueReason', displayName: 'OverdueReason', width: '150' },
                    { field: 'comments', displayName: 'Comments', width: '150' },
                    { field: 'memoExpirationDate', displayName: 'Comments ExpirationDate', width: '150', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                    { field: 'status', displayName: 'Status', width: '100' },
                    { field: 'closeDate', displayName: 'CloseDate', width: '120' },
                    { field: 'feedback', displayName: 'FeedBack', width: '120' },
                    { field: 'sendDate', displayName: 'SendDate', width: '200' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

                $scope.feedbackhistList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: false,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', width: '50', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex_notfeedback}}</span>' },
                    { field: 'reportdate', displayName: 'Report Date', width: '200', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                    { field: 'filename', displayName: 'File Name', width: '800' },
                    { field: 'download', displayName: 'Download', width: '100', cellTemplate:'<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.downloadhist(row.entity.filepath)" ng-show="true" title="DownLoad">&nbsp;DownLoad</a>' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            //NotFeedback单页容量变化
            $scope.pageSizeChanged_notfeedback = function (selectedLevel_notfeedback) {
                //reportProxy.getNotFeedbackList($scope.currentPage_notfeedback, selectedLevel_notfeedback, function (result) {
                //    $scope.itemsperpage_notfeedback = selectedLevel_notfeedback;
                //    $scope.totalItems_notfeedback = result.count;
                //    $scope.notFeedbackList.data = result.dataRows;
                //    $scope.startIndex_notfeedback = ($scope.currentPage_notfeedback - 1) * $scope.itemsperpage_notfeedback;
                //});
            };

            //NotFeedback翻页
            $scope.pageChanged_notfeedback = function () {
                //reportProxy.getNotFeedbackList($scope.currentPage_notfeedback, $scope.itemsperpage_notfeedback, function (result) {
                //    $scope.totalItems_notfeedback = result.count;
                //    $scope.notFeedbackList.data = result.dataRows;
                //    $scope.startIndex_notfeedback = ($scope.currentPage_notfeedback - 1) * $scope.itemsperpage_notfeedback;
                //}, function (error) {
                //    alert(error);
                //});
            };

            //HasFeedback单页容量变化
            $scope.pageSizeChanged_hasfeedback = function (selectedLevel_hasfeedback) {
                //reportProxy.getHasFeedbackList($scope.currentPage_hasfeedback, selectedLevel_hasfeedback, function (result) {
                //    $scope.itemsperpage_hasfeedback = selectedLevel_hasfeedback;
                //    $scope.totalItems_hasfeedback = result.count;
                //    $scope.hasFeedbackList.data = result.dataRows;
                //    $scope.startIndex_hasfeedback = ($scope.currentPage_hasfeedback - 1) * $scope.itemsperpage_hasfeedback;
                //});
            };

            //HasFeedback翻页
            $scope.pageChanged_hasfeedback = function () {
                //reportProxy.getHasFeedbackList($scope.currentPage_hasfeedback, $scope.itemsperpage_hasfeedback, function (result) {
                //    $scope.totalItems_hasfeedback = result.count;
                //    $scope.hasFeedbackList.data = result.dataRows;
                //    $scope.startIndex_hasfeedback = ($scope.currentPage_hasfeedback - 1) * $scope.itemsperpage_hasfeedback;
                //}, function (error) {
                //    alert(error);
                //});
            };


            //FeedbackHistory单页容量变化
            $scope.pageSizeChanged_hist = function (selectedLevel_hist) {
                reportProxy.getFeedbackHistoryList($scope.currentPage_hist, selectedLevel_hist, function (result) {
                    $scope.itemsperpage_hist = selectedLevel_hist;
                    $scope.totalItems_hist = result.count;
                    $scope.feedbackhistList.data = result.dataRows;
                    $scope.startIndex_hist = ($scope.currentPage_hist - 1) * $scope.itemsperpage_hist;
                });
            };

            //FeedbackHistory翻页
            $scope.pageChanged_hist = function () {
                reportProxy.getFeedbackHistoryList($scope.currentPage_hist, $scope.itemsperpage_hist, function (result) {
                    $scope.totalItems_hist = result.count;
                    $scope.feedbackhistList.data = result.dataRows;
                    $scope.startIndex_hist = ($scope.currentPage_hist - 1) * $scope.itemsperpage_hist;
                }, function (error) {
                    alert(error);
                });
            };

            $scope.pageChanged_notfeedback();
            $scope.pageChanged_hasfeedback();
            $scope.pageChanged_hist();

            //下载Feedback Report
            $scope.exportReport = function () {
                reportProxy.downloadFeedbackReport(function (path) {
                    window.location = path;
                    alert("Export Successful!");
                }, function (res) {
                    alert(res);
                });
            };

            $scope.downloadhist = function (filepath) {
                window.location = filepath;
            };

            //Detail单页容量变化
            $scope.pageSizeChanged = function (selectedLevelId) {
                if ($scope.startDate === null || $scope.startDate === "") {
                    alert("Please select start date!");
                    return;
                } 
                var sDate = $filter('date')($scope.startDate, "yyyy-MM");
                reportProxy.getFeedbackDetails(sDate,$scope.currentPage, selectedLevelId, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                })
            };

            //Detail翻页
            $scope.pageChanged = function () {
                if ($scope.startDate === null || $scope.startDate === "") {
                    alert("Please select start date!");
                    return;
                } 
                var sDate = $filter('date')($scope.startDate, "yyyy-MM");
                reportProxy.getFeedbackDetails(sDate, $scope.currentPage, $scope.itemsperpage, function (result) {
                    $scope.totalItems = result.count;
                    $scope.reportList.data = result.dataRows;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                }, function (error) {
                    alert(error);
                });
            };

            //检索明细
            $scope.QueryDetail = function () {
                $scope.pageChanged();
            };

            $scope.exportDetail = function () {
                if ($scope.startDate === null || $scope.startDate === "") {
                    alert("Please select start date!");
                    return;
                } 
                var sDate = $filter('date')($scope.startDate, "yyyy-MM");
                reportProxy.downloadFeedbackDetail(sDate,function (path) {
                    window.location = path;
                    alert("Export Successful!");
                }, function (res) {
                    alert(res);
                });
            };

        }]);