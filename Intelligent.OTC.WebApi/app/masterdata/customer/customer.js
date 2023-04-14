angular.module('app.masterdata.customer', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/customer', {
                templateUrl: 'app/masterdata/customer/customer-list.tpl.html',
                controller: 'customerListCtrl',
                resolve: {
                    //首次加载第一页
                    statuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("005");
                    }],
                    custPaging: ['customerProxy', function (customerProxy) {
                        var now = new Date().format("yyyy-MM-dd");
                        return customerProxy.customerPaging(1, 20, "", "");
                    }],
                    internallist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("024");
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('customerListCtrl',
    ['$scope', '$interval', 'customerProxy', 'modalService', 'custPaging', 'statuslist', 'internallist','siteProxy',
        function ($scope, $interval, customerProxy, modalService, custPaging, statuslist, internallist, siteProxy) {
            $scope.$parent.helloAngular = "OTC - Customer";
            siteProxy.GetLegalEntity('', function (list) {
                $scope.legalEntityList = list;
            }, function (res) {
                alert(res);
                });

            

            $scope.statuslist = statuslist;
            $scope.internallist = internallist;
            var now = new Date();
            //$scope.startTime=now.format("yyyy-MM-dd");
            $scope.customerList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: true,
                columnDefs: [
                    { field: 'collector', displayName: 'Collector', width: '100' },
                    { field: 'organization', displayName: 'Legal Entity', width: '125' },
                    { field: 'customerNum', displayName: 'Customer No.', width: '120' },
                    { field: 'siteUseId', displayName: 'Site Use ID', width: '110' },
                    { field: 'customerName', displayName: 'Customer Name', width: '170' },
                    //{ field: 'star', displayName: 'Star', width: '170', cellTemplate: '<input required class="rb-rating" type="text" value="{{row.entity.star}}" title="" />' },
                    { field: 'crediT_TREM', displayName: 'PaymentTerm', width: '120' },
                    { field: 'lsr', displayName: 'ISR Name', width: '100' },
                    { field: 'fsr', displayName: 'FSR Name', width: '100' },
                    { field: 'groupName', displayName: 'Group Name', width: '120' },
                    //{ field: 'flgName', displayName: 'Account Status', width: '120' },
                    { field: 'ebname', displayName: 'EBName', width: '125' },
                    { field: 'localizE_CUSTOMER_NAME', displayName: 'Localize Customer Name', width: '185' },
                    { field: 'contacT_LANGUAGE', displayName: 'Contact Language', width: '150' },
                    { field: 'branch', displayName: 'Branch', width: '130' },
                    { field: 'badDebt', displayName: 'BadDebt', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'litigation', displayName: 'Litigation', width: '130' },
                    { field: 'alternatE_NAME', displayName: 'Alternate Name', width: '130' },
                    { field: 'country', displayName: 'Country', width: '100' },
                    { field: 'translateD_CUSTOMER_NAME', displayName: 'Translated Customer Name', width: '200' },
                    {
                        field: 'specialNotes', displayName: 'Special Notes', width: '130', cellTooltip:
                            function (row, col) {
                                return row.entity.specialNotes;
                            }
                    },
                    {
                        field: 'comment', displayName: 'Comment', width: '200', cellTooltip:
                            function (row, col) {
                                return row.entity.comment;
                            }
                    },
                    { field: 'commentExpirationDate', enableCellEdit: false, displayName: 'Comment ExpirationDate', cellFilter: 'date:\'yyyy-MM-dd\'', width: '150' },
                    { field: 'ptpdate', enableCellEdit: false, displayName: 'ptp Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '150' },
                    { field: 'ptpamount', displayName: 'ptp Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' }

                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //console.log($('.rb-rating'));
            
            var i = 0;

            $scope.searchCustomer = function () {

                //组合过滤条件
                var filterStr = '';
                if ($scope.custCode) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(CustomerNum,'" + $scope.custCode + "'))";
                    } else {
                        filterStr += "&$filter=(contains(CustomerNum,'" + $scope.custCode + "'))";
                    }
                }
                if ($scope.custName) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(CustomerName,'" + $scope.custName + "'))"
                    } else {
                        filterStr += "&$filter=(contains(CustomerName,'" + $scope.custName + "'))";
                    }
                }
                if ($scope.status) {
                    if (filterStr !== "") {
                        filterStr += "and (IsHoldFlg eq '" + $scope.status + "')";
                    } else {
                        filterStr += "&$filter=(IsHoldFlg eq '" + $scope.status + "')";
                    }
                }
                
                if ($scope.siteUseId) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(SiteUseId,'" + $scope.siteUseId + "'))"
                    } else {
                        filterStr += "&$filter=(contains(SiteUseId,'" + $scope.siteUseId + "'))";
                    }
                }
                if ($scope.collector) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(Collector,'" + $scope.collector + "'))"
                    } else {
                        filterStr += "&$filter=(contains(Collector,'" + $scope.collector + "'))";
                    }
                }
                //if ($scope.isic) {
                //    if (filterStr !== "") {
                //        filterStr += "and (ExcludeFlg eq '" + $scope.isic + "')";
                //    } else {
                //        filterStr += "&$filter=(ExcludeFlg eq '" + $scope.isic + "')";
                //    }
                //}
                if ($scope.misCollect) {
                    if (filterStr !== "") {
                        filterStr += "and (Collector eq null)";
                    } else {
                        filterStr += "&$filter=(Collector eq null)";
                    }
                }
                if ($scope.LocalizeCustomerName) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(LOCALIZE_CUSTOMER_NAME,'" + $scope.LocalizeCustomerName + "'))"
                    } else {
                        filterStr += "&$filter=(contains(LOCALIZE_CUSTOMER_NAME,'" + $scope.LocalizeCustomerName + "'))";
                    }
                }
                //if ($scope.misGroup) {
                //    if (filterStr !== "") {
                //        filterStr += "and (BillGroupName eq null)";
                //    } else {
                //        filterStr += "&$filter=(BillGroupName eq null)";
                //    }
                //}

                if ($scope.legalEntity) {
                    if (filterStr !== "") {
                        filterStr += "and (Organization eq '" + $scope.legalEntity + "')";
                    } else {
                        filterStr += "&$filter=(Organization eq '" + $scope.legalEntity + "')";
                    }
                }
                if ($scope.ebName) {
                    if (filterStr !== "") {
                        filterStr += "and (contains(Ebname,'" + $scope.ebName + "'))"
                    } else {
                        filterStr += "&$filter=(contains(Ebname,'" + $scope.ebName + "'))";
                    }
                }

                if ($scope.startTime) {
                    alert($("#startTime input").val());
                    if (filterStr !== "") {
                        filterStr += "and (CreateTime ge " + $("#startTime input").val() + ")";
                    } else {
                        filterStr += "&$filter=(CreateTime ge " + $("#startTime input").val() + ")";
                    }
                }
                if ($scope.endTime) {
                    if (filterStr !== "") {
                        filterStr += "and (CreateTime le " + $("#endTime input").val() + ")";
                    } else {
                        filterStr += "&$filter=(CreateTime le " + $("#endTime input").val() + ")";
                    }
                }

                //if ($scope.billCode) {
                //    if (filterStr != "") {
                //        filterStr += "and (contains(BillGroupCode,'" + $scope.billCode + "'))"
                //    } else {
                //        filterStr += "&$filter=(contains(BillGroupCode,'" + $scope.billCode + "'))";
                //    }
                //}
                //if ($scope.country) {
                //    if (filterStr != "") {
                //        filterStr += "and (contains(Country,'" + $scope.country + "'))"
                //    } else {
                //        filterStr += "&$filter=(contains(Country,'" + $scope.country + "'))";
                //    }
                //}
                filstr = filterStr;
                //    $scope.currentPage = 1;
                customerProxy.customerPaging($scope.currentPage, $scope.itemsperpage, filstr, $scope.Contacter, function (list) {
                    if (list !== null) {
                        $scope.customerList.data = new Array();
                        
                        $interval(function () {
                            $scope.totalItems = list[0].count;
                            $scope.customerList.data = list[0].results;

                            //$scope.gridApi.selection.selectRow($scope.customerList.data[0]);
                        }, 0, 1);

                        $interval(function () {
                            $('.rb-rating').rating({
                                'showCaption': false,
                                'showClear': false,
                                'disabled': true,
                                'stars': '3',
                                'min': '0',
                                'max': '3',
                                'step': '1',
                                'size': 'sx',
                                'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                            });
                            //$scope.gridApi.selection.selectRow($scope.customerList.data[0]);
                        }, 10, 1);
                    }
                })
            };

            if (custPaging) {
                $scope.customerList.data = custPaging[0].results;
                $scope.totalItems = custPaging[0].count; //查询结果初始化

                

                $interval(function () {
                    $scope.gridApi.selection.selectRow($scope.customerList.data[0]);
                    $('.rb-rating').rating({
                        'showCaption': false,
                        'showClear': false,
                        'disabled': true,
                        'stars': '3',
                        'min': '0',
                        'max': '3',
                        'step': '1',
                        'size': 'sx',
                        'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                    });

                    //$('.rb-rating').rating('refresh', {
                    //    showClear: false,
                    //    disabled: !$inp.attr('disabled')
                    //});
                }, 0, 1);
            }

            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页     
            //var filstr = "&$filter=(CreateTime ge " + now.format("yyyy-MM-dd") + ")";
            var filstr = "";
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];
            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                var index = $scope.currentPage;
                customerProxy.customerPaging(index, selectedLevelId, filstr, $scope.Contacter,function (list) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.customerList.data = new Array();

                    $interval(function () {
                        $scope.customerList.data = list[0].results;
                        $scope.totalItems = list[0].count;
                    }, 0, 1);
                    
                    $interval(function () {
                        $scope.gridApi.selection.selectRow($scope.customerList.data[0]);
                        $('.rb-rating').rating({
                            'showCaption': false,
                            'showClear': false,
                            'disabled': true,
                            'stars': '3',
                            'min': '0',
                            'max': '3',
                            'step': '1',
                            'size': 'sx',
                            'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                        });
                    }, 10, 1);
                });
            };

            //翻页
            $scope.pageChanged = function () {
                var index = $scope.currentPage;
                customerProxy.customerPaging(index, $scope.itemsperpage, filstr, $scope.Contacter, function (list) {
                    $scope.customerList.data = new Array();
                    $interval(function () {
                        $scope.totalItems = list[0].count;
                        $scope.customerList.data = list[0].results;
                    }, 0, 1);
                    
                    $interval(function () {
                        $scope.gridApi.selection.selectRow($scope.customerList.data[0]);
                        $('.rb-rating').rating({
                        'showCaption': false,
                        'showClear': false,
                        'disabled': true,
                        'stars': '3',
                        'min': '0',
                        'max': '3',
                        'step': '1',
                        'size': 'sx',
                        'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
                    });
                    }, 10, 1);
                    
                }, function (error) {
                    alert(error);
                });
            };

            $scope.resetSearch = function () {
                filstr = "";
                $scope.custCode = "";
                $scope.custName = "";
                $scope.status = "";
                $scope.groupName = "";
                $scope.collector = "";
                $scope.isic = "";
                $scope.startTime = "";
                $scope.endTime = "";
                $scope.misCollect = "";
                $scope.misGroup = "";
                $scope.billCode = "";
                $scope.country = "";
            }

            $scope.editCustomer = function () {
                if ($scope.gridApi.selection.getSelectedRows()) {
                    var strids = [];
                    var row = $scope.gridApi.selection.getSelectedRows()[0];
                    //console.log(row);
                    strids.push(row.customerNum);
                    strids.push(row.siteUseId);
                    window.open('#/cust/masterData/' + strids);
                }
            }

            $scope.addCustomer = function () {
                var strids = "newCust";
                window.open('#/cust/masterData/' + strids);
            }




            $scope.Del = function () {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customer/delConfirm.tpl.html',
                    controller: 'delConfirmCtrl',
                    windowClass: 'modalDialog',
                    resolve: {
                        gridApi: function ()
                        { return $scope.gridApi }
                    }
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    //alert(res);
                    $scope.searchCustomer();
                });
            }

            $scope.exportCust = function () {
                var num = $scope.custCode;
                var name = $scope.custName;
                var status = $scope.status;
                var siteUseId = $scope.siteUseId;
                var collector = $scope.collector;
                var begintime = $scope.startTime;
                var endtime = $scope.endTime;
                var miscollector = $scope.misCollect;
                var misgroup = $scope.misGroup;
                var billcode = $scope.billCode;
                var country = $scope.country;
                //var siteUseId = $scope.siteUseId;
                var legalEntity = $scope.legalEntity;
                var ebName = $scope.ebName;
                customerProxy.exportByCondition(num, name, status, collector, begintime, endtime,
                    miscollector, misgroup, billcode, country, siteUseId, legalEntity, ebName,
                    function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                    function (res) { alert(res); });
            };

            $scope.exportComment = function () {
                customerProxy.exportComment(
                    function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                    function (res) { alert(res); });
            };

            $scope.exportCommentFromCsSales = function () {
                customerProxy.exportCommentSales(
                    function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                    function (res) { alert(res); });
            };

            $scope.importComment = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importComment.tpl.html',
                    controller: 'importCommentCtrl',
                    windowClass: 'modalDialog',
                    resolve: {
                        title: function () { return "Import Comments File"; },
                        importType: function () { return "importcomment"; }
                    },
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importCommentFromCsSales = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importComment.tpl.html',
                    controller: 'importCommentCtrl',
                    windowClass: 'modalDialog',
                    resolve: {
                        title: function () { return "Import Comments Cs/Sales File";},
                        importType: function () { return "importcommentfromcssales"; }
                    },
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importCust = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/import.tpl.html',
                    controller: 'importCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importPayment = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importPayment.tpl.html',
                    controller: 'importPaymentCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importContactor = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importContactor.tpl.html',
                    controller: 'importContactorCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.exportContactor = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/exportContactor.tpl.html',
                    controller: 'exportContactorCtrl',
                    size: 'lg',
                    resolve: {
                        legalList: function () { return $scope.legalEntityList; }
                    },
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importAP = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importAP.tpl.html',
                    controller: 'importAPCtrl',
                    windowClass: 'modalDialog'
                   
                    
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.ImportEBBranch = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importEBBranch.tpl.html',
                    controller: 'importEBBranchCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importLitigation = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importLitigation.tpl.html',
                    controller: 'importLitigationCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.importBadDebt = function () {
                var modalDefaults = {
                    templateUrl: 'app/common/upload/importBadDebt.tpl.html',
                    controller: 'importBadDebtCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.updateContactor = function () {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactor/contactor-update.tpl.html',
                    controller: 'contactorUpdateCtrl',
                    size: 'lg',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            $scope.replaceContactor = function () {
                window.open('#/admin/contactor/replace');
            };

        }]);




