angular.module('app.dunning', [])
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
        .when('/dunning', {
            templateUrl: 'app/dunning/dunning-list.tpl.html',
            controller: 'dunningListCtrl',
            resolve: {
            }
        });
}])

.controller('dunningListCtrl',
['$scope', 'collectorSoaProxy', 'baseDataProxy', 'permissionProxy', 'siteProxy', 'invoiceProxy', 'modalService', 'dunningProxy',
'$interval', 'customerProxy', 'contactProxy', 'customerPaymentbankProxy', 'customerPaymentcircleProxy', 'uiGridConstants',
function ($scope, collectorSoaProxy, baseDataProxy, permissionProxy, siteProxy, invoiceProxy, modalService, dunningProxy,
$interval, customerProxy, contactProxy, customerPaymentbankProxy, customerPaymentcircleProxy, uiGridConstants) {
    //*******************Ready parameters**************
    //float menu 加载 
    $scope.floatMenuOwner = ['dunningListCtrl'];

    $scope.lblfilter = true;
    $scope.divexecute = true;
    $scope.divdone = false;
    $scope.divcomplete = false;
    $scope.searchtype = "execute";
    var order = "&$orderby= LastRemind asc, Class asc,Risk desc,BillGroupName asc";
    var order1 = "&$orderby= ProcessId asc, Class asc,Risk desc,BillGroupName asc";
    var filstr = "";
    $scope.totalNum = "";
    //$scope.totalARBalance = "";
    //$scope.totalPastDueAmt = "";
    //$scope.totalOver90Days = "";
    //$scope.totalCreditLimit = "";
    $scope.dropdownvalue = "Not Start";

    //alerttype DropDownList binding
    $scope.typeList = [
                { "id": 2, "levelName": '2nd Reminder' },
                { "id": 3, "levelName": 'Final Reminder' }
    ];
    //########### paging init##############
    $scope.maxSize = 10; //paging display max
    //execute
    $scope.slexecute = 15;  //init paging size(ng-model)
    $scope.iperexecute = 15;   //init paging size(parameter)
    $scope.curpexecute = 1;     //init page
    //done
    $scope.sldone = 15;  //
    $scope.iperdone = 15;   //
    $scope.curpdone = 1;     //
    //paging size
    $scope.levelList = [
                    { "id": 15, "levelName": '15' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' },
                    { "id": 999999, "levelName": 'ALL' }
    ];


    //********************* list Grid Binding**********************
    $scope.executeList = {
        noUnselect: true,
        columnDefs: [
                    {
                        field: 'lastRemind', displayName: 'SOA Task #', width: '70'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.CreateOrViewDun(row.entity,\'create\')">{{row.entity.lastRemind}}</a></div>'
                    },
                    {
                        field: 'customerNum', displayName: 'Customer NO.', width: '70'
                        //, cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.CreateOrViewDun(row.entity,\'create\')">{{row.entity.customerNum}}</a></div>'
                    },
                    {
                        field: 'alertType', displayName: 'Remind Type', width: '70',
                        cellTemplate: '<div style="height:30px;vertical-align:middle"><label style="line-height:28px">{{grid.appScope.CheckType(row.entity)}}</label></div>'
                    },
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '70' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    { field: 'billGroupCode', displayName: 'Factory Group Code', width: '160' },
                    { field: 'billGroupName', displayName: 'Factory Group Name', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    { field: 'class', displayName: 'Customer Class', width: '70' },
                    { field: 'risk', displayName: 'Risk Score', width: '70', type: 'number', cellClass: 'right' },
            { field: 'totalAmt', displayName: 'Total A/R Balance', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
            { field: 'pastDueAmt', displayName: 'Past Due Amount', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
            { field: 'fDueOver90Amt', displayName: 'Over 90 days', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
            { field: 'creditLimit', displayName: 'Credit Limit', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '80' },
                    { field: 'operator', displayName: 'Collector', width: '90' }
                    //,{
                    //    field: 'contactExist', displayName: 'Check', width: '80',
                    //    cellTemplate: '<div><label ng-show="row.entity.existCont == \'N\'">no contact</label></div>'
                    //}
        ]
    };

    $scope.doneList = {
        columnDefs: [
                    {
                        name: 'pid', displayName: 'Task #', width: '70'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.CreateOrViewDun(row.entity,\'view\')">{{row.entity.processId}}</a></div>'
                    },
                    {
                        field: 'alertType', displayName: 'Remind Type', width: '70',
                        cellTemplate: '<div style="height:30px;vertical-align:middle">{{grid.appScope.CheckType(row.entity)}}</div>'
                        //, filter: {
                        //    term: '',
                        //    type: uiGridConstants.filter.SELECT,
                        //    selectOptions: [{ value: 2, label: '2nd' }, { value: 3, label: 'Final' }]
                        //}, cellFilter: 'mapType'
                    },
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '70' },
                    { field: 'customerNum', displayName: 'Customer NO.', width: '70' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    { field: 'billGroupCode', displayName: 'Factory Group Code', width: '160' },
                    { field: 'billGroupName', displayName: 'Factory Group Name', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    { field: 'class', displayName: 'Customer Class', width: '70' },
                    { field: 'risk', displayName: 'Risk Score', width: '70', type: 'number', cellClass: 'right' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'pastDueAmt', displayName: 'Past Due Amount', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'fDueOver90Amt', displayName: 'Over 90 days', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '80' },
                    { field: 'operator', displayName: 'Collector', width: '90' }
        ]

    };

    $scope.completeList = {
        showGridFooter: true,
        showColumnFooter: true,
        enableFiltering: true,
        //enableFooterTotalSelected: false,
        columnDefs: [
                    {
                        field: 'processId', displayName: 'Task #', width: '70'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.CreateOrViewDun(row.entity,\'complete\')">{{row.entity.processId}}</a></div>'
                    },
                    {
                        field: 'alertType', displayName: 'Remind Type', width: '70',
                        cellTemplate: '<div style="height:30px;vertical-align:middle">{{grid.appScope.CheckType(row.entity)}}</div>'
                        , filter: {
                            term: '',
                            type: uiGridConstants.filter.SELECT,
                            selectOptions: [{ value: 2, label: '2nd' }, { value: 3, label: 'Final' }]
                        }, cellFilter: 'mapType'
                    },
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '70' },
                    { field: 'customerNum', displayName: 'Customer NO.', width: '70' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    { field: 'billGroupCode', displayName: 'Factory Group Code', width: '160' },
                    { field: 'billGroupName', displayName: 'Factory Group Name', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    { field: 'class', displayName: 'Customer Class', width: '70' },
                    { field: 'risk', displayName: 'Risk Score', width: '70', type: 'number', cellClass: 'right' },
                    {
                        field: 'totalAmt', displayName: 'Total A/R Balance', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'pastDueAmt', displayName: 'Past Due Amount', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'fDueOver90Amt', displayName: 'Over 90 days', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'creditLimit', displayName: 'Credit Limit', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '80' },
                    { field: 'operator', displayName: 'Collector', width: '90' }
        ]

    };

    $scope.init = function () {

        dunningProxy.dunningPaging(1, 15, order + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=&$filter=(LastRemind ne null) and (TaskId eq '')", function (list) {
            $scope.ttexecute = list[0].count; //init count
            $scope.executeList.data = list[0].results; //init list

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            //float menu 加载 
            $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
        }, function (error) {
            alert(error);
        });

    }

    $scope.init();

    //############################# execute ######################
    //paging size change
    $scope.pschangeexecute = function (selectedLevelId) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpexecute;
        dunningProxy.dunningPaging(index, selectedLevelId, filstr, function (list) {
            $scope.iperexecute = selectedLevelId;
            $scope.executeList.data = list[0].results;
            $scope.ttexecute = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
        });
    };

    //paging change
    $scope.executepChanged = function () {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpexecute;
        dunningProxy.dunningPaging(index, $scope.iperexecute, filstr, function (list) {
            $scope.executeList.data = list[0].results;
            $scope.ttexecute = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
        }, function (error) {
            alert(error);
        });
    };

    //############################# done ######################
    //paging size change
    $scope.pschangedone = function (selectedLevelId) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpdone;
        dunningProxy.dunningPaging(index, selectedLevelId, filstr, function (list) {
            $scope.iperdone = selectedLevelId;
            $scope.doneList.data = list[0].results;
            $scope.ttdone = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
        });
    };

    //paging change
    $scope.donepChanged = function (curpdone) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpdone;
        dunningProxy.dunningPaging(curpdone, $scope.iperdone, filstr, function (list) {
            $scope.doneList.data = list[0].results;
            $scope.ttdone = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
        }, function (error) {
            alert(error);
        });
    };

    //openfilter
    var isShow = 0; //0:hide;1:show
    var baseFlg = true;
    $scope.openFilter = function () {

        //*************************aging filter base binding*****************************s
        if (baseFlg) {
            baseDataProxy.SysTypeDetails("004,005,006,029", function (res) {
                angular.forEach(res, function (r) {
                    $scope.istatuslist = r["004"];
                    $scope.cusstatus = r["005"];
                    $scope.cusclass = r["006"];
                    $scope.invoiceTrackStates = r["029"];
                });
            });

            //Legal Entity DropDownList binding
            siteProxy.Site("", function (legal) {
                $scope.legallist = legal;
            });
        }
        //*************************aging filter base binding*****************************e

        if (isShow == 0) {
            $("#divAgingSearch").show();
            isShow = 1;
            baseFlg = false;
        } else if (isShow == 1) {
            $("#divAgingSearch").hide();
            isShow = 0;
            baseFlg = false;
        }
    }

    //reset Search conditions
    $scope.resetSearch = function () {
        filstr = "";
        $scope.custCode = "";
        $scope.custName = "";
        $scope.class = "";
        $scope.status = "";
        $scope.legal = "";
        $scope.type = "";

        $scope.billGroupCode = "";
        $scope.billGroupName = "";
        $scope.states = "";
        $scope.trackStates = "";

        $scope.invoiceNum = "";
        $scope.soNum = "";
        $scope.poNum = "";
        $scope.invoiceMemo = "";
    }

    buildFilter = function () {
        //multi-conditions
        var filterStr = "";
        if ($scope.searchtype == "execute") {
            filterStr = order + "&$filter=(LastRemind ne null) and (TaskId eq '')";
            //} else if ($scope.searchtype == "batch") {
            //    filterStr = order2 + "&$filter=(BatchType eq 1)";
        } else if ($scope.searchtype == "done") {
            filterStr = order1 + "&$filter=(TaskId ne '')";
        }
        if ($scope.custCode) {
            filterStr += " and (contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "'))";
        }

        if ($scope.custName) {
            filterStr += " and (contains(CustomerName,'" + encodeURIComponent($scope.custName) + "'))"
        }

        if ($scope.class) {
            filterStr += " and (Class eq '" + $scope.class + "')";
        }

        if ($scope.status) {
            filterStr += " and (IsHoldFlg eq '" + $scope.status + "')";
        }

        if ($scope.legal) {
            filterStr += " and (LegalEntity eq '" + $scope.legal + "')";
        }

        if ($scope.type) {
            filterStr += " and (AlertType eq " + $scope.type + ")";
        }

        //bill Grop Code
        if ($scope.billGroupCode) {
            filterStr += " and (contains(BillGroupCode,'" + encodeURIComponent($scope.billGroupCode) + "'))";
        }
        //bill Grop Code
        if ($scope.billGroupName) {
            filterStr += " and (contains(BillGroupName,'" + encodeURIComponent($scope.billGroupName) + "'))";
        }

        //invoice Memo
        if ($scope.invoiceMemo) {
            filterStr = "&invoiceMemo=" + $scope.invoiceMemo + filterStr;
        } else {
            filterStr = "&invoiceMemo=" + filterStr;
        }

        //invoice PO
        if ($scope.poNum) {
            filterStr = "&poNum=" + $scope.poNum + filterStr;
        } else {
            filterStr = "&poNum=" + filterStr;
        }

        //invoice SO
        if ($scope.soNum) {
            filterStr = "&soNum=" + $scope.soNum + filterStr;
        } else {
            filterStr = "&soNum=" + filterStr;
        }

        //invoice Num
        if ($scope.invoiceNum) {
            filterStr = "&invoiceNum=" + $scope.invoiceNum + filterStr;
        } else {
            filterStr = "&invoiceNum=" + filterStr;
        }

        //invoice states
        if ($scope.states) {
            filterStr = "&invoiceState=" + $scope.states + filterStr;
        } else {
            filterStr = "&invoiceState=" + filterStr;
        }
        //invoice Track States
        if ($scope.trackStates) {
            filterStr = "&invoiceTrackState=" + $scope.trackStates + filterStr;
        } else {
            filterStr = "&invoiceTrackState=" + filterStr;
        }

        return filterStr;
    };

    //Do search
    $scope.searchCollection = function () {

        //multi-conditions
        filstr = buildFilter();
        if ($scope.searchtype == "execute") {
            //current page
            $scope.curpexecute = 1;
            dunningProxy.dunningPaging($scope.curpexecute, $scope.iperexecute, filstr, function (list) {
                $scope.ttexecute = list[0].count;
                $scope.executeList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        } else if ($scope.searchtype == "done") {
            //current page
            $scope.curpdone = 1;
            dunningProxy.dunningPaging($scope.curpdone, $scope.iperdone, filstr, function (list) {
                $scope.ttdone = list[0].count;
                $scope.doneList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        }

    };

    //*************************Change Tab *****************************************s
    $scope.changetab = function (type) {
        if (type == "execute") {
            $scope.dropdownvalue = "Not Start";
            $("#liexecute").addClass("active");
            $("#lidone").removeClass("active");
            $("#licomplete").removeClass("active");
            $("#divFinishSearch").hide();
            $scope.lblfilter = true;
            $scope.divexecute = true;
            $scope.divdone = false;
            $scope.divcomplete = false;
            $scope.searchtype = "execute";
            $scope.slexecute = 15;  //init paging size(ng-model)
            $scope.iperexecute = 15;   //init paging size(parameter)
            $scope.curpexecute = 1;
            dunningProxy.dunningPaging($scope.curpexecute, 15, order + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=&$filter=(LastRemind ne null) and (TaskId eq '')", function (list) {
                $scope.ttexecute = list[0].count;
                $scope.executeList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        } else if (type == "done") {
            $scope.dropdownvalue = "Work in Progress";
            $("#liexecute").removeClass("active");
            $("#lidone").addClass("active");
            $("#licomplete").removeClass("active");
            $("#divFinishSearch").hide();
            $scope.lblfilter = true;
            $scope.divexecute = false;
            $scope.divdone = true;
            $scope.divcomplete = false;
            $scope.searchtype = "done";
            $scope.curpdone = 1;
            $scope.sldone = 15;  //
            $scope.iperdone = 15;   //
            dunningProxy.dunningPaging($scope.curpdone, 15, order1 + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=&$filter=(TaskId ne '')", function (list) {
                $scope.ttdone = list[0].count;
                $scope.doneList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        } else if (type == "complete") {
            $scope.dropdownvalue = "Work Completed";
            $("#liexecute").removeClass("active");
            $("#lidone").removeClass("active");
            $("#licomplete").addClass("active");
            $("#divFinishSearch").show();
            $scope.lblfilter = false;
            $scope.divexecute = false;
            $scope.divdone = false;
            $scope.divcomplete = true;
            $("#divAgingSearch").hide();
            isShow = 0;
            $scope.searchtype = "complete";

            collectorSoaProxy.query({ Period: 'Period' }, function (list) {
                $scope.periodlist = list;
                $scope.period = $scope.periodlist[0].id;
            });

            dunningProxy.getNoPaging("finish", function (list) {
                $scope.completeList.data = list;
            }, function (error) {
                alert(error);
            });

        }


    }

    //*************************Change Tab *****************************************e

    $scope.CreateOrViewDun = function (obj, type) {
        dunningProxy.checkPermission(obj.referenceNo, function (re) {
            if (re == 1) {
                alert("Sorry, one or more of the chosen customers could not be executed by current collector .Please refresh the page .")
            } else {
                window.open('#/dunning/sendDun/' + obj.customerNum + '/' + type + '/' + obj.alertType + '/' + obj.id + '/' + obj.referenceNo);
            }
        }, function (error) {
            alert(error);
        });
    }


    $scope.periodchange = function () {
        dunningProxy.query({ PeriodId: $scope.period }, function (list) {
            $scope.completeList.data = list;
        }, function (error) {
            alert(error);
        });
    }

    $scope.CheckType = function (obj) {
        if (obj.alertType == 2) {
            return "2nd";
        } else {
            return "Final";
        }

    }

    //footer items calculate
    $scope.calculate = function (currentPage, itemsperpage, count) {
        if (count == 0) {
            $scope.fromItem = 0;
        } else {
            $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
        }
        $scope.toItem = (currentPage - 1) * itemsperpage + count;
    }

}])
.filter('mapType', function () {
    var typeHash = {
        2: '2nd',
        3: 'Final'
    };
    return function (input) {
        if (!input) {
            return '';
        } else {
            return typeHash[input];
        }
    };
});