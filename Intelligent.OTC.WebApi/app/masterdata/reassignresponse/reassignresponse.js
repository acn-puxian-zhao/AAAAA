angular.module('app.masterdata.reassignresponse', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/admin/reassignresponse', {
            templateUrl: 'app/masterdata/reassignresponse/reassignresponse-list-tpl.html',
            controller: 'ReassignResponseCtrl',
            resolve: {
                //首次加载第一页
                mailGrid: ['mailProxy', function (mailProxy) {
                    return mailProxy.initRecMailPaging(1, 10, "");
                } ]
            }
        });
    } ])
    .controller('ReassignResponseCtrl', ['$scope', 'mailGrid', 'mailProxy',
    function ($scope, mailGrid, mailProxy) {


        var filterStr = ""
        $scope.totalItems = mailGrid[0].count;
        $scope.list = mailGrid[0].results;
        $scope.itemsperpage = 10;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页

        //翻页
        $scope.pageChanged = function () {
            var index = $scope.currentPage;
            mailProxy.initRecMailPaging(index, $scope.itemsperpage, filterStr, function (list) {
                $scope.list = list[0].results;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        };

        //加载nggrid数据绑定
        $scope.recMailGrid = {
            data: 'list',
            columnDefs: [
                    { field: 'sortId', displayName: '#', width: '20' },
                    { field: 'from', displayName: 'From', width: '200' },
                    { field: 'createTime', displayName: 'Date', width: '200' },
                    { field: 'subject', displayName: 'Title', width: '300' },
                    { field: 'bussinessReference', displayName: 'Customer', width: '200' },
                    { name: 'o', displayName: 'Operation', width: '200',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="grid.appScope.DelPaymentcircle(row.entity)"> Reassign</a></div>'
                    }
                    ]
        };

    } ])