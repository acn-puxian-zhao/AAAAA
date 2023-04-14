angular.module('app.task', ['ui.grid.grouping'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/task', {
                templateUrl: 'app/task/task-list.tpl.html',
                controller: 'taskListCtrl',
                title: "Task",
                resolve: {
                }
            });
    }])
    .controller('taskListCtrl',
        ['$scope', '$filter', '$q', 'taskProxy', 'siteProxy', '$interval', 'modalService', 'APPSETTING', 'generateSOAProxy', 'contactProxy', 'contactHistoryProxy',
            function ($scope, $filter, $q, taskProxy, siteProxy, $interval, modalService, APPSETTING, generateSOAProxy, contactProxy, contactHistoryProxy) {
                $scope.$parent.helloAngular = "OTC - Task";
                //float menu 加载 ---alex
                $scope.floatMenuOwner = ['taskListCtrl'];

                $scope.statuslist = [
                    { "detailValue": "", "detailName": '全部' },
                    { "detailValue": "0", "detailName": '待执行' },
                    { "detailValue": "1", "detailName": '已完成' },
                    { "detailValue": "2", "detailName": '已取消' }
                ];
                $scope.status = "";

                $scope.pmtStatusList = [
                    { "detailValue": "", "detailName": 'All' },
                    { "detailValue": "0", "detailName": 'New' },
                    { "detailValue": "-", "detailName": 'Canceled' },
                    { "detailValue": "1", "detailName": 'Sended' }
                ];
                $scope.pmtStatus = "0";

                $scope.ptpStatusList = [
                    { "detailValue": "", "detailName": 'All' },
                    { "detailValue": "001", "detailName": 'Open' },
                    { "detailValue": "002", "detailName": 'OverPTP' }
                ];
                $scope.ptpStatus = "001";

                $scope.disputeStatusList = [
                    { "detailValue": "", "detailName": 'All' },
                    { "detailValue": "026001", "detailName": 'Open' },
                    { "detailValue": "026012", "detailName": 'Resolved' },
                    { "detailValue": "026011", "detailName": 'Cancel' }
                ];
                $scope.disputeStatus = "026001";

                $scope.legalEntity = "";
                $scope.custNum = "";
                $scope.custName = "";
                $scope.SiteUseId = "";
                $scope.startDate = new Date();

                $scope.levelList = [
                    { "id": 15, "levelName": '15' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' },
                    { "id": 999999, "levelName": 'ALL' }
                ];

                $scope.maxSize = 10; //paging display max
                $scope.slexecute = 999999;  //init paging size(ng-model)
                $scope.iperexecute = 999999;   //init paging size(parameter)
                $scope.curpexecute = 1;     //init page
                $scope.totalNum = "";
                $scope.displayClosed = 0;

                $scope.startIndex_taskPMT = 0;
                $scope.selectedLevel_taskPMT = 15;  //下拉单页容量初始化
                $scope.itemsperpage_taskPMT = 15;
                $scope.currentPage_taskPMT = 1; //当前页
                $scope.maxSize_taskPMT = 10; //分页显示的最大页

                $scope.startIndex_taskSOA = 0;
                $scope.selectedLevel_taskSOA = 15;  //下拉单页容量初始化
                $scope.itemsperpage_taskSOA = 15;
                $scope.currentPage_taskSOA = 1; //当前页
                $scope.maxSize_taskSOA = 10; //分页显示的最大页 


                $scope.DateFPMT = $filter('date')(new Date(), "yyyy-MM-dd");
                $scope.DateTPMT = $filter('date')(new Date(), "yyyy-MM-dd");

                $scope.DateFPTP = $filter('date')(new Date(), "yyyy-MM-dd");
                $scope.DateTPTP = $filter('date')(new Date(), "yyyy-MM-dd");

                $scope.DateF = $filter('date')(new Date(), "yyyy-MM-dd");
                $scope.DateT = $filter('date')(new Date(), "yyyy-MM-dd");

                $("#PMT_F").addClass("ng-hide");
                $("#PMT_T").addClass("ng-hide");
                
                $("#PTP_F").addClass("ng-hide");
                $("#PTP_T").addClass("ng-hide");

                $("#Remind_F").addClass("ng-hide");
                $("#Remind_T").addClass("ng-hide");
                
                $scope.PMTSelected = 0;
                $scope.PMTTotal = 0;
                $scope.gridApis = [];
                $scope.inv = [];

                $scope.isshow = false;

                //是否显示查询条件
                $scope.lblfilter = true;

                //左侧导航显示、隐藏控制
                $scope.menuToggle = function () {
                    $("#wrapper").toggleClass("toggled");
                }

                //查询条件展开、合上
                var baseFlg = true;
                var isShow = 0; //0:hide;1:show
                $scope.openFilter = function () {
                    if (isShow === 0) {
                        $("#divAgingSearch").show();
                        isShow = 1;
                        baseFlg = false;
                    } else if (isShow === 1) {
                        $("#divAgingSearch").hide();
                        isShow = 0;
                        baseFlg = false;
                    }
                };

                $scope.createGrid = function () {
                    $scope.createPmtGrid();
                    $scope.createPtpGrid();
                    $scope.createDisputeGrid();
                    $scope.createRemindding();
                    $scope.createTaskPmtList();
                    $scope.createTaskSoaList();
                };

                $scope.createPmtGrid = function () {
                    //PMT销账指单列表
                    $scope.pmtList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: true,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            //{
                            //    name: 'PMT', displayName: 'Mail', width: '80', pinnedLeft: true,
                            //    cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.sendPMTMail(row.entity.id, row.entity.customerNum, row.entity.siteUseId)">SendMail</a></div>'
                            //},
                            {
                                name: 'operation', displayName: 'Operation', width: '60', pinnedLeft: true,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.editPmtTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.invoiceNum, row.entity.balanceAmt, \'Ignore\',row.entity.haspmt)">忽略</a></div>'
                            },
                            { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal', width: '60' },
                            { field: 'customerNum', enableCellEdit: false, displayName: 'Cust No', width: '90' },
                            //{ field: 'siteUseId', enableCellEdit: false, displayName: 'SiteUseId', width: '90' },
                            {
                                field: 'siteUseId', displayName: 'SiteUseId', width: '90',
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.ViewSoa(row.entity.customerNum,row.entity.siteUseId)">{{row.entity.siteUseId}}</a></div>'
                            },
                            { field: 'currency', enableCellEdit: false, displayName: 'Currency', width: '80' },
                            { field: 'class', enableCellEdit: false, displayName: 'Class', width: '60' },
                            { field: 'invoiceNum', enableCellEdit: false, displayName: 'Invoice No', width: '140' },
                            { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'balanceAmt', enableCellEdit: false, displayName: 'AMT', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'trackStatusName', enableCellEdit: false, displayName: 'Status', width: '80' },
                            { field: 'trackDate', enableCellEdit: false, displayName: 'Status Date', width: '145', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                            { field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '300' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApis[0] = gridApi;
                            $scope.gridApi = gridApi;
                            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                if (row.isSelected === true) {
                                    $scope.PMTTotal = $scope.PMTTotal + row.entity.balanceAmt;
                                    $scope.PMTSelected = 0;
                                    //行选中
                                    taskProxy.queryPMTDetailTask(row.entity.siteUseId, row.entity.balanceAmt, function (list) {
                                        $scope.pmtDetailList.data = list;
                                    }, function (error) {
                                        alert(error);
                                    });
                                } else {
                                    $scope.pmtDetailList.data = [];
                                    $scope.PMTTotal = $scope.PMTTotal - row.entity.balanceAmt;
                                }
                            });
                        }
                    };
                    $scope.pmtDetailList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: true,
                        enableSelectAll: false,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;color:{{row.entity.color}}">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            {
                                field: 'invoiceNum', enableCellEdit: false, displayName: 'InvoiceNum', width: '110',
                                cellTemplate: '<div style="height:30px;padding-top:5px;vertical-align:middle;text-align:center;color:{{row.entity.color}}">{{row.entity.invoiceNum}}</div>'
                            },
                            {
                                field: 'balanceAmt', enableCellEdit: false, displayName: 'BalanceAmt', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right',
                                cellTemplate: '<div style="height:30px;padding-top:5px;vertical-align:middle;text-align:right;color:{{row.entity.color}}">{{row.entity.balanceAmt|number:2}}</div>'
                            },
                            {
                                field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '200',
                                cellTemplate: '<div style="height:30px;padding-top:5px;vertical-align:middle;text-align:right;color:{{row.entity.color}}">{{row.entity.comments}}</div>'
                            }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApis[1] = gridApi;
                            $scope.gridApi = gridApi;
                            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                if (row.isSelected === true) {
                                    //行选中
                                    $scope.PMTSelected += row.entity.balanceAmt;
                                } else {
                                    $scope.PMTSelected -= row.entity.balanceAmt;
                                }
                            });
                        }
                    };
                };

                $scope.createPtpGrid = function () {
                    //PTP列表
                    $scope.ptpList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: false,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            //{
                            //    name: 'operation', displayName: '操作', width: '160', pinnedLeft: true,
                            //    cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.editPtpTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.promiseDate, \'Executed\')">Executed</a> | <a style="line-height:28px" ng-click="grid.appScope.editPtpTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.promiseDate,\'Broken\')">Broken</a> | <a style="line-height:28px" ng-click="grid.appScope.editPtpTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.promiseDate,\'Cancel\')">Cancel</a></div>'
                            //},
                            { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal', width: '60' },
                            { field: 'customerNum', enableCellEdit: false, displayName: 'Cust No', width: '80' },
                            {
                                field: 'siteUseId', displayName: 'SiteUseId', width: '90',
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.ViewSoa(row.entity.customerNum,row.entity.siteUseId)">{{row.entity.siteUseId}}</a></div>'
                            },
                            { field: 'promiseDate', enableCellEdit: false, displayName: 'PromiseDate', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'promissAmount', enableCellEdit: false, displayName: 'PromissAmount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'isPartialPay', enableCellEdit: false, displayName: 'IsPartialPay', width: '90' },
                            { field: 'payer', enableCellEdit: false, displayName: 'Payer', width: '70' },
                            { field: 'ptpStatusName', enableCellEdit: false, displayName: 'Status', width: '110' },
                            { field: 'status_Date', enableCellEdit: false, displayName: 'Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            //{ field: 'createTime', enableCellEdit: false, displayName: 'Create Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '300' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                if (row.isSelected === true) {
                                    //行选中
                                    taskProxy.queryPTPDetailTask(row.entity.id, function (list) {
                                        $scope.ptpDetailList.data = list;
                                    }, function (error) {
                                        alert(error);
                                    });
                                } else {
                                    $scope.ptpDetailList.data = [];
                                }
                            });
                        }
                    };
                    $scope.ptpDetailList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: true,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            { field: 'invoiceNum', enableCellEdit: false, displayName: 'InvoiceNum', width: '110' },
                            { field: 'balanceAmt', enableCellEdit: false, displayName: 'BalanceAmt', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right', pinnedLeft: true },
                            //{ field: 'ptpDate', enableCellEdit: false, displayName: 'PtpDate', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '200' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.createDisputeGrid = function () {
                    //Dispute列表
                    $scope.disputeList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: false,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            {
                                name: 'operation', displayName: '操作', width: '120', pinnedLeft: true,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.editDisputeTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.issueReasonName, \'Resolved\')">Resolved</a> | <a style="line-height:28px" ng-click="grid.appScope.editDisputeTask(row.entity.id, row.entity.siteUseId, row.entity.customerNum, row.entity.issueReasonName, \'Cancel\')">Cancel</a></div>'
                            },
                            { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal', width: '60' },
                            { field: 'customerNum', enableCellEdit: false, displayName: 'Cust No', width: '80' },
                            { field: 'siteUseId', enableCellEdit: false, displayName: 'SiteUseId', width: '80' },
                            { field: 'issueReasonName', enableCellEdit: false, displayName: 'Reason', width: '250' },
                            { field: 'disputeStatusName', enableCellEdit: false, displayName: 'Status', width: '110' },
                            { field: 'status_Date', enableCellEdit: false, displayName: 'Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            //{ field: 'createTime', enableCellEdit: false, displayName: 'Create Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '300' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                if (row.isSelected === true) {
                                    //行选中
                                    taskProxy.queryDisputeDetailTask(row.entity.id, function (list) {
                                        $scope.disputeDetailList.data = list;
                                    }, function (error) {
                                        alert(error);
                                    });
                                } else {
                                    $scope.disputeDetailList.data = [];
                                }
                            });
                        }
                    };
                    $scope.disputeDetailList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: true,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            { field: 'invoiceNum', enableCellEdit: false, displayName: 'InvoiceNum', width: '110' },
                            { field: 'balanceAmt', enableCellEdit: false, displayName: 'BalanceAmt', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right', pinnedLeft: true },
                            { field: 'comments', enableCellEdit: false, displayName: 'Comments', width: '200' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.createRemindding = function () {
                    $scope.reminddingList = {
                        showGridFooter: true,
                        enableFiltering: true,
                        enableFullRowSelection: true,
                        multiSelect: false,
                        columnDefs: [
                            { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', width: '30' },
                            { field: 'task_date', enableCellEdit: false, displayName: 'Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal', width: '60' },
                            { field: 'customerNum', enableCellEdit: false, displayName: 'Cust No', width: '80' },
                            { field: 'customerName', enableCellEdit: false, displayName: 'Cust Name', width: '300' },
                            { field: 'siteUseId', enableCellEdit: false, displayName: 'SiteUseId', width: '80' },
                            { field: 'task_content', enableCellEdit: false, displayName: 'Content', width: '800' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.createTaskPmtList = function () {
                    $scope.taskPMTList = {
                        showGridFooter: true,
                        multiSelect: false,
                        enableFullRowSelection: false,
                        enableFiltering: true,
                        columnDefs: [
                            { name: 'RowNo', field: '', width: '30', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex_taskPMT}}</span>' },
                            {
                                name: 'operation', displayName: 'Operation', width: '60', pinnedLeft: true,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.sendTaskByUser(1,row.entity.templeteLanguage, row.entity.deal, row.entity.region, row.entity.eid, row.entity.periodId, row.entity.alertType, row.entity.toTitle, row.entity.toName, row.entity.ccTitle, row.entity.customerNum, row.entity.responseDate)">Send</a></div>'
                            },
                            { field: 'eid', displayName: 'EID', width: '100' },
                            { field: 'actionDate', displayName: 'ActionDate', width: '80', cellClass: 'center'  },
                            { field: 'templeteLanguageName', displayName: 'TempleteLanguageName', width: '120' },
                            { field: 'region', displayName: 'Region', width: '80' },
                            { field: 'periodId', displayName: 'PeriodId', width: '80' },
                            { field: 'alertType', displayName: 'AlertType', width: '80' },
                            { field: 'toTitle', displayName: 'ToTitle', width: '100' },
                            { field: 'toName', displayName: 'ToName', width: '150' },
                            { field: 'ccTitle', displayName: 'CCTitle', width: '200' },
                            { field: 'responseDate', displayName: 'ResponseDate', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'customerNum', displayName: 'CustomerNum', width: '120' },
                            { field: 'comment', displayName: 'Comment', width: '300' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.createTaskSoaList = function () {
                    $scope.taskSOAList = {
                        showGridFooter: true,
                        multiSelect: false,
                        enableFullRowSelection: false,
                        enableFiltering: true,
                        noUnselect: false,
                        columnDefs: [
                            { name: 'RowNo', field: '', width: '30', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1 + startIndex_taskPMT}}</span>' },
                            {
                                name: 'operation', displayName: 'Operation', width: '60', pinnedLeft: true,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.sendTaskByUser(2,row.entity.templeteLanguage, row.entity.deal, row.entity.region, row.entity.eid, row.entity.periodId, row.entity.alertType, row.entity.toTitle, row.entity.toName, row.entity.ccTitle, row.entity.customerNum, row.entity.responseDate)">Send</a></div>'
                            },
                            { field: 'eid', displayName: 'EID', width: '100' },
                            { field: 'actionDate', displayName: 'ActionDate', width: '80', cellClass: 'center' },
                            { field: 'templeteLanguageName', displayName: 'TempleteLanguageName', width: '120' },
                            { field: 'region', displayName: 'Region', width: '80' },
                            { field: 'periodId', displayName: 'PeriodId', width: '80' },
                            { field: 'alertType', displayName: 'AlertType', width: '80' },
                            { field: 'toTitle', displayName: 'ToTitle', width: '100' },
                            { field: 'toName', displayName: 'ToName', width: '150' },
                            { field: 'ccTitle', displayName: 'CCTitle', width: '200' },
                            { field: 'responseDate', displayName: 'ResponseDate', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'customerNum', displayName: 'CustomerNum', width: '120' },
                            { field: 'customerName', displayName: 'CustomerName', width: '200' },
                            { field: 'comment', displayName: 'Comment', width: '300' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.sendTaskByUser = function (type, templeteLanguage, deal, region, eid, periodId, alertType, toTitle, toName, ccTitle, customerNum, responseDate)
                {
                    if (toTitle == "" || toTitle == null) {
                        alert("[ToTitle] is null, can't send mail.")
                        return;
                    }
                    if (toName == "" || toName == null) {
                        alert("[ToName] is null, can't send mail.")
                        return;
                    }

                    taskProxy.sendTaskByUser(templeteLanguage, deal, region, eid, periodId, alertType, toTitle, toName, ccTitle, customerNum, responseDate, function (result) {
                        //刷新列表
                        if (type == 1) {
                            $scope.pageChanged_taskPMT();
                        } else if (type == 2) {
                            $scope.pageChanged_taskSOA();
                        }
                        alert("Mail send success!")
                    }, function (error) {
                        //刷新列表
                        if (type == 1) {
                            $scope.pageChanged_taskPMT();
                        } else if (type == 2) {
                            $scope.pageChanged_taskSOA();
                        }
                        alert(error);
                    });
                }

                //taskPMT单页容量变化
                $scope.pageSizeChanged_taskPMT = function (selectedLevel_taskPMT) {
                    taskProxy.gettaskPMTSendList($scope.currentPage_taskPMT, selectedLevel_taskPMT, function (result) {
                        $scope.itemsperpage_taskPMT = selectedLevel_taskPMT;
                        $scope.totalItems_taskPMT = result.count;
                        $scope.taskPMTList.data = result.dataRows;
                        $scope.startIndex_taskPMT = ($scope.currentPage_taskPMT - 1) * $scope.itemsperpage_taskPMT;
                    });
                };

                //taskPMT翻页
                $scope.pageChanged_taskPMT = function () {
                    taskProxy.gettaskPMTSendList($scope.currentPage_taskPMT, $scope.itemsperpage_taskPMT, function (result) {
                        $scope.totalItems_taskPMT = result.count;
                        $scope.taskPMTList.data = result.dataRows;
                        $scope.startIndex_taskPMT = ($scope.currentPage_taskPMT - 1) * $scope.itemsperpage_taskPMT;
                    }, function (error) {
                        alert(error);
                    });
                };

                //taskSOA单页容量变化
                $scope.pageSizeChanged_taskSOA = function (selectedLevel_taskSOA) {
                    taskProxy.gettaskSOASendList($scope.currentPage_taskSOA, selectedLevel_taskSOA, function (result) {
                        $scope.itemsperpage_taskSOA = selectedLevel_taskSOA;
                        $scope.totalItems_taskSOA = result.count;
                        $scope.taskSOAList.data = result.dataRows;
                        $scope.startIndex_taskSOA = ($scope.currentPage_taskSOA - 1) * $scope.itemsperpage_taskSOA;
                    });
                };

                //taskSOA翻页
                $scope.pageChanged_taskSOA = function () {
                    taskProxy.gettaskSOASendList($scope.currentPage_taskSOA, $scope.itemsperpage_taskSOA, function (result) {
                        $scope.totalItems_taskSOA = result.count;
                        $scope.taskSOAList.data = result.dataRows;
                        $scope.startIndex_taskSOA = ($scope.currentPage_taskSOA - 1) * $scope.itemsperpage_taskSOA;
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.BatchIgnore = function () {
                    var currentSelection = $scope.gridApis[0].selection.getSelectedRows();
                    angular.forEach(currentSelection, function (item, index, objs) {
                        if (item.haspmt == '0') {
                            //$scope.editPmtTask(item.id, item.siteuseid, item.customerno, item.invoiceNum, item.balanceAmt, 'Ignore', item.haspmt);
                            taskProxy.saveTaskPMT(item.customerNum, item.siteUseId, item.invoiceNum, 'Ignore', '', function () {
                                $scope.pmtDetailList.data = [];
                                $scope.pmtList.data = [];
                                $scope.searchPMT();
                            });
                        }
                    });
                };

                $scope.ViewSoaMail = function (lastSoaMailId) {
                    if (lastSoaMailId !== '0') {
                        window.open('#/maillist/maildetail/' + lastSoaMailId);
                    }
                };

                $scope.ViewSoa = function (customerNo, siteuseid) {
                    window.open('#/soa/sendSoa/' + customerNo + '/create/' + siteuseid);
                };

                $scope.openTask = function (item) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/taskedit.tpl.html',
                        controller: 'taskeditCtrl',
                        size: 'mid',
                        resolve: {
                            taskStatus: function () { return item.tasK_STATUS; },
                            taskId: function () { return item.taskid; },
                            taskDate: function () { return item.tasK_DATE; },
                            taskType: function () { return item.tasK_TYPE; },
                            taskContent: function () { return item.tasK_CONTENT; },
                            deal: function () { return item.deal; },
                            legalEntity: function () { return item.legaL_ENTITY; },
                            customerNo: function () { return item.customeR_NUM; },
                            siteUseId: function () { return item.siteuseid; },
                            isAuto: function () { return item.isauto; }
                        }
                        , windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'submit') {
                            $scope.init(false);
                        }
                    });
                };

                $scope.newTask = function (deal, legalEntity, customerNo, siteUseId) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/taskedit.tpl.html',
                        controller: 'taskeditCtrl',
                        size: 'mid',
                        resolve: {
                            taskStatus: function () { return '1'; },
                            taskId: function () { return ''; },
                            taskDate: function () { return new Date(); },
                            taskType: function () { return ''; },
                            taskContent: function () { return ''; },
                            deal: function () { return deal; },
                            legalEntity: function () { return legalEntity; },
                            customerNo: function () { return customerNo; },
                            siteUseId: function () { return siteUseId; },
                            isAuto: function () { return '0'; }
                        }
                        , windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'submit') {
                            $scope.init(false);
                        }
                    });
                };

                $scope.editPmtTask = function (id, siteuseid, customerno, invoiceNum, balanceAmt, status, haspmt) {
                    if (haspmt == "-") {
                        alert("Cancelled,Can't cancel.");
                        return;
                    } if (haspmt == "1") {
                        alert("Sented,Can't cancel.");
                        return;
                    }
                    var modalDefaults = {
                        templateUrl: 'app/soa/task_pmtedit.tpl.html',
                        controller: 'taskpmteditCtrl',
                        size: 'mid',
                        resolve: {
                            id: function () { return id; },
                            siteuseid: function () { return siteuseid; },
                            customerno: function () { return customerno; },
                            invoiceNum: function () { return invoiceNum; },
                            balanceAmt: function () { return balanceAmt; },
                            status: function () { return status; }
                        }
                        , windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'submit') {
                            $scope.pmtDetailList.data = [];
                            $scope.pmtList.data = [];
                            $scope.searchPMT();
                        }
                    });
                };

                $scope.editPtpTask = function (id, siteuseid, customerno, promisedate, status) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/task_ptpedit.tpl.html',
                        controller: 'taskptpeditCtrl',
                        size: 'mid',
                        resolve: {
                            id: function () { return id; },
                            siteuseid: function () { return siteuseid; },
                            customerno: function () { return customerno; },
                            promisedate: function () { return promisedate; },
                            status: function () { return status; }
                        }
                        , windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'submit') {
                            $scope.ptpDetailList.data = [];
                            $scope.ptpList.data = [];
                            $scope.searchPTP();
                        }
                    });
                };

                $scope.editDisputeTask = function (id, siteuseid, customerno, issuereason, status) {
                    var modalDefaults = {
                        templateUrl: 'app/soa/task_disputeedit.tpl.html',
                        controller: 'taskdisputeeditCtrl',
                        size: 'mid',
                        resolve: {
                            id: function () { return id; },
                            siteuseid: function () { return siteuseid; },
                            customerno: function () { return customerno; },
                            issuereason: function () { return issuereason; },
                            status: function () { return status; }
                        }
                        , windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'submit') {
                            $scope.disputeDetailList.data = [];
                            $scope.disputeList.data = [];
                            $scope.searchDispute();
                        }
                    });
                }

                $scope.createGrid();

                $scope.sendmail = function () {
                    $scope.strcus = "";
                    $scope.suid = "";
                    $scope.mType = "099";
                    angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId !== 0) {
                            $scope.inv.push(rowItem.id);
                        }
                    });
                    angular.forEach($scope.gridApis[1].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId !== 0) {
                            $scope.inv.push(rowItem.id);
                        }
                    });
                    if ($scope.inv.length === 0) {
                        alert("Please choose 1 invoice at least .");
                        return;
                    }
                    if ($scope.inv)
                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return ""; },
                                siteuseId: function () { return "" },
                                invoicenums: function () { return $scope.inv; },
                                mType: function () { return $scope.mType; },
                                instance: function () {
                                    return getMailInstance($scope.strcus, $scope.suid, $scope.mType);
                                },
                                mailDefaults: function () {
                                    return {
                                        mailType: 'NE',
                                        templateChoosenCallBack: selectMailInstanceById,
                                        mailUrl: generateSOAProxy.sendEmailUrl,
                                    };
                                }
                            },
                            windowClass: 'modalDialog'
                        };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "sent") {
                            $scope.reSeachContactList();
                            document.getElementById("asend").style.color = "green";
                        }
                    }, function (err) {
                        alert(err);
                    });

                };

                $scope.reSeachContactList = function () {
                    angular.forEach($scope.num, function (cus) {
                        collectorSoaProxy.query({ CustNumsFCH: cus }, function (list) {
                            angular.forEach($scope.sendsoa, function (row) {
                                if (row.customerCode == cus) {
                                    row['hisg'].data = list
                                }
                            });
                            //$scope.contactList.data = list;
                        });
                    });
                }

                var getMailInstance = function (custNums, suid, mType) {
                    var instance = {};
                    var allDefered = $q.defer();

                    $q.all([
                        getMailInstanceMain(custNums, suid, $scope.inv, mType),
                    ])
                        .then(function (results) {
                            instance = results[0];
                            allDefered.resolve(instance);
                        });

                    return allDefered.promise;
                };

                var getMailInstanceMain = function (custNums, suid, ids, mType) {

                    var instanceDefered = $q.defer();
                    generateSOAProxy.getMailInstance(custNums, suid, ids, mType, $scope.fileType, function (res) {
                        var instance = res;
                        renderInstance(instance, custNums, suid, mType);

                        instanceDefered.resolve(instance);
                    }, function (error) {
                        alert(error);
                    });

                    return instanceDefered.promise;
                };

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
                    instance.invoiceIds = $scope.inv;
                    //soaFlg
                    instance.soaFlg = "1";
                    //Bussiness_Reference
                    var customerMails = [];
                    angular.forEach(custNums.split(','), function (cust) {
                        customerMails.push({ MessageId: instance.messageId, CustomerNum: cust, SiteUseId: suid });
                    });
                    instance.CustomerMails = customerMails; //$routeParams.nums;
                    //mailTitle
                    instance["title"] = "Free Mail";
                    //mailType
                    instance.mailType = mType + ",SOA";
                };

                //查询条件JSON
                buildFilter = function () {
                    var search = {};
                    return angular.toJson(search);
                };

                //初始化数据查询
                $scope.init = function (retrieveDetail) {
                    if ($scope.startDate === null || $scope.startDate === "") {
                        alert("Please select start date!");
                    } else {
                        //重新定义GRID，列名变了
                        //$scope.createGrid();
                        var sDate = $filter('date')($scope.startDate, "yyyy-MM-dd HH:mm:ss");
                        filstr = buildFilter();

                        $scope.searchPMT();
                        $scope.searchPTP();
                        $scope.searchDispute();
                        $scope.searchRemindding();
                        $scope.pageChanged_taskPMT();
                        $scope.pageChanged_taskSOA();
                    }
                };

                $scope.searchPMT = function (pmtStatus) {
                    if (pmtStatus || pmtStatus == "") {
                        $scope.pmtStatus = pmtStatus;
                    }
                    //$("#PMT_F").removeClass("ng-hide");
                    //$("#PMT_T").removeClass("ng-hide");
                    //if (pmtStatus == "0") {
                    //    $("#PMT_F").addClass("ng-hide");
                    //    $("#PMT_T").addClass("ng-hide");
                    //}
                    if ($scope.pmtStatus == '0') {
                        $scope.isshow = true;
                    } else {
                        $scope.isshow = false;
                    }

                    var DateF = $filter('date')($scope.DateFPMT, "yyyy-MM-dd");
                    var DateT = $filter('date')($scope.DateTPMT, "yyyy-MM-dd");
                    //检索PMT列表数据
                    taskProxy.queryPMTTask($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, $scope.pmtStatus, DateF, DateT, function (list) {
                            $scope.pmtDetailList.data = [];
                            $scope.pmtList.data = list;
                        }, function (error) {
                            alert(error);
                    });
                };

                $scope.exportPMT = function (pmtStatus) {
                    if (pmtStatus || pmtStatus == "") {
                        $scope.pmtStatus = pmtStatus;
                    }
                    //$("#PMT_F").removeClass("ng-hide");
                    //$("#PMT_T").removeClass("ng-hide");
                    //if (pmtStatus == "0") {
                    //    $("#PMT_F").addClass("ng-hide");
                    //    $("#PMT_T").addClass("ng-hide");
                    //}

                    var DateF = $filter('date')($scope.DateFPMT, "yyyy-MM-dd");
                    var DateT = $filter('date')($scope.DateTPMT, "yyyy-MM-dd");
                    
                    taskProxy.exportPMT($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, $scope.pmtStatus, DateF, DateT, function (fileId) {
                        // return fieid;
                        window.location = fileId;
                    }, function (err) {
                    });
                };

                $scope.searchPTP = function (ptpStatus) {
                    if (ptpStatus || ptpStatus == "") {
                        $scope.ptpStatus = ptpStatus;
                    }
                    //$("#PTP_F").removeClass("ng-hide");
                    //$("#PTP_T").removeClass("ng-hide");
                    //if (ptpStatus == "001") {
                    //    $("#PTP_F").addClass("ng-hide");
                    //    $("#PTP_T").addClass("ng-hide");
                    //}
                    var DateF = $filter('date')($scope.DateFPTP, "yyyy-MM-dd");
                    var DateT = $filter('date')($scope.DateTPTP, "yyyy-MM-dd");
                    //检索PTP列表数据
                    taskProxy.queryPTPTask($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, $scope.ptpStatus, DateF, DateT, function (list) {
                        $scope.ptpDetailList.data = [];
                        $scope.ptpList.data = list;
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.exportPTP = function (ptpStatus) {
                    if (ptpStatus || ptpStatus == "") {
                        $scope.ptpStatus = ptpStatus;
                    }
                    //$("#PTP_F").removeClass("ng-hide");
                    //$("#PTP_T").removeClass("ng-hide");
                    //if (ptpStatus == "001") {
                    //    $("#PTP_F").addClass("ng-hide");
                    //    $("#PTP_T").addClass("ng-hide");
                    //}
                    var DateF = $filter('date')($scope.DateFPTP, "yyyy-MM-dd");
                    var DateT = $filter('date')($scope.DateTPTP, "yyyy-MM-dd");
                    taskProxy.exportPTP($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, $scope.ptpStatus, DateF, DateT, function (fileId) {
                       // return fieid;
                        window.location =  fileId;

                    }, function (err) {
                    });
                };

                $scope.searchDispute = function (disputeStatus) {
                if (disputeStatus) {
                    $scope.disputeStatus = disputeStatus;
                }
                //检索Dispute列表数据
                taskProxy.queryDisputeTask($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, $scope.disputeStatus, function (list) {
                    $scope.disputeDetailList.data = [];
                    $scope.disputeList.data = list;
                }, function (error) {
                    alert(error);
                });
            };

            $scope.searchRemindding = function () {
                if (!$scope.DateF) {
                    alert("Date From can't null.");
                    return;
                }
                if (!$scope.DateT) {
                    alert("Date From can't null.");
                    return;
                }
                var DateF = $filter('date')($scope.DateF, "yyyy-MM-dd");
                var DateT = $filter('date')($scope.DateT, "yyyy-MM-dd");
                //检索Remindding列表数据
                taskProxy.queryTaskRemindding($scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, DateF, DateT, function (list) {
                    $scope.reminddingList.data = list;
                }, function (error) {
                    alert(error);
                });
            };

            $scope.export = function () {
                var sDate = $filter('date')($scope.startDate, "yyyy-MM-dd HH:mm:ss");
                window.location = APPSETTING['serverUrl'] + '/api/task/exporttask?' +
                    'cLegalEntity=' + $scope.legalEntity + '&cCustNum=' + $scope.custNum + '&cCustName' + $scope.custName + '&cSiteUseId=' + $scope.SiteUseId + '&cDate=' + sDate + '&cStatus=' + $scope.status;
            };
            $scope.exportSoaDate = function () {
                window.location = APPSETTING['serverUrl'] + '/api/task/exportsoadate?' +
                    'cLegalEntity=' + $scope.legalEntity + '&cCustNum=' + $scope.custNum + '&cCustName' + $scope.custName + '&cSiteUseId=' + $scope.SiteUseId;
            };


            $scope.popup = {
                opened: false
            };

            $scope.open = function () {
                $scope.popup.opened = true;
            };

            //Legal Entity DropDownList binding
            siteProxy.Site("", function (legal) {
                $scope.legallist = legal;
            });

            $scope.init(true);

            $scope.resetSearch = function () {
                $scope.status = "";
                $scope.legalEntity = "";
                $scope.custNum = "";
                $scope.custName = "";
                $scope.SiteUseId = "";
                $scope.startDate = new Date();
            };
            
            $scope.changetab = function (type) {

                if (type == "vatimport") {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/myinvoices/vatimport.tpl.html',
                        controller: 'vatimportInstanceCtrl',
                        size: 'lg',
                        resolve: {
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
                }
               
            }
     }]);