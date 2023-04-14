angular.module('app.identifyCustomer', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/identifyCustomer', {
                templateUrl: 'app/cashapplication/identifyCustomer/identifyCustomer.tpl.html',
                controller: 'identifyCustomerCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('identifyCustomerCtrl',
        ['$scope', '$filter', '$interval', 'caHisDataProxy', 'modalService', '$location',
            function ($scope, $filter, $interval, caHisDataProxy, modalService, $location) {
                
                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];


                // bank grid start
                $scope.bankStartIndex = 0;
                $scope.bankSelectedLevel = 20;  //下拉单页容量初始化
                $scope.bankItemsperpage = 20;
                $scope.bankCurrentPage = 1; //当前页
                $scope.bankMaxSize = 10; //分页显示的最大页  

                $scope.bankHisDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    //enableCellEditOnFocus: true,
                    data: 'bankList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'matchStatus', displayName: 'Match Status', width: '120' },
                        { field: 'transactionNumber', displayName: 'Transaction Number', width: '120' },
                        { field: 'transactionAmount', displayName: 'Transaction Amount', width: '120' },
                        { field: 'currency', displayName: 'Currency', width: '120' },
                        { field: 'valueDate', displayName: 'Value Date', width: '120' },
                        {
                            field: 'agentCustomerNumber', displayName: 'Agent Customer Number', width: '120',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewAgentCustomer(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.agentCustomerNumber}}</a></div>'
                        },
                        { field: 'agentCustomerName', displayName: 'Agent Customer Name', width: '120' },
                        {
                            field: 'paymentCustomerNumber', displayName: 'Payment Customer Number', width: '120',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewPaymentCustomer(row.entity.customerNo,row.entity.siteUseId)">{{row.entity.agentCustomerNumber}}</a></div>'
                        },
                        { field: 'paymentCustomerName', displayName: 'Payment Customer Name', width: '120' },
                        { field: 'siteUseId', displayName: 'Site Use ID', width: '120' },
                        { field: 'reference1', displayName: 'Reference 1', width: '120' },
                        { field: 'reference2', displayName: 'Reference 2', width: '120' },
                        { field: 'reference3', displayName: 'Reference 3', width: '120' },
                        { field: 'fixedBankCharge', displayName: 'Fixed Bank Charge', width: '120', enableCellEdit: true},
                        { field: 'chargeRangeFrom', displayName: 'Charge Range From', width: '120', enableCellEdit: true },
                        { field: 'chargeRangeTo', displayName: 'Charge Range To', width: '120', enableCellEdit: true },
                        { field: 'chargeType', displayName: 'Charge Type', width: '120' },
                        { field: 'updateDate', displayName: 'Last Modified Time', width: '120' },
                        { field: 'createDate', displayName: 'Create Time', width: '120' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.bankGridApi = gridApi;

                    }
                };

                //Detail单页容量变化
                $scope.bankPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getBankHisDataDetails($scope.bankCurrentPage, selectedLevelId, function (result) {
                        $scope.bankItemsperpage = selectedLevelId;
                        $scope.bankTotalItems = result.count;
                        $scope.bankList = result.dataRows;
                        $scope.bankStartIndex = ($scope.currentPage - 1) * $scope.bankItemsperpage;
                    })
                };

                //Detail翻页
                $scope.bankPageChanged = function () {
                    caHisDataProxy.getBankHisDataDetails($scope.bankCurrentPage, $scope.bankItemsperpage, function (result) {
                        $scope.bankTotalItems = result.count;
                        $scope.bankList = result.dataRows;
                        $scope.bankStartIndex = ($scope.bankCurrentPage - 1) * $scope.bankItemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.bankPageChanged();

                $scope.ViewCustomer = function (customerNo, siteuseid) {
                    window.open('#/cust/masterData/' + customerNo + ',' + siteuseid);
                };

                // bank grid end

                // bank search start

                //查询条件展开、合上
                var bankShow = false;
                $scope.bankOpenFilter = function () {
                    bankShow = !bankShow;
                    if (bankShow) {
                        $("#bankDataSearch").show();
                    } else {
                        $("#bankDataSearch").hide();
                    }
                };

                // bank search end

                // bank operation start

                // 查看Agent Customer列表
                $scope.viewAgentCustomer = function (bankId) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/agentcustomer/agentcustomer.tpl.html',
                        controller: 'agentCustomerCtrl',
                        size: 'lg',
                        resolve: {
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.viewPaymentCustomer = function (bankId) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/paymentcustomer/paymentcustomer.tpl.html',
                        controller: 'paymentCustomerCtrl',
                        size: 'lg',
                        resolve: {
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                };

                // bank operation end


                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                }

            }]);