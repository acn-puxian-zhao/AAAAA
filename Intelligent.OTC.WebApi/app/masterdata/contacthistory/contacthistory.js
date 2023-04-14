angular.module('app.masterdata.contacthistory', [])
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
        .when('/common/contacthistory', {
            templateUrl: 'app/masterdata/contacthistory/contacthistory-list.tpl.html',
            controller: 'contacthistoryList',
            resolve: {}
        });
} ])
    .controller('contacthistoryList',
    ['$scope', 'custnum', 'site', 'contactHistoryProxy', 'baseDataProxy', '$uibModalInstance', 'contactProxy', 'contactTypeInfo',
    function ($scope, custnum, site, contactHistoryProxy, baseDataProxy, $uibModalInstance, contactProxy, contactTypeInfo) {

        //contactTypeName for grid
        $scope.ContactTypeDPlist = contactTypeInfo;


        var filstr = " &$orderby= ContactDate desc &$filter=(CustomerNum eq '" + custnum + "') and (SiteCode eq '" + site + "') ";

        contactHistoryProxy.contactHistoryPaging(1, 10, filstr, function (contactHistoryLi) {
            $scope.list = contactHistoryLi[0].results; //首次当前页数据
            $scope.totalItems = contactHistoryLi[0].count; //查询结果初始化数量
        });

        $scope.itemsperpage = 10;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页

        //加载nggrid数据绑定
        $scope.contactHistoryList = {
            data: 'list',
            columnDefs: [
                            { field: 'contactType', displayName: 'ContactType',
                                cellTemplate: '<div hello="{valueMember: \'contactType\', basedata: \'grid.appScope.ContactTypeDPlist\'}"></div>'
                            },
                            { field: 'contacterId', displayName: 'Contact'

                            },
                            { field: 'collectorId', displayName: 'Operator' },
                            { field: 'contactDate', displayName: 'Contact Date', cellFilter: 'date:yyyy/MM/dd' },
                            { field: 'comments', displayName: 'Comments' },
                            ]
        };


        //翻页
        $scope.pageChanged = function () {

            var index = $scope.currentPage;
            contactHistoryProxy.contactHistoryPaging(index, $scope.itemsperpage, filstr, function (list) {
                $scope.list = list[0].results;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        };
        $scope.closeContactHistory = function () {
            $uibModalInstance.close();
        };


        $scope.searchContactHis = function () {

            filstr = " &$orderby= ContactDate desc &$filter=(CustomerNum eq '" + custnum + "') and (SiteCode eq '" + site + "') ";
            //组合过滤条件
            var filterStr = filstr;

            if ($scope.contactTypeValue) {
                filterStr += " and (ContactType eq '" + $scope.contactTypeValue + "')";
            }

            if ($scope.selectedDate) {
                filterStr += " and (ContactDate eq " + $scope.selectedDate + ")";
            }
            filstr = filterStr;

            contactHistoryProxy.contactHistoryPaging($scope.currentPage, $scope.itemsperpage, filterStr, function (lists) {
                if (lists != null) {
                    $scope.totalItems = lists[0].count;
                    $scope.list = lists[0].results;
                }
            })

        };



    } ]);