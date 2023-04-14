angular.module('app.ptpInfo', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
    }])

.controller('ptpInfoCL', ['$scope', 'custNum','siteUseId', 'PTPPaymentProxy', '$uibModalInstance', 'modalService', '$sce', 
    function ($scope, custNum, siteUseId, PTPPaymentProxy, $uibModalInstance, modalService, $sce) {
        var filter = '';
        PTPPaymentProxy.queryPTPPayment(1, 20, filter, custNum, siteUseId, function (list) {
            if (list.length > 0) {
                $scope.ptpInfo.data = list;
                $scope.totalItems = list[0].collectorId;
            }
            else {
                $scope.ptpInfo.data = list;
                $scope.totalItems = 0;
            }
        }, function (res) {
            alert(res);
            })

        
        var ispartiaPay = new Array();
        var yes = {};
        yes.id = true;
        yes.name = 'true';
        var no = {};
        no.id = false;
        no.name = 'false';
        ispartiaPay[0] = yes;
        ispartiaPay[1] = no;

        var ptpStatus = new Array();
        var open = {};
        open.id = '001';
        open.name = 'Open';
        var closed = {};
        closed.id = '002';
        closed.name = 'Closed';
        ptpStatus[0] = open;
        ptpStatus[1] = closed;

        $scope.ptpInfo = {
            multiSelect: false,
            enableFullRowSelection: false,
            noUnselect: false,
            columnDefs: [
                {
                    field: 'id', displayName: 'ID', width: '100', enableCellEdit: false,
                    cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.updatePTPPayment(row.entity.id,row.entity.isPartialPay,row.entity.promiseDate,row.entity.promissAmount,row.entity.ptpStatus,row.entity.comments)">{{row.entity.id}}</a></div>'
                },
                { field: 'customerNum', displayName: 'Customer No.', width: '135', enableCellEdit: false },
                { field: 'siteUseId', displayName: 'SiteUseId', width: '135', enableCellEdit: false },
                { field: 'promiseDate', displayName: 'PromiseDate', width: '135', cellFilter: 'date:\'yyyy-MM-dd\'' }, //date
                {
                    field: 'isPartialPay', displayName: 'IsPartialPay', width: '135',
                    editableCellTemplate: 'ui-grid/dropdownEditor',
                    editDropdownOptionsFunction: function (rowEntity, colDef) { return ispartiaPay },
                    editDropdownIdLabel: 'id', editDropdownValueLabel: 'name'
                },


                { field: 'promissAmount', displayName: 'PromissAmount',  width: '135' }, //text
                { field: 'comments', displayName: 'Comments', width: '135' }, //text
                {
                    field: 'ptpStatus', displayName: 'PTPStatus', width: '135',
                    editableCellTemplate: 'ui-grid/dropdownEditor',
                    editDropdownOptionsFunction: function (rowEntity, colDef) { return ptpStatus },
                    editDropdownIdLabel: 'id', editDropdownValueLabel: 'name'
                }
            ],
            onRegisterApi: function (gridApi) {
                //set gridApi on scope
                $scope.gridApi = gridApi;
                $scope.gridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-ptpSecond.tpl.html',
                        controller: 'contactPtpSecondCtrl',
                        size: 'lg',
                        resolve: {
                            id: function () {
                                return row.entity.id;
                            },
                            siteUseId: function () {
                                return row.entity.siteUseId;
                            },
                            customerNo: function () {
                                return row.entity.customerNum;
                            },
                            isPartialPay: function () {
                                return row.entity.isPartialPay;
                            },
                            promiseDate: function () {
                                return row.entity.promiseDate;
                            },
                            promiseAmount: function () {
                                return row.entity.promissAmount;
                            },
                            ptpStatus: function () {
                                return row.entity.ptpStatus;
                            },
                            comments: function () {
                                return row.entity.comments;
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            PTPPaymentProxy.queryPTPPayment(1, 20, filter, custNum, siteUseId, function (list) {
                                if (list.length > 0) {
                                    $scope.ptpInfo.data = list;
                                    $scope.totalItems = list[0].collectorId;
                                }
                                else {
                                    $scope.ptpInfo.data = list;
                                    $scope.totalItems = 0;
                                }
                            }, function (res) {
                                alert(res);
                            });
                            $scope.init();
                        }
                    });
                    
                });
            }
        }

        $scope.updatePTPPayment = function (id, isPP, promiseDate, promiseAmount, ptpStatus, comments) {
            var modalDefaults = {
                templateUrl: 'app/common/contactdetail/contact-ptpSecond.tpl.html',
                controller: 'contactPtpSecondCtrl',
                size: 'lg',
                resolve: {
                    id: function () {
                        return id;
                    },
                    siteUseId: function () {
                        return '';
                    },
                    customerNo: function () {
                        return '';
                    },
                    isPartialPay: function () {
                        return isPP;
                    },
                    promiseDate: function () {
                        return promiseDate;
                    },
                    promiseAmount: function () {
                        return promiseAmount;
                    },
                    ptpStatus: function () {
                        return ptpStatus;
                    },
                    comments: function () {
                        return comments;
                    }
                },
                windowClass: 'modalDialog'
            };
            modalService.showModal(modalDefaults, {}).then(function (result) {
                if (result == "submit") {
                    PTPPaymentProxy.queryPTPPayment(1, 20, filter, custNum, siteUseId, function (list) {
                        if (list.length > 0) {
                            $scope.ptpInfo.data = list;
                            $scope.totalItems = list[0].collectorId;
                        }
                        else {
                            $scope.ptpInfo.data = list;
                            $scope.totalItems = 0;
                        }
                    }, function (res) {
                        alert(res);
                    });
                    $scope.init();
                }
            });

        }

        //分页容量下拉列表定义
        $scope.levelList = [
            { "id": 20, "levelName": '20' },
            { "id": 500, "levelName": '500' },
            { "id": 1000, "levelName": '1000' },
            { "id": 2000, "levelName": '2000' },
            { "id": 5000, "levelName": '5000' }
        ];

        $scope.selectedLevel = 20;  //下拉单页容量初始化
        $scope.itemsperpage = 20;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页     

        //单页容量变化
        $scope.pagesizechange = function (selectedLevelId) {
            //filstr = buildFilter();
            var index = $scope.currentPage;
            $scope.totalItems
        };

        //翻页
        $scope.pageChanged = function () {
            $scope.totalItems
        };

        function IsDate(id, sm, idValue, mystring) {
            var reg = /^(\d{4})-(\d{2})-(\d{2})(T(\d{1,2}):(\d{1,2}):(\d{1,2}))?$/;
            var str = mystring;
            var arr = reg.exec(str);
            if (str == "") return true;
            if (!reg.test(str) && RegExp.$2 <= 12 && RegExp.$3 <= 31) {
                //alert("请保证" + id + "是" + idValue + "的" + sm
                //    + "中输入的日期格式为yyyy-mm-dd的正确的日期!");
                alert("Please ensure that the Date format entered in the " + sm + " of " + idValue + " is yyyy-mm-dd!");
                return false;
            }
            return true;
        }

        $scope.save = function () {
            var breakFlag = false;
            angular.forEach($scope.ptpInfo.data, function (temp) {
                if (!IsDate("id", "Promise Date", temp["id"],temp["promiseDate"])) {
                    breakFlag = true;
                }
            });

            if (breakFlag)
            {
                return;
            }


            PTPPaymentProxy.updatePTPPayment($scope.ptpInfo.data, function (res) {
                alert(res);
            }, function (res) {
                alert(res);
            });
        };

        $scope.close = function () {
            $uibModalInstance.close();
        };
    } ]);