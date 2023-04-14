angular.module('app.allinfo', [])
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
        .when('/allinfo', {
            templateUrl: 'app/allcontactinfo/allinfo.tpl.html',
            controller: 'allinfoCtrl',
            resolve: {
            }
        });
}])

    .controller('allinfoCtrl', ['$scope', 'allinfoProxy', '$interval', 'baseDataProxy', 'siteProxy', 'APPSETTING', 'permissionProxy','modalService',
        function ($scope, allinfoProxy, $interval, baseDataProxy, siteProxy, APPSETTING, permissionProxy, modalService) {
        
        $scope.$parent.helloAngular = "OTC - All Accounts";
        $scope.floatMenuOwner = ['allinfoCtrl'];
        $scope.ptpOverDue = false;
        $scope.allinfoList = {
            data: 'allinfos',
            enableFiltering: true,
            columnDefs: [
                { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    {
                    name: 'legalEntity', displayName: 'Legal Entity', width: '110'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openAllInvoice(row.entity.customerNum,row.entity.legalEntity,row.entity.siteUseId)">{{row.entity.legalEntity}}</a></div>'
                    ,pinnedLeft: true
                    },
                    {
                        field: 'customerNum', displayName: 'Customer NO.', pinnedLeft: true, width: '120',
                        cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openAllInvoice(row.entity.customerNum,row.entity.legalEntity,row.entity.siteUseId)">{{row.entity.customerNum}}</a></div>'
                    },
                    { field: 'customerName', displayName: 'Customer Name', pinnedLeft: true, width: '140' },
                    {
                        field: 'siteUseId', displayName: 'Site Use Id', width: '110', pinnedLeft: true,
                        cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openAllInvoice(row.entity.customerNum,row.entity.legalEntity,row.entity.siteUseId)">{{row.entity.siteUseId}}</a></div>'
                    },
                    { field: 'collectoR_CONTACT', displayName: 'Contact', width: '100' },
                    { field: 'class', displayName: 'Customer Class', width: '120' },
                { field: 'arBalanceAmtPeroid', displayName: 'AR balance Period', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                { field: 'pastDueAmount', displayName: 'Over Due Amount', width: '150', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'collector', displayName: 'Collector', width: '140' },
                    
                    
                    { field: 'sales', displayName: 'Sales', width: '140' },
                    
                    { field: 'cs', displayName: 'CS', width: '120' },
                { field: 'creditLimit', displayName: 'Credit Limit', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'creditTremDescription', displayName: 'Payment Term Desc', width: '160' },
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    {
                        field: 'accountPtpAmount', displayName: 'Account PTP Amount', width: '160',
                        cellTemplate: '<div style="height:30px;vertical-align:middle;"><a style="line-height:28px" ng-click="grid.appScope.openAccountPtp(row.entity.customerNum,row.entity.siteUseId)">{{row.entity.accountPtpAmount | number:2}}</a></div>',
                        cellClass: 'right'
                    },
                { field: 'comment', displayName: 'Comments', width: '120' },
                
            ],

            onRegisterApi: function (gridApi) {
                //set gridApi on scope
            $scope.gridApi = gridApi;

        }
        }

        var filstr = "";

        $scope.maxSize = 10; //paging display max
        $scope.slexecute = 15;  //init paging size(ng-model)
        $scope.iperexecute = 15;   //init paging size(parameter)
        $scope.curpexecute = 1;     //init page
        $scope.totalNum = "";
        $scope.levelList = [
                    { "id": 15, "levelName": '15' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' },
                    { "id": 999999, "levelName": 'ALL' }
        ];
        allinfoProxy.allinfoPaging($scope.curpexecute, $scope.iperexecute, "&isPTPOverDue=" + $scope.ptpOverDue + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=" + filstr, function (list)
        {
            //$scope.totalItems = list[0].count;
            $scope.allinfos = list[0].results;
            $scope.ttexecute = list[0].count;
            $scope.totalNum = list[0].count;
            if (list[0].results == "undefined" || list[0].results == undefined || list[0].results == null) {
                //float menu 加载 
                $scope.calculate($scope.curpexecute, $scope.iperexecute, 0);
                $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
            }
            else
            {
                //float menu 加载 
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
                $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
            }
        }, function (error) { alert(error); });

        $scope.menuToggle = function () {
            $("#wrapper").toggleClass("toggled");
        }

        //paging size change
        $scope.pschangeexecute = function (slexecute) {
            var index = 1;
            $scope.curpexecute = 1;     //init page
            filstr = buildFilter();
            var apiparams = "&isPTPOverDue=" + $scope.ptpOverDue + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=";
            filstr = apiparams + filstr;
            allinfoProxy.allinfoPaging(index, slexecute, filstr, function (list) {
                $scope.iperexecute = slexecute;
                $scope.allinfos = list[0].results;
                $scope.ttexecute = list[0].count;
                $scope.totalNum = list[0].count;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            });
        };

        //paging change
        $scope.executepChanged = function () {
            var index = $scope.curpexecute;
            filstr = buildFilter();
            var apiparams = "&isPTPOverDue=" + $scope.ptpOverDue + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=";
            filstr = apiparams + filstr;
            allinfoProxy.allinfoPaging(index, $scope.iperexecute, filstr, function (list) {
                $scope.allinfos = list[0].results;
                $scope.ttexecute = list[0].count;
                $scope.totalNum = list[0].count;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        };

        $scope.calculate = function (currentPage, itemsperpage, count) {
            if (count == 0) {
                $scope.fromItem = 0;
            } else {
                $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
            }
            $scope.toItem = (currentPage - 1) * itemsperpage + count;
        }

        //*************************Send Soa *****************************************s
        $scope.openAllList = function () {

            var strids = []; //= "";
            var strSites = [];
            //var ccount = 0;
            strids.push("10000" + ";" + "A-company");
            //if ($scope.gridApi.selection.getSelectedRows()) {
            //    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
            //        strids.push(rowItem.customerNum + ";" + rowItem.legalEntity);
                    
            //    });
            //}
            if (strids == "") {
                alert("Please choose 1 customer at least .")
            } else {
                window.open('#/allcontactinfo/allaccount/' + strids + '/');
            }

        }

        $scope.init = function () {
            $scope.executepChanged();
        }

        //*********************open account ptp***************************************
        $scope.accountPtp = function () {

            var selCnt = 0;
            var custNo1 = "";
            var siteUseId1 = "";
            var legalentty1 = "";
            var pastDueAmt1 = 0;
            var invId1 = [];

            if ($scope.gridApi.selection.getSelectedRows()) {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    selCnt++;

                    custNo1 = rowItem.customerNum;
                    siteUseId1 = rowItem.siteUseId;
                    legalentty1 = rowItem.legalEntity;
                    if (rowItem.pastDueAmount != null){
                        pastDueAmt1 = rowItem.pastDueAmount;
                    }
                });
            }
            if (selCnt == 0 || selCnt > 1) {
                alert("Please choose 1 customer only.")
            } else {
                var modalDefaults = {
                    templateUrl: 'app/common/contactdetail/contact-ptp.tpl.html',
                    controller: 'contactPtpCtrl',
                    size: 'lg',
                    resolve: {
                        custnum: function () {
                            return custNo1;
                        },
                        invoiceIds: function () {
                            return invId1;
                        },
                        siteUseId: function () {
                            return siteUseId1;
                        },
                        customerNo: function () {
                            return custNo1;
                        },
                        legalEntity: function () {
                            return legalentty1;
                        },
                        contactId: function () {
                            return "";
                        },
                        relatedEmail: function () {
                            return "";
                        },
                        contactPerson: function () {
                            return "";
                        },
                        proamount: function () {
                            return pastDueAmt1.toFixed(2);
                        }
                    },
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    if (result[0] == "submit") {
                        $scope.init();
                    }
                });
            }

        }

        //*********************open customer aging comment***************************************
        $scope.accountComment = function () {

            var selCnt = 0;
            var custNo1 = "";
            var siteUseId1 = "";
            var legalentty1 = "";
            var comment = "";

            if ($scope.gridApi.selection.getSelectedRows()) {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    selCnt++;
                    comment = rowItem.comment;
                    custNo1 = rowItem.customerNum;
                    siteUseId1 = rowItem.siteUseId;
                    legalentty1 = rowItem.legalEntity;
                });
            }
            if (selCnt == 0 || selCnt > 1) {
                alert("Please choose 1 customer only.")
            } else {
                var modalDefaults = {
                    templateUrl: 'app/common/contactdetail/contact-comment.tpl.html',
                    controller: 'contactCommentCtrl',
                    size: 'mid',
                    resolve: {
                        comment: function () {
                            return comment;
                        },
                        siteUseId: function () {
                            return siteUseId1;
                        },
                        customerNo: function () {
                            return custNo1;
                        },
                        legalEntity: function () {
                            return legalentty1;
                        }
                    },
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    if (result[0] == "submit") {
                        $scope.init();
                    }
                });
            }

        }

        $scope.openAccountPtp = function (custNum,siteuseid) {
            var modalDefaults = {
                templateUrl: 'app/allcontactinfo/ptpInfo/ptpInfo.tpl.html',
                controller: 'ptpInfoCL',
                size: 'lg',
                resolve: {
                    custNum: function () { return custNum },
                    siteUseId: function () { return siteuseid; }
                }
                , windowClass: 'modalDialog'
            };

            modalService.showModal(modalDefaults, {}).then(function (result) {
            });
        }

        //*********************open all invoice***************************************
        $scope.openAllInvoice = function (custNO, legal, siteuseid) {

            var strids = []; //= "";
            var strSites = [];
            //var ccount = 0;
            strids.push("custNo=" + custNO + "&legal=" + legal + "&siteUseID=" + siteuseid);
            //if ($scope.gridApi.selection.getSelectedRows()) {
            //    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
            //        strids.push(rowItem.customerNum + ";" + rowItem.legalEntity);

            //    });
            //}
            if (strids == "") {
                alert("There are some issue on Data, please try to contact with Admin.")
            } else {
                window.open('#/myinvoices?' + strids);
            }

        }

        //*************************Send Soa *****************************************s
        $scope.openSOA = function (custNO,legal,siteuseid) {

            var strids = []; //= "";
            var strSites = [];
            //var ccount = 0;
            strids.push(custNO + ";" + legal + ";" + siteuseid);
            //if ($scope.gridApi.selection.getSelectedRows()) {
            //    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
            //        strids.push(rowItem.customerNum + ";" + rowItem.legalEntity);

            //    });
            //}
            if (strids == "") {
                alert("There are some issue on Data, please try to contact with Admin.")
            } else {
                window.open('#/allcontactinfo/allaccount/' + strids + '/');
            }

        }

        //*************************aging search *****************************************s
        //openfilter
        var isShow = 0; //0:hide;1:show
        var baseFlg = true;
        $scope.showCollector = false;
        $scope.openFilter = function () {

            //*************************aging filter base binding*****************************s
            if (baseFlg) {
                baseDataProxy.SysTypeDetails("006,004,029,005", function (res) {
                    angular.forEach(res, function (r) {
                        $scope.cusclass = r["006"];
                        $scope.istatuslist = r["004"];
                        $scope.invoiceTrackStates = r["029"];
                        $scope.cusstatus = r["005"];
                    });
                });

                //Legal Entity DropDownList binding
                siteProxy.Site("", function (legal) {
                    $scope.legallist = legal;
                });

                permissionProxy.getCurrentUser("dummy", function (user) {
                    if (user.actionPermissions.indexOf('alldataforsupervisor') >= 0) {
                        $scope.showCollector = true;
                        //Collector List
                        permissionProxy.query({ 'getCollectList': 'dummy' }, function (res) {
                            $scope.collectorList = res;
                        })
                    }
                })
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
            $scope.siteUseId = "";
            //$scope.billGroupCode = "";
            //$scope.billGroupName = "";
            //$scope.states = "";
            //$scope.trackStates = "";
            //$scope.invoiceNum = "";
            //$scope.soNum = "";
            //$scope.poNum = "";
            //$scope.invoiceMemo = "";
        }

            $scope.changetab = function (type) {

                if (type == "vatimport") {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/myinvoices/vatimport.tpl.html',
                        controller: 'vatimportInstanceCtrl',
                        size: 'lg',
                        resolve: {
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                }

            }

        $scope.searchCollection = function () {
            filstr = buildFilter();

            //string invoiceState, string invoiceTrackState, string invoiceNum, string soNum, string poNum, string invoiceMemo
            //var apiparams = "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum&=&invoiceMemo=";
            var apiparams = "&isPTPOverDue=" + $scope.ptpOverDue + "&invoiceState=&invoiceTrackState=&invoiceNum=&soNum=&poNum=&invoiceMemo=";
            filstr = apiparams + filstr;
            var index = 1;
            $scope.curpexecute = 1;     //init page
            allinfoProxy.allinfoPaging(index, $scope.iperexecute, filstr, function (list) {
                $scope.allinfos = list[0].results;
                $scope.ttexecute = list[0].count;
                $scope.totalNum = list[0].count;
                //$scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            });

        }
        $scope.export = function () {
            //window.location = APPSETTING['serverUrl'] + '/api/contactCustomer?exportlist=1';
            window.location = APPSETTING['serverUrl'] + '/api/allinfo?' +
                'cCode=' + $scope.custCode + '&cName=' + $scope.custName + '&level=' + $scope.class + '&bCode=' + $scope.billGroupCode + '&bName=' + $scope.billGroupName + '&legal=' + $scope.legal +
                '&state=' + $scope.states + '&tstate=' + $scope.trackStates + '&iNum=' + $scope.invoiceNum + '&pNum=' + $scope.poNum + '&sNum=' + $scope.soNum + '&memo=' + $scope.memo + '&oper=' + $scope.collector;
                
        }
        buildFilter = function () {
            var filterStr = "";
            if ($scope.custCode) {
                if (filterStr == "") {
                    filterStr += "&$filter=(contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "'))";
                } else {
                    filterStr += " and (contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "'))";
                }
            }

            if ($scope.custName) {
                if (filterStr == "") {
                    filterStr += "&$filter=(contains(CustomerName,'" + encodeURIComponent($scope.custName) + "'))"
                } else {
                    filterStr += " and (contains(CustomerName,'" + encodeURIComponent($scope.custName) + "'))"
                }
            }

            if ($scope.class) {
                if (filterStr == "") {
                    filterStr += "&$filter=(Class eq '" + $scope.class + "')";
                } else {
                    filterStr += " and (Class eq '" + $scope.class + "')";
                }
            }

            if ($scope.legal) {
                if (filterStr == "") {
                    filterStr += "&$filter=(LegalEntity eq '" + $scope.legal + "')"
                } else {
                    filterStr += " and (LegalEntity eq '" + $scope.legal + "')";
                }
            }

            if ($scope.siteUseId) {
                
                    if (filterStr == "") {
                        //filterStr = "siteUseId=" + $scope.siteUseId + filterStr;
                        filterStr += " &$filter=(contains(SiteUseId,'" + encodeURIComponent($scope.siteUseId) + "'))"
                    }
                 else {
                        //filterStr += filterStr + "&siteUseId=";
                        filterStr += " and (contains(SiteUseId,'" + encodeURIComponent($scope.siteUseId) + "'))"

                }
            }
                
            return filterStr;
        };

    }]);