angular.module('app.agingreport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/agingreport', {
                templateUrl: 'app/common/report/arreport.tpl.html',

                controller: 'agingreportCtrl',
                resolve: {
                    //首次加载第一页
                    statuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("097");
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
    .controller('agingreportCtrl',
    ['$scope', '$interval', 'agingReportProxy', 'modalService', 'statuslist', 'legallist', 'ebList','docTypelist',
        function ($scope, $interval, agingReportProxy, modalService, statuslist, legallist, ebList, docTypelist) {
            $scope.$parent.helloAngular = "OTC - Aging Report";
            $scope.legallist = legallist;
            $scope.statuslist = statuslist;
            $scope.ebList = ebList;
            $scope.docuTypelist = docTypelist;
            $scope.status = "000";
            $scope.fromItem = 0;

            $scope.regionlist = [
                { "id": 'CN-CCNC', "levelName": 'CN-CCNC' },
                { "id": 'CN-Waching', "levelName": 'CN-Waching' },
                { "id": 'CN-SC', "levelName": 'CN-SC' },
                { "id": 'CN-SEED', "levelName": 'CN-SEED' },
                { "id": 'TW', "levelName": 'TW' },
                { "id": 'KR', "levelName": 'KR' },
                { "id": 'HK', "levelName": 'HK' },
                { "id": 'ASEAN', "levelName": 'ASEAN' },
                { "id": 'ATM-TW', "levelName": 'ATM-TW' },
                { "id": 'ATM-SZ', "levelName": 'ATM-SZ' },
                { "id": 'UA', "levelName": 'UA' },
                { "id": 'UA-TW', "levelName": 'UA-TW' }
            ];

            $scope.selectedLevel = 15;  //下拉单页容量初始化
            $scope.itemsperpage = 15;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页   
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            $scope.selectedLevelsummary = 15;  //下拉单页容量初始化
            $scope.itemsperpagesummary = 15;
            $scope.currentPagesummary = 1; //当前页
            $scope.maxSizesummary = 10; //分页显示的最大页   
            //分页容量下拉列表定义
            $scope.levelListsummary = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            var now = new Date();
                       
            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', displayName: 'No', field: '', enableSorting: false, pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'ebname', displayName: 'Ebname', width: '200' },
                    { field: 'customertype', displayName: 'Customertype', width: '100' },
                    { field: 'accntNumber', displayName: 'Accnt Number', width: '90' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '90' },
                    { field: 'customerName', displayName: 'Customer Name', width: '200' },
                    { field: 'sellingLocationCode', displayName: 'Selling Location Code', width: '100' },
                    { field: 'class', displayName: 'Class', width: '70' },
                    { field: 'trxNum', displayName: 'Trx Num', width: '100' },
                    { field: 'trxDate', displayName: 'Trx Date', width: '80', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                    { field: 'dueDate', displayName: 'Due Date', width: '80', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                    { field: 'dueDays', displayName: 'Due Days', width: '80', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining', displayName: 'Amt Remaining', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amountWoVat', displayName: 'Amount Wo Vat', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'paymentTermName', displayName: 'Payment Term Name', width: '120' },
                    { field: 'overCreditLmt', displayName: 'Over Credit Lmt', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'overCreditLmtAcct', displayName: 'Over Credit Lmt Acct', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'funcCurrCode', displayName: 'Func Curr Code', width: '100', cellClass: 'center' },
                    { field: 'invCurrCode', displayName: 'Inv Curr Code', width: '100', cellClass: 'center' },
                    { field: 'salesName', displayName: 'Sales Name', width: '100' },
                    { field: 'agingBucket', displayName: 'Aging Bucket', width: '100' },
                    { field: 'paymentTermDesc', displayName: 'Payment Term Desc', width: '200' },
                    { field: 'sellingLocationCode2', displayName: 'Selling Location Code2', width: '100' },
                    { field: 'isr', displayName: 'Isr', width: '100' },
                    { field: 'fsr', displayName: 'Fsr', width: '100' },
                    { field: 'orgId', displayName: 'Org Id', width: '70' },
                    { field: 'cmpinv', displayName: 'Cmpinv', width: '100' },
                    { field: 'salesOrder', displayName: 'Sales Order', width: '100' },
                    { field: 'cpo', displayName: 'Cpo', width: '100' },
                    { field: 'fsrNameHist', displayName: 'Fsr Name Hist', width: '100' },
                    { field: 'isrNameHist', displayName: 'Isr Name Hist', width: '100' },
                    { field: 'eb', displayName: 'Eb', width: '200' },
                    { field: 'amtRemainingTran', displayName: 'Amt Remaining Tran', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                var index = $scope.currentPage;
                if (!$scope.selected_region || $scope.selected_region.length == 0) {
                    alert("Please select one Region.");
                    return;
                }
                var regionString = $scope.getRegionString();
                agingReportProxy.queryReport(regionString, $scope.legalentity, $scope.custName, $scope.siteUseId, $scope.invoicecode, $scope.status, $scope.docType, $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.invoiceMemo, $scope.eb, /*$scope.ptpDateFrom, $scope.ptpDateTo,*/ $scope.invoiceDateFrom, $scope.invoiceDateTo, $scope.DuedateFrom, $scope.DuedateTo, index, selectedLevelId, function (json) {
                    $scope.itemsperpage = selectedLevelId;
                    if (json !== null) {
                        $scope.reportList.data = json.detail;
                        $scope.totalItems = json.detailcount;
                    }
                    $scope.calculate(index, $scope.itemsperpage, json.detail.length);
                });
            };

            //翻页
            $scope.pageChanged = function () {
                //alert("d");
                var index = $scope.currentPage;
                if (!$scope.selected_region || $scope.selected_region.length == 0) {
                    alert("Please select one Region.");
                    return;
                }
                var regionString = $scope.getRegionString();
                agingReportProxy.queryReport(regionString, $scope.legalentity, $scope.custName, $scope.siteUseId, $scope.invoicecode, $scope.status, $scope.docType, $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.invoiceMemo, $scope.eb, /*$scope.ptpDateFrom, $scope.ptpDateTo,*/ $scope.invoiceDateFrom, $scope.invoiceDateTo, $scope.DuedateFrom, $scope.DuedateTo, index, $scope.itemsperpage, function (json) {
                    $scope.reportList.data = json.detail;
                    $scope.totalItems = json.detailcount;
                    $scope.calculate(index, $scope.itemsperpage, json.detail.length);
                }, function (error) {
                    alert(error);
                });

            };


            $scope.reportListsummary = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: false,
                columnDefs: [
                    { name: 'RowNo', displayName: 'No', field: '', enableSorting: false, pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'ebname', displayName: 'Ebname', width: '200' },
                    { field: 'accntNumber', displayName: 'Accnt Number', width: '90', cellClass: 'center' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '90', cellClass: 'center' },
                    { field: 'customerName', displayName: 'Customer Name', width: '200' },
                    { field: 'paymentTermDesc', displayName: 'Payment Term Name', width: '120' },
                    { field: 'overCreditLmt', displayName: 'Over Credit Lmt', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'funcCurrCode', displayName: 'Func Curr Code', width: '100', cellClass: 'center' },
                    { field: 'fsr', displayName: 'Fsr', width: '100' },
                    { field: 'amtRemaining01To15', displayName: 'AmtRemaining01To15', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'amtRemaining16To30', displayName: 'AmtRemaining16To30', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'amtRemaining31To45', displayName: 'AmtRemaining31To45', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'amtRemaining46To60', displayName: 'AmtRemaining46To60', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining61To90', displayName: 'AmtRemaining61To90', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining91To120', displayName: 'AmtRemaining91To120', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining121To180', displayName: 'AmtRemaining121To180', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining181To270', displayName: 'AmtRemaining181To270', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining271To360', displayName: 'AmtRemaining271To360', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemaining360Plus', displayName: 'AmtRemaining360Plus', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'amtRemainingTotalFutureDue', displayName: 'AmtRemainingTotalFutureDue', width: '170', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //单页容量变化
            $scope.pagesizechangesummary = function (selectedLevelIdsummary) {
                var index = $scope.currentPagesummary;
                if (!$scope.selected_region || $scope.selected_region.length == 0) {
                    return;
                }
                var regionString = $scope.getRegionString();
                agingReportProxy.querysummary(regionString, $scope.legalentity, $scope.custName, $scope.siteUseId, $scope.invoicecode, $scope.status, $scope.docType, $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.invoiceMemo, $scope.eb, /*$scope.ptpDateFrom, $scope.ptpDateTo,*/ $scope.invoiceDateFrom, $scope.invoiceDateTo, $scope.DuedateFrom, $scope.DuedateTo, index, selectedLevelIdsummary, function (json) {
                    $scope.itemsperpagesummary = selectedLevelIdsummary;
                    if (json !== null) {
                        $scope.reportListsummary.data = json.summary;
                        $scope.totalItemssummary = json.summarycount;
                    }
                    $scope.calculatesummary(index, $scope.itemsperpagesummary, json.summary.length);
                });
            };

            //翻页
            $scope.pageChangedsummary = function () {
                //alert("d");
                var index = $scope.currentPagesummary;
                if (!$scope.selected_region || $scope.selected_region.length == 0) {
                    return;
                }
                var regionString = $scope.getRegionString();
                agingReportProxy.querysummary(regionString, $scope.legalentity, $scope.custName, $scope.siteUseId, $scope.invoicecode, $scope.status, $scope.docType, $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.invoiceMemo, $scope.eb, /*$scope.ptpDateFrom, $scope.ptpDateTo,*/ $scope.invoiceDateFrom, $scope.invoiceDateTo, $scope.DuedateFrom, $scope.DuedateTo, index, $scope.itemsperpagesummary, function (json) {
                    $scope.reportListsummary.data = json.summary;
                    $scope.totalItemssummary = json.summarycount;
                    $scope.calculatesummary(index, $scope.itemsperpagesummary, json.summary.length);
                }, function (error) {
                    alert(error);
                });
            };

            $scope.searchReport = function () {
                $scope.pageChanged();
                $scope.pageChangedsummary();
            };

            $scope.resetSearch = function () {
                filstr = "";
                $scope.region = "";
                $scope.custName = "";
                $scope.status = "000";
                $scope.legalentity = "";
                $scope.invoicecode = "";
                $scope.DuedateFrom = "";
                $scope.DuedateTo = "";
                $scope.siteUseId = "";
                $scope.eb = "";
                $scope.docType = "";
                $scope.poNum = "";
                $scope.soNum = "";
                $scope.creditTerm = "";
                $scope.invoiceMemo = "";
                //$scope.ptpDateFrom = "";
                //$scope.ptpDateTo = "";
                $scope.invoiceDateFrom = "";
                $scope.invoiceDateTo = "";
                $scope.reportList.data = [];
                $scope.reportListsummary.data = [];
            };

            $scope.resetSearch();

            $scope.calculate = function (currentPage, itemsperpage, count) {
                if (count === 0) {
                    $scope.fromItem = 0;
                } else {
                    $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                }
                $scope.toItem = (currentPage - 1) * itemsperpage + count;
            };
            $scope.calculatesummary = function (currentPagesummary, itemsperpagesummary, count) {
                if (count === 0) {
                    $scope.fromItemsummary = 0;
                } else {
                    $scope.fromItemsummary = (currentPagesummary - 1) * itemsperpagesummary + 1;
                }
                $scope.toItemsummary = (currentPagesummary - 1) * itemsperpagesummary + count;
            };

            $scope.exportReport = function () {
                if (!$scope.selected_region || $scope.selected_region.length == 0) {
                    alert("Please select one Region.");
                    return;
                }
                var regionString = $scope.getRegionString();
                agingReportProxy.downloadReport(regionString, $scope.legalentity, $scope.custName, $scope.siteUseId, $scope.invoicecode, $scope.status, $scope.docType, $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.invoiceMemo, $scope.eb, /*$scope.ptpDateFrom, $scope.ptpDateTo,*/ $scope.invoiceDateFrom, $scope.invoiceDateTo, $scope.DuedateFrom, $scope.DuedateTo, function (path) {
                    if (path !== null) {
                        window.location = path;
                        alert("Export Successful!");
                    }
                });
            };

            // 点击选中事件
            $scope.updateAll_region = function (val) {
                if (val) {
                    $scope.selected_region = [];
                    $scope.regionlist.forEach(row => {
                        row.check = true;
                        $scope.selected_region.push(row.levelName);
                    });
                } else {
                    $scope.selected_region = [];

                    $scope.regionlist.forEach(row => {
                        row.check = false;
                    });
                }
            };

            //默认全选
            $scope.updateAll_region(true);
            
            // 复选框选中事件
            $scope.updateSelected_region = function ($event, id) {
                let checkbox = $event.target;
                let action = (checkbox.checked ? 'add' : 'delete');
                if (action === 'add' && $scope.selected_region.indexOf(id) === -1) {
                    $scope.selected_region.push(id);
                    $scope.regionlist.forEach(row => {
                        if (row === id) {
                            row.check = true;
                        }
                    });
                }
                if (action === 'delete' && $scope.selected_region.indexOf(id) !== -1) {
                    let idx = $scope.selected_region.indexOf(id);
                    $scope.selected_region.splice(idx, 1);
                    $scope.selected_region.forEach(row => {
                        if (row === id) {
                            row.check = false;
                        }
                    });
                }
            };

            $scope.getRegionString = function () {
                var regionString;
                $scope.selected_region.forEach(row => {
                    if (regionString) {
                        regionString += ";" + row;
                    } else {
                        regionString = row;
                    }
                });
                return regionString;
            };

        }]);




