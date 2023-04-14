angular.module('app.agingdetail', [])
.config(['crudRouteProvider', '$routeProvider', function (crudRouteProvider, $routeProvider) {

    crudRouteProvider
    .routesFor('agingdetail', 'aging', 'aging/:customerId')
        .whenList({
        });
} ])

    .controller('agingdetailListCtrl',
    ['$scope', 'modalService', 'invoiceProxy', 'contactProxy', 'baseDataProxy', 'customerProxy',
    function ($scope, modalService, invoiceProxy, contactProxy, baseDataProxy, customerProxy) {
        $("#divAgingInfo").hide();
        //*******************************invoice*******************************s
        //***************get num & site************************************s
        var url = window.location.hash;
        var arr = new Array();
        arr = url.split('/');
        var id = arr[2];
        var num = "";
        var site = "";
        var filstr = "";
        invoiceProxy.queryObject({ id: id }, function (cust) {
            $scope.aging = cust;
            num = cust.customerNum;
            site = cust.siteCode;
            //***************get list************************s
            //首次加载invoice
            filstr = "&$filter=(CustomerNum eq '" + num + "') and (SiteCode eq '" + site + "') ";
            invoiceProxy.invoicePaging(1, 10, filstr, function (invoicelist) {
                $scope.list = invoicelist[0].results; //首次当前页数据
                $scope.totalItems = invoicelist[0].count; //查询结果初始化数量
            });
            //首次加载contact
            contactProxy.forCustomer(site, num, function (contactlist) {
                $scope.conlist = contactlist;
            });
            //加载customer
            customerProxy.queryObject({ num: num, site: site }, function (customerEntity) {
                $scope.customer = customerEntity;
            });
            //***************get list************************e
        })
        //***************get num & site************************************e


        //Invoice Status DropDownList数据邦定
        baseDataProxy.SysTypeDetail("004", function (istatuslist) {
            $scope.istatuslist = istatuslist;
        });
        //Cash Status DropDownList数据绑定
        baseDataProxy.SysTypeDetail("008", function (cstatuslist) {
            $scope.cstatuslist = cstatuslist;
        });

        //get the contactType
        baseDataProxy.SysTypeDetail("009", function (contactTypes) {
            $scope.bdContactType = contactTypes;
        });



        $scope.selectedLevel = 10;  //下拉单页容量初始化
        $scope.itemsperpage = 10;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页
        $scope.selectedItems = []; //初始化选中array为空

        //分页容量下拉列表定义
        $scope.pagelevel = [
                            { "id": 10, "levelName": '10' },
                            { "id": 20, "levelName": '20' },
                            { "id": 50, "levelName": '50' },
                            { "id": 100, "levelName": '100' }
                            ];
        //加载nggrid数据绑定
        $scope.invoiceList = {
            data: 'list',
            multiSelect: true,
            enableRowSelection: true,
            enableSelectAll: true,
            enableSorting: true,
            columnDefs: [
                            { field: 'invoiceNum', displayName: 'Invoice #' },
                            { field: 'invoiceDate', displayName: 'Invoice Date' },
                            { field: 'creditTrem', displayName: 'Credit Term' },
                            { field: 'dueDate', displayName: 'Due Date' },
                            { field: 'poNum', displayName: 'Purchase Order' },
                            { field: 'soNum', displayName: 'Sale Order' },
                            { field: 'mstCustomer', displayName: 'RBO' },
                            { field: 'currency', displayName: 'Invoice Currency' },
                            { field: 'originalAmt', displayName: 'Orginal Invoice Amount' },
                            { field: 'balanceAmt', displayName: 'Outstanding Invoice Amount' },
                            { field: 'dl', displayName: 'Days Late' },
                            { field: 'states', displayName: 'Status' },
            //                            { field: '', displayName: 'Cash Status' },
                            {field: 'orderBy', displayName: 'Order By' },
                            { field: 'remark', displayName: 'Comments',
                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><input ng-model="row.entity.remark" type="text" value={{row.entity.remark}}></div>'
                            },
                            { name: 'op', displayName: 'Operation',
                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="grid.appScope.SaveInvoice(row.entity)"> Save </a></div>'
                            }
                        ],
            onRegisterApi: function (gridApi) {
                //set gridApi on scope
                $scope.gridApi = gridApi;
            }
        };

        //清空过滤条件
        $scope.resetSearch = function () {
            filstr = "&$filter=(CustomerNum eq '" + num + "') and (SiteCode eq '" + site + "') ";
            $scope.istatus = "";
            $scope.invoicenos = "";
        }
        //*******************************invoice*******************************e

        //*******************************invoice Paging*******************************s
        //单页容量变化
        $scope.pagesizechange = function (selectedLevelId) {

            var index = $scope.currentPage;
            invoiceProxy.invoicePaging(index, selectedLevelId, filstr, function (list) {
                $scope.itemsperpage = selectedLevelId;
                $scope.list = list[0].results;
            });
        };

        //翻页
        $scope.pageChanged = function () {

            var index = $scope.currentPage;
            invoiceProxy.invoicePaging(index, $scope.itemsperpage, filstr, function (list) {
                $scope.list = list[0].list;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        };
        //*******************************invoice Paging*******************************e

        //*******************************invoice Search*******************************s
        $scope.searchInvoice = function () {
            filstr = "&$filter=(CustomerNum eq '" + num + "') and (SiteCode eq '" + site + "') ";
            //组合过滤条件
            var filterStr = '';
            if ($scope.invoicenos) {
                filterStr += " and (";
                var invoicenos = $.trim($scope.invoicenos.toString()).replace("，", ","); //the input of nums textarea
                var nums = ""; //string of nums filter
                var nos = new Array(); // array of nums filter
                nos = invoicenos.split(',');
                $.each(nos, function () {
                    if (nums == "") {
                        nums += " InvoiceNum eq '" + $.trim(this) + "' ";
                    } else {
                        nums += " or InvoiceNum eq '" + $.trim(this) + "' ";
                    }
                })
                filterStr += nums + ")";
            }

            if ($scope.istatus) {
                filterStr += "and (States eq '" + $scope.istatus + "')";
            }

            filstr = filstr + filterStr;
            //数据数量和当前页数据
            $scope.currentPage = 1;

            invoiceProxy.invoicePaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                $scope.totalItems = list[0].count;
                $scope.list = list[0].results;
            }, function (error) {
                alert(error);
            });

        };

        //*******************************invoice Search*******************************e

        //*******************************contact*******************************s
        $scope.contactList = {
            data: 'conlist',
            columnDefs: [
                            { field: 'name', displayName: 'Contact Name' },
                            { field: 'department', displayName: 'Department' },
                            { field: 'title', displayName: 'Title' },
                            { field: 'number', displayName: 'Contact Num' },
                            { field: 'emailAddress', displayName: 'Email' },
                            { name: 'op', displayName: 'Operation',
                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="grid.appScope.EditContacterInfo(row.entity)"> Edit </a>&nbsp; <a ng-click="grid.appScope.Delcontacter(row.entity)"> Del </a> &nbsp; ' +
                                '<input type="button" id="btnContact"  style="width:53px" ng-click="grid.appScope.openContactCust(row.entity)"  value="Contact" /></div>'

                            }
                        ]
        };
        //*******************************contact*******************************e
        //*******************************AgingInfo*******************************s
        var isShow = 0; //0:hide;1:show
        $scope.showInfo = function () {
            if (isShow == 0) {
                $("#divAgingInfo").show();
                isShow = 1;
            } else if (isShow == 1) {
                $("#divAgingInfo").hide();
                isShow = 0;
            }
        };
        //*******************************AgingInfo*******************************e
        //*******************************SaveInvoice*******************************s
//        $scope.SaveInvoice = function () {
//            this.$parent.row.entity.$update(function () {
//                alert("Save Success");
//            }, function () {
//                alert("Save Error");
//            });
        //        }

        $scope.SaveInvoice = function (entity) {
            entity.$update(function () {
                alert("Save Success");
            }, function () {
                alert("Save Error");
            });
        }

        //*******************************SaveInvoice*******************************e
        //*******************************SaveCustomer*******************************s
        $scope.saveCustomer = function () {
            $scope.customer.$update(function () {
                alert("Save Success");
            }, function () {
                alert("Save Error");
            });
        }

        //*******************************SaveCustomer*******************************e
        //*******************************AccountInfo_Modal*******************************s
        $scope.AccountInfo = function (size, aging, specialNotes) {
            modalService.showModal({
                animation: true,
                templateUrl: 'myModalContent1.html',
                size: size,
                controller: ['$scope', '$modalInstance', 'item', 'notes',
                                function ($scope, $modalInstance, item, notes) {
                                    $scope.item = item;
                                    $scope.notes = notes;
                                    //close 
                                    $scope.closeaccount = function () {
                                        $modalInstance.close();
                                    }
                                } ],

                resolve: {
                    item: function () {
                        return aging;
                    },
                    notes: function () {
                        return specialNotes;
                    }
                }
            })
        };
        //*******************************AccountInfo_Modal*******************************e

        //*******************************show contactHistory*******************************s
        $scope.openContactHistory = function () {

            var modalDefaults = {
                templateUrl: 'app/common/contacthistory/contacthistory-list.tpl.html',
                controller: 'contacthistoryList',
                size: 'lg',
                resolve: { custnum: function () { return num; }, site: function () { return site; }, contactTypeInfo: function () { return $scope.bdContactType; } }

            };

            modalService.showModal(modalDefaults, {}).then(function (result) {

            });

        };
        //*******************************show contactHistory*******************************e
        //*******************************Del contacterInfo*******************************s
        $scope.Delcontacter = function (entity) {
            entity.$remove(function () {
                alert("Delete Success");
                contactProxy.forCustomer(site, num, function (contactlist) {
                    $scope.conlist = contactlist;
                });
            }, function () {
                alert("Delete Error");
            });
        };

        //*******************************Del contacterInfo*******************************e
        //*******************************openContactCust*******************************s
        //$scope.openContactCust = function (row) {
        $scope.openContactCust = function () {
            var modalDefaults = {
                templateUrl: 'app/aging/contactcust/contactcust-list.tpl.html',
                controller: 'contactCustCtrl',
                size: 'lg',
                resolve: {
                    //cont: function () { return row; },
                    num: function () { return num; },
                    site: function () { return site; },
                    istatus: function () { return $scope.istatuslist; },
                    contactTypeInfo: function () { return $scope.bdContactType; } 
                }
            };

            modalService.showModal(modalDefaults, {}).then(function (result) {
                $scope.searchInvoice();
            });

        };
        //*******************************openContactCust*******************************e
        //*******************************openContactDetail*******************************s
        $scope.EditContacterInfo = function (row) {

            var modalDefaults = {
                templateUrl: 'app/common/contact/contact-edit.tpl.html',
                controller: 'contactEditCtrl',
                resolve: { cont: function () { return row; } }
            };


            modalService.showModal(modalDefaults, {}).then(function (result) {
                contactProxy.forCustomer(site, num, function (contactlist) {
                    $scope.conlist = contactlist;
                });

            });
        };
        //*******************************openContactDetail*******************************s

        //*******************************showGenerateSOA*******************************s
        $scope.openGenerateSoa = function () {
            var strids = "";
            if ($scope.gridApi.selection.getSelectedRows()) {

                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (strids == "") {
                        strids += rowItem.id;
                    } else {
                        strids += "," + rowItem.id;
                    }
                });
            }
            if (strids == "") {
                alert("Please choose 1 invoice at least .")
            } else {
                var modalDefaults = {
                    templateUrl: 'app/aging/generatesoa/generatesoa-list.tpl.html',
                    controller: 'generatesoaCL',
                    size: 'lg',
                    resolve: {
                        custnum: function () { return num; },
                        selectedInvoiceId: function () { return strids; }
                    }

                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });
            }
        };
        //*******************************showGenerateSOA*******************************e

    } ]);