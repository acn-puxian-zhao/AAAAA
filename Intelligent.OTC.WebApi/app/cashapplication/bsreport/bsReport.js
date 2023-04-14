angular.module('app.cashapplication.bsReport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/bsReport', {
                templateUrl: 'app/cashapplication/bsreport/bsReport-list.tpl.html',
                controller: 'bsReportCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('bsReportCtrl',
        ['$scope', '$interval', 'caCommonProxy', 'modalService',
            function ($scope, $interval, caCommonProxy,modalService) {

                caCommonProxy.GetDateByDay(-7, function (result) {
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

                $scope.levelList_bsReport = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];
                // bank grid start
                $scope.bsReportStartIndex = 0;
                $scope.bsReportSelectedLevel = 20;  //下拉单页容量初始化
                $scope.bsReportItemsperpage = 20;
                $scope.bsReportCurrentPage = 1; //当前页
                $scope.bsReportMaxSize = 10; //分页显示的最大页  

                $scope.createbsReportGrid = function () {
                    $scope.bsReport = {
                        multiSelect: false,
                        enableFullRowSelection: false,
                        enableFiltering: true,
                        noUnselect: true,
                        enableRowSelection: true,
                        enableSelectAll: false,
                        enableRowHeaderSelection: false,
                        columnDefs: [
                            { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '60', cellClass: 'center' },
                            { field: 'transactionINC', displayName: 'Transaction INC', width: '80', cellClass: 'left' },
                            { field: 'bankID', displayName: 'Bank ID', width: '100', cellClass: 'left' },
                            { field: 'accountNumber', displayName: 'Account Number', width: '100', cellClass: 'left' },
                            { field: 'accountID', displayName: 'Account ID', width: '60', cellClass: 'left' },
                            { field: 'accountName', displayName: 'Account Name', width: '150', cellClass: 'left' },
                            { field: 'accountOwnerID', displayName: 'Account Owner ID', width: '80', cellClass: 'left' },
                            { field: 'accountCountry', displayName: 'Account Country', width: '60', cellClass: 'left' },
                            { field: 'transactionDate', displayName: 'Transaction Date', width: '80', cellClass: 'center' },
                            { field: 'valueDate', displayName: 'Value Date', width: '80', cellClass: 'center' },
                            { field: 'currency', displayName: 'Currency', width: '50', cellClass: 'center' },
                            { field: 'amount', displayName: 'Amount', width: '100', cellFilter: 'number:2', cellClass: 'right' },
                            { field: 'referenceDRNM', displayName: 'Reference DR NM', width: '120', cellClass: 'left' },
                            { field: 'referenceBRCR', displayName: 'Reference BRCR', width: '80', cellClass: 'left' },
                            { field: 'referenceDESCR', displayName: 'Reference DESCR', width: '80', cellClass: 'left' },
                            { field: 'referenceENDTOEND', displayName: 'Reference ENDTOEND', width: '80', cellClass: 'left' },
                            { field: 'description', displayName: 'Description', width: '200', cellClass: 'left' },
                            { field: 'referenceBRTN', displayName: 'Reference BRTN', width: '80', cellClass: 'left' },
                            { field: 'referenceDRBNK', displayName: 'Reference DR BNK', width: '80', cellClass: 'left' },
                            { field: 'userCode', displayName: 'User Code', width: '80', cellClass: 'left' },
                            { field: 'userCodeDescription', displayName: 'User Code Description', width: '80', cellClass: 'left' },
                            { field: 'itemType', displayName: 'Item Type', width: '50', cellClass: 'left' },
                            { field: 'owner', displayName: 'Owner', width: '80', cellClass: 'left' },
                            { field: 'cheque', displayName: 'Cheque', width: '80', cellClass: 'left' },
                            { field: 'needchecking', displayName: 'NeedChecking', width: '80', cellClass: 'left' },
                            { field: 'area', displayName: 'Area', width: '80', cellClass: 'left' },
                            { field: 'week', displayName: 'Week', width: '80', cellClass: 'left' },
                            { field: 'type', displayName: 'Type', width: '80', cellClass: 'left' },
                            { field: 'unknownType', displayName: 'Unknown Type', width: '80', cellClass: 'left' },
                            { field: 'customerName', displayName: 'Customer Name', width: '200', cellClass: 'left' },
                            { field: 'account', displayName: 'Account', width: '80', cellClass: 'left' },
                            { field: 'siteUseId', displayName: 'Site Use Id', width: '80', cellClass: 'left' },
                            { field: 'eB_Name', displayName: 'EB_Name', width: '120', cellClass: 'left' },
                            { field: 'term', displayName: 'Term', width: '100', cellClass: 'left' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.gridApi = gridApi;
                        }
                    };
                };

                //Detail单页容量变化
                $scope.bsReportPageSizeChange = function (selectedLevelId) {
                    caCommonProxy.getbsReport($scope.valueDateF, $scope.valueDateT, $scope.bsReportCurrentPage, selectedLevelId, function (result) {
                        $scope.bsReportItemsperpage = selectedLevelId;
                        $scope.bsReportTotalItems = result.count;
                        $scope.bsReport.data = result.dataRows;
                        $scope.bsReportStartIndex = ($scope.currentPage - 1) * $scope.bsReportItemsperpage;
                        $scope.calculate_bsReport($scope.bsReportCurrentPage, $scope.bsReportItemsperpage, $scope.bsReportTotalItems);
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.pageChanged = function () {
                    if ($scope.valueDateF && $scope.valueDateT) {
                        caCommonProxy.getbsReport($scope.valueDateF, $scope.valueDateT, $scope.bsReportCurrentPage, $scope.bsReportItemsperpage, function (result) {
                            $scope.bsReport.data = result.dataRows;
                            $scope.bsReportTotalItems = result.count;
                            $scope.calculate_bsReport($scope.bsReportCurrentPage, $scope.bsReportItemsperpage, $scope.bsReportTotalItems);
                        }, function (error) {
                            alert(error);
                            });
                    }

                };


                $scope.calculate_bsReport = function (currentPage, itemsperpage, count) {
                    if (count === 0) {
                        $scope.fromItem_bsReport = 0;
                    } else {
                        $scope.fromItem_bsReport = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem_bsReport = (currentPage - 1) * itemsperpage + count;
                };

                $scope.createbsReportGrid();

                $scope.pageChanged();

                $scope.export = function () {
                    caCommonProxy.exportbsReport($scope.valueDateF, $scope.valueDateT, function (path) {
                        if (path !== null) {
                            window.location = path;
                            alert("Export Successful!");
                        }
                    });
                };

        }]);




