angular.module('app.myinvoices', ['ui.grid.edit', 'ngSanitize'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/myinvoices', {
                templateUrl: 'app/myinvoices/myinvoices.tpl.html',
                controller: 'myinvoicesCtrl',
                resolve: {
                }
            });
    }])
    .controller('myinvoicesCtrl',
        ['$scope', '$sce', 'modalService', '$interval', 'invoiceProxy', '$routeParams', 'mailProxy', 'customerPaymentbankProxy',
            'customerPaymentcircleProxy', 'collectorSoaProxy', 'generateSOAProxy', 'contactCustomerProxy', 'baseDataProxy',
            'APPSETTING', 'siteProxy', 'uiGridConstants', 'dunningProxy', 'commonProxy', 'myinvoicesProxy', 'permissionProxy', 'ebProxy',
            'contactProxy', '$q', 'contactHistoryProxy',
            function ($scope, $sce, modalService, $interval, invoiceProxy, $routeParams, mailProxy, customerPaymentbankProxy,
                customerPaymentcircleProxy, collectorSoaProxy, generateSOAProxy, contactCustomerProxy, baseDataProxy,
                APPSETTING, siteProxy, uiGridConstants, dunningProxy, commonProxy, myinvoicesProxy, permissionProxy, ebProxy,
                contactProxy, $q, contactHistoryProxy
            ) {
                $scope.$parent.helloAngular = "OTC - All Invoices";
                $scope.custCode = $routeParams.custNo;
                $scope.siteUseid = $routeParams.siteUseID;
                //##########################
                //init parameter
                //##########################
                $scope.floatMenuOwner = ['allinfoCtrl'];
                $scope.maxSize = 10; //paging display max
                $scope.slexecute = 15;  //init paging size(ng-model)
                $scope.iperexecute = 15;   //init paging size(parameter)
                $scope.curpexecute = 1;     //init page
                $scope.totalNum = "";
                $scope.displayClosed = 0;

                $scope.mType = "001";

                $scope.fileTypeList = [
                    { "id": "ALL", "levelName": 'ALL' },
                    { "id": "PDF", "levelName": 'PDF' },
                    { "id": "XLS", "levelName": 'XLS' }
                ];

                $scope.fileType = "XLS";

                $scope.levelList = [
                    { "id": 15, "levelName": '15' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' },
                    { "id": 999999, "levelName": 'ALL' }
                ];
                $scope.displaylist = [
                    { "id": 0, "detailName": 'No' },
                    { "id": 1, "detailName": 'Yes' },
                    { "id": 2, "detailName": 'All' }
                ];

                var order = "&$orderby= InvoiceNum asc";
                var filstr = "";

                $scope.DateInt = function () {
                    var date = new Date();
                    var currentDate = new Date(date.getFullYear(), date.getMonth() + 1, date.getDate());
                    $scope.currentDate = currentDate;
                }

                $scope.menuToggle = function () {
                    $("#wrapper").toggleClass("toggled");
                }

                $scope.invoiceList = {
                    showGridFooter: true,
                    enableFiltering: true,
                    columnDefs: [
                        { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal', width: '60' },
                        { field: 'notClear', enableCellEdit: false, displayName: 'NotClear', width: '80' },
                        {
                            field: 'invoiceNum', name: 'tmp', displayName: 'Invoice NO.', enableCellEdit: false, width: '110',
                            cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceD(row.entity.invoiceNum,row.entity.invoiceId,\'invdetaillist\')">{{row.entity.invoiceNum}}</a></div>'
                        },
                        //{
                        //    field: 'isBanlance', enableCellEdit: false, displayName: 'Reconciled', width: '100',
                        //    cellTemplate: '<div style="margin-top:5px;color:#FFA500">{{row.entity.isBanlance}}</div>'
                        //},
                        {
                            field: 'customerName', enableCellEdit: false, displayName: 'Customer Name', width: '135'
                            , cellTemplate: '<div class="ui-grid-cell-contents ng-binding ng-scope" style="color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};">{{row.entity.customerName}}</div>'
                        },
                        { field: 'customerNum', enableCellEdit: false, displayName: 'Customer NO.', width: '110' },
                        { field: 'siteUseId', enableCellEdit: false, displayName: 'SiteUseId', width: '90' },
                        { field: 'class', enableCellEdit: false, displayName: 'Class', width: '80' },
                        { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                        { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                        { field: 'dayS_LATE_SYS', enableCellEdit: false, displayName: 'DueDays', width: '80', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                        { field: 'overdueReason', enableCellEdit: false, displayName: 'OverDue Reason', width: '150' },
                        { field: 'ptP_DATE', enableCellEdit: false, displayName: 'PTP date ', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                        { field: 'currency', enableCellEdit: false, displayName: 'InvCurrCode', width: '100' },
                        { field: 'balanceAmt', enableCellEdit: false, displayName: 'Amt Remaining', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'cs', enableCellEdit: false, displayName: 'CS', width: '120' },
                        { field: 'sales', enableCellEdit: false, displayName: 'Sales', width: '120' },
                        { field: 'branchSalesFinance', enableCellEdit: false, displayName: 'Branch/Sales/Finance Manager', width: '160' },
                        {
                            field: 'originalAmt', enableCellEdit: false, displayName: 'Original Invoice Amount', cellFilter: 'number:2', type: 'number', width: '185'
                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                    return 'uigridred right';
                                }
                                else {
                                    return 'right';
                                }
                            }
                        },
                        { field: 'creditTrem', enableCellEdit: false, displayName: 'Credit Term', width: '160' },
                        { field: 'agingBucket', enableCellEdit: false, displayName: 'Aging Bucket', width: '120' },
                        { field: 'ebname', enableCellEdit: false, displayName: 'Eb', width: '160' },
                        { field: 'consignmentNumber', enableCellEdit: false, displayName: 'Consignment Number', width: '160' },
                        { field: 'ptP_Identified_Date', enableCellEdit: false, displayName: 'PTP Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '150' },
                        { field: 'payment_Date', enableCellEdit: false, displayName: 'Payment date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                        { field: 'ptpComment', enableCellEdit: false, displayName: 'PTP Comment', width: '140' },
                        {
                            field: 'disputeFlag', enableCellEdit: false, displayName: 'Dispute(Y / N)', width: '80'
                        },
                        { field: 'dispute_Identified_Date', enableCellEdit: false, displayName: 'Dispute Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '175' },
                        { field: 'disputeStatus', enableCellEdit: false, displayName: 'Dispute Status', width: '140' },
                        { field: 'dispute_Reason', enableCellEdit: false, displayName: 'Dispute Reason', width: '140' },
                        { field: 'disputeComment', enableCellEdit: false, displayName: 'Dispute Comment', width: '140' },
                        { field: 'owner_Department', enableCellEdit: false, displayName: 'Action Owner-Department', width: '190' },
                        {
                            field: 'balanceMemo', name: 'tmp1', enableCellEdit: false, displayName: 'Invoice Memo', width: '140',
                            cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.id,row.entity.invoiceNum, row.entity.balanceMemo,row.entity.memoExpirationDate)"></a>'
                            + '<label id="lbl{{row.entity.id}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum, row.entity.balanceMemo ,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.balanceMemo.substring(0,7)}}...</label></div>'
                        },
                        {
                            field: 'memoExpirationDate', enableCellEdit: false, displayName: 'Memo Expiration Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '175'
                            , cellTemplate: '<div class="ui-grid-cell-contents ng-binding ng-scope" style="color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};"><a style="color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};line-height:28px" ng-click="grid.appScope.openInvoiceD(row.entity.invoiceNum,row.entity.id,\'dateHislist\')">{{row.entity.memoExpirationDate|date:\'yyyy-MM-dd\'}}</a></div>'
                        },
                        { field: 'collectoR_CONTACT', enableCellEdit: false, displayName: 'Contact', width: '90' },
                        { field: 'vaT_NO', enableCellEdit: false, displayName: 'VAT No.', width: '120' },
                        { field: 'vaT_DATE', enableCellEdit: false, displayName: 'VAT date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '120' },
                        {
                            field: 'trackStates', enableCellEdit: false, displayName: 'Current Status', width: '130'
                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (grid.getCellValue(row, col) === '010') {
                                    return 'uigridred';
                                }
                            }
                            , filter: {
                                term: '',
                                type: uiGridConstants.filter.SELECT,
                                selectOptions: [
                                    { value: '000', label: 'Open' },
                                    { value: '001', label: 'Responsed OverDue Reason' },
                                    { value: '002', label: 'Wait for 2nd Time Confirm PTP' },
                                    { value: '003', label: 'PTP Confirmed' },
                                    { value: '004', label: 'Wait for Payment Reminding' },
                                    { value: '005', label: 'Wait for 1st Time Dunning' },
                                    { value: '006', label: 'Wait for 2nd Time Dunning' },
                                    { value: '007', label: 'Dispute Identified' },
                                    { value: '008', label: 'Wait for 2nd Time Dispute contact' },
                                    { value: '009', label: 'Wait for Dispute Responds' },
                                    { value: '010', label: 'Dispute Resolved' },
                                    { value: '011', label: 'Wait for 2nd Time Dispute respond' },
                                    { value: '012', label: 'Escalation' },
                                    { value: '013', label: 'Write off uncollectible accounts' },
                                    { value: '014', label: 'Closed' },
                                    { value: '015', label: 'Payment Notice Received' },
                                    { value: '016', label: 'Cancel' }]
                            }
                            , cellFilter: 'mapTrack'
                        },
                        { field: 'finishedStatus', displayName: 'Finished Status', width: '130' },
                        { field: 'lastUpdateDate', enableCellEdit: false, displayName: 'Last Update Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '140' },
                        { field: 'collector', enableCellEdit: false, displayName: 'Collector', width: '100' },
                        { field: 'next_Action_Date', enableCellEdit: false, displayName: 'Next Action Date', width: '145', cellFilter: 'date:\'yyyy-MM-dd\'' },
                        { field: 'poNum', enableCellEdit: false, displayName: 'PO Num', width: '105' },
                        { field: 'soNum', enableCellEdit: false, displayName: 'SO Num', width: '105' },
                        { field: 'isForwarder', enableCellEdit: false, displayName: 'IsForwarder', width: '110' },
                        { field: 'forwarder', enableCellEdit: false, displayName: 'Forwarder', width: '100' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;

                        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                            if (row.entity.id !== 0) {
                                $scope.invoiceSum();
                            }
                        });
                        gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
                            $scope.invoiceSumBatch();
                        });
                    }
                }

                //##########################
                //init && search
                //##########################
                $scope.init = function () {
                    //filstr = buildFilter();
                    if ($scope.siteUseid) {
                        $scope.pschangeexecute(999999);
                    } else {
                        myinvoicesProxy.invoicePaging(1, $scope.iperexecute, $scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate,
                            $scope.legal, $scope.siteUseid, $scope.invoiceNum, $scope.poNum, $scope.soNum, $scope.creditTerm,
                            $scope.docuType, $scope.trackStates, $scope.memo, $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,
                            $scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs, $scope.sales, $scope.overdueReason,
                            function (list) {
                                $scope.invoiceList.data = list.dataRows;
                                $scope.ttexecute = list.count;
                                $scope.totalNum = list.count;
                                //float menu 加载 
                                $scope.calculate($scope.curpexecute, $scope.iperexecute, list.dataRows.length);
                                $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
                            }, function (error) { alert(error); });
                    }
                };

                //************************** get current date ***********************
                $scope.DateInt();

                //*********************** openfilter ***********************
                var isShow = 0; //0:hide;1:show
                var baseFlg = true;
                $scope.showCollector = false;
                $scope.openFilter = function () {

                    //*************************aging filter base binding*****************************s
                    if (baseFlg) {
                        baseDataProxy.SysTypeDetails("004,029,031,049", function (res) {
                            angular.forEach(res, function (r) {
                                $scope.istatuslist = r["004"];
                                $scope.invoiceTrackStates = r["029"];
                                $scope.docuTypelist = r["031"];
                                $scope.overdueReasons = r["049"];
                                //$scope.displaylist = r["032"];
                                $scope.trackStates = "000";
                            });
                        });
                        //Legal Entity DropDownList binding
                        siteProxy.Site("", function (legal) {
                            $scope.legallist = legal;
                        });
                        //EB
                        ebProxy.Eb("", function (eb) {
                            $scope.ebList = eb;
                        });
                        permissionProxy.getCurrentUser("dummy", function (user) {
                            if (user.actionPermissions.indexOf('alldataforsupervisor') >= 0) {
                                $scope.showCollector = true;
                                //Collector List
                                permissionProxy.query({ 'getCollectList': 'dummy' }, function (res) {
                                    $scope.collectorList = res;
                                })
                            }
                        })
                    }
                    //*************************aging filter base binding*****************************e

                    if (isShow == 0) {
                        $("#divAgingSearch").show();
                        isShow = 1;
                        baseFlg = false;
                    } else if (isShow == 1) {
                        $("#divAgingSearch").hide();
                        isShow = 0;
                        baseFlg = false;
                    }
                }

                $scope.searchCollection = function () {
                    //filstr = buildFilter();

                    var index = 1;
                    $scope.curpexecute = 1;     //init page
                    myinvoicesProxy.invoicePaging(1, $scope.iperexecute, $scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate,
                        $scope.legal, $scope.siteUseid, $scope.invoiceNum, $scope.poNum, $scope.soNum, $scope.creditTerm,
                        $scope.docuType, $scope.trackStates, $scope.memo, $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,
                        $scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs, $scope.sales, $scope.overdueReason,
                        function (list) {
                            $scope.invoiceList.data = list.dataRows;
                            $scope.ttexecute = list.count;
                            $scope.totalNum = list.count;
                            $scope.calculate($scope.curpexecute, $scope.iperexecute, list.dataRows.length);
                        }, function (error) {

                        }
                    );
                }

                $scope.freshCollection = function () {
                    myinvoicesProxy.invoicePaging($scope.curpexecute, $scope.iperexecute, $scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate,
                        $scope.legal, $scope.siteUseid, $scope.invoiceNum, $scope.poNum, $scope.soNum, $scope.creditTerm,
                        $scope.docuType, $scope.trackStates, $scope.memo, $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,
                        $scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs, $scope.sales, $scope.overdueReason,
                        function (list) {
                            $scope.invoiceList.data = list.dataRows;
                            $scope.ttexecute = list.count;
                            $scope.totalNum = list.count;
                            $scope.calculate($scope.curpexecute, $scope.iperexecute, list.dataRows.length);
                        }, function (error) {

                        });
                }

                //reset Search conditions
                $scope.resetSearch = function () {
                    filstr = "";
                    $scope.custCode = "";
                    $scope.custName = "";
                    $scope.siteUseid = "";
                    //$scope.billGroupCode = "";
                    //$scope.billGroupName = "";

                    $scope.invoiceNum = "";
                    $scope.poNum = "";
                    $scope.soNum = "";
                    $scope.rbo = "";
                    $scope.creditTerm = "";

                    $scope.displayClosed = 0;
                    $scope.legal = "";
                    $scope.docuType = "";
                    $scope.states = "";
                    $scope.trackStates = "";

                    $scope.memo = "";
                    $scope.collector = "";
                    $scope.ptpDateF = "";
                    $scope.ptpDateT = "";
                    $scope.memoDateF = "";
                    $scope.memoDateT = "";
                    $scope.invoiceDateF = "";
                    $scope.invoiceDateT = "";
                    $scope.dueDateF = "";
                    $scope.dueDateT = "";
                    $scope.eb = "";
                    $scope.consignmentNumber = "";
                    $scope.balanceMemo = "",
                    $scope.memoExpirationDate = "";
                    $scope.cs = "";
                    $scope.sales = "";
                    $scope.overdueReason = "";
                }

                buildFilter = function () {
                    //multi-conditions
                    var filterStr = "";
                    filterStr += order + "&$filter=(InvoiceNum ne '')";

                    //row one
                    if ($scope.custCode) {
                        //filterStr += " and (contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "'))";
                        filterStr += " and ((contains('" + encodeURIComponent($scope.custCode) + "',CustomerNum)) or (contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "')))";
                    }

                    if ($scope.custName) {
                        filterStr += " and (contains(CustomerName,'" + encodeURIComponent($scope.custName) + "'))"
                    }
                    if ($scope.eb) {
                        filterStr += " and (contains(Ebname,'" + encodeURIComponent($scope.eb) + "'))"
                    }

                    if ($scope.consignmentNumber) {
                        filterStr += " and (contains(ConsignmentNumber,'" + encodeURIComponent($scope.consignmentNumber) + "'))"
                    } 
                    if ($scope.memoExpirationDate) {
                        filterStr += " and (contains(memoExpirationDate,'" + encodeURIComponent($scope.memoExpirationDate) + "'))"
                    } 
                    if ($scope.dueDateF) {
                        filterStr += "and (DueDate ge " + $scope.dueDateF + ")";
                    }
                    if ($scope.legal) {
                        filterStr += " and (LegalEntity eq '" + $scope.legal + "')";
                    }

                    if ($scope.siteUseid) {
                        //filterStr += " and (contains(CustomerNum,'" + encodeURIComponent($scope.siteUseid) + "'))";
                        filterStr += " and ((contains('" + encodeURIComponent($scope.siteUseid) + "',SiteUseId)) or (contains(SiteUseId,'" + encodeURIComponent($scope.siteUseid) + "')))";
                    }
                    //if ($scope.billGroupCode) {
                    //    filterStr += " and (contains(GroupCodeOld,'" + encodeURIComponent($scope.billGroupCode) + "'))";
                    //}

                    //if ($scope.billGroupName) {
                    //    filterStr += " and (contains(GroupNameOld,'" + encodeURIComponent($scope.billGroupName) + "'))";
                    //}

                    //row two
                    if ($scope.invoiceNum) {
                        filterStr += " and (contains(InvoiceNum,'" + encodeURIComponent($scope.invoiceNum) + "'))";
                    }

                    if ($scope.poNum) {
                        filterStr += " and (contains(PoNum,'" + encodeURIComponent($scope.poNum) + "'))";
                    }

                    if ($scope.soNum) {
                        filterStr += " and (contains(SoNum,'" + encodeURIComponent($scope.soNum) + "'))";
                    }

                    //if ($scope.rbo) {
                    //    filterStr += " and (contains(MstCustomer,'" + encodeURIComponent($scope.rbo) + "'))";
                    //}

                    if ($scope.creditTerm) {
                        filterStr += " and (contains(CreditTrem,'" + encodeURIComponent($scope.creditTerm) + "'))";
                    }

                    //row three


                    if ($scope.docuType) {
                        filterStr += " and (Class eq '" + $scope.docuType + "')";
                    }

                    //if ($scope.states) {
                    //    filterStr += " and (States eq '" + $scope.states + "')";
                    //}
                    if (!$scope.trackStates) {
                        //filterStr += " and (TrackStates ne '013') and (TrackStates ne '014') and (TrackStates ne '016')";
                        filterStr = "&closeType=000" + filterStr;
                    }
                    else {
                        if ($scope.trackStates) {
                            if ($scope.trackStates === "000") {
                                filterStr += " and (TrackStates ne '013') and (TrackStates ne '014') and (TrackStates ne '016')";
                            }
                            else {
                                filterStr += " and (TrackStates eq '" + $scope.trackStates + "')";
                            }
                        }
                    }


                    //row four
                    if ($scope.memo) {
                        filterStr += " and (contains(Comments,'" + encodeURIComponent($scope.memo) + "'))";
                    }

                    //if ($scope.collector) {
                    //    filterStr += " and (contains(Collector,'" + encodeURIComponent($scope.collector) + "'))";
                    //}

                    if ($scope.ptpDateF) {
                        filterStr += "and (PtpDate ge " + $scope.ptpDateF + ")";
                    }

                    if ($scope.ptpDateT) {
                        filterStr += "and (PtpDate le " + $scope.ptpDateT + ")";
                    }
                    if ($scope.memoDateF) {
                        filterStr += "and (memoDate ge " + $scope.memoDateF + ")";
                    }

                    if ($scope.memoDateT) {
                        filterStr += "and (memoDate le " + $scope.memoDateT + ")";
                    }

                    //row five
                    if ($scope.invoiceDateF) {
                        filterStr += "and (InvoiceDate ge " + $scope.invoiceDateF + ")";
                    }

                    if ($scope.invoiceDateT) {
                        filterStr += "and (InvoiceDate le " + $scope.invoiceDateT + ")";
                    }

                    if ($scope.dueDateF) {
                        filterStr += "and (DueDate ge " + $scope.dueDateF + ")";
                    }

                    if ($scope.dueDateT) {
                        filterStr += "and (DueDate le " + $scope.dueDateT + ")";
                    }

                    //GroupName
                    //if ($scope.billGroupName) {
                    //    filterStr = "&groupName=" + $scope.billGroupName + filterStr;
                    //} else {
                    //    filterStr = "&groupName=" + filterStr;
                    //}

                    //Display Closed
                    //filterStr = "&closeType=" + $scope.displayClosed + filterStr;
                    //if ($scope.displayClosed) {
                    //filterStr = "&closeType=" + $scope.trackStates + filterStr;
                    //} else {
                    //    filterStr = "&closeType=" + filterStr;
                    //}
                    return filterStr;
                };

                //##########################
                //export
                //##########################

                $scope.export = function () {

                    window.location = APPSETTING['serverUrl'] + '/api/myinvoices?' +
                        'custCode=' + $scope.custCode + '&custName=' + $scope.custName + '&eb=' + $scope.eb + '&consignmentNumber=' + $scope.consignmentNumber + '&balanceMemo=' + $scope.balanceMemo + '&memoExpirationDate=' + $scope.memoExpirationDate + '&legal=' + $scope.legal + '&siteUseid=' + $scope.siteUseid + '&invoiceNum=' + $scope.invoiceNum +
                        '&poNum=' + $scope.poNum + '&soNum=' + $scope.soNum + '&creditTerm=' + $scope.creditTerm + '&docuType=' + $scope.docuType + '&invoiceTrackStates=' + $scope.trackStates + '&memo=' + $scope.memo +
                        '&ptpDateF=' + $scope.ptpDateF + '&ptpDateT=' + $scope.ptpDateT + '&memoDateF=' + $scope.memoDateF + '&memoDateT=' + $scope.memoDateT + '&invoiceDateF=' + $scope.invoiceDateF + '&invoiceDateT=' + $scope.invoiceDateT + '&dueDateF=' + $scope.dueDateF + '&dueDateT=' + $scope.dueDateT + '&cs=' + $scope.cs + '&sales=' + $scope.sales + '&overdueReason=' + $scope.overdueReason;
                }

                $scope.exportSoa = function () {

                    myinvoicesProxy.exportSoa($scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate, $scope.legal, $scope.siteUseid, $scope.invoiceNum,
                        $scope.poNum, $scope.soNum, $scope.creditTerm, $scope.docuType, $scope.trackStates, $scope.memo,
                        $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,$scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs,
                        $scope.sales, $scope.overdueReason, function (fileId) {
                        // return fieid;
                        window.location = APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileId

                    }, function (err) {
                    })

                }

                $scope.import = function () {
                    alert('Import!');
                }

                //################################################################
                //change page size && change page && calculate page parameter
                //################################################################
                //paging size change
                $scope.pschangeexecute = function (slexecute) {
                    var index = 1;
                    $scope.curpexecute = 1;     //init page
                    //filstr = buildFilter();
                    myinvoicesProxy.invoicePaging(index, slexecute, $scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate,
                        $scope.legal, $scope.siteUseid, $scope.invoiceNum, $scope.poNum, $scope.soNum, $scope.creditTerm,
                        $scope.docuType, $scope.trackStates, $scope.memo, $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,
                        $scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs, $scope.sales, $scope.overdueReason,
                        function (list) {
                            $scope.invoiceList.data = list.dataRows;
                            $scope.iperexecute = slexecute;
                            $scope.ttexecute = list.count;
                            $scope.totalNum = list.count;
                            $scope.calculate($scope.curpexecute, $scope.iperexecute, list.dataRows.length);
                        });
                };

                //paging change
                $scope.executepChanged = function () {

                    var index = $scope.curpexecute;
                    //filstr = buildFilter();
                    myinvoicesProxy.invoicePaging(index, $scope.iperexecute, $scope.custCode, $scope.custName, $scope.eb, $scope.consignmentNumber, $scope.balanceMemo, $scope.memoExpirationDate,
                        $scope.legal, $scope.siteUseid, $scope.invoiceNum, $scope.poNum, $scope.soNum, $scope.creditTerm,
                        $scope.docuType, $scope.trackStates, $scope.memo, $scope.ptpDateF, $scope.ptpDateT, $scope.memoDateF, $scope.memoDateT,
                        $scope.invoiceDateF, $scope.invoiceDateT, $scope.dueDateF, $scope.dueDateT, $scope.cs, $scope.sales, $scope.overdueReason,
                        function (list) {
                            $scope.invoiceList.data = list.dataRows;
                            $scope.ttexecute = list.count;
                            $scope.totalNum = list.count;
                            $scope.calculate($scope.curpexecute, $scope.iperexecute, list.dataRows.length);
                        }, function (error) {
                            alert(error);
                        });
                };

                //calculate page parameter
                $scope.calculate = function (currentPage, itemsperpage, count) {
                    if (count == 0) {
                        $scope.fromItem = 0;
                    } else {
                        $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem = (currentPage - 1) * itemsperpage + count;
                }


                var selectMailInstanceById = function (custNums, id, siteUseId, templateType, templatelang, ids) {
                    var instance = {};
                    var instanceDefered = $q.defer();
                    //=========added by alex body中显示附件名+Currency=== $scope.inv 追加 ======
                    generateSOAProxy.getMailInstById(custNums, id, siteUseId, templateType, templatelang, ids, function (res) {
                        instance = res;
                        renderInstance(instance, custNums, siteUseId);

                        instanceDefered.resolve(instance);
                    });

                    return instanceDefered.promise;
                };

                var renderInstance = function (instance, custNums, suid, mType) {
                    //subject
                    //instance.subject = 'SOA-' + $scope.shortsub.join('-');
                    //invoiceIds
                    instance.invoiceIds = $scope.inv;
                    //soaFlg
                    instance.soaFlg = "1";
                    //Bussiness_Reference
                    var customerMails = [];
                    var arrCustNums = custNums.split(',');
                    var arrsuids = suid.split(',');
                    for (var i = 0; i < arrsuids.length; i++) {
                        var sid = arrsuids[i];
                        var cno = arrCustNums[i];
                        var fd = customerMails.find(function (x) {
                            return x.SiteUseId === sid
                        });
                        if (fd) continue;
                        var newCM = { MessageId: instance.messageId, CustomerNum: cno, SiteUseId: sid };
                        customerMails.push(newCM);
                    }
                    //angular.forEach(custNums.split(','), function (cust) {

                    //    customerMails.push({ MessageId: instance.messageId, CustomerNum: cust, SiteUseId: suid });
                    //});
                    instance.CustomerMails = customerMails; //$routeParams.nums;
                    //mailTitle
                    instance["title"] = "Create SOA";
                    //mailType
                    instance.mailType = mType + ",SOA";
                };

                var checkSOAMail = function (instance) {
                    if (!instance.attachments) {
                        if (!confirm("Not Include Attachment ,continue ?")) {
                            return false;
                        }
                        return true;
                    }
                    return true;
                }

                var getMailInstanceMain = function (custNums, suid, ids, mType, fileType) {

                    var instanceDefered = $q.defer();
                    generateSOAProxy.getMailInstance(custNums, suid, ids, mType, fileType, function (res) {
                        var instance = res;
                        renderInstance(instance, custNums, suid, mType);

                        instanceDefered.resolve(instance);
                    }, function (error) {
                        alert(error);
                    });

                    return instanceDefered.promise;
                };

                var getMailInstanceTo = function (custNums, suid) {

                    var toDefered = $q.defer();
                    //TO
                    contactProxy.query({
                        customerNums: custNums, siteUseid: suid
                    }, function (contactor) {
                        //2016-01-14 start
                        var to_cc = {};
                        to_cc.to = new Array();
                        to_cc.cc = new Array();

                        angular.forEach(contactor, function (item) {
                            //2016-01-14 start
                            if (item.toCc == "2") {
                                if (to_cc.cc.indexOf(item.emailAddress) < 0) {
                                    to_cc.cc.push(item.emailAddress);
                                }
                            }
                            else if (item.toCc == "1") {
                                if (to_cc.to.indexOf(item.emailAddress) < 0) {
                                    to_cc.to.push(item.emailAddress);
                                }
                            }

                        });

                        //var greeting = '<p>Dear ' + contName.substring(1, contName.length - 1) + '</p>';
                        //Mailinstance.body = greeting + Mailinstance.body;
                        //toDefered.resolve(cons.join(";"));
                        toDefered.resolve(to_cc);

                    });
                    return toDefered.promise;
                };

                var getMailInstance = function (custNums, suid, mType) {
                    var instance = {};
                    var allDefered = $q.defer();

                    $q.all([
                        getMailInstanceMain(custNums, suid, $scope.inv, mType, $scope.fileType),
                        //getMailInstanceTo(custNums, suid)
                        //=========added by alex body中显示附件名+Currency======
                        //getMailInstanceAttachment($scope.inv)
                        //=====================================================
                    ])
                        .then(function (results) {
                            instance = results[0];
                            //2016-01-14 start
                            //instance.to = results[1];
                            //instance.to = "";
                            //instance.cc = "";
                            //instance.to = results[1].to.join(";");
                            //instance.cc = results[1].cc.join(";");
                            //2016-01-14 End
                            //=========added by alex body中显示附件名+Currency======
                            //instance.attachments = results[2].attachments;
                            //instance.attachment = results[2].attachment;
                            //=====================================================
                            allDefered.resolve(instance);
                        });

                    return allDefered.promise;
                };

                $scope.clearPTP = function () {
                    if ($scope.gridApi.selection.getSelectedRows().length == 0) {
                        alert("Please choose 1 invoice at least .");
                        return;
                    }
                    if (confirm("Are you sure clear PTP")) {
                        var idList = new Array();
                        angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.id != 0) {
                                idList.push(rowItem.id + '|' + rowItem.invoiceNum);
                            }
                        });

                        invoiceProxy.clearPTP(idList, function (res) {
                            alert(res);
                            $scope.init();
                        }, function (res) {
                            alert(res);
                        });
                    }

                }

                $scope.batcheditNotClear = function (type) {
                    if ($scope.gridApi.selection.getSelectedRows().length === 0) {
                        alert("Please choose 1 invoice at least .");
                        return;
                    }
                    if (confirm("Are you sure change NotClear")) {
                        var idList = new Array();
                        idList.push(type);
                        angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.id !== 0) {
                                idList.push(rowItem.id);
                            }
                        });
                        invoiceProxy.setNotClear(idList, function (res) {
                            alert(res);
                            $scope.init();
                        }, function (res) {
                            alert(res);
                        });
                    }
                };

                $scope.clearOverdueReason = function () {
                    if ($scope.gridApi.selection.getSelectedRows().length == 0) {
                        alert("Please choose 1 invoice at least .");
                        return;
                    }

                    if (confirm("Are you sure clear overdue reason")) {
                        var idList = new Array();
                        angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.id != 0) {
                                idList.push(rowItem.id + '|' + rowItem.invoiceNum);
                            }
                        });

                            invoiceProxy.clearOverdueReason(idList, function (res) {
                                alert(res);
                                $scope.init();
                            }, function (res) {
                                alert(res);
                            });
                        }
    
                }

                $scope.arraydistinct = function (oriarray) {
                    if (oriarray != null) {
                        if (oriarray.length > 0) {
                            var toarray = [];
                            var tmpobj = {};
                            for (var i = 0; i < $scope.pm.length; i++) {
                                if (!tmpobj[oriarray[i]]) {
                                    tmpobj[oriarray[i]] = 1;
                                    toarray.push(oriarray[i]);
                                }
                            }

                            return toarray;
                        } else {
                            return null;
                        }

                        return null;
                    }
                }

                //##########################
                //change invoice status
                //##########################
                $scope.changetab = function (type) {
                    //get selected invoiceIds
                    $scope.inv = [];
                    $scope.invNumList = [];
                    $scope.custNumList = [];
                    $scope.siteUseIdList = [];
                    $scope.shortsub = [];
                    $scope.invNew = [];
                    $scope.pm = [];
                    $scope.invStatus = [];

                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;

                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
                            $scope.invNumList.push(rowItem.invoiceNum);
                            $scope.custNumList.push(rowItem.customerNum);
                            $scope.siteUseIdList.push(rowItem.siteUseId);
                            $scope.invNew.push(rowItem.id);
                            $scope.pm.push(rowItem.balanceAmt);
                            if (rowItem.invoiceTrack == "001" || rowItem.invoiceTrack == "002" || rowItem.invoiceTrack == "003") {
                                $scope.invStatus.push({ id: rowItem.invoiceId, status: "ptp" });
                            } else if (rowItem.invoiceTrack == "007" || rowItem.invoiceTrack == "008" || rowItem.invoiceTrack == "009" || rowItem.invoiceTrack == "010" || rowItem.invoiceTrack == "011") {
                                $scope.invStatus.push({ id: rowItem.invoiceId, status: "dispute" });
                            }

                            $scope.shortsub.push(rowItem.customerNum);
                            $scope.shortsub.push(rowItem.customerName.replace('*', '').replace('<', '').replace('>', '').replace('|', '').replace('?', '').replace('%', '').replace('#', '').replace(/\//g, '').replace(/\\/g, ''));

                            if (isfirstrow == false) {
                                if ($scope.suid != rowItem.siteUseId || $scope.custNo != rowItem.customerNum || $scope.legalentty != rowItem.legalEntity) {
                                    isonecustomer = false;
                                }
                            } else {
                                $scope.suid = rowItem.siteUseId;
                                $scope.custNo = rowItem.customerNum;
                                $scope.legalentty = rowItem.legalEntity;
                                isfirstrow = false;
                            }
                        }
                    });

                    // distinct
                    $scope.custNumList = $scope.arraydistinct($scope.custNumList);
                    $scope.siteUseIdList = $scope.arraydistinct($scope.siteUseIdList);

                    if ($scope.custNumList != null) {
                        $scope.custNums = $scope.custNumList.join(',');
                    }
                    if ($scope.siteUseIdList) {
                        $scope.siteUseId = $scope.siteUseIdList.join(',');
                    }

                    var ccount = 0;
                    if ($scope.pm != null) {
                        for (var i = 0; i < $scope.pm.length; i++) {

                            ccount = ccount + $scope.pm[i];
                        }
                    }

                    if (($scope.inv == "" || $scope.inv == null) && type != "vatimport" ) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }

                    if (type == "dispute") {

                        if (isonecustomer == false) {
                            alert("Please choose 1 SiteUseId at most .")
                            return;
                        }

                        for (var i = 0; i < $scope.invStatus.length; i++) {
                            if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                                alert("the selected invoices contains dispute invoice already!");
                                return;
                            }
                        }

                        //var relatedMail = "";
                        //relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");

                        contactHistoryProxy.queryObject({ type: 'dispute' }, function (disInvInstance) {
                            disInvInstance["title"] = "Dispute Reason";
                            var modalDefaults = {
                                templateUrl: 'app/common/contactdetail/contact-dispute.tpl.html',
                                controller: 'contactDisputeCtrl',
                                size: 'lg',
                                resolve: {
                                    disInvInstance: function () { return disInvInstance; },
                                    custnum: function () { return ""; },
                                    invoiceIds: function () { return $scope.inv; },
                                    contactId: function () { return ""; },
                                    relatedEmail: function () { return ""; },
                                    contactPerson: function () { return ""; },
                                    siteUseId: function () {
                                        return $scope.suid;
                                    },
                                    //customerNo: function () {
                                    //    return $scope.custNo;
                                    //},
                                    legalEntity: function () {
                                        return $scope.legalentty;
                                    }
                                },
                                windowClass: 'modalDialog'
                            };
                            modalService.showModal(modalDefaults, {}).then(function (result) {
                                if (result == "submit") {
                                    $scope.init();
                                }
                            });
                        });
                    }
                    else if (type == "ptp") {

                        //if (isonecustomer == false) {
                        //    alert("Please choose 1 customer at most .")
                        //    return;
                        //}

                        for (var i = 0; i < $scope.invStatus.length; i++) {
                            if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                                alert("the selected invoices contains ptp invoice already!");
                                return;
                            }
                        }

                        var suiList = $scope.siteUseId.split(',');
                        var sui = suiList[0];
                        angular.forEach(suiList, function (temp) {
                            if (temp != sui) {
                                alert("You have to choose the same Site Use Id data.");
                                checkFlag = true;
                                return;
                            }
                        });

                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-ptp.tpl.html',
                            controller: 'contactPtpCtrl',
                            size: 'lg',
                            resolve: {
                                custnum: function () {
                                    return $scope.custNo;
                                },
                                invoiceIds: function () {
                                    return $scope.invNew;
                                },
                                siteUseId: function () {
                                    return $scope.siteUseId;
                                },
                                customerNo: function () {
                                    return $scope.custNums;
                                },
                                legalEntity: function () {
                                    return $scope.legalentty;
                                },
                                contactId: function () {
                                    return "";
                                },
                                relatedEmail: function () {
                                    return "";
                                },
                                contactPerson: function () {
                                    return "";
                                },
                                proamount: function () {
                                    return ccount.toFixed(2);
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result[0] == "submit") {
                                $scope.refreshListVal("004002", "005", result[1], result[2]);
                                $scope.init();
                            }
                        });
                    }
                    else if (type == "notice") {

                        //if (isonecustomer == false) {
                        //    alert("Please choose 1 customer at most .")
                        //    return;
                        //}

                        //if ($scope.inv != "" && $scope.inv != null && $scope.inv.length > 1) {
                        //    alert("Please choose 1 invoice at least .")
                        //    return;
                        //}

                        var checkFlag = false;
                        var suiList = $scope.siteUseId.split(',');
                        var sui = suiList[0];
                        angular.forEach(suiList, function (temp) {
                            if (temp != sui) {
                                alert("You have to choose the same Site Use Id data.");
                                checkFlag = true;
                                return;
                            }
                        });

                        if (checkFlag) {
                            return;
                        }

                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-notice.tpl.html',
                            controller: 'contactNoticeCtrl',
                            size: 'lg',
                            resolve: {
                                custnum: function () {
                                    return "";
                                },
                                invoiceIds: function () {
                                    return $scope.invNew;
                                },
                                siteUseId: function () {
                                    return suiList[0];
                                },
                                customerNo: function () {
                                    return $scope.custNo;
                                },
                                legalEntity: function () {
                                    return $scope.legalentty;
                                },
                                contactId: function () {
                                    return "";
                                },
                                relatedEmail: function () {
                                    return "";
                                },
                                contactPerson: function () {
                                    return "";
                                },
                                proamount: function () {
                                    return ccount.toFixed(2);
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result[0] == "submit") {
                                $scope.refreshListVal("004012", result[1], result[2], null);
                                $scope.init();
                            }
                        });
                    }
                    else if (type == "mail001" || type == "mail002" || type == "mail003" || type == "mail005") {
                        //********************************Added by zhangYu******************************************//
                        $scope.invoiceNums = $scope.inv.join(",");

                        $scope.mType = type.substr(4, 3);


                        generateSOAProxy.getGenerateSOACheck($scope.custNums, $scope.siteUseId, $scope.inv, $scope.mType, $scope.fileType, function (res) {

                            var modalDefaults = {
                                templateUrl: 'app/common/mail/mail-instance.tpl.html',
                                controller: 'mailInstanceCtrl',
                                size: 'customSize',
                                resolve: {
                                    custnum: function () { return $scope.custNums; },
                                    siteuseId: function () { return $scope.siteUseId; },
                                    invoicenums: function () { return $scope.invoiceNums; },
                                    mType: function () {
                                        return $scope.mType;
                                    },
                                    //                                    selectedInvoiceId: function () { return $scope.inv; },
                                    instance: function () {
                                        return getMailInstance($scope.custNums, $scope.siteUseId, $scope.mType);
                                    },
                                    mailDefaults: function () {
                                        return {
                                            mailType: 'NE',
                                            templateChoosenCallBack: selectMailInstanceById,
                                            mailUrl: generateSOAProxy.sendEmailUrl,
                                            checkCallBack: checkSOAMail,
                                        };
                                    }
                                },
                                windowClass: 'modalDialog'
                            };

                            modalService.showModal(modalDefaults, {}).then(function (result) {
                                if (result == "sent") {

                                }
                            }, function (err) {
                                alert(err);
                                    });
                        }, function (error) {
                            alert(error);
                            return;
                        });
                    }
                    else if (type == "download") {


                        invoiceProxy.exportfiles($scope.invNew, $scope.custNums, $scope.siteUseId, $scope.fileType, function (fileId) {
                            // return fieid;
                            window.location = APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileId

                        }, function (err) {
                        })
                        //$scope.invoiceNums = $scope.invNumList.join(",");

                        //var modalDefaults = {
                        //    templateUrl: 'app/common/mail/mail-instance.tpl.html',
                        //    controller: 'mailInstanceCtrl',
                        //    size: 'customSize',
                        //    resolve: {
                        //        invoiceIds: function () {
                        //            return $scope.invNew;
                        //        },
                        //        custnum: function () { return $scope.custNums; },
                        //        siteuseId: function () { return $scope.siteUseId; },
                        //        //                                    selectedInvoiceId: function () { return $scope.inv; },
                        //        instance: function () {
                        //            return getMailInstance($scope.custNums, $scope.siteUseId);
                        //        },
                        //        generateSOAProxy.geneateSoaByIds(invoiceIds, custnum, $scope.suid, function (res) 
                        //        mailDefaults: function () {
                        //            return {
                        //                mailUrl: invoiceProxy.exportfiles,
                        //            };
                        //        }
                        //    },
                        //    windowClass: 'modalDialog'
                        //};
                    }
                    else if (type == "changeinvoicestatus") {

                        // All Invoice 下 Change Invoice Status 可能会影响多个 Alert 及 Dispute Task,为提高性能，暂不重新计算 Alert 及 Dispute 状态
                        var modalDefaults = {
                            templateUrl: 'app/common/changeinvoicestatus/changeInvoiceStatus-list.tpl.html',
                            controller: 'changeInvoiceStatusCtrl',
                            resolve: {
                                status: function () { return $scope.invstatusValue; },
                                invNums: function () { return $scope.inv; },
                                disputeId: function () { return 0; }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                $scope.init();
                            }
                        });

                    }
                    else if (type == "vatimport") {
                        //导入文件

                        var modalDefaults = {
                            templateUrl: 'app/myinvoices/vatimport.tpl.html',
                            controller: 'vatimportInstanceCtrl',
                            size: 'lg',
                            resolve: {
                                //selectedInvoiceId: function () { return $scope.inv; },
                                //instance: function () {
                                //    return getMailInstance($scope.custNums, $scope.siteUseId);
                                //},
                                //mailDefaults: function () {
                                //    return {
                                //        mailType: 'NE',
                                //        templateChoosenCallBack: selectMailInstanceById,
                                //        mailUrl: generateSOAProxy.sendEmailUrl,
                                //        checkCallBack: checkSOAMail,
                                //    };
                                //}
                            },
                            windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                $scope.init();
                            }
                        }, function (err) {
                            alert(err);
                        });

                        //显示预览效果

                    }
                    else if (type == "overdue") {

                        var invoiceNum = "";
                        for (var i = 0; i < $scope.invNumList.length; i++) {
                            invoiceNum += $scope.invNumList[i];
                            if (i < $scope.invNumList.length - 1) {
                                invoiceNum += ",";
                            }
                        }

                        invoiceProxy.getOverdueReason(invoiceNum, function (overdueReasonInstance) {
                            var modalDefaults = {
                                templateUrl: 'app/common/contactdetail/contact-overdue.tpl.html',
                                windowClass: 'modalDialog',
                                controller: 'contactOverdueCtrl',
                                size: 'lg',
                                resolve: {
                                    overdueReasonInstance: function () { return overdueReasonInstance; },
                                    overdueReasons: ['baseDataProxy', function (baseDataProxy) {
                                        return baseDataProxy.SysTypeDetail("049");
                                    }],
                                }
                            };
                            modalService.showModal(modalDefaults, {}).then(function (result) {
                                if (result == "submit") {
                                    $scope.init();
                                }
                            });
                        }, function () {

                        })
                    }
                }


                $scope.init();
                if ($scope.siteUseid) {
                    $scope.slexecute = 999999;  //init paging size(ng-model)
                    $scope.iperexecute = 999999;   //init paging size(parameter)
                }


                //##########################
                //invoice memo edit
                //##########################
                //********************** edit one memo //**********************s
                $scope.editMemoShow = function (invoiceId, invoiceNum, memo, memoDate) {
                    if (invoiceId != 0) {
                        $scope.selectText = memo;
                        var h = document.documentElement.clientHeight;
                        var w = document.documentElement.clientWidth;
                        var content = document.getElementById('boxEdit');
                        var contentWidth = $('#boxEdit').css('width').replace('px', '');
                        var contentHeight = $('#boxEdit').css('height').replace('px', '');
                        var stop = self.pageYOffset;
                        var sleft = self.pageXOffset;
                        var left = w / 2 - contentWidth / 2 + sleft;
                        var top = h / 2 - contentHeight / 2 + stop;
                        $('#boxEdit').css({ 'left': left + 'px', 'top': top + 'px' });
                        $('#txtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 130 + 'px' });
                        var str = '';
                        var str1 = '';
                        str = 'Invoice :"' + invoiceNum + '" Memo : ';
                        str1 = memo;
                        $("#hiddenInvId").val(invoiceId);   
                        if (memoDate != null && memoDate != undefined) {
                            $("#boxCommDate").find("input").val(memoDate.substring(0, 10));
                        }
                        else {
                            $("#boxCommDate").find("input").val(memoDate);
                        }
                        $("#lblBoxTitle").html(str);
                        $("#boxEdit").show();
                    }
                }

                $scope.editMemoSave = function () {
                    var list = [];
                    var invoiceId = $("#hiddenInvId").val();
                    var memo = $("#txtBox").val();
                    var memoDate = $("#boxCommDate").find("input").val();  
                    list.push('2');
                    list.push(invoiceId);
                    list.push(memo);
                    list.push(memoDate);
                    collectorSoaProxy.savecommon(list, function () {
                        $scope.saveBack(invoiceId, memo, memoDate);
                        $scope.editMemoClose();
                    });
                }

                $scope.saveBack = function (invoiceId, memo, memoDate) {

                    angular.forEach($scope.gridApi.grid.rows, function (rowItem) {
                        if (rowItem.entity.id == invoiceId) {
                            rowItem.entity.balanceMemo = memo;
                            rowItem.entity.memoExpirationDate = memoDate;
                        }
                    });

                }

                $scope.editMemoClose = function () {
                    $("#boxEdit").hide();
                }
                //********************** edit one memo //**********************e

                //******************************* edit batch memo *******************************s
                $scope.batcheditMemoShow = function () {
                    $scope.inv = [];
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
                        }
                    });

                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                    } else {
                        var h = document.documentElement.clientHeight;
                        var w = document.documentElement.clientWidth;
                        var content = document.getElementById('boxEdit');
                        var contentWidth = $('#boxEdit').css('width').replace('px', '');
                        var contentHeight = $('#boxEdit').css('height').replace('px', '');
                        var stop = self.pageYOffset;
                        var sleft = self.pageXOffset;
                        var left = w / 2 - contentWidth / 2 + sleft;
                        var top = h / 2 - contentHeight / 2 + stop;
                        $('#boxEditBatch').css({ 'left': left + 'px', 'top': top + 'px' });
                        $('#batchtxtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 155 + 'px' });
                        var str = '';
                        str = "All Selected Invoices' Memo Will Be Entirely Updated By Follow:"
                        $("#batchhiddenInvId").val($scope.inv);
                        $("#batchtxtBox").val("");
                        $("#boxCommentExpirationDate").find("input").val("");
                        $("#batchlblBoxTitle").html(str);
                        $("#boxEditBatch").show();
                    }
                }

                $scope.clearInvoiceMemo = function () {
                    $scope.inv = [];
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
                        }
                    });

                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .");
                    } else {
                        if (confirm("Are you sure clear comments")) {
                            invoiceProxy.clearComments($scope.inv, function (res) {
                                alert(res);
                                $scope.init();
                            }, function (res) {
                                alert(res);
                                return;
                            });
                        }
                    }
                }

                $scope.batcheditMemoSave = function () {
                    var list = [];
                    var invoiceIds = $("#batchhiddenInvId").val().toString();
                    var memo = $("#batchtxtBox").val();
                    var memoDate = $("#boxCommentExpirationDate").find("input").val();
                    if (memo.length > 8000) {
                        alert("input 8000 character at most");
                        return;
                    }
                    list.push("5");
                    list.push(invoiceIds);
                    list.push(memo);
                    list.push(memoDate);
                    collectorSoaProxy.savecommon(list, function () {
                        $scope.batchsaveBack(memo, memoDate);
                        $scope.batcheditMemoClose();
                    });
                }

                $scope.batchsaveBack = function (memo, memoDate) {
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            rowItem.balanceMemo = memo + '\r\n' + rowItem.balanceMemo;
                            rowItem.memoExpirationDate = memoDate;
                        }
                    });
                }

                $scope.batcheditMemoClose = function () {
                    $("#boxEditBatch").hide();
                }
                //******************************* edit batch memo *******************************e
                $scope.memoShow = function (invNum, memo, e) {
                    $('#box').css({ 'left': e.pageX - 410 + 'px', 'top': e.pageY - 100 + 'px' });
                    var str = '';
                    str = 'Invoice :"' + invNum + '" Memo : <br>' + memo;
                    $("#box").html(str);
                    $("#box").show();

                }

                $scope.memoHide = function () {
                    $("#box").hide();
                }


                //##########################
                //list params
                //##########################
                //*********************************** open invoice history *********************************
                $scope.openInvoiceH = function (inNum) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/invhistory/invhistory.tpl.html',
                        controller: 'invHisCL',
                        size: 'lg',
                        resolve: {
                            inNum: function () { return inNum; }
                        }
                        , windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {

                    });

                }

                $scope.openInvoiceD = function (inNum, invId, flag) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/invdetail/invdetail.tpl.html',
                        controller: 'invDetCL',
                        size: 'lg',
                        resolve: {
                            inNum: function () { return inNum; },
                            invId: function () { return invId; },
                            flag: function () { return flag; }
                        }
                        , windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {

                    });

                }

                //******************************** days late ******************************
                $scope.calDaysLate = function (obj) {
                    var aDate = obj.dueDate.toString().substring(0, 10).split('-');
                    var date = new Date(aDate[0], aDate[1], aDate[2]);
                    return (($scope.currentDate - date) / 86400000);
                }

                //********After ckick [ptp][payment][disput] button,refresh the value [invoiceStatus][TrackStatus][memo] to  invoiceList
                $scope.refreshListVal = function (invStatus, trackStatus, memo, ptpDate) {
                    if (invStatus == '004012') {
                        if (trackStatus == 'Contra') {
                            trackStatus = '018';
                        } else if (trackStatus == 'Breakdown') {
                            trackStatus = '019';
                        } else {
                            trackStatus = '006';
                        }
                    }
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            rowItem.states = invStatus;
                            rowItem.trackStates = trackStatus;
                            if (memo != null) { rowItem.balanceMemo = memo + rowItem.balanceMemo; }
                            if (ptpDate != null) { rowItem.ptpDate = ptpDate; }
                        }
                    });
                }

                //*************calculate invoice list checked total*************
                $scope.invoiceSum = function () {
                    var total = 0;
                    var currAry = [];
                    var totalAry = [];
                    var str = "";
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (currAry.indexOf(rowItem.currency) < 0) {
                            currAry.push(rowItem.currency);
                            totalAry.push(rowItem.balanceAmt);
                        } else {
                            var index = currAry.indexOf(rowItem.currency);
                            total = totalAry[index] + rowItem.balanceAmt;
                            totalAry.splice(index, 1, total);
                        }
                    });

                    for (var index in currAry) {
                        str += " " + currAry[index] + ':' + $scope.formatNumber(totalAry[index], 2, 1);
                    }
                    $("#footcalcu").html(str);
                }

                $scope.invoiceSumBatch = function () {
                    var total = 0;
                    var currAry = [];
                    var totalAry = [];
                    var str = "";

                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (currAry.indexOf(rowItem.currency) < 0) {
                            currAry.push(rowItem.currency);
                            totalAry.push(rowItem.balanceAmt);
                        } else {
                            var index = currAry.indexOf(rowItem.currency);
                            total = totalAry[index] + rowItem.balanceAmt;
                            totalAry.splice(index, 1, total);
                        }
                    });
                    for (var index in currAry) {
                        str += " " + currAry[index] + ':' + $scope.formatNumber(totalAry[index], 2, 1);
                    }
                    $("#footcalcu").html(str);
                }


                //****format number
                /* 
                    将数值四舍五入后格式化. 
                    @param num 数值(Number或者String) 
                    @param cent 要保留的小数位(Number) 
                    @param isThousand 是否需要千分位 0:不需要,1:需要(数值类型); 
                    @return 格式的字符串,如'1,234,567.45' 
                    @type String 
                */
                $scope.formatNumber =
                    function (num, cent, isThousand) {
                        num = num.toString().replace(/\$|\,/g, '');
                        if (isNaN(num))//检查传入数值为数值类型. 
                            num = "0";
                        if (isNaN(cent))//确保传入小数位为数值型数值. 
                            cent = 0;
                        cent = parseInt(cent);
                        cent = Math.abs(cent);//求出小数位数,确保为正整数. 
                        if (isNaN(isThousand))//确保传入是否需要千分位为数值类型. 
                            isThousand = 0;
                        isThousand = parseInt(isThousand);
                        if (isThousand < 0)
                            isThousand = 0;
                        if (isThousand >= 1) //确保传入的数值只为0或1 
                            isThousand = 1;
                        sign = (num == (num = Math.abs(num)));//获取符号(正/负数) 
                        //Math.floor:返回小于等于其数值参数的最大整数 
                        num = Math.floor(num * Math.pow(10, cent) + 0.50000000001);//把指定的小数位先转换成整数.多余的小数位四舍五入. 
                        cents = num % Math.pow(10, cent); //求出小数位数值. 
                        num = Math.floor(num / Math.pow(10, cent)).toString();//求出整数位数值. 
                        cents = cents.toString();//把小数位转换成字符串,以便求小数位长度. 
                        while (cents.length < cent) {//补足小数位到指定的位数. 
                            cents = "0" + cents;
                        }
                        if (isThousand == 0) //不需要千分位符. 
                            return (((sign) ? '' : '-') + num + '.' + cents);
                        //对整数部分进行千分位格式化. 
                        for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
                            num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                                num.substring(num.length - (4 * i + 3));
                        return (((sign) ? '' : '-') + num + '.' + cents);
                    }
            }])
    .filter('mapClass', function () {
        var typeHash = {
            'DM': 'DM',
            'CM': 'CM',
            'INV': 'INV',
            'Payment': 'Payment'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    })
    .filter('mapStatus', function () {
        var typeHash = {
            '004001': 'Open',
            '004002': 'PTP',
            '004003': 'Paid',
            '004004': 'Dispute',
            '004005': 'Cancelled',
            '004006': 'Uncollectable',
            '004007': 'WriteOff',
            '004008': 'PartialPay',
            '004010': 'Broken PTP',
            '004009': 'Closed',
            '004011': 'Hold',
            '004012': 'Payment'

        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    })
    .filter('mapTrack', function () {
        var typeHash = {
            '001': 'Responsed OverDue Reason',
            '002': 'Wait for 2nd Time Confirm PTP',
            '003': 'PTP Confirmed',
            '004': 'Wait for Payment Reminding',
            '005': 'Wait for 1st Time Dunning',
            '006': 'Wait for 2nd Time Dunning',
            '007': 'Dispute Identified',
            '008': 'Wait for 2nd Time Dispute contact',
            '009': 'Wait for 1st Time Dispute respond',
            '010': 'Dispute Resolved',
            '011': 'Wait for 2nd Time Dispute respond',
            '012': 'Escalation',
            '013': 'Write off uncollectible accounts',
            '014': 'Closed',
            '015': 'Payment Notice Received',
            '016': 'Cancel',
            '000': 'Open'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    });