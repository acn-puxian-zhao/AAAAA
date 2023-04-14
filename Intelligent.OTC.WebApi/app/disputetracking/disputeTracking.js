angular.module('app.disputetracking', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/disputetracking', {
                templateUrl: 'app/disputetracking/disputeTracking-list.tpl.html',
                controller: 'disputeTrackingCtrl',
                resolve: {
                }
            });
    }])

    .controller('disputeTrackingCtrl',
    ['$scope', 'modalService', 'baseDataProxy', 'disputeTrackingProxy','ebProxy',
        function ($scope, modalService, baseDataProxy, disputeTrackingProxy, ebProxy) {
            $scope.$parent.helloAngular = "OTC - Dispute";
            var filterFlg = false;
            $scope.disputeType = "0";
            $scope.totalNum = "";
            $scope.floatMenuOwner = ['disputeTrackingCtrl'];

            $scope.invoiceNum = "";

            $scope.disputeTrackingList = {
                multiSelect: false,
                enableFullRowSelection: false,
              //  data:'members',
                columnDefs: [
                    //{ name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    {
                        field: 'id', displayName: 'ID',
                        cellTemplate: '<div style="height:30px; line-height:30px; text-align:center;"><a href="#/disputetracking/dispute/{{row.entity.id}}">{{row.entity.id}}</a></div>'
                    },
                    { field: 'legalEntity', displayName: 'Legal Entity' ,width:'110'},
                    {
                        field: 'customerNum', displayName: 'Customer NO.', width: '120',
                        cellTemplate: '<div style="height:30px; line-height:30px; text-align:center;"><a href="#/disputetracking/dispute/{{row.entity.id}}">{{row.entity.customerNum}}</a></div>'
                    },
                    { field: 'customerName', displayName: 'Customer Name', width: '140'},
                    {
                        field: 'siteUseid', displayName: 'Site Use Id', width: '100',
                        cellTemplate: '<div style="height:30px; line-height:30px; text-align:center;"><a href="#/disputetracking/dispute/{{row.entity.id}}">{{row.entity.siteUseid}}</a></div>'
                    },
                    { field: 'issueReason', displayName: 'Reason', width: '100' },
                    { field: 'disputeStatus', displayName: 'Status', width: '100' },
                    { field: 'statuS_DATE', displayName: 'StatusDate', cellFilter: 'date:\'yyyy-MM-dd\'', width: '105' },
                    { field: 'contact', displayName: 'Contact', width: '100' },
                    { field: 'classLevel', displayName: 'Customer Class', width: '130'},
                    { field: 'totalAmt', displayName: 'Total A/R Balance', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'dueoverTotalAmt', displayName: 'Over Due Amount', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'collector', displayName: 'Collector', width: '100'},
                    { field: 'sales', displayName: 'Sales',  width: '80' },
                    { field: 'customerService', displayName: 'CS', width: '60'},
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'creditTerm', displayName: 'Payment Term Desc', width: '150'},
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '130', cellFilter: 'number:2', type: 'number', cellClass: 'right'  },
                    { field: 'ebname', displayName: 'EB Name', width: '240' },
                    { field: 'actionOwnerDepartmentCode', displayName: 'ActionOwnerDepartment', width: '180' }
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;

                    //*************************Contact **************************************s
                    gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                        var disputeId = row.entity.id;

                        window.open('#/disputetracking/dispute/' + disputeId);
                    });
                    //*************************Contact **************************************e
                }
            };
            //$scope.members = [
            //    { "legalEnity": "292", "customerCode": "1039470", "customerName": "3B INDUSTRIAL ENTERPRISES", "siteUseId": "2089985", "contact": "aaa@gmail.com(TOm);bbb@gmail.com(cat)", "customerClass": "Excellent", "totalArBalance": "6,000", "overDueAmount": "5,000", "collector": "Tom", "sales": "Yip, Doreen May Fun", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "15 DAYS NET", "totalFutureDue": "1000" },
            //    { "legalEnity": "292", "customerCode": "1058419", "customerName": "3D TECHNOLOGIES (SHENZHEN) CO., LTD.", "siteUseId": "2143194", "contact": "aaa@gmail.com(TOm);bbb@gmail.com(cat)", "customerClass": "Good", "totalArBalance": "2,000", "overDueAmount": "2,000", "collector": "Tom", "sales": "Leung, Candy Oi Yee", "cs": "Frank", "creditLimit": "2,000", "paymentTermDesc": "30 DAYS NET", "totalFutureDue": "0" },
            //    { "legalEnity": "292", "customerCode": "1088594", "customerName": "A FORCE TECHNOLOGY LIMITED", "siteUseId": "2243249", "contact": "aaa@gmail.com(TOm);bbb@gmail.com(cat)", "customerClass": "Issue", "totalArBalance": "1398", "overDueAmount": "1,000", "collector": "Tom", "sales": "Chen, Johnny Yu Chun Johnny", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "30 DAYS NET", "totalFutureDue": "398" },
            //    { "legalEnity": "292", "customerCode": "1040104", "customerName": "ABO ELECTRONICS LIMITED", "siteUseId": "2090704", "contact": "aaa@gmail.com(TOm);bbb@gmail.com(cat)", "customerClass": "Excellent", "totalArBalance": "1500", "overDueAmount": "1,000", "collector": "Tom", "sales": "Tang, Fion Yin Yee", "cs": "Frank", "creditLimit": "2,000", "paymentTermDesc": "After Month End Statement + 30 DAYS", "totalFutureDue": "500" },
            //    { "legalEnity": "292", "customerCode": "1075319", "customerName": "ACE PROGRESS ENTERPRISE LIMITED", "siteUseId": "2202159", "contact": "aaa@gmail.com(TOm)", "customerClass": "Good", "totalArBalance": "1500", "overDueAmount": "1,000", "collector": "Tom", "sales": "Lo, Denise Yuen Han", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "COD Company Cheque", "totalFutureDue": "500" },
            //    { "legalEnity": "292", "customerCode": "1040920", "customerName": "ACOUSTIC ARC INTERNATIONAL LTD", "siteUseId": "2091593", "contact": "aaa@gmail.com(TOm)", "customerClass": "Issue", "totalArBalance": "1300", "overDueAmount": "1,000", "collector": "Tom", "sales": "Li, Alex Chiu Ming", "cs": "Frank", "creditLimit": "2,000", "paymentTermDesc": "After Month End Statement + 30 DAYS", "totalFutureDue": "300" },
            //    { "legalEnity": "292", "customerCode": "1038669", "customerName": "ACTIVE INTELLIGENT TECHNOLOGIES LIMITED", "siteUseId": "2089073", "contact": "aaa@gmail.com(TOm)", "customerClass": "Excellent", "totalArBalance": "7,000", "overDueAmount": "5,000", "collector": "Tom", "sales": "Kwan, Terry Kwan Lung", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "30 DAYS NET", "totalFutureDue": "2000" },
            //    { "legalEnity": "292", "customerCode": "1040303", "customerName": "ADRIANO TECHNOLOGY COMPANY LTD", "siteUseId": "2090942", "contact": "aaa@gmail.com(TOm)", "customerClass": "Good", "totalArBalance": "2500", "overDueAmount": "1,000", "collector": "Tom", "sales": "Lai, Jason Cp Cheuk Pong Jason", "cs": "Frank", "creditLimit": "2,000", "paymentTermDesc": "After Month End Statement + 60 DAYS", "totalFutureDue": "1500" },
            //    { "legalEnity": "292", "customerCode": "1040303", "customerName": "ADRIANO TECHNOLOGY COMPANY LTD", "siteUseId": "2090943", "contact": "aaa@gmail.com(TOm)", "customerClass": "Issue", "totalArBalance": "2000", "overDueAmount": "2,000", "collector": "Tom", "sales": "Chan, Yuki Chui Fun", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "After Month End Statement + 60 DAYS", "totalFutureDue": "0" },
            //    { "legalEnity": "292", "customerCode": "1040312", "customerName": "ADROIT ELECTRONICS CO., LIMITED", "siteUseId": "2090953", "contact": "aaa@gmail.com(TOm)", "customerClass": "Excellent", "totalArBalance": "1398", "overDueAmount": "1,000", "collector": "Tom", "sales": "Chen, Johnny Yu Chun Johnny", "cs": "Frank", "creditLimit": "2,000", "paymentTermDesc": "30 DAYS NET", "totalFutureDue": "398" },
            //    { "legalEnity": "292", "customerCode": "1041133", "customerName": "ADVANCE FORWARDING ENTERPRISE", "siteUseId": "2091814", "contact": "aaa@gmail.com(TOm)", "customerClass": "Good", "totalArBalance": "1500", "overDueAmount": "1,000", "collector": "Tom", "sales": "Chung, Anna", "cs": "Jason", "creditLimit": "2,000", "paymentTermDesc": "COD Company Cheque", "totalFutureDue": "500" },

            //];

            //issue reason status binding
            baseDataProxy.SysTypeDetail("025", function (list) {
                $scope.issueReasonList = list;
            });

            //dispute status binding
            baseDataProxy.SysTypeDetail("026", function (list) {
                list.pop();
                list.pop();
                $scope.statusList = list;
            });

            //invoice status binding
            baseDataProxy.SysTypeDetail("004", function (istatuslist) {
                $scope.istatuslist = istatuslist;
            });
            //Invoice Track States DropDownList binding
            baseDataProxy.SysTypeDetail("029", function (invoiceTrackStatesList) {
                $scope.invoiceTrackStates = invoiceTrackStatesList;
            });

            //*************************aging paging******************************************s
            //paging init
            $scope.selectedLevel = 15;  //init paging size(ng-model)
            $scope.itemsperpage = 15;   //init paging size(parameter)
            $scope.currentPage = 1;     //init page
            $scope.maxSize = 10; //paging display max

            var filstr = "";
            disputeTrackingProxy.disputeTrackingPaging($scope.currentPage, $scope.itemsperpage, "&InvoiceNum=" + $scope.invoiceNum + "&$filter=(DisputeType eq '" + $scope.disputeType + "')", function (list) {
                $scope.totalItems = list[0].count; //init count
                $scope.disputeTrackingList.data = list[0].results; //init list
                $scope.totalNum = list[0].count;
                $scope.calculate(list[0].results.length);
                $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
            }, function (error) {
                alert(error);
            });

            //paging size
            $scope.levelList = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];
            //paging size change
            $scope.pagesizechange = function (selectedLevelId) {

                //multi-conditions
                filstr = buildFilter();

                if ($scope.invoiceNum == undefined || $scope.invoiceNum == 'undefined') {
                    $scope.invoiceNum = "";
                }
                var index = $scope.currentPage;

                if (filterFlg) {
                    disputeTrackingProxy.disputeTrackingPaging(index, selectedLevelId, "&InvoiceNum=" + $scope.invoiceNum + filstr, function (list) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.disputeTrackingList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        $scope.calculate(list[0].results.length);
                    });
                } else {
                    disputeTrackingProxy.disputeTrackingPaging(index, selectedLevelId, "&InvoiceNum=" + $scope.invoiceNum + filstr, function (list) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.disputeTrackingList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        $scope.calculate(list[0].results.length);
                    });
                }
            };

            //paging change
            $scope.pageChanged = function () {

                //multi-conditions
                filstr = buildFilter();

                var index = $scope.currentPage;

                if ($scope.invoiceNum == undefined || $scope.invoiceNum == 'undefined')
                {
                    $scope.invoiceNum = "";
                }

                if (filterFlg) {
                    disputeTrackingProxy.disputeTrackingPaging(index, $scope.itemsperpage, "&InvoiceNum=" + $scope.invoiceNum +  filstr, function (list) {
                        $scope.disputeTrackingList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                } else {
                    disputeTrackingProxy.disputeTrackingPaging(index, $scope.itemsperpage, "&InvoiceNum=" + $scope.invoiceNum + filstr, function (list) {
                        $scope.disputeTrackingList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                        $scope.totalNum = list[0].count;
                        $scope.calculate(list[0].results.length);
                    }, function (error) {
                        alert(error);
                    });
                }
            };
            //*************************aging paging******************************************e

            //openfilter
            var isShow = 0; //0:hide;1:show
            $scope.openFilter = function () {
                if (isShow == 0) {
                    $("#divAgingSearch").show();
                    isShow = 1;
                } else if (isShow == 1) {
                    $("#divAgingSearch").hide();
                    isShow = 0;
                }
                //EB
                ebProxy.Eb("", function (eb) {
                    $scope.ebList = eb;
                });
            }

            $scope.menuToggle = function () {
                $("#wrapper").toggleClass("toggled");
            }

            //reset Search conditions
            $scope.resetSearch = function () {
                filstr = "";
                $scope.custName = "";
                $scope.from = "";
                $scope.to = "";
                $scope.issueReason = "";
                $scope.status = "";
                $scope.disputeType = "0";
                $scope.cancelled = "";
                $scope.closed = "";
                //2016-01-07
                $scope.customerNo = "";
                $scope.siteUseId = "";
                $scope.states = "";
                $scope.trackStates = "";
                $scope.invoiceNum = "";
                //2016-01-07
            }

            //校验输入的条件Id是否为数字，如果不是强转成数字
            $scope.checkNum = function () {
                var reg = new RegExp("^[0-9]*$");
                if (!reg.test($scope.disId)) {
                    $scope.disId = parseInt($scope.disId);
                }
            }

            buildFilter = function () {
                if ($scope.cancelled || $scope.closed) {
                    $scope.disputeType = "1";
                } else {
                    $scope.disputeType = "0";
                }

                //multi-conditions
                var filterStr = "&$filter=(DisputeType eq '" + $scope.disputeType + "')";
                if ($scope.custName) {
                    if (filterStr != "") {
                        filterStr += " and (contains(CustomerName,'" + escape($scope.custName.replace("'", "''")) + "'))";
                    } else {
                        filterStr += "&$filter=(contains(CustomerName,'" + escape($scope.custName.replace("'", "''")) + "'))";
                    }
                }

                //added by zhangYu //2016-01-07
                //invoice states
                if ($scope.states) {
                    filterStr += " and (contains(States,'" + $scope.states + "'))";

                }
                //invoice Track States
                if ($scope.trackStates) {
                    filterStr += " and (contains(TrackStates,'" + $scope.trackStates + "'))";
                }
                //bill Grop Code
                if ($scope.customerNo) {
                    filterStr += " and (contains(CustomerNum, '" + encodeURIComponent($scope.customerNo.replace("'", "''")) + "'))";

                }
                //siteUseId
                if ($scope.siteUseId) {
                    filterStr += " and (contains(SiteUseid, '" + encodeURIComponent($scope.siteUseId.replace("'", "''")) + "'))";

                }
                //ebname
                if ($scope.eb) {
                    filterStr += " and (contains(Ebname, '" + encodeURIComponent($scope.eb.replace("'", "''")) + "'))";
                }
                //Id
                if ($scope.disId) {
                    if (filterStr != "") {
                        filterStr += " and (Id eq " + $scope.disId + ")";
                    } else {
                        filterStr += "&$filter=(Id eq " + $scope.disId + ")";
                    }
                }

                //added by zhangYu //2016-01-07

                if ($scope.from) {
                    if (filterStr != "") {
                        filterStr += " and (date(CreateDate) ge " + $scope.from + ")";
                    } else {
                        filterStr += "&$filter=(date(CreateDate) ge " + $scope.from + ")";
                    }
                }
                if ($scope.to) {
                    if (filterStr != "") {
                        filterStr += " and (date(CreateDate) le " + $scope.to + ")";
                    } else {
                        filterStr += "&$filter=(date(CreateDate) le " + $scope.to + ")";
                    }
                }
                if ($scope.issueReason) {
                    if (filterStr != "") {
                        filterStr += " and (IssueReason eq '" + $scope.issueReason + "')";
                    } else {
                        filterStr += "&$filter=(IssueReason eq '" + $scope.issueReason + "')";
                    }
                }
                if ($scope.status) {
                    if (filterStr != "") {
                        filterStr += " and (DisputeStatus eq '" + $scope.status + "')";
                    } else {
                        filterStr += "&$filter=(DisputeStatus eq '" + $scope.status + "')";
                    }
                }
                if ($scope.cancelled && $scope.closed) {
                    if (filterStr != "") {
                        filterStr += " and (DisputeStatus eq 'Cancelled' or DisputeStatus eq 'Closed')";
                    } else {
                        filterStr += "&$filter=(DisputeStatus eq 'Cancelled' or DisputeStatus eq 'Closed')";
                    }
                } else {
                    if ($scope.cancelled) {
                        if (filterStr != "") {
                            filterStr += " and (DisputeStatus eq 'Cancelled')";
                        } else {
                            filterStr += "&$filter=(DisputeStatus eq 'Cancelled')";
                        }
                    }
                    if ($scope.closed) {
                        if (filterStr != "") {
                            filterStr += " and (DisputeStatus eq 'Closed')";
                        } else {
                            filterStr += "&$filter=(DisputeStatus eq 'Closed')";
                        }
                    }
                }

                return filterStr;
            };

            //Do search
            $scope.searchCollection = function () {

                filterFlg = true;

                //multi-conditions
                filstr = buildFilter();
                //current page
                $scope.currentPage = 1;

                disputeTrackingProxy.disputeTrackingPaging($scope.currentPage, $scope.itemsperpage, "&InvoiceNum=" + $scope.invoiceNum + filstr, function (list) {
                    $scope.totalItems = list[0].count; //init count
                    $scope.disputeTrackingList.data = list[0].results; //init list
                    $scope.totalNum = list[0].count;
                    $scope.calculate(list[0].results.length);
                }, function (error) {
                    alert(error.message);
                });

            };

            //footer items calculate
            $scope.calculate = function (count) {
                if (count == 0) {
                    $scope.fromItem = 0;
                } else {
                    $scope.fromItem = ($scope.currentPage - 1) * $scope.itemsperpage + 1;
                }
                $scope.toItem = ($scope.currentPage - 1) * $scope.itemsperpage + count;
            }
        }])