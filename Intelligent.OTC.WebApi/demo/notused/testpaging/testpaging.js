angular.module('app.testpaging', [])

    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
        .when('/testpaging', {
            templateUrl: 'app/testpaging/testpaging-list.tpl.html',
            controller: 'testpagingListCtrl',
            resolve: {
                //首次加载第一页
                testPaging: ['testPagingProxy', function (testPagingProxy) {
                    return testPagingProxy.fortestPaging(1, 250, "");
                } ]
                ,
                //数据总数
                pagecount: ['testPagingProxy', function (testPagingProxy) {
                    return testPagingProxy.pagecount("");
                } ]
            }
        });
    } ])

.controller('testpagingListCtrl', ['$scope', 'crudListExtensions', 'testPaging', 'pagecount', 'testPagingProxy', 'baseDataProxy', 'siteProxy', function ($scope, crudListExtensions, testPaging, pagecount, testPagingProxy, baseDataProxy, siteProxy) {
    //Customer Class DropDownList数据邦定
    baseDataProxy.SysTypeDetail("P", function (cusclass) {
        $scope.cusclass = cusclass;
    });
    //Legal Entity DropDownList数据绑定
    siteProxy.Site("001", function (legallist) {
        $scope.legallist = legallist;
    });
    //Status Entity DropDownList数据绑定
    baseDataProxy.SysTypeDetail("001", function (statuslist) {
        $scope.statuslist = statuslist;
    });

    $scope.testPagings = testPaging;
    $scope.list = testPaging;
    var filstr = "";

    $scope.selectedUsers = [];

    $scope.$watchCollection('selectedUsers', function () {
        $scope.selectedUser = angular.copy($scope.selectedUsers[0]);
    });
    $scope.testPagings = {
        data: 'list',
        jqueryUITheme: true,
        multiSelect: true,
        showSelectionCheckbox: true,
        showFooter: true,
        enableColumnResize: true,
        selectedItems: $scope.selectedUsers,
        columnDefs: [
                { field: 'siteCode', displayName: 'Legal Entity' },
                { field: 'customerNum', displayName: 'Customer Code' },
                { field: 'customerName', displayName: 'Customer Name' },
                { field: 'billGroupName', displayName: 'Factory Group Name' },
                { field: 'customerClass', displayName: 'Customer Class' },
                { field: 'riskScore', displayName: 'Risk Score' },
                { field: 'totalAmt', displayName: 'Total A/R Balance' },
                { field: 'due90Amt', displayName: 'Over 90 days' },
                { field: 'creditLimit', displayName: 'Credit Limit' },
                { field: 'accountStatus', displayName: 'Status' },
                { field: 'accountStatus', displayName: 'Account Status' },
                { field: 'operator', displayName: 'Operator' },
                { field: '', displayName: 'Operation',
                    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="Save(selectedUser)"> Save </a><a ng-click="Del(selectedUser)">  Del  </a></div>'
                }
        //                    
             ]


    };
    angular.extend($scope, crudListExtensions('/testpaging'));

    //保存单行数据
    $scope.Save = function () {
        alert("save");
    }
    //删除单行数据
    $scope.Del = function () {
//        alert("del");
//        $http.get('api/testpaging/', { params: { id: '5' }
//        }).success(function (data, status, headers, config) {
//            //加载成功之后做一些事  
//        }).error(function (data, status, headers, config) {
//            //处理错误  
        //        }); 
        $scope.selectedUser.prototype.$remove = function (savecb, errorSavecb) { alert('remove called'); };
 
    }

    //单页容量下拉列表定义
    $scope.levelList = [
                        { "id": 250, "levelName": '250' },
                        { "id": 500, "levelName": '500' },
                        { "id": 1000, "levelName": '1000' },
                        { "id": 2000, "levelName": '2000' },
                        { "id": 5000, "levelName": '5000' }
                        ];

    $scope.selectedLevel = 250;  //下拉单页容量初始化

    $scope.totalItems = pagecount.length;
    $scope.itemsperpage = 250;
    $scope.currentPage = 1;
    $scope.maxSize = 10;
    //单页容量变化
    $scope.pagesizechange = function (selectedLevelId) {

        var index = $scope.currentPage;
        testPagingProxy.fortestPaging(index, selectedLevelId, filstr, function (list) {
            $scope.itemsperpage = selectedLevelId;
            $scope.list = list;
            $scope.testPagings = {
                data: 'list',
                jqueryUITheme: true,
                multiSelect: true,
                showFooter: true,
                enableColumnResize: true,
                selectedItems: $scope.selectedUsers,
                columnDefs: [
                    { field: 'siteCode', displayName: 'Legal Entity' },
                    { field: 'customerNum', displayName: 'Customer Code' },
                    { field: 'customerName', displayName: 'Customer Name' },
                    { field: 'billGroupName', displayName: 'Factory Group Name' },
                    { field: 'customerClass', displayName: 'Customer Class' },
                    { field: 'riskScore', displayName: 'Risk Score' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance' },
                    { field: 'due90Amt', displayName: 'Over 90 days' },
                    { field: 'creditLimit', displayName: 'Credit Limit' },
                    { field: 'accountStatus', displayName: 'Status' },
                    { field: 'accountStatus', displayName: 'Account Status' },
                    { field: 'operator', displayName: 'Operator' },
                    { field: '', displayName: 'Operation' ,
                    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="Save(selectedUser)"> Save </a><a ng-click="Del(selectedUser)">  Del  </a></div>'}
                    ]
            }
        });
    };

    //翻页
    $scope.pageChanged = function () {

        var index = $scope.currentPage;
        testPagingProxy.fortestPaging(index, $scope.itemsperpage, filstr, function (list) {
            $scope.list = list;
            $scope.testPagings = {
                data: 'list',
                jqueryUITheme: true,
                multiSelect: true,
                showFooter: true,
                enableColumnResize: true,
                selectedItems: $scope.selectedUsers,
                columnDefs: [
                    { field: 'siteCode', displayName: 'Legal Entity' },
                    { field: 'customerNum', displayName: 'Customer Code' },
                    { field: 'customerName', displayName: 'Customer Name' },
                    { field: 'billGroupName', displayName: 'Factory Group Name' },
                    { field: 'customerClass', displayName: 'Customer Class' },
                    { field: 'riskScore', displayName: 'Risk Score' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance' },
                    { field: 'due90Amt', displayName: 'Over 90 days' },
                    { field: 'creditLimit', displayName: 'Credit Limit' },
                    { field: 'accountStatus', displayName: 'Status' },
                    { field: 'accountStatus', displayName: 'Account Status' },
                    { field: 'operator', displayName: 'Operator' },
                    { field: '', displayName: 'Operation' ,
                    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="Save(selectedUser)"> Save </a><a ng-click="Del(selectedUser)">  Del  </a></div>'}
                    ]
            };
        }, function (error) {
            alert(error);
        });
    };

    //查询
    $scope.searchCollection = function () {

        //组合过滤条件
        var filterStr = '';
        if ($scope.custCode) {
            if (filterStr != "") {
                filterStr += "and (contains(CustomerNum,'" + $scope.custCode + "'))";
            } else {
                filterStr += "&$filter=(contains(CustomerNum,'" + $scope.custCode + "'))";
            }
        }
        if ($scope.custName) {
            if (filterStr != "") {
                filterStr += "and (contains(CustomerName,'" + $scope.custName + "'))"
            } else {
                filterStr += "&$filter=(contains(CustomerName,'" + $scope.custName + "'))";
            }

        }
        if ($scope.class) {
            if (filterStr != "") {
                filterStr += "and (CustomerClass eq '" + $scope.class + "')";
            } else {
                filterStr += "&$filter=(CustomerClass eq '" + $scope.class + "')";
            }
        }
        if ($scope.status) {
            if (filterStr != "") {
                filterStr += "and (AccountStatus eq '" + $scope.status + "')";
            } else {
                filterStr += "&$filter=(AccountStatus eq '" + $scope.status + "')";
            }
        }
        if ($scope.legal) {
            if (filterStr != "") {
                filterStr += "and (SiteCode eq '" + $scope.legal + "')";
            } else {
                filterStr += "&$filter=(SiteCode eq '" + $scope.legal + "')";
            }
        }
        filstr = filterStr;

        //数据总数
        testPagingProxy.pagecount(filstr, function (list1) {
            $scope.totalItems = list1.length;
        });

        $scope.currentPage = 1;
        testPagingProxy.searchPaging($scope.currentPage, $scope.itemsperpage, filterStr, function (list) {
            $scope.list = list;
            $scope.testPagings = {
                data: 'list',
                jqueryUITheme: true,
                multiSelect: true,
                showFooter: true,
                enableColumnResize: true,
                selectedItems: $scope.selectedUsers,
                columnDefs: [
                        { field: 'siteCode', displayName: 'Legal Entity' },
                        { field: 'customerNum', displayName: 'Customer Code' },
                        { field: 'customerName', displayName: 'Customer Name' },
                        { field: 'billGroupName', displayName: 'Factory Group Name' },
                        { field: 'customerClass', displayName: 'Customer Class' },
                        { field: 'riskScore', displayName: 'Risk Score' },
                        { field: 'totalAmt', displayName: 'Total A/R Balance' },
                        { field: 'due90Amt', displayName: 'Over 90 days' },
                        { field: 'creditLimit', displayName: 'Credit Limit' },
                        { field: 'accountStatus', displayName: 'Status' },
                        { field: 'accountStatus', displayName: 'Account Status' },
                        { field: 'operator', displayName: 'Operator' },
                        { field: '', displayName: 'Operation' ,
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="Save(selectedUser)"> Save </a><a ng-click="Del(selectedUser)">  Del  </a></div>'
                        }
                        ]
            }
        })

    };

    //清空过滤条件
    $scope.resetSearch = function () {
        filstr = "";
        $scope.custCode = "";
        $scope.custName = "";
        $scope.invNum = "";
        $scope.class = "";
        $scope.status = "";
        $scope.legal = "";
        //数据总数
        testPagingProxy.pagecount(filstr, function (list1) {
            $scope.totalItems = list1.length;
        });
    }


} ])
;