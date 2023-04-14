angular.module('app.aging', [])
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
        .when('/aging', {
            templateUrl: 'app/aging/aging-list.tpl.html',
            controller: 'agingListCtrl',
            resolve: {
                //首次加载第一页
                //                aging: ['agingProxy', function (agingProxy) {
                //                    return agingProxy.agingPaging(1, 20, "");
                //                } ],
                bdHoldstatus: ['baseDataProxy', function (baseDataProxy) {
                    return baseDataProxy.SysTypeDetail("005");
                } ],
                bdCusclass: ['baseDataProxy', function (baseDataProxy) {
                    return baseDataProxy.SysTypeDetail("006");
                } ]
            }
        });
} ])

.controller('agingListCtrl',
['$scope', 'agingProxy', 'baseDataProxy', 'permissionProxy', 'siteProxy', 'bdHoldstatus', 'bdCusclass',
function ($scope, agingProxy, baseDataProxy, permissionProxy, siteProxy, bdHoldstatus, bdCusclass) {
    var _TeamLead = "003"; //role
    var userId;     //current userId
    var userRole;   //current userRole

    //Customer Class DropDownList数据邦定
    $scope.cusclass = bdCusclass;

    //Legal Entity DropDownList数据绑定
    siteProxy.Site("", function (legallist) {
        $scope.legallist = legallist;
    });

    //Aging Status DropDownList数据绑定
    baseDataProxy.SysTypeDetail("001", function (statuslist) {
        $scope.statuslist = statuslist;
    });

    //Account States数据绑定
    $scope.bdHoldstatus = bdHoldstatus;

    //collector Entity DropDownList
    permissionProxy.getCurrentUser("dummy", function (user) {
        //首次加载第一页
        agingProxy.agingPaging($scope.currentPage, $scope.itemsperpage, "&$filter=(Collector eq '" + user.eid + "')", function (list) {
            $scope.totalItems = list[0].count; //查询结果初始化
            $scope.list = list[0].results; //首次当前页数据
        }, function (error) {
            alert(error);
        });

        permissionProxy.query({ eid: user.eid, dummy: "GetTeamMemeber" }, function (users) {
            $scope.collectorList = users;
        });
        $scope.collector = user.eid;
        userId = user.eid;
        userRole = user.sysUserRole[0].sysRole.roleId;
    });

    //$scope.list = aging[0].results; //首次当前页数据
    $scope.selectedLevel = 20;  //下拉单页容量初始化
    //$scope.totalItems = aging[0].count; //查询结果初始化
    $scope.itemsperpage = 20;
    $scope.currentPage = 1; //当前页
    $scope.maxSize = 10; //分页显示的最大页
    var filstr = "";

    buildFilter = function () {
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

        if ($scope.astatus) {
            if (filterStr != "") {
                filterStr += "and (IsHoldFlg eq '" + $scope.astatus + "')";
            } else {
                filterStr += "&$filter=(IsHoldFlg eq '" + $scope.astatus + "')";
            }
        }

        if ($scope.legal) {
            if (filterStr != "") {
                filterStr += "and (SiteCode eq '" + $scope.legal + "')";
            } else {
                filterStr += "&$filter=(SiteCode eq '" + $scope.legal + "')";
            }
        }

        if ($scope.collector) {
            if (filterStr != "") {
                filterStr += "and (Collector eq '" + $scope.collector + "')";
            } else {
                filterStr += "&$filter=(Collector eq '" + $scope.collector + "')";
            }
        }

        return filterStr;
    };


    //分页容量下拉列表定义
    $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                    ];

    //加载nggrid数据绑定
    $scope.agingList = {
        data: 'list',
        columnDefs: [
                    { field: 'siteCode', displayName: 'Legal Entity', width: '100' },
                    { field: 'customerNum', displayName: 'Customer Code', width: '100' },
                    { field: 'customerName', displayName: 'Customer Name', width: '255' },
                    { field: 'billGroupName', displayName: 'Factory Group Name', width: '255' },
                    { field: 'customerClass', displayName: 'Customer Class', width: '70',
                        cellTemplate: '<div hello="{valueMember: \'customerClass\', basedata: \'grid.appScope.cusclass\'}"></div>'
                    },
                    { field: 'riskScore', displayName: 'Risk Score', width: '80' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance', width: '90' },
                    { name: 'tmp', displayName: 'Past Due Amount', width: '100', cellTemplate: '<div>{{row.entity.totalAmt-row.entity.currentAmt}}</div>'
                    },
                    { field: 'due90Amt', displayName: 'Over 90 days', width: '90' },
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '70' },
                    { field: 'accountStatus', displayName: 'Status', width: '70' },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '70',
                        cellTemplate: '<div hello="{valueMember: \'isHoldFlg\', basedata: \'grid.appScope.bdHoldstatus\'}"></div>'
                    },
                    { field: 'collector', displayName: 'Operator', width: '100' },
                    { name: 'operation', displayName: 'Operation', width: '100',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                        '<input type="button" id="btnProcess" style=" height:20px;font-size:smaller;text-align :center;" class="tn btn-default text-center" ng-click="grid.appScope.openDetail(row.entity)" ng-disabled="grid.appScope.isDisabledByRole(row.entity.collector)" value="Process" /></div>', pinnedRight: true
                    }
                    ]
    };

    $scope.$on('search', function (e) {
        $scope.searchCollection();
    });
    //查询
    $scope.searchCollection = function () {

        //组合过滤条件
        filstr = buildFilter();
        //数据数量和当前页数据
        $scope.currentPage = 1;

        agingProxy.agingPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
            $scope.totalItems = list[0].count;
            $scope.list = list[0].results;
        }, function (error) {
            alert(error);
        });

    };

    //单页容量变化
    $scope.pagesizechange = function (selectedLevelId) {
    
        //组合过滤条件
        filstr = buildFilter();

        var index = $scope.currentPage;
        agingProxy.agingPaging(index, selectedLevelId, filstr, function (list) {
            $scope.itemsperpage = selectedLevelId;
            $scope.list = list[0].results;
            $scope.totalItems = list[0].count;
        });
    };

    //翻页
    $scope.pageChanged = function () {

        //组合过滤条件
        filstr = buildFilter();

        var index = $scope.currentPage;
        agingProxy.agingPaging(index, $scope.itemsperpage, filstr, function (list) {
            $scope.list = list[0].results;
            $scope.totalItems = list[0].count;
        }, function (error) {
            alert(error);
        });
    };

    $scope.$on('resetSearch', function (e) {
        $scope.resetSearch();
    });
    //清空过滤条件
    $scope.resetSearch = function () {
        filstr = "";
        $scope.custCode = "";
        $scope.custName = "";
        $scope.invNum = "";
        $scope.class = "";
        $scope.status = "";
        $scope.astatus = "";
        $scope.legal = "";
        $scope.collector = "";
    }

    //打开Detail
    $scope.openDetail = function (row) {
        window.location.href = '#/aging/' + row.id + '/agingdetail';
    }

    //added by zhangyu process button disabled
    $scope.isDisabledByRole = function (eid) {
        if (userRole == _TeamLead) {
            return false;
        }
        else if (userId == eid) {
            return false;
        }
        else {
            return true;
        }
    };

} ]);