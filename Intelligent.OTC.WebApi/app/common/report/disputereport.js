angular.module('app.disputereport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/disputereport', {
                templateUrl: 'app/common/report/disputereport.tpl.html',

                controller: 'disputereportCtrl',
                resolve: {
                    //首次加载第一页
                    trackstatuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("029");
                    }],
                    statuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("026");
                    }],
                    internallist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("025");
                    }],
                    legallist: ['siteProxy', function (siteProxy) {
                        return siteProxy.Site("");
                    }],
                    departmentlist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("038");
                    }],
                    ebList: ['ebProxy', function (ebProxy) {
                        return ebProxy.Eb();
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('disputereportCtrl',
    ['$scope', '$interval', 'disputeReportProxy', 'modalService', 'statuslist', 'internallist', 'legallist', 'departmentlist', 'ebList','trackstatuslist',
        function ($scope, $interval, disputeReportProxy, modalService, statuslist, internallist, legallist, departmentlist, ebList, trackstatuslist) {
            //console.log(statuslist);
            $scope.$parent.helloAngular = "OTC - Dispute Report";
            //var dispute = {};
            //dispute.detailName = "Dispute Identified";
            //dispute.detailValue = "000";
            //statuslist.push(dispute);

            $scope.legallist = legallist;
            $scope.statuslist = statuslist;
            $scope.internallist = internallist;
            $scope.departmentlist = departmentlist;
            $scope.ebList = ebList;
            $scope.trackstatuslist = trackstatuslist;
            //default
            //$scope.trackstatus = "007";
            //$scope.status = "026001";
            var search = {};
            //search.trackstatus = $scope.trackstatus;
            //search.status = $scope.status;
            disputeReportProxy.queryReport(1, 20, angular.toJson(search), function (json) {
                $scope.totalItems = json.totalItems;
                $scope.members = json.list;
            });
            var now = new Date();

            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: false,
                data: 'members',
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'customerName', displayName: 'Customer Name', width: '120' },
                    { field: 'accntNumber', displayName: 'Accnt Number', width: '120' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '100' },
                    { field: 'sellingLocationCode', displayName: 'Selling Location Code', width: '170' },
                    { field: 'class', displayName: 'Class', width: '70' },
                    { field: 'trxNum', displayName: 'Trx Num', width: '100' },
                    { field: 'trxDate', displayName: 'Trx Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'dueDate', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\''},
                    { field: 'paymentTermName', displayName: 'Payment Term Name', width: '150' },
                    { field: 'overCreditLmt', displayName: 'Over Credit Lmt', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'overCreditLmtAcct', displayName: 'Over Credit Lmt Acct', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'funcCurrCode', displayName: 'Func Curr Code', width: '110' },
                    { field: 'invCurrCode', displayName: 'Inv Curr Code', width: '110' },
                    { field: 'salesName', displayName: 'Sales Name', width: '110' },
                    { field: 'dueDays', displayName: 'Due Days', width: '110', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining', displayName: 'Amt Remaining', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amountWoVat', displayName: 'Amount Wo Vat', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'agingBucket', displayName: 'Aging Bucket', width: '110' },
                    { field: 'paymentTermDesc', displayName: 'Payment Term Desc', width: '150' },
                    { field: 'sellingLocationCode2', displayName: 'Selling Location Code2', width: '170' },
                    { field: 'ebname', displayName: 'Ebname', width: '120' },
                    { field: 'customertype', displayName: 'Customertype', width: '120' },
                    { field: 'isr', displayName: 'Isr', width: '120' },
                    { field: 'fsr', displayName: 'Fsr', width: '120' },
                    { field: 'orgId', displayName: 'Org Id', width: '80' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '80' },
                    { field: 'salesOrder', displayName: 'Sales Order', width: '100' },
                    { field: 'cpo', displayName: 'Cpo', width: '80' },
                    { field: 'fsrNameHist', displayName: 'Fsr Name Hist', width: '120' },
                    { field: 'isrNameHist', displayName: 'Isr Name Hist', width: '120' },
                    { field: 'eb', displayName: 'Eb', width: '120' },
                    { field: 'localName', displayName: 'Local Name', width: '120' },
                    { field: 'vatNo', displayName: 'VAT No.', width: '80' },
                    { field: 'vatDate', displayName: 'VAT date', width: '80' },
                    { field: 'collector', displayName: 'Collector', width: '80' },
                    { field: 'currentStatus', displayName: 'Current status', width: '120' },
                    { field: 'lastupdatedate', displayName: 'Last update date', width: '120', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    //{ field: 'clearingDocument', displayName: 'Clearing Document', width: '140' },
                    { field: 'clearingDate', displayName: 'Clearing Date', width: '120', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'ptpIdentifiedDate', displayName: 'PTP Identified Date', width: '150', cellFilter: 'date:\'yyyy-MM-dd\''},
                    { field: 'ptpDate', displayName: 'PTP date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'ptpDatehis', displayName: 'PTP Date His', width: '140' },
                    { field: 'ptpBroken', displayName: 'PTP Broken', width: '120' },
                    { field: 'ptpComment', displayName: 'PTP Comment', width: '140' },
                    { field: 'dispute', displayName: 'Dispute', width: '120' },
                    { field: 'disputeIdentifiedDate', displayName: 'Dispute Identified Date', width: '150', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'disputeStatus', displayName: 'Dispute Status', width: '120' },
                    { field: 'disputeReason', displayName: 'Dispute Reason', width: '120' },
                    { field: 'disputeComment', displayName: 'Dispute Comment', width: '140' },
                    { field: 'actionOwnerDepartment', displayName: 'Action Owner-Department', width: '160' },
                    { field: 'actionOwnerName', displayName: 'Action Owner-Name', width: '150' },
                    { field: 'nextActionDate', displayName: 'Next Action Date', width: '150', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    { field: 'commentsHelpNeeded', displayName: 'Invoice Memo', width: '160' },
                    { field: 'isForwarder', displayName: 'IsForward', width: '110' },
                    { field: 'forwarder', displayName: 'Forward', width: '90' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            $scope.searchReport = function () {
                //start llf 20171127 21:40
                var search = {};
                search.legalEntity = $scope.legalEntity;
                search.custCode = $scope.custCode;
                search.custName = $scope.custName;
                search.invoicecode = $scope.invoicecode;
                search.status = $scope.status;
                search.reason = $scope.reason;
                search.siteUseId = $scope.siteUseId;
                search.eb = $scope.eb;
                search.department = $scope.department;
                search.duedateFrom = $scope.DuedateFrom;
                search.duedateTo = $scope.DuedateTo;
                search.trackstatus = $scope.trackstatus;
                search.closed = $scope.closed;
                search.disclosed = $scope.disclosed;
                //end llf

                $scope.currentPage = 1;
                disputeReportProxy.queryReport($scope.currentPage, $scope.itemsperpage, angular.toJson(search), function (json) {
                    if (json != null) {
                        $scope.totalItems = json.totalItems;
                        $scope.members = json.list;
                    }
                })
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
                { "id": 5000, "levelName": '5000' }
            ];
            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                var search = {};
                search.legalEntity = $scope.legalEntity;
                search.custCode = $scope.custCode;
                search.custName = $scope.custName;
                search.invoicecode = $scope.invoicecode;
                search.status = $scope.status;
                search.reason = $scope.reason;
                search.siteUseId = $scope.siteUseId;
                search.eb = $scope.eb;
                search.department = $scope.department;
                search.duedateFrom = $scope.DuedateFrom;
                search.duedateTo = $scope.DuedateTo;
                search.trackstatus = $scope.trackstatus;
                search.closed = $scope.closed;
                search.disclosed = $scope.disclosed;
                var index = $scope.currentPage;
                disputeReportProxy.queryReport(index, selectedLevelId, angular.toJson(search), function (json) {
                    $scope.itemsperpage = selectedLevelId;
                    if (json.list != null) {
                        $scope.totalItems = json.totalItems;
                        $scope.members = json.list;
                    }
                })
            };

            //翻页
            $scope.pageChanged = function () {
                var search = {};
                search.legalEntity = $scope.legalEntity;
                search.custCode = $scope.custCode;
                search.custName = $scope.custName;
                search.invoicecode = $scope.invoicecode;
                search.status = $scope.status;
                search.reason = $scope.reason;
                search.siteUseId = $scope.siteUseId;
                search.eb = $scope.eb;
                search.department = $scope.department;
                search.duedateFrom = $scope.DuedateFrom;
                search.duedateTo = $scope.DuedateTo;
                search.trackstatus = $scope.trackstatus;
                search.closed = $scope.closed;
                search.disclosed = $scope.disclosed;
                //alert("d");
                var index = $scope.currentPage;
                disputeReportProxy.queryReport(index, $scope.itemsperpage, angular.toJson(search), function (json) {
                    $scope.members = json.list;
                    $scope.totalItems = json.totalItems;
                }, function (error) {
                    alert(error);
                });
            };

            $scope.resetSearch = function () {
                filstr = "";
                $scope.legalEntity = "";
                $scope.custCode = "";
                $scope.custName = "";
                $scope.status = "";
                $scope.invoicecode = "";
                $scope.reason = "";
                $scope.siteUseId = "";
                $scope.eb = "";
                $scope.department = "";
                $scope.DuedateFrom = "";
                $scope.DuedateTo = "";
                $scope.trackstatus = "";
                $scope.closed = "";
                $scope.disclosed = "";
            }

            $scope.exportReport = function () {
                var search = {};
                search.legalEntity = $scope.legalEntity;
                search.custCode = $scope.custCode;
                search.custName = $scope.custName;
                search.invoicecode = $scope.invoicecode;
                search.status = $scope.status;
                search.reason = $scope.reason;
                search.siteUseId = $scope.siteUseId;
                search.eb = $scope.eb;
                search.department = $scope.department;
                search.DuedateFrom = $scope.DuedateFrom;
                search.FuedateTo = $scope.DuedateTo;
                search.trackstatus = $scope.trackstatus;
                search.closed = $scope.closed;
                search.disclosed = $scope.disclosed;
                disputeReportProxy.downloadReport(angular.toJson(search), function (path) {
                    window.location = path;
                    alert("Export Successful!");
                },
                    function (res) { alert(res); });
            };

        }]);




