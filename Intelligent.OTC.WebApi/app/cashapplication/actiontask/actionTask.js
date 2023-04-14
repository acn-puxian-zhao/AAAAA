angular.module('app.actionTask', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/actiontask', {
                templateUrl: 'app/cashapplication/actiontask/actionTask.tpl.html',
                controller: 'actionTaskCtrl',
                resolve: {         
                    statusList: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("088");
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('actionTaskCtrl',
        ['$scope', '$filter', '$interval', 'caCommonProxy', 'caHisDataProxy', 'mailProxy', 'APPSETTING', '$sce', 'modalService', '$location', 'statusList',
            function ($scope, $filter, $interval, caCommonProxy, caHisDataProxy, mailProxy, APPSETTING, $sce, modalService, $location, statusList) {

                //查询条件展开、合上
                var taskShow = false;
                $scope.taskOpenFilter = function () {
                    taskShow = !taskShow;
                    if (taskShow) {
                        $("#taskSearch").show();
                    } else {
                        $("#taskSearch").hide();
                    }
                };

                $scope.statusList = statusList;
                $scope.transactionNumber = '';
                $scope.status = '';
                $scope.currency = '';
                $scope.dateF = '';
                $scope.dateT = '';

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];
                
                // task grid start
                $scope.startIndex = 0;
                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页  

                $scope.taskDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    enableRowSelection: false,
                    enableRowHeaderSelection: false,
                    data: 'taskList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'legalEntity', displayName: 'Legal Entity', width: '120' },
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '70', cellClass: 'center' },
                        { field: 'transactioN_AMOUNT', displayName: 'Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'matcH_STATUS_NAME', displayName: 'Status', width: '120'},
                        {
                            field: 'uploadtime', displayName: 'Upload Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-if="row.entity.hasuploadfile" ng-click="grid.appScope.download(row.entity.uploadfilepath,row.entity.uploadfilename)" ng-show="true" title="{{row.entity.uploadfilename}}">{{row.entity.uploadtime | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'
                        },
                        {
                            field: 'identifY_TIME', displayName: 'Identify Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.openIdentify(row.entity.identifY_TASKID)" ng-show="true" >{{row.entity.identifY_TIME | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'},
                        {
                            field: 'advisoR_TIME', displayName: 'Advisor Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.openAdvisor(row.entity.advisoR_TASKID)" ng-show="true" >{{row.entity.advisoR_TIME | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'},
                        {
                            field: 'advisoR_MailDate', displayName: 'Advisor Mail', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-if="row.entity.hasadvisormail" ng-click="grid.appScope.showmail(row.entity.advisoR_MailId)" ng-show="true" title="Show Mail">{{row.entity.advisoR_MailDate | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'},
                        {
                            field: 'recoN_TIME', displayName: 'Recon Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.openReconDetail(row.entity.recoN_TASKID)" ng-show="true" >{{row.entity.recoN_TIME | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'},
                        {
                            field: 'adjustment_time', displayName: 'Adjustment Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.openAdjustmentDetail(row.entity.transactioN_NUMBER)" ng-show="true" >{{row.entity.adjustment_time | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'
                        },
                        {
                            field: 'pmtmaiL_DATE', displayName: 'PMT Detail Mail', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.showmail(row.entity.pmtmaiL_MESSAGEID)" ng-show="true" >{{row.entity.pmtmaiL_DATE | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'
                        },

                        {
                            field: 'applY_TIME', displayName: 'Post Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-if="row.entity.haspostfile" ng-click="grid.appScope.download(row.entity.postfilepath,row.entity.postfilename)" ng-show="true" title="{{row.entity.postfilename}}">{{row.entity.applY_TIME | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'
                        },
                        {
                            field: 'clearinG_TIME', displayName: 'Clear Time', width: '125', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-if="row.entity.hasclearfile" ng-click="grid.appScope.download(row.entity.clearfilepath,row.entity.clearfilename)" ng-show="true" title="{{row.entity.clearfilename}}">{{row.entity.clearinG_TIME | date:\'yyyy-MM-dd HH:mm:ss\'}}</a>' +
                                '</div>'
                        },

                        //{
                        //    field: 'action', displayName: 'Action', width: '10%',
                        //    cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewdDetail(row.entity.id)">{{row.entity.status == "2"?"Detail":""}}</a></div>'
                        //},
                        { field: 'reF1', displayName: 'Description', width: '200', cellClass: 'left' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {                   
                    caCommonProxy.getActionTaskDatas($scope.transactionNumber, $scope.status, $scope.currency, $scope.dateF, $scope.dateT, $scope.currentPage, selectedLevelId, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    });                
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    //if ($scope.menuregion && $scope.menuregion !== 'undefined') {
                        caCommonProxy.getActionTaskDatas($scope.transactionNumber, $scope.status, $scope.currency, $scope.dateF, $scope.dateT, $scope.currentPage, $scope.itemsperpage, function (result) {
                            $scope.totalItems = result.count;
                            $scope.taskList = result.dataRows;
                            $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                        }, function (error) {
                            alert(error);
                        });
                    //}
                };

                //$scope.menuregion_change = function (menuregionid) {
                    //$scope.currentPage = 1;
                    //$scope.menuregion = menuregionid;
                 //   $scope.pageChanged();
                //};

                $scope.goback = function () {
                    $location.path("/ca/index");
                };

                $scope.init = function () {
                    //$scope.menuregion_change();
                    //$scope.currentPage = 1;
                    $scope.pageChanged();
                };

                $scope.resetSearch = function () {
                    $scope.transactionNumber = '';
                    $scope.status = '';
                    $scope.currency = '';
                    $scope.dateF = '';
                    $scope.dateT = '';
                    $scope.init();
                };

                //download
                $scope.download = function (fullNamePath, filename) {
                    if (fullNamePath === null || fullNamePath === "") {
                        alert("There is no need to download the file!");
                    } else {
                        caHisDataProxy.GetFileFromWebApi(fullNamePath, function (data) {
                            if (data.byteLength > 0) {
                                var blob = new Blob([data], { type: "application/vnd.ms-excel" });
                                var objectUrl = URL.createObjectURL(blob);
                                var aForExcel = $("<a><span class='forExcel'>下载excel</span></a>").attr("href", objectUrl);
                                aForExcel.attr("download", filename);
                                $("body").append(aForExcel);
                                $(".forExcel").click();
                                aForExcel.remove();
                            }
                            else {
                                alert("File not find!");
                            }
                        }, function (ex) { alert(ex) });
                        //window.location = fullNamePath;                  
                    }
                };

                $scope.showmail = function (mailid) {
                    mailProxy.queryObject({ messageId: mailid }, function (mailInstance) {
                        //mailType
                        mailInstance["title"] = "Mail View";
                        mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);
                        $scope.invoiceNums = "";

                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return ""; },
                                siteuseId: function () { return ""; },
                                invoicenums: function () { return ""; },
                                instance: function () { return mailInstance },
                                mType: function () { return "" },
                                mailDefaults: function () {
                                    return {
                                        mailType: 'VI'
                                    };
                                }
                            },
                            windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {

                        });
                    });
                };

                $scope.pageChanged();
                // task grid end
                                              
                // task operation start
                //跳转
                $scope.viewdDetail = function (id) {
                    $location.path("/ca/reconDetail");
                };
                // task operation end

                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                }

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                }

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                }

                $scope.openIdentify = function (taskId) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/actiontask/identify.tpl.html?id=1',
                        controller: 'identifyCtrl',
                        size: 'customSize',
                        resolve: {
                            taskId:function () {
                                return taskId;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        //$scope.bankPageChanged();
                    });
                }

                $scope.openAdvisor = function (taskId) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/actiontask/advisor.tpl.html?id=1',
                        controller: 'advisorCtrl',
                        size: 'customSize',
                        resolve: {
                            taskId: function () {
                                return taskId;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        //$scope.bankPageChanged();
                    });
                }

                $scope.openReconDetail = function (taskId) {
                    var bankIds = [];
                    //
                    caHisDataProxy.getBankByTaskId(taskId, function (result) {
                        if (result) {
                            for (let i in result) {
                                bankIds.push(result[i].id);
                            }
                            localStorage.setItem('isReconAdjustment', true);
                            localStorage.setItem('reconAdjustIds', bankIds);
                            $location.path("/ca/reconDetail");
                        }
                    })                    
                }

                $scope.openAdjustmentDetail = function (transactionNum) {
                    var bankIds = [];
                    //
                    caHisDataProxy.getBankByTranc(transactionNum, function (result) {
                        if (result) {
                            for (let i in result) {
                                bankIds.push(result[i].id);
                            }
                            localStorage.setItem('isReconAdjustment', true);
                            localStorage.setItem('reconAdjustIds', bankIds);
                            $location.path("/ca/reconDetail");
                        }
                    })
                }

            }])
    .controller('identifyCtrl',
        ['$scope', '$filter', '$uibModalInstance', 'caHisDataProxy', 'taskId',
            function ($scope, $filter, $uibModalInstance, caHisDataProxy, taskId) {
                $scope.taskId = taskId;

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];


                // bank grid start
                $scope.identifyStartIndex = 0;
                $scope.identifySelectedLevel = 20;  //下拉单页容量初始化
                $scope.identifyItemsperpage = 20;
                $scope.identifyCurrentPage = 1; //当前页
                $scope.identifyMaxSize = 10; //分页显示的最大页  

                $scope.identifyHisDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    //enableCellEditOnFocus: true,
                    data: 'identifyList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'matcH_STATUS', displayName: 'Match Status', width: '120', cellFilter: 'mapMatchStatus' },
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'currency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center'  },
                        { field: 'forwarD_NUM', displayName: 'Payer Number', width: '120', cellClass: 'center'},
                        { field: 'forwarD_NAME', displayName: 'Payer Name', width: '200' },
                        {
                            field: 'customeR_NUM', displayName: 'Customer Number', width: '120', cellClass: 'center'
                        },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '200' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.identifyGridApi = gridApi;

                        //$scope.identifyList = identifyList;

                    }
                };

                //Detail单页容量变化
                $scope.identifyPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getIdentifyHisDataDetails($scope.taskId, $scope.identifyCurrentPage, selectedLevelId, function (result) {
                        $scope.identifyItemsperpage = selectedLevelId;
                        $scope.identifyTotalItems = result.count;
                        $scope.identifyList = result.dataRows;
                        $scope.identifyStartIndex = ($scope.currentPage - 1) * $scope.identifyItemsperpage;
                    })
                };

                //Detail翻页
                $scope.identifyPageChanged = function () {
                    caHisDataProxy.getIdentifyHisDataDetails($scope.taskId, $scope.identifyCurrentPage, $scope.identifyItemsperpage, function (result) {
                        $scope.identifyTotalItems = result.count;
                        $scope.identifyList = result.dataRows;
                        $scope.identifyStartIndex = ($scope.identifyCurrentPage - 1) * $scope.identifyItemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.identifyPageChanged();

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

            }])
    .controller('advisorCtrl',
        ['$scope', '$filter', '$uibModalInstance', 'caHisDataProxy', 'taskId',
            function ($scope, $filter, $uibModalInstance, caHisDataProxy, taskId) {

                $scope.taskId = taskId;

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];


                // bank grid start
                $scope.advisorStartIndex = 0;
                $scope.advisorSelectedLevel = 20;  //下拉单页容量初始化
                $scope.advisorItemsperpage = 20;
                $scope.advisorCurrentPage = 1; //当前页
                $scope.advisorMaxSize = 10; //分页显示的最大页  

                $scope.advisorHisDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    //enableCellEditOnFocus: true,
                    data: 'advisorList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'matcH_STATUS', displayName: 'Match Status', width: '120', cellFilter: 'mapMatchStatus'},
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '120' },
                        { field: 'currency', displayName: 'Currency', width: '120', cellClass: 'center' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '120' },
                        {
                            field: 'forwarD_NUM', displayName: 'Payer Number', width: '120', cellClass: 'center',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewAgentCustomer(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.agentCustomerNumber}}</a></div>'
                        },
                        { field: 'forwarD_NAME', displayName: 'Payer Name', width: '200' },
                        {
                            field: 'customeR_NUM', displayName: 'Customer Number', width: '120', cellClass: 'center',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewPaymentCustomer(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.agentCustomerNumber}}</a></div>'
                        },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '200' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.advisorGridApi = gridApi;

                        //$scope.advisorList = advisorList;
                    }
                };

                //Detail单页容量变化
                $scope.advisorPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getAdvisorHisDataDetails($scope.taskId, $scope.advisorCurrentPage, selectedLevelId, function (result) {
                        $scope.advisorItemsperpage = selectedLevelId;
                        $scope.advisorTotalItems = result.count;
                        $scope.advisorList = result.dataRows;
                        $scope.advisorStartIndex = ($scope.currentPage - 1) * $scope.advisorItemsperpage;
                    })
                };

                //Detail翻页
                $scope.advisorPageChanged = function () {
                    caHisDataProxy.getAdvisorHisDataDetails($scope.taskId, $scope.advisorCurrentPage, $scope.advisorItemsperpage, function (result) {
                        $scope.advisorTotalItems = result.count;
                        $scope.advisorList = result.dataRows;
                        $scope.advisorStartIndex = ($scope.advisorCurrentPage - 1) * $scope.advisorItemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.advisorPageChanged();

                $scope.cancel = function () {
                    $uibModalInstance.close();
                };
            }])
    .filter('mapMatchStatus', function () {
                var statusHash = {
                    '-1': 'Newly Uploaded',
                    '0': 'Unknown_Identify',
                    '1': 'Unknown_Advisor',
                    '2': 'Unmatched',
                    '3': 'Reconed Unmatched',
                    '4': 'Matched',
                    '7': 'Ignore',
                    '8': 'Dispute',
                    '9': 'Closed'
                };
                return function (input) {
                    if (!input) {
                        return '';
                    } else {
                        return statusHash[input];
                    }
                };
            })

    .filter('mapStatus', function () {
        var statusHash = {
            1: 'Processing',
            2: 'Complete'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return statusHash[input];
            }
        };
    });