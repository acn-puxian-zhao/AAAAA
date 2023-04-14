angular.module('app.cashapplicationcountreport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/cacountreport', {
                templateUrl: 'app/common/report/cashapplicationcountreport.tpl.html',

                controller: 'cacountreportCtrl',
                resolve: {
                    legallist: ['siteProxy', function (siteProxy) {
                        return siteProxy.Site("");
                    }],
                }
            });
    }])

    //*****************************************header***************************s
    .controller('cacountreportCtrl',
        ['$scope', '$interval', 'caCommonProxy', 'modalService', 'legallist',
            function ($scope, $interval, caCommonProxy, modalService, legallist) {
            $scope.$parent.helloAngular = "OTC - CashApplication Count Report";

            $scope.legalentitylist = legallist;

            $scope.selected = [];// 用来保存选中的数据的值
            // 点击选中事件
            $scope.updateAll = function (val) {
                if (val) {
                    $scope.selected = [];
                    $scope.legalentitylist.forEach(row => {
                        row.check = true;
                        $scope.selected.push(row.legalEntity);
                    });
                } else {
                    $scope.selected = [];
                    $scope.legalentitylist.forEach(row => {
                        row.check = false;
                    });
                }
            };

            $scope.updateAll(true);

            var now = new Date();

            $scope.reportList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                data: 'members',
                columnDefs: [
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '150', cellClass: 'left'},
                    { field: 'totalBS', displayName: 'TotalBS', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'closedBS', displayName: 'ClosedBS', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'isReconed', displayName: 'IS Reconed', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right'},
                    { field: 'pmtDetail', displayName: 'Has PMTDetail', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right'},
                    { field: 'pmtDetailPersent', displayName: 'PMTDetail Persent %', width: '150', cellClass: 'right' },
                    { field: 'ar', displayName: 'AR', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'arPersent', displayName: 'AR Persent %', width: '150', cellClass: 'right' },
                    { field: 'ptp', displayName: 'PTP', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'manual', displayName: 'MANUAL', width: '150', cellFilter: 'number:0', type: 'number', cellClass: 'right'}
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

                $scope.pmtList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    enableHorizontalScrollbar: true,
                    noUnselect: false,
                    data: 'pmts',
                    columnDefs: [
                        { field: 'groupType', displayName: 'GroupType', width: '100', cellClass: 'left' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'left' },
                        { field: 'bstype', displayName: 'BSTYPE', width: '100', cellClass: 'left' },
                        { field: 'transactioN_Number', displayName: 'Transaction Number', width: '150', cellClass: 'left' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'forwarD_NUM', displayName: 'Forward Num', width: '150', cellClass: 'left' },
                        { field: 'forwarD_NAME', displayName: 'Forward Name', width: '250', cellClass: 'left' },
                        { field: 'customeR_NUM', displayName: 'Customer Num', width: '150', cellClass: 'left' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '250', cellClass: 'left' },
                        { field: 'pmtnumber', displayName: 'PMT Number', width: '150', cellClass: 'left' },
                        { field: 'groupNo', displayName: 'GroupNo', width: '150', cellClass: 'left' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                $scope.arList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    enableHorizontalScrollbar: true,
                    noUnselect: false,
                    data: 'ars',
                    columnDefs: [
                        { field: 'groupType', displayName: 'GroupType', width: '100', cellClass: 'left' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'left' },
                        { field: 'bstype', displayName: 'BSTYPE', width: '100', cellClass: 'left' },
                        { field: 'transactioN_Number', displayName: 'Transaction Number', width: '150', cellClass: 'left' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'forwarD_NUM', displayName: 'Forward Num', width: '150', cellClass: 'left' },
                        { field: 'forwarD_NAME', displayName: 'Forward Name', width: '250', cellClass: 'left' },
                        { field: 'customeR_NUM', displayName: 'Customer Num', width: '150', cellClass: 'left' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '250', cellClass: 'left' },
                        { field: 'pmtnumber', displayName: 'PMT Number', width: '150', cellClass: 'left' },
                        { field: 'groupNo', displayName: 'GroupNo', width: '150', cellClass: 'left' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                $scope.ptpList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    enableHorizontalScrollbar: true,
                    noUnselect: false,
                    data: 'ptps',
                    columnDefs: [
                        { field: 'groupType', displayName: 'GroupType', width: '100', cellClass: 'left' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'left' },
                        { field: 'bstype', displayName: 'BSTYPE', width: '100', cellClass: 'left' },
                        { field: 'transactioN_Number', displayName: 'Transaction Number', width: '150', cellClass: 'left' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'forwarD_NUM', displayName: 'Forward Num', width: '150', cellClass: 'left' },
                        { field: 'forwarD_NAME', displayName: 'Forward Name', width: '250', cellClass: 'left' },
                        { field: 'customeR_NUM', displayName: 'Customer Num', width: '150', cellClass: 'left' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '250', cellClass: 'left' },
                        { field: 'pmtnumber', displayName: 'PMT Number', width: '150', cellClass: 'left' },
                        { field: 'groupNo', displayName: 'GroupNo', width: '150', cellClass: 'left' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                $scope.manualList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableHorizontalScrollbar: true,
                    enableFiltering: true,
                    noUnselect: false,
                    data: 'manuals',
                    columnDefs: [
                        { field: 'groupType', displayName: 'GroupType', width: '100', cellClass: 'left' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'left' },
                        { field: 'bstype', displayName: 'BSTYPE', width: '100', cellClass: 'left' },
                        { field: 'transactioN_Number', displayName: 'Transaction Number', width: '150', cellClass: 'left' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'currency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'forwarD_NUM', displayName: 'Forward Num', width: '150', cellClass: 'left' },
                        { field: 'forwarD_NAME', displayName: 'Forward Name', width: '250', cellClass: 'left' },
                        { field: 'customeR_NUM', displayName: 'Customer Num', width: '150', cellClass: 'left' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '250', cellClass: 'left' },
                        { field: 'pmtnumber', displayName: 'PMT Number', width: '150', cellClass: 'left' },
                        { field: 'groupNo', displayName: 'GroupNo', width: '150', cellClass: 'left' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

            $scope.searchReport = function () {
                if ($scope.selected.length == 0) {
                    alert("Please select LegalEntity!");
                    return;
                }

                if (!$scope.ValueDateFrom) {
                    alert("Please select ValueDate From!");
                    return;
                }
                if (!$scope.ValueDateTo) {
                    $scope.ValueDateTo = '';
                }
                $scope.LegalEntitySelected = $scope.selected.join(',');
                caCommonProxy.queryCashApplicationCountReport($scope.LegalEntitySelected, $scope.ValueDateFrom, $scope.ValueDateTo, function (result) {
                    if (result !== null) {
                        $scope.members = result.total;
                        $scope.pmts = result.pmtList;
                        $scope.ars = result.arList;
                        $scope.ptps = result.ptpList;
                        $scope.manuals = result.manualList;
                    }
                });
            };

            $scope.exportReport = function () {
                if ($scope.selected.length == 0) {
                    alert("Please select LegalEntity!");
                    return;
                }

                if (!$scope.ValueDateFrom) {
                    alert("Please select ValueDate From!");
                    return;
                }
                if (!$scope.ValueDateTo) {
                    $scope.ValueDateTo = '';
                }
                $scope.LegalEntitySelected = $scope.selected.join(',');
                caCommonProxy.exportCashApplicationCountReport($scope.LegalEntitySelected, $scope.ValueDateFrom, $scope.ValueDateTo);
            };

        }]);