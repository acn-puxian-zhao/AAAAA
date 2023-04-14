angular.module('app.cashapplication.statusReport', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/statusreport', {
                templateUrl: 'app/cashapplication/statusReport/statusReport-list.tpl.html',
                controller: 'statusReportListCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('statusReportListCtrl',
        ['$scope', '$interval', 'statusReportProxy','caCommonProxy', 'modalService',
            function ($scope, $interval, statusReportProxy, caCommonProxy,modalService) {

                
            caCommonProxy.GetDateByMonth(-1, function (result) {
                $scope.valueDateF = result;
                $scope.pageChanged();
            }, function (error) {
                alert(error);
            });
            caCommonProxy.GetDateByMonth(0, function (result) {
                $scope.valueDateT = result;
                $scope.pageChanged();
            }, function (error) {
                alert(error);
            });

            $scope.dataList = {
                multiSelect: false,
                enableFullRowSelection: true,
                enableFiltering: true,
                noUnselect: true,
                enableRowSelection: true,
                enableSelectAll: false,
                enableRowHeaderSelection :false,
                columnDefs: [
                    { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'createDate', displayName: 'Date', width: '10%' },
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '10%' },
                    { field: 'total', displayName: 'Total', width: '10%', cellFilter: 'number:0',  cellClass: 'right' },
                    { field: 'unknowCount', displayName: 'Unknow Count', width: '10%', cellFilter: 'number:0', cellClass: 'right' },                   
                    { field: 'unknowPercent', displayName: 'Unknow Percent', width: '10%', cellClass: 'right' },
                    { field: 'unMatchCount', displayName: 'UnMatch Count', width: '10%', cellFilter: 'number:0', cellClass: 'right' },
                    { field: 'unMatchPercent', displayName: 'UnMatch Percent', width: '10%', cellClass: 'right' },
                    { field: 'matchCount', displayName: 'Match Count', width: '10%', cellFilter: 'number:0', cellClass: 'right' },
                    { field: 'matchPercent', displayName: 'Match Percent', width: '10%', cellClass: 'right' }

                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

                $scope.pageChanged = function () {
                    if ($scope.valueDateF && $scope.valueDateT) {
                        statusReportProxy.getStatusReport($scope.valueDateF, $scope.valueDateT, function (result) {
                            $scope.dataList.data = result;
                        }, function (error) {
                            alert(error);
                            });
                    }
                };

            $scope.pageChanged();


            $scope.export = function () {
                
                statusReportProxy.export($scope.valueDateF, $scope.valueDateT);
                };

                function getNowFormatDate(date) {
                var seperator1 = "-";
                var year = date.getFullYear();
                var month = date.getMonth() + 1;
                var strDate = date.getDate();
                if (month >= 1 && month <= 9) {
                    month = "0" + month;
                }
                if (strDate >= 0 && strDate <= 9) {
                    strDate = "0" + strDate;
                }
                var currentdate = year + seperator1 + month + seperator1 + strDate;
                return currentdate;
            }

        }]);




