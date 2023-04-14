angular.module('app.dailyReport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/dailyReport', {
            templateUrl: 'app/common/report/dailyreport-list.tpl.html',
            controller: 'reportCtrl',
            resolve: {
             
            }
        });
    }])

//*****************************************header***************************s
    .controller('reportCtrl', ['$scope', 'periodProxy', '$http','modalService','dailyReportProxy','uiGridConstants',
    function ($scope, periodProxy, $http, modalService, dailyReportProxy, uiGridConstants) {

        $scope.selectedLevel = 20;  //下拉单页容量初始化
        $scope.itemsperpage = 20;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页
        var filstr = "";

        dailyReportProxy.dailyReportPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
            $scope.totalItems = list[0].count;
            $scope.list = list[0].results;
        }, function (error) {
            alert(error);
        });


        $scope.CreateReport = function () {
            dailyReportProxy.refreshDownloadFile(function (res) {
                window.location = res;

                var index = $scope.currentPage;
                dailyReportProxy.dailyReportPaging($scope.currentPage, $scope.selectedLevel, filstr, function (list) {
                    $scope.itemsperpage = $scope.selectedLevel;
                    $scope.totalItems = list[0].count;
                    $scope.list = list[0].results;
                }, function (error) {
                    alert(error);
                });
                });
            }, function (error) {
                alert(error);
        }
        //分页容量下拉列表定义
        $scope.levelList = [
            { "id": 20, "levelName": '20' },
            { "id": 500, "levelName": '500' },
            { "id": 1000, "levelName": '1000' },
            { "id": 2000, "levelName": '2000' },
            { "id": 5000, "levelName": '5000' },
            { "id": 999999, "levelName": 'ALL' }
        ];
        $scope.collectorList = {
            data: 'collectorlist',
            //showGridFooter: true,
            //showColumnFooter: true,
            columnDefs: [
                { field: 'eid', displayName: 'EID', pinnedLeft: true, width: '100' },
                { field: 'teamName', displayName: 'TEAM', pinnedLeft: true, width: '165' },
                { field: 'totalOpenAcc', displayName: 'Total Open Accounts', width: '160' },  //, aggregationType: uiGridConstants.aggregationTypes.max, name: 'Max'
           //     { field: 'wip', displayName: 'SOA WIP', aggregationType: uiGridConstants.aggregationTypes.sum },
                { field: 'periodInialAmt', displayName: 'Period Initial Total Amt', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'finishSOAAcc', displayName: 'SOA Finished Accounts', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'closedAcc', displayName: 'Closed Accounts', width: '145', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'unFinishSOAAcc', displayName: 'SOA Unfinished Accounts', width: '170' },
                { field: 'coverageSOAAmt', displayName: 'SOA Coverage(AMT)', width: '160', cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.changFormat(row.entity.coverageSOAAmt)}}</div>' },
             //   { field: 'coverageSOAVol', displayName: 'SOA Coverage(VOL)', width: '100' },
                { field: 'coverageSOAVol', displayName: 'SOA Coverage(VOL)', width: '160', cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.changFormat(row.entity.coverageSOAVol)}}</div>' },
                { field: 'toBe2ndAcc', displayName: 'To-Be 2nd Dunning Accounts', width: '190' },
                { field: 'finish2ndAcc', displayName: '2nd Dunning Reminder Finished Accounts', width: '260' },
                { field: 'finish2ndAmt', displayName: '2nd Dunning Reminder Finished Amt', width: '230', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'toBe3ndAcc', displayName: 'To-Be Final Dunning Reminder Accounts', width: '250' },
                { field: 'finish3ndAcc', displayName: 'Final Dunning Reminder Finished Accounts', width: '265' },
                { field: 'finish3ndAmt', displayName: 'Final Dunning Reminder Finished Amt', width: '240', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'ptpAmt', displayName: 'PTP Amt', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'coveragePtpAmt', displayName: 'PTP Coverage(AMT)', width: '165', cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.changFormat(row.entity.coveragePtpAmt)}}</div>' },
                { field: 'paymentNoticeAmt', displayName: 'Payment Notice Amt', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                { field: 'coveragepaymentNoticeAmt', displayName: 'Payment Notice Coverage(AMT)', width: '210', cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.changFormat(row.entity.coveragepaymentNoticeAmt)}}</div>' },
                { field: 'disputeAmt', displayName: 'Dispute Amt', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
            ],
            //enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER // 去除横向拖动条
        }

        $scope.changFormat = function (obj) {
            var num = obj * 100;
            //var strnum = num + "";
            //var formatnum = strnum.substring(0, strnum.indexOf(".") + 3) + '%';
            var formatnum = num.toFixed(2) + '%';
            return formatnum;
        }

        //加载nggrid数据绑定
        $scope.reportList = {
            data: 'list',
            columnDefs: [
                    { field: 'periodId', displayName: 'PeriodId', width: '300' },
                    { field: 'operator', displayName: 'Operator', width: '300' },
                    { field: 'downloadTime', displayName: 'Create Time', width: '300', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                    {
                        name: "downlaod", displayName: 'DailyCollectorReport', width: '300',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
                                      '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity.downloadFileFullname)" title="DownLoad"></a>' +
                                      '</div>'
                    }
            ]
        };

        //download
        $scope.download = function (fullNamePath) {
            if (fullNamePath == null || fullNamePath == "") {
                alert("There is no need to download the file!");
            } else {
                window.location = fullNamePath;
            }
        };

        //单页容量变化
        $scope.pagesizechange = function (selectedLevelId) {
            var index = $scope.currentPage;
            dailyReportProxy.dailyReportPaging($scope.currentPage, selectedLevelId, filstr, function (list) {
                $scope.itemsperpage = selectedLevelId;
                $scope.totalItems = list[0].count;
                $scope.list = list[0].results;
            }, function (error) {
                alert(error);
            });
        };

        //翻页
        $scope.pageChanged = function () {

            var index = $scope.currentPage;
            dailyReportProxy.dailyReportPaging(index, $scope.itemsperpage, filstr, function (list) {
                $scope.list = list[0].results;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        }

        $scope.showReport=function(type){
            if (type == "execute") {
                $scope.reportshow = true;
                dailyReportProxy.collectorReportPaging("report", function (list) {
                    $scope.collectorlist = list;
                }, function (error) {
                    alert(error);
                })

            }
        }

    }])
