angular.module('app.contactcustomer', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/contactcustomer', {
                templateUrl: 'app/contactcustomer/contactCustomer-list.tpl.html',
                controller: 'contactCustomerCtrl',
                resolve: {
                    type: function () {
                        return 'CC';//contact Customer
                    }
                }
            })
            .when('/breakPTP', {
                templateUrl: 'app/contactcustomer/contactCustomer-list.tpl.html',
                controller: 'contactCustomerCtrl',
                resolve: {
                    type: function () {
                        return 'BPTP';//break PTP
                    }
                }
            })
            .when('/holdCustomer', {
                templateUrl: 'app/contactcustomer/contactCustomer-list.tpl.html',
                controller: 'contactCustomerCtrl',
                resolve: {
                    type: function () {
                        return 'HC';//hold customer
                    }
                }
            });
    }])

    .controller('contactCustomerCtrl',
    ['$scope', 'modalService', 'baseDataProxy', 'siteProxy', 'permissionProxy', 'contactCustomerProxy',
        'type', 'breakPtpProxy', 'holdCustomerProxy', 'unholdCustomerProxy', 'APPSETTING',
        function ($scope, modalService, baseDataProxy, siteProxy, permissionProxy, contactCustomerProxy,
            type, breakPtpProxy, holdCustomerProxy, unholdCustomerProxy, APPSETTING) {


            ////*************************aging filter base binding*****************************s
            ////Customer Class DropDownList binding
            //baseDataProxy.SysTypeDetail("006", function (cusclasslist) {
            //    $scope.cusclass = cusclasslist;
            //})

            ////Legal Entity DropDownList binding
            //siteProxy.Site("", function (legal) {
            //    $scope.legallist = legal;
            //});

            ////paymentstatus binding
            //baseDataProxy.SysTypeDetail("018", function (paystatus) {
            //    $scope.paystatuslist = paystatus;
            //})
            ////contactTypes binding
            //baseDataProxy.SysTypeDetail("009", function (contactTypes) {
            //    $scope.bdContactType = contactTypes;
            //});
            ////invoice status binding
            //baseDataProxy.SysTypeDetail("004", function (istatuslist) {
            //    $scope.istatuslist = istatuslist;
            //});

            ////New Mail Status DropDownList binding
            //baseDataProxy.SysTypeDetail("024", function (mailstatuslist) {
            //    $scope.mailstatus = mailstatuslist;
            //});

            //Customer Status DropDownList binding
            //baseDataProxy.SysTypeDetail("005", function (cusstatuslist) {
            //$scope.cusstatus = bdCustomerStatus;
            //});


            ////Invoice Track States DropDownList binding
            //baseDataProxy.SysTypeDetail("029", function (invoiceTrackStatesList) {
            //    $scope.invoiceTrackStates = invoiceTrackStatesList;
            //});
            //*************************aging filter base binding*****************************e

            //paging init
            $scope.selectedLevel = 15;  //init paging size(ng-model)
            $scope.itemsperpage = 15;   //init paging size(parameter)
            $scope.currentPage = 1;     //init page
            $scope.maxSize = 10; //paging display max
            var filstr = "";
            //paging size
            $scope.levelList = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            //=============================filter function==================================
            //reset Search conditions
            $scope.resetSearch = function () {
                filstr = "";
                $scope.custCode = "";
                $scope.custName = "";
                $scope.class = "";
                $scope.status = "";
                $scope.legal = "";
                //user need search all
                $scope.mstatus = "";
                //added by zhangYu
                $scope.billGroupCode = "";
                $scope.billGroupName = "";
                $scope.states = "";
                $scope.trackStates = "";
                //add by pxc
                $scope.invoiceNum = "";
                $scope.soNum = "";
                $scope.poNum = "";
                $scope.invoiceMemo = "";
            }

            buildFilter = function () {
                //multi-conditions
                if (type == "CC") {
                    var filterStr = "";
                } else {
                    var filterStr = "&$filter=(Operator eq '" + $scope.collector + "')";
                }
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

                if ($scope.status) {
                    if (filterStr == "") {
                        filterStr += "&$filter=(CusStatus eq '" + $scope.status + "')";
                    } else {
                        filterStr += " and (CusStatus eq '" + $scope.status + "')";
                    }
                }

                //bill Grop Code
                if ($scope.billGroupCode) {
                    if (filterStr == "") {
                        filterStr += "&$filter=(contains(BillGroupCode,'" + encodeURIComponent($scope.billGroupCode) + "'))";
                    } else {
                        filterStr += " and (contains(BillGroupCode,'" + encodeURIComponent($scope.billGroupCode) + "'))";
                    }
                }
                //bill Grop Code
                if ($scope.billGroupName) {
                    if (filterStr == "") {
                        filterStr += "&$filter=(contains(BillGroupName,'" + encodeURIComponent($scope.billGroupName) + "'))";
                    } else {
                        filterStr += " and (contains(BillGroupName,'" + encodeURIComponent($scope.billGroupName) + "'))";
                    }
                }

                if (type == "CC") {
                    //mail flag
                    if ($scope.mstatus) {
                        if ($scope.mstatus == "") {
                            if (filterStr == "") {
                                filterStr += "&$filter=((MailFlag eq '0') OR (MailFlag eq '1'))";
                            } else {
                                filterStr += " and ((MailFlag eq '0') OR (MailFlag eq '1'))";
                            }
                        } else {
                            if (filterStr == "") {
                                filterStr += "&$filter=(MailFlag eq '" + $scope.mstatus + "')";
                            } else {
                                filterStr += " and (MailFlag eq '" + $scope.mstatus + "')";
                            }
                        }
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

                    if ($scope.legal) {
                        filterStr = "&legalEntity=" + $scope.legal + filterStr;
                    } else {
                        filterStr = "&legalEntity=" + filterStr;
                    }

                    //invoice Track States
                    if ($scope.trackStates) {
                        filterStr = "&invoiceTrackState=" + $scope.trackStates + filterStr;
                    } else {
                        filterStr = "&invoiceTrackState=" + filterStr;
                    }

                    //invoice states
                    if ($scope.states) {
                        filterStr = "&invoiceState=" + $scope.states + filterStr;
                    } else {
                        filterStr = "&invoiceState=" + filterStr;
                    }

                    //special Notes
                    //if ($scope.states) {
                    //    filterStr = "&comments=" + $scope.specialNotes + filterStr;
                    //} else {
                    //    filterStr = "&comments=" + filterStr;
                    //}
                } else {

                    if ($scope.legal) {
                        //filterStr += " and (LegalEntity eq '" + $scope.legal + "')";
                        filterStr += " and ( contains(LegalEntity, '" + $scope.legal + "'))";
                    }
                    //invoice states
                    if ($scope.states) {
                        filterStr += " and (contains(States,'" + $scope.states + "'))";

                    }
                    //invoice Track States
                    if ($scope.trackStates) {
                        filterStr += " and (contains(TrackStates,'" + $scope.trackStates + "'))";
                    }
                }
                return filterStr;
            };
            //==============================================================================

            $scope.actionType = type;//ng-show Function button use
            $scope.totalNum = "";
            //$scope.totalARBalance = "";
            //$scope.totalPastDueAmt = "";
            //$scope.totalOver90Days = "";
            //$scope.totalCreditLimit = "";
            $scope.isExport = false;
            //user need search all
            $scope.mstatus = "";
            //hold customer
            $scope.type = type;
            $scope.dropdownvalue = "Tohold Accounts";
            $scope.user = "";
            //*************************Contact Customer List*********************************s
            var filterFlg = false;

            $scope.contactCustomerList = {
                multiSelect: false,
                enableFullRowSelection: true,
                columnDefs: [
                    {
                        field: 'legalEntity', displayName: 'Legal Entity'
                    },
                    {
                        field: 'customerNum', displayName: 'Customer NO.'
                    },
                    {
                        field: 'customerName', displayName: 'Customer Name'
                    },
                    {
                        field: 'billGroupCode', displayName: 'Factory Group Code'
                    },
                    {
                        field: 'billGroupName', displayName: 'Factory Group Name'
                    },
                    {
                        field: 'class', displayName: 'Customer Class'
                    },
                    { field: 'risk', displayName: 'Risk Score', type: 'number', cellClass: 'right' },
                    {
                        field: 'totalAmt', displayName: 'Total A/R Balance', cellFilter: 'number:2', type: 'number', cellClass: 'right'
                    },
                    {
                        field: 'pastDueAmt', displayName: 'Past Due Amount', cellFilter: 'number:2', type: 'number', cellClass: 'right'
                    },
                    {
                        field: 'fDueOver90Amt', displayName: 'Over 90 days', cellFilter: 'number:2', type: 'number', cellClass: 'right'
                    },
                    {
                        field: 'creditLimit', displayName: 'Credit Limit', cellFilter: 'number:2', type: 'number', cellClass: 'right'
                    },
                    {
                        field: 'isHoldFlg', displayName: 'Account Status'
                    },
                    {
                        field: 'operator', displayName: 'Collector'
                    }
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;

                    //*************************Contact **************************************s
                    gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                        var strLegalEntity = row.entity.legalEntity;
                        var cusNum = row.entity.customerNum;
                        if (type == "CC")//contactCustomer
                        { window.open('#/contactcustomer/contactmaster/' + cusNum + '/' + strLegalEntity); }
                        else if (type == "BPTP")//break PTP
                        {
                            window.open('#/contactcustomer/breakptp/' + cusNum + '/' + strLegalEntity);
                        }
                        else if (type == "HC")//holdCustomer
                        {
                            window.open('#/contactcustomer/holdcustomer/' + cusNum + '/' + $scope.dropdownvalue + '/' + strLegalEntity);
                        }

                    });
                    //*************************Contact **************************************e
                }
            };

            $scope.floatMenuOwner = ['contactCustomerCtrl'];

            $scope.init = function () {
                if (type == "CC") {
                    $scope.isExport = true;
                    //permissionProxy.getCurrentUser("dummy", function (user) {
                    contactCustomerProxy.contactCustomerPaging($scope.currentPage, $scope.itemsperpage, "&invoiceState=&invoiceTrackState=&legalEntity=&invoiceNum=&soNum=&poNum=&invoiceMemo=&$filter= (MailFlag eq '1')", function (list) {
                        $scope.totalItems = list[0].count; //init count
                        $scope.contactCustomerList.data = list[0].results; //init list
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                        $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
                    }, function (error) {
                        alert(error);
                    });
                    //    $scope.collector = user.eid;
                    //    $scope.mailc = user.email;
                    //});
                }
                else if (type == "BPTP") {
                    $scope.isExport = false;
                    permissionProxy.getCurrentUser("dummy", function (user) {
                        breakPtpProxy.breakPTPPaging($scope.currentPage, $scope.itemsperpage, "&$filter=(Operator eq '" + user.eid + "')", function (list) {
                            $scope.totalItems = list[0].count; //init count
                            $scope.contactCustomerList.data = list[0].results; //init list
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                            $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
                        }, function (error) {
                            alert(error);
                        });
                        $scope.collector = user.eid;
                        $scope.mailc = user.email;
                    });
                }
                else if (type == "HC")//Hold Customer
                {
                    $scope.isExport = false;
                    permissionProxy.getCurrentUser("dummy", function (user) {
                        $scope.user = user;
                        holdCustomerProxy.holdCustomerPaging($scope.currentPage, $scope.itemsperpage, "&$filter=(Operator eq '" + user.eid + "')", function (list) {
                            $scope.totalItems = list[0].count; //init count
                            $scope.contactCustomerList.data = list[0].results; //init list
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                            $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
                        }, function (error) {
                            alert(error);
                        });
                        $scope.collector = user.eid;
                        $scope.mailc = user.email;
                    });


                }
            }

            $scope.init();

            //*************************Contact Customer List*********************************e

            //*************************aging paging******************************************s
            //paging size change
            $scope.pagesizechange = function (selectedLevelId) {

                //multi-conditions
                filstr = buildFilter();

                var index = $scope.currentPage;
                if (type == "CC") {
                    if (filterFlg) {
                        contactCustomerProxy.contactCustomerPaging(index, selectedLevelId, filstr, function (list) {
                            $scope.itemsperpage = selectedLevelId;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        });
                    } else {
                        contactCustomerProxy.contactCustomerPaging(index, selectedLevelId, filstr, function (list) {
                            $scope.itemsperpage = selectedLevelId;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        });
                    }//filerFlg
                }//type
                else if (type == "BPTP") {
                    breakPtpProxy.breakPTPPaging(index, selectedLevelId, filstr, function (list) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    });
                }//type
                else if (type == "HC") {
                    if ($scope.dropdownvalue == "Tohold Accounts") {
                        holdCustomerProxy.holdCustomerPaging(index, selectedLevelId, filstr, function (list) {
                            $scope.itemsperpage = selectedLevelId;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        });
                    }
                    else if ($scope.dropdownvalue == "Onhold Accounts") {
                        unholdCustomerProxy.unholdCustomerPaging(index, selectedLevelId, filstr, function (list) {
                            $scope.itemsperpage = selectedLevelId;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        });
                    }//UnHold end
                }//HC end




            };

            //paging change
            $scope.pageChanged = function () {

                //multi-conditions
                filstr = buildFilter();

                var index = $scope.currentPage;
                if (type == "CC") {
                    if (filterFlg) {
                        contactCustomerProxy.contactCustomerPaging(index, $scope.itemsperpage, filstr, function (list) {
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        contactCustomerProxy.contactCustomerPaging(index, $scope.itemsperpage, filstr, function (list) {
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    }
                }
                else if (type == "BPTP") {
                    breakPtpProxy.breakPTPPaging(index, $scope.itemsperpage, filstr, function (list) {
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
                else if (type == "HC") {
                    if ($scope.dropdownvalue == "Tohold Accounts") {
                        holdCustomerProxy.holdCustomerPaging(index, $scope.itemsperpage, filstr, function (list) {
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalItems = list[0].count;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    }
                    else if ($scope.dropdownvalue == "Onhold Accounts") {
                        unholdCustomerProxy.unholdCustomerPaging(index, $scope.itemsperpage, filstr, function (list) {
                            $scope.totalItems = list[0].count;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    }//UnHold  END
                }//HC END
            };
            //*************************aging paging******************************************e

            //*************************aging search *****************************************s
            //openfilter
            var isShow = 0; //0:hide;1:show
            var baseFlg = true;
            $scope.openFilter = function () {

                //*************************aging filter base binding*****************************s
                if (baseFlg) {
                    baseDataProxy.SysTypeDetails("006,018,009,004,024,029,005", function (res) {
                        angular.forEach(res, function (r) {
                            $scope.cusclass = r["006"];
                            $scope.paystatuslist = r["018"];
                            $scope.bdContactType = r["009"];
                            $scope.istatuslist = r["004"];
                            $scope.mailstatus = r["024"];
                            $scope.invoiceTrackStates = r["029"];
                            $scope.cusstatus = r["005"];
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

            //Do search
            $scope.searchCollection = function () {

                filterFlg = true;

                //multi-conditions
                filstr = buildFilter();
                //current page
                $scope.currentPage = 1;
                if (type == "CC") {
                    $scope.isExport = true;
                    contactCustomerProxy.contactCustomerPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                        $scope.totalItems = list[0].count;
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
                else if (type == "BPTP") {
                    $scope.isExport = false;
                    breakPtpProxy.breakPTPPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                        $scope.totalItems = list[0].count;
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
                else if (type == "HC") {
                    $scope.isExport = false;
                    if ($scope.dropdownvalue == "Tohold Accounts") {
                        holdCustomerProxy.holdCustomerPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                            $scope.totalItems = list[0].count;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    }
                    else if ($scope.dropdownvalue == "Onhold Accounts") {
                        unholdCustomerProxy.unholdCustomerPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                            $scope.totalItems = list[0].count;
                            $scope.contactCustomerList.data = list[0].results;
                            $scope.totalNum = list[0].count;
                            //$scope.totalARBalance = list[0].totalAmount;
                            //$scope.totalPastDueAmt = list[0].totalPastDue;
                            //$scope.totalOver90Days = list[0].totalOver90Days;
                            //$scope.totalCreditLimit = list[0].totalCreditLimit;
                            $scope.calculate(list[0].results.length);
                        }, function (error) {
                            alert(error);
                        });
                    }//UnHold END
                }//HC END
            };
            //*************************aging search *****************************************e

            //footer items calculate
            $scope.calculate = function (count) {
                if (count == 0) {
                    $scope.fromItem = 0;
                } else {
                    $scope.fromItem = ($scope.currentPage - 1) * $scope.itemsperpage + 1;
                }
                $scope.toItem = ($scope.currentPage - 1) * $scope.itemsperpage + count;
            }

            $scope.export = function () {
                window.location = APPSETTING['serverUrl'] + '/api/contactCustomer?exportlist=1';
            }
            //*********************************************Hold/unHold Customer*****************************************s
            $scope.changetab = function (type) {
                $scope.resetSearch();
                if (type == "hold") {
                    $scope.dropdownvalue = "Tohold Accounts";
                    $("#liHold").addClass("active");
                    $("#liunHold").removeClass("active");

                    $scope.isExport = false;
                    holdCustomerProxy.holdCustomerPaging($scope.currentPage, $scope.itemsperpage, "&$filter=(Operator eq '" + $scope.user.eid + "')", function (list) {
                        $scope.totalItems = list[0].count;
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
                else if (type == "unhold") {
                    $scope.dropdownvalue = "Onhold Accounts";
                    $("#liunHold").addClass("active");
                    $("#liHold").removeClass("active");

                    $scope.isExport = false;
                    unholdCustomerProxy.unholdCustomerPaging($scope.currentPage, $scope.itemsperpage, "&$filter=(Operator eq '" + $scope.user.eid + "')", function (list) {
                        $scope.totalItems = list[0].count;
                        $scope.contactCustomerList.data = list[0].results;
                        $scope.totalNum = list[0].count;
                        //$scope.totalARBalance = list[0].totalAmount;
                        //$scope.totalPastDueAmt = list[0].totalPastDue;
                        //$scope.totalOver90Days = list[0].totalOver90Days;
                        //$scope.totalCreditLimit = list[0].totalCreditLimit;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
            }//end changetab
            //*********************************************Hold/unHold Customer*****************************************s
        }])