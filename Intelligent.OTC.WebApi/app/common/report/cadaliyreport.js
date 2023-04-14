angular.module('app.cadaliyreport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/cadaliyreport', {
                templateUrl: 'app/common/report/cadaliyreport.tpl.html',

                controller: 'cadaliyreportCtrl',
                resolve: {
                    legallist: ['siteProxy', function (siteProxy) {
                        return siteProxy.Site("");
                    }],
                    bsTypelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("085");
                    }],
                }
            });
    }])

    //*****************************************header***************************s
    .controller('cadaliyreportCtrl',
        ['$scope', '$interval', 'caCommonProxy', 'modalService', 'legallist', 'bsTypelist',
            function ($scope, $interval, caCommonProxy, modalService, legallist, bsTypelist) {
                $scope.$parent.helloAngular = "OTC - CashApplication Count Report";

                $scope.legalentitylist = legallist;
                $scope.bsTypelist = bsTypelist;
                $scope.enters = [{ value: "已入账" }, { value: "入账中" }, { value: "未入账" }];
                $scope.crossOffs = [{ value: "已消账" }, { value: "消账中" }, { value: "未消账" }];
                $scope.enterMails = [{ value: "Initialized" }, { value: "发送中" }, { value: "Finish" }, { value: "已发送" }, { value: "未发送" }];
                console.log($scope.enterMails);

                var now = new Date();

                $scope.reportList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    data: 'members',
                    columnDefs: [
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '150', cellClass: 'left' },
                        { field: 'bstype', displayName: 'BSTYPE', width: '150' },
                        { field: 'transactioN_NUMBER', displayName: 'TRANSACTION_NUMBER', width: '180' },
                        { field: 'transactioN_AMOUNT', displayName: 'TRANSACTION_AMOUNT', width: '180', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                        { field: 'valuE_DATE', displayName: 'VALUE_DATE', width: '150', cellFilter: 'date:\'yyyy-MM-dd\'' },
                        { field: 'currency', displayName: 'CURRENCY', width: '150', cellClass: 'right' },
                        { field: 'currenT_AMOUNT', displayName: 'CURRENT_AMOUNT', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                        { field: 'uncleaR_AMOUNT', displayName: 'UNCLEAR_AMOUNT', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                        { field: 'creatE_DATE', displayName: 'CREATE_DATE', width: '150', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'applY_STATUS', displayName: 'APPLY_STATUS', width: '150' },
                        { field: 'applY_TIME', displayName: 'APPLY_TIME', width: '150', cellClass: 'left', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'clearinG_STATUS', displayName: 'CLEARING_STATUS', width: '150' },
                        { field: 'clearinG_TIME', displayName: 'CLEARING_TIME', width: '150', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'postMailStatus', displayName: 'PostMailStatus', width: '150' },
                        { field: 'postMailSendTime', displayName: 'PostMailSendTime', width: '150', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'postMailSubject', displayName: 'PostMailSubject', width: '150' },
                        { field: 'postMailTo', displayName: 'PostMailTo', width: '150' },
                        { field: 'postMailCc', displayName: 'PostMailCc', width: '150' },
                        { field: 'clearMailStatus', displayName: 'ClearMailStatus', width: '150' },
                        { field: 'clearSendTime', displayName: 'ClearSendTime', width: '150', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'clearMailSubject', displayName: 'ClearMailSubject', width: '150' },
                        { field: 'clearMailTo', displayName: 'ClearMailTo', width: '150' },
                        { field: 'clearMailCc', displayName: 'ClearMailCc', width: '150' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
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
                    var index = $scope.currentPage;
                    caCommonProxy.queryCadaliyReport($scope.currentPage, selectedLevelId, $scope.legalEntity, $scope.bsType, $scope.CreateDateFrom, $scope.CreateDateTo,
                        $scope.transNumber, $scope.transAmount, $scope.ValueDateFrom,
                        $scope.ValueDateTo, $scope.enter, $scope.enterMail,
                        $scope.crossOff, $scope.crossOffMail, function (list) {
                            $scope.itemsperpage = selectedLevelId;
                            if (list.length > 0) {
                                $scope.members = list;
                                $scope.totalItems = list[0].count;
                            }
                            else {
                                $scope.members = list;
                                $scope.totalItems = 0;
                            }
                        }, function (res) {
                            alert(res);
                        });
                };

                //翻页
                $scope.pageChanged = function () {
                    //filstr = buildFilter();
                    //alert("d");
                    var index = $scope.currentPage;

                    caCommonProxy.queryCadaliyReport($scope.currentPage, $scope.itemsperpage, $scope.legalEntity, $scope.bsType, $scope.CreateDateFrom, $scope.CreateDateTo,
                        $scope.transNumber, $scope.transAmount, $scope.ValueDateFrom,
                        $scope.ValueDateTo, $scope.enter, $scope.enterMail,
                        $scope.crossOff, $scope.crossOffMail, function (list) {
                        if (list.length > 0) {
                            $scope.members = list;
                            $scope.totalItems = list[0].count;
                        }
                        else {
                            $scope.members = list;
                            $scope.totalItems = 0;
                        }
                    }, function (res) {
                        alert(res);
                    });
                };

                $scope.searchReport = function () {
                    $scope.currentPage = 1;
                    caCommonProxy.queryCadaliyReport($scope.currentPage, $scope.itemsperpage, $scope.legalEntity, $scope.bsType, $scope.CreateDateFrom, $scope.CreateDateTo,
                        $scope.transNumber, $scope.transAmount, $scope.ValueDateFrom,
                        $scope.ValueDateTo, $scope.enter, $scope.enterMail,
                        $scope.crossOff, $scope.crossOffMail, function (result) {
                            if (result !== null && result.length > 0) {
                                $scope.members = result;
                                $scope.totalItems = result[0].count;
                            }
                            else {
                                $scope.members = result;
                                $scope.totalItems = 0;
                            }
                            //$scope.members = result;
                        });
                };

                $scope.exportReport = function () {
                    caCommonProxy.exportCadaliyReport($scope.legalEntity, $scope.bsType, $scope.CreateDateFrom, $scope.CreateDateTo,
                        $scope.transNumber, $scope.transAmount, $scope.ValueDateFrom,
                        $scope.ValueDateTo, $scope.enter, $scope.enterMail,
                        $scope.crossOff, $scope.crossOffMail);
                };

            }]);