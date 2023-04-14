angular.module('app.dailyAgingreport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/dailyAgingreport', {
                templateUrl: 'app/common/report/dailyAgingreport.tpl.html',

                controller: 'dailyAgingreportCtrl',
                resolve: {
                    //首次加载第一页
                    statuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("029");
                    }],
                    //internallist: ['baseDataProxy', function (baseDataProxy) {
                    //    return baseDataProxy.SysTypeDetail("024");
                    //}],
                    legallist: ['siteProxy', function (siteProxy) {
                        return siteProxy.Site("");
                    }],
                    docTypelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("031");
                    }],
                    ebList: ['ebProxy', function (ebProxy) {
                        return ebProxy.Eb();
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('dailyAgingreportCtrl',
    ['$scope', '$interval', 'agingReportProxy', 'dailyAgingProxy', 'modalService', 'statuslist', 'legallist', 'ebList','docTypelist',
        function ($scope, $interval, agingReportProxy, dailyAgingProxy, modalService, statuslist, legallist, ebList, docTypelist) {
            var search = {};
            search.status = $scope.status;
            var now = new Date();
            var filter = '';
            dailyAgingProxy.queryDailyAgingReport(1, 20, filter, '', '', '', '', function (list) {
                if (list.length > 0) {
                    $scope.reportList.data = list;
                    $scope.totalItems = list[0].count;
                }
                else
                {
                    $scope.reportList.data = list;
                    $scope.totalItems = 0;
                }
            }, function (res) {
                alert(res);
            });
                       
            $scope.legallist = legallist;

            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'collector', displayName: 'Collector', width: '120' },
                    { field: 'legalEntity', displayName: 'Org Id', width: '120' },
                    { field: 'customerName', displayName: 'Customer Name', width: '120' },
                    { field: 'accntNumber', displayName: 'Accnt Number', width: '120' },
                    { field: 'siteUseId', displayName: 'SiteUseId', width: '100' },
                    { field: 'paymentTermDesc', displayName: 'Payment Term Desc', width: '170' },
                    { field: 'ebname', displayName: 'Ebname', width: '140' },
                    { field: 'overCreditLmt', displayName: 'Over Credit Lmt', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'funcCurrCode', displayName: 'Func Curr Code', width: '120' },
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due15Amt', displayName: '001-015', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due30Amt', displayName: '016-030', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due45Amt', displayName: '031-045', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due60Amt', displayName: '046-060', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due90Amt', displayName: '061-090', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due120Amt', displayName: '091-120', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'largerDue120Amt', displayName: '120+', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'totalAR', displayName: 'Total AR', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'totalOverDue', displayName: 'Total Over Due', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'lsr', displayName: 'LSR', width: '120' },
                    { field: 'fsr', displayName: 'FSR', width: '120' },
                    { field: 'specialNote', displayName: 'Special Note', width: '250' },
                    { field: 'comments', displayName: 'Payment Comment', width: '250' },
                    { field: 'ptpComment', displayName: 'PTP Comment', width: '250' },
                    { field: 'totalPTPAmount', displayName: 'Total PTP Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'disputeAmount', displayName: 'Dispute Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'customerODPercent', displayName: 'Customer Over Due %', width: '190', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'disputeODPercent', displayName: 'Over Due Analysis (Dispute) %', width: '190', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'ptpODPercent', displayName: 'Over Due Analysis (PTP) %', width: '200', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'othersODPercent', displayName: 'Over Due Analysis (Others) %', width: '190', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'disputeAnalysis', displayName: 'Dispute Analysis', width: '150' },
                    { field: 'automaticSendMailDate', displayName: 'Automatic Send Mail Date', width: '180' },
                    { field: 'automaticSendMailCount', displayName: 'Automatic Send Mail Count', width: '180', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'followUpCallDate', displayName: ' Follow Up Call Date', width: '150' },
                    { field: 'followUpCallCount', displayName: 'Follow Up Call Count', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'currentMonthCustomerContact', displayName: 'Current Month Customer Contact', width: '220' },
                    { field: 'comments', displayName: 'Comments', width: '300' },
                    { field: 'commentExpirationDate', displayName: 'Comment ExpirationDate', width: '150', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                    { field: 'commentLastDate', displayName: 'Comment LastDate', width: '190', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }

                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };
            //$scope.openInvoiceH = function (inNum) {
            //    console.log(inNum);
            //    var modalDefaults = {
            //        templateUrl: 'app/soa/invhistory/invhistory.tpl.html',
            //        controller: 'invHisCL',
            //        size: 'lg',
            //        resolve: {
            //            inNum: function () { return inNum; }
            //        }
            //        , windowClass: 'modalDialog'
            //    };
            //    modalService.showModal(modalDefaults, {}).then(function (result) {
            //    });
            //};

            $scope.searchReport = function () {
                //filstr = buildFilter();
                $scope.currentPage = 1;

                dailyAgingProxy.queryDailyAgingReport($scope.currentPage, $scope.itemsperpage, filter, $scope.legalEntity, $scope.custName, $scope.custNum, $scope.siteUseId, function (list) {
                    if (list.length > 0) {
                        $scope.reportList.data = list;
                        $scope.totalItems = list[0].count;
                    }
                    else {
                        $scope.reportList.data = list;
                        $scope.totalItems = 0;
                    }
                }, function (res) {
                    alert(res);
                });
            };

            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页     
            var filstr = "";
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];
            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                //filstr = buildFilter();

                var index = $scope.currentPage;

                dailyAgingProxy.queryDailyAgingReport($scope.currentPage, selectedLevelId, filter, $scope.legalEntity, $scope.custName, $scope.custNum, $scope.siteUseId, function (list) {
                    $scope.itemsperpage = selectedLevelId;
                    if (list.length > 0) {
                        $scope.reportList.data = list;
                        $scope.totalItems = list[0].count;
                    }
                    else {
                        $scope.reportList.data = list;
                        $scope.totalItems = 0;
                    }
                }, function (res) {
                    alert(res);
                });
            };

            //翻页
            $scope.pageChanged = function () {
                filstr = buildFilter();
                //alert("d");
                var index = $scope.currentPage;

                dailyAgingProxy.queryDailyAgingReport($scope.currentPage, $scope.itemsperpage, filter, $scope.legalEntity, $scope.custName, $scope.custNum, $scope.siteUseId, function (list) {
                    if (list.length > 0) {
                        $scope.reportList.data = list;
                        $scope.totalItems = list[0].count;
                    }
                    else {
                        $scope.reportList.data = list;
                        $scope.totalItems = 0;
                    }
                }, function (res) {
                    alert(res);
                });
            };

            $scope.resetSearch = function () {
                filstr = "";
                $scope.legalEntity = "";
                $scope.custNum = "";
                $scope.custName = "";
                $scope.siteUseId = "";
            }

            $scope.exportReport = function () {
                //filstr = buildFilter();
                dailyAgingProxy.downloadReport(filstr, $scope.legalEntity, $scope.custName, $scope.custNum, $scope.siteUseId, function (path) {
                    window.location = path;
                    alert("Export Successful!");
                },
                    function (res) { alert(res); });
            };

            $scope.exportDARReport = function () {
                dailyAgingProxy.downloadReportNew(filstr, $scope.legalEntity, $scope.custName, $scope.custNum, $scope.siteUseId, function (path) {
                    window.location = path;
                    alert("Export Successful!");
                },
                    function (res) { alert(res); });
            };

            buildFilter = function () {
                var search = {};
                search.legalentity = $scope.legalentity;
                search.custCode = $scope.custCode;
                search.custName = $scope.custName;
                search.siteUseId = $scope.siteUseId;
                search.status = $scope.status;
                search.invoicecode = $scope.invoicecode;
                search.eb = $scope.eb;
                search.docType = $scope.docType;
                search.poNum = $scope.poNum;
                search.soNum = $scope.soNum;
                search.creditTerm = $scope.creditTerm;
                search.invoiceMemo = $scope.invoiceMemo;
                search.ptpDateFrom = $scope.ptpDateFrom;
                search.ptpDateTo = $scope.ptpDateTo;
                search.invoiceDateFrom = $scope.invoiceDateFrom;
                search.invoiceDateTo = $scope.invoiceDateTo;
                search.duedateFrom = $scope.DuedateFrom;
                search.duedateTo = $scope.DuedateTo;
                return angular.toJson(search);
            };
        }]);




