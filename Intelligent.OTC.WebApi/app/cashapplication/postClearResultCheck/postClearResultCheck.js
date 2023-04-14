angular.module('app.cashapplication.postClearResultCheck', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/postClearResultCheck', {
                templateUrl: 'app/cashapplication/postClearResultCheck/postClearResultCheck-list.tpl.html',
                controller: 'postClearResultCheckCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('postClearResultCheckCtrl',
        ['$scope', '$interval', 'caCommonProxy', 'modalService',
            function ($scope, $interval, caCommonProxy,modalService) {

                caCommonProxy.GetDateByDay(-1, function (result) {
                    $scope.valueDateF = result;
                    $scope.pageChanged();
                }, function (error) {
                    alert(error);
                });
                    caCommonProxy.GetDateByDay(0, function (result) {
                    $scope.valueDateT = result;
                    $scope.pageChanged();
                }, function (error) {
                    alert(error);
                });


                $scope.createpostListGrid = function () {
                    $scope.postList = {
                        multiSelect: false,
                        enableFullRowSelection: false,
                        enableFiltering: true,
                        noUnselect: true,
                        enableRowSelection: true,
                        enableSelectAll: false,
                        enableRowHeaderSelection: false,
                        columnDefs: [
                            { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            {
                                field: 'status', displayName: 'Status', width: '60', cellClass: 'center',
                                cellTemplate: '<div style="margin-top:6px;color:red">{{row.entity.status }}</div>'
                            },
                            { field: 'changeDate', displayName: 'Post Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '80', cellClass: 'center' },
                            { field: 'bsTransactionInc', displayName: 'BSTransactionInc', width: '140', cellClass: 'left' },
                            { field: 'bsCurrency', displayName: 'BSCurrency', width: '80', cellClass: 'center' },
                            { field: 'postAmount', displayName: 'PostAmount', width: '120', cellFilter: 'number:2', cellClass: 'right' },
                            { field: 'customerNum', displayName: 'CustomerNum', width: '120', cellClass: 'center' },
                            { field: 'siteUseId', displayName: 'SiteUseId', width: '120', cellClass: 'center' },
                            { field: 'oracleChange', displayName: 'OracleChange', width: '120', cellFilter: 'number:2', cellClass: 'right' },
                            { field: 'charge', displayName: 'Charge', width: '120', cellFilter: 'number:2', cellClass: 'right' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.createclearListGrid = function () {
                    $scope.clearList = {
                        multiSelect: false,
                        enableFullRowSelection: true,
                        enableFiltering: true,
                        noUnselect: true,
                        enableRowSelection: true,
                        enableSelectAll: false,
                        enableRowHeaderSelection: false,
                        columnDefs: [
                            { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            {
                                field: 'status', displayName: 'Status', width: '60', cellClass: 'center',
                                cellTemplate: '<div style="margin-top:6px;color:red">{{row.entity.status }}</div>'
                            },
                            { field: 'changeDate', displayName: 'Clear Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '80', cellClass: 'center' },
                            { field: 'bsTransactionInc', displayName: 'BSTransactionInc', width: '140', cellClass: 'left' },
                            { field: 'bsCurrency', displayName: 'BSCurrency', width: '80', cellClass: 'center' },
                            { field: 'customerNum', displayName: 'CustomerNum', width: '120', cellClass: 'center' },
                            { field: 'siteUseId', displayName: 'SiteUseId', width: '120', cellClass: 'center' },
                            { field: 'invoiceNum', displayName: 'InvoiceNum', width: '120', cellClass: 'left' },
                            { field: 'clearAmount', displayName: 'ClearAmount', width: '120', cellFilter: 'number:2', cellClass: 'right' },
                            { field: 'oracleChange', displayName: 'OracleChange', width: '120', cellFilter: 'number:2', cellClass: 'right' },
                            { field: 'charge', displayName: 'Charge', width: '120', cellFilter: 'number:2', cellClass: 'right' }

                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                $scope.pageChanged = function () {
                    if ($scope.valueDateF && $scope.valueDateT) {
                        caCommonProxy.getCaPostResultCheck($scope.valueDateF, $scope.valueDateT, function (result) {
                            $scope.postList.data = result;
                        }, function (error) {
                            alert(error);
                            });
                        caCommonProxy.getCaClearResultCheck($scope.valueDateF, $scope.valueDateT, function (result) {
                            $scope.clearList.data = result;
                        }, function (error) {
                            alert(error);
                        });
                    }
                };

                $scope.createpostListGrid();
                $scope.createclearListGrid();

                $scope.pageChanged();

                $scope.selectClearPage = function () {
                    $scope.createclearListGrid();
                };

                $scope.export = function () {
                    caCommonProxy.exportPostClearResult($scope.valueDateF, $scope.valueDateT);
                };

        }]);




