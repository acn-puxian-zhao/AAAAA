angular.module('app.followup', ['ui.grid.grouping'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/followup', {
                templateUrl: 'app/soa/task-list.tpl.html',
                controller: 'taskListFollowUpCtrl',
                title: "FollowUp",
                resolve: {
                }
            });
    }])
    .controller('taskListFollowUpCtrl',
    ['$scope', '$filter', '$q', 'taskProxy', 'siteProxy', '$interval', 'modalService', 'APPSETTING', 'generateSOAProxy', 'contactProxy','contactHistoryProxy',
        function ($scope, $filter, $q, taskProxy, siteProxy, $interval, modalService, APPSETTING, generateSOAProxy, contactProxy, contactHistoryProxy) {
            $scope.$parent.helloAngular = "OTC - Follow Up";
            //float menu 加载 ---alex
            $scope.floatMenuOwner = ['taskListFollowUpCtrl'];

            $scope.statuslist = [
                { "detailValue": "", "detailName": '全部' },
                { "detailValue": "0", "detailName": '待执行' },
                { "detailValue": "1", "detailName": '已完成' },
                { "detailValue": "2", "detailName": '已取消' }
            ];
            $scope.status = "";

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

            $scope.DateF = new Date();
            $scope.DateT = new Date();

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

                //*************************aging filter base binding*****************************s
                if (baseFlg) {
                    //baseDataProxy.SysTypeDetails("004,005,006,029", function (res) {
                    //    angular.forEach(res, function (r) {
                    //        $scope.istatuslist = r["004"];
                    //        $scope.cusstatus = r["005"];
                    //        $scope.cusclass = r["006"];
                    //        $scope.invoiceTrackStates = r["029"];
                    //    });
                    //});

                    ////Legal Entity DropDownList binding
                    //siteProxy.Site("", function (legal) {
                    //    $scope.legallist = legal;
                    //});
                    ////EB
                    //ebProxy.Eb("", function (eb) {
                    //    $scope.ebList = eb;
                    //});

                }
                //*************************aging filter base binding*****************************e

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
                $scope.taskList = {
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    multiSelect: false,
                    columnDefs: [
                        { name: 'RowNo', field: '', enableSorting: false, displayName: '', width: '40', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'legalEntity', displayName: 'Legal', width: '60', pinnedLeft: true },
                        {
                            field: 'customerNo',name: 'customer', displayName: 'Cust No', width: '80', pinnedLeft: true,
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.ViewMyInvoice(row.entity.legalEntity,row.entity.customerNo,row.entity.siteUseId)">{{row.entity.customerNo}}</a></div>'
                        },
                        {
                            field: 'customerName', displayName: 'Cust Name', width: '150', pinnedLeft: true,
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px;color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};" ng-click="grid.appScope.ViewCustomer(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.customerName}}</a></div>'
                        },
                        {
                            field: 'siteUseId', displayName: 'SiteUseId', width: '80', pinnedLeft: true,
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.ViewSoa(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.siteUseId}}</a></div>'
                        },
                        { field: 'collector', name: 'collector', displayName: 'Collector', width: '80', pinnedLeft: true },
                        //{ field: 'contactor', name: 'Contact', displayName: 'Contact', width: '100', pinnedLeft: true },
                        { field: 'currency', name: 'currency', displayName: 'Curr', width: '50', pinnedLeft: true },
                        { field: 'ebName', name: 'ebName', displayName: 'EbName', width: '100', pinnedLeft: true },
                        { field: 'creditTerm', name: 'creditTerm', displayName: 'creditTerm', width: '100', pinnedLeft: true },
                        { field: 'sales', name: 'Sales', displayName: 'Sales', width: '100' },
                        { field: 'cs', name: 'CS', displayName: 'CS', width: '100' },
                        {
                            field: 'lastsenddate', name: 'MailDate', displayName: 'MailDate', cellFilter: 'date:\'yyyy-MM-dd\'', width: '90',
                            cellTemplate: '<div style="margin-top:5px;color:#CD3333;text-align:center">{{row.entity.lastsenddate|date:\'yyyy-MM-dd\'}}</div>'
                        },
                        { field: 'totalAr', displayName: 'Total AR', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'notOverdue', displayName: 'NotOverdue', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                        { field: 'overdue', displayName: 'OverDue', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        {
                            field: 'overdue60', displayName: '60+', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right',
                            sort: {
                                direction: 'DESC',
                                priority: 1
                            }
                        },
                        { field: 'overdue120', displayName: '120+', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'overdue270', displayName: '270+', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'overdue360', displayName: '360+', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        //{
                        //    name: 'responseTimes', enableCellEdit: false, displayName: 'Response', width: '80', pinnedLeft: true ,
                        //    cellTemplate: '<div style="text-align:center; line-height:30px">{{row.entity.responseTimes}} <a style="float: right;margin-right: 10px;" ng-click="grid.appScope.addResponse(row.entity)"> <i class="glyphicon glyphicon-plus"></i> </a></div>'
                        //},
                        //{ field: 'star', displayName: 'Star', width: '70', cellTemplate: '<input required class="rb-rating" type="text" value="{{row.entity.star}}" title="" />', pinnedLeft: true },
                        {
                            name: 'Add', displayName: 'Add', width: '50',
                            cellTemplate: '<a ng-click="grid.appScope.newTask(row.entity.deal,row.entity.legalEntity,row.entity.customerNo,row.entity.siteUseId)">Add</a>', cellClass: 'center',
                            enableColumnResizing: true
                        },
                        {
                            name: 'curMonth', displayName: 'Comments', width: '270'
                            ,
                            //cellTemplate: '<div style="height:60px; overflow:auto"><div>' +
                            //    '<span ng-repeat="item in row.entity.taskDetail" >' +
                            //    '<img style="margin-left:5px;width:10px;heigh:10px" ng-show="item.tasK_STATUS == 1" src="~/../Content/images/Execute.png">' +
                            //    '<img style="margin-left:5px;width:8px;heigh:8px" ng-show="item.tasK_STATUS == 2" src="~/../Content/images/259.png">' +
                            //    '<img style="margin-left:1px;width:14px;heigh:14px" ng-show="item.tasK_STATUS == 0" src="~/../Content/images/Contact.png">' +
                            //    '<span style="color:#96CDCD;padding-left:4px" ng-if="item.isauto==1">Auto</span><span style="color:#FF8247;padding-left:4px" ng-if="item.isauto==0">User</span><a ng-click="grid.appScope.openTask(item)"> {{item.tasK_TYPE}}: {{item.tasK_CONTENT}}</a><hr style="margin-top:1px;margin-bottom:0px;height:1px;border:none dotted #185598;" />' +
                            //    '</span></div></div>',
                            cellTemplate: '<div style="height:60px; overflow:auto"><div>' +
                                '<span ng-repeat="item in row.entity.taskDetail" >' +
                                '<a ng-click="grid.appScope.openTask(item)">{{item.tasK_DATE|date:\'yyyyMMdd\'}}-{{item.tasK_TYPE}}:{{item.tasK_CONTENT}}</a><hr style="margin-top:1px;margin-bottom:0px;height:1px;border:none dotted #185598;" />' +
                                '</span></div></div>',
                            enableColumnResizing: true
                        },
                        {
                            field: 'commentExpirationDate', displayName: 'Comment Expiration Date', width: '160', 
                            cellTemplate: '<div style="margin-top:5px;text-align:center;color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};">{{row.entity.commentExpirationDate|date:\'yyyy-MM-dd\'}}</div>'},
                    ],

                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;

                    }
                };
            };

            //发票行，添加客户回复
            $scope.addResponse = function (row) {

                contactHistoryProxy.get("", function (responseInstance) {
                    responseInstance["title"] = "Response Create";
                    responseInstance.logAction = "ALL ACCOUNT";
                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-response.tpl.html',
                        windowClass: 'modalDialog',
                        controller: 'contactResponseCtrl',
                        size: 'lg',
                        resolve: {
                            responseInstance: function () { return responseInstance; },
                            custnum: function () { return row.customerNo; },
                            invoiceIds: function () { return []; },
                            siteuseId: function () { return row.siteUseId; },
                            legalEntity: function () { return row.legalEntity; }
                        }
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init(false);
                        }
                    });
                }, function () {

                })
            }


            $scope.ViewCustomer = function (customerNo, siteuseid) {
                window.open('#/cust/masterData/' + customerNo + ',' + siteuseid);
            };
            $scope.ViewMyInvoice = function (legal, customerNo, siteuseid) {
                window.open('#/myinvoices?custNo=' + customerNo + '&legal=' + legal + '&siteUseID=' + siteuseid);
            };
            $scope.ViewSoa = function (customerNo, siteuseid) {
                window.open('#/soa/sendSoa/' + customerNo + '/create/' + siteuseid);
            };
            $scope.ViewSoaMail = function (lastSoaMailId) {
                if (lastSoaMailId !== '0') {
                    window.open('#/maillist/maildetail/' + lastSoaMailId);
                }
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

            $scope.createGrid();

            //查询条件JSON
            buildFilter = function () {
                var search = {};
                return angular.toJson(search);
            };

            //################################################################
            //change page size && change page && calculate page parameter
            //################################################################
            //paging size change
            $scope.pschangeexecute = function (slexecute) {
                var index = 1;
                $scope.curpexecute = 1;     //init page
                var sDate = $filter('date')($scope.startDate, "yyyy-MM-dd HH:mm:ss");
                filstr = buildFilter();
                taskProxy.queryTask(index, slexecute, filstr, $scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, sDate, $scope.status, function(list) {
                    $scope.taskList.data = list.taskRow;
                    $scope.iperexecute = slexecute;
                    $scope.ttexecute = list.count;
                    $scope.totalNum = list.count;
                    $scope.calculate($scope.curpexecute, $scope.iperexecute, list.taskRow.length);
                    $interval(function () {
                        $(".ui-grid-canvas").children('div').children('div').children('div').css('height', '100px');
                        $('.rb-rating').rating({
                            'showCaption': false,
                            'showClear': false,
                            'disabled': true,
                            'stars': '3',
                            'min': '0',
                            'max': '3',
                            'step': '1',
                            'size': 'ss',
                            'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                        });
                    }, 0, 1);
                }, function (error) {
                    alert(error);
                });

            };

            //paging change
            $scope.executepChanged = function () {
                var index = $scope.curpexecute;
                var sDate = $filter('date')($scope.startDate, "yyyy-MM-dd HH:mm:ss");
                filstr = buildFilter();
                taskProxy.queryTask(index, $scope.iperexecute, filstr, $scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, sDate, $scope.status, function (list) {
                    $scope.taskList.data = list.taskRow;
                    $scope.ttexecute = list.count;
                    $scope.totalNum = list.count;
                    $scope.calculate($scope.curpexecute, $scope.iperexecute, list.taskRow.length);
                    $interval(function () {
                        $(".ui-grid-canvas").children('div').children('div').children('div').css('height', '100px');
                        $('.rb-rating').rating({
                            'showCaption': false,
                            'showClear': false,
                            'disabled': true,
                            'stars': '3',
                            'min': '0',
                            'max': '3',
                            'step': '1',
                            'size': 'ss',
                            'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                        });
                    }, 0, 1);
                }, function (error) {
                    alert(error);
                });
            };

            //calculate page parameter
            $scope.calculate = function (currentPage, itemsperpage, count) {
                if (count === 0) {
                    $scope.fromItem = 0;
                } else {
                    $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                }
                $scope.toItem = (currentPage - 1) * itemsperpage + count;
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
                    taskProxy.queryTask(1, $scope.iperexecute, filstr, $scope.legalEntity, $scope.custNum, $scope.custName, $scope.SiteUseId, sDate, $scope.status, function (list) {
                        $scope.taskList.data = list.taskRow;
                        $scope.ttexecute = list.count;
                        $scope.totalNum = list.count;
                        $scope.calculate($scope.curpexecute, $scope.iperexecute, list.taskRow.length);
                        if (retrieveDetail) {
                        $interval(function () {
                            $(".ui-grid-canvas").children('div').children('div').children('div').css('height', '60px');
                            $(".tab-content .ui-grid-canvas").children('div').children('div').children('div').css('height', '25px');
                            //$scope.searchPMT();
                            //$scope.searchPTP();
                            //$scope.searchDispute();
                            //$scope.searchRemindding();
                            }, 0, 1);
                        }

                    }, function (error) {
                        alert(error);
                    });
                }
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