angular.module('app.collection', ['angularFileUpload'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
        .when('/collection/new', {
            templateUrl: 'app/collection/collection-edit.tpl.html',
            controller: 'collectionEditCtrl',
            resolve: {
                cust: ['collectionProxy', function (collectionProxy) {
                    return new collectionProxy();
                } ]
            }
        })
        .when('/collection/:itemId', {
            templateUrl: 'app/collection/collection-edit.tpl.html',
            controller: 'collectionEditCtrl',
            resolve: {
                cust: ['collectionProxy', '$route', function (collectionProxy, $route) {
                    return collectionProxy.getById($route.current.params.itemId);
                } ]
            }
        })
        .when('/collection', {
            templateUrl: 'app/collection/collection-list.tpl.html',
            controller: 'testpagingListCtrl',
            resolve: {
                //首次加载第一页
                testPaging: ['testPagingProxy', function (testPagingProxy) {
                    return testPagingProxy.newSearchPaging(1, 20, "");
                } ]
                //                ,
                //                //数据总数
                //                pagecount: ['testPagingProxy', function (testPagingProxy) {
                //                    return testPagingProxy.pagecount("");
                //                } ]

            }
        });
    } ])

    .controller('collectionInfoCtrl', ['$scope', 'cust', function ($scope, cust) {
        $scope.cust = cust;
    } ])

    .controller('collectionEditCtrl', ['$scope', 'cust', '$location', 'i18nNotifications', function ($scope, cust, $location, i18nNotifications) {
        $scope.cust = cust;

        $scope.onSave = function () {
            i18nNotifications.pushForNextRoute('crud.customer.save.success', 'success');
            $location.path('/collection');
        };

        $scope.onUpdate = function () {
            i18nNotifications.pushForNextRoute('crud.customer.save.success', 'success');
            $location.path('/collection');
        };

        $scope.onRemove = function () {
            i18nNotifications.pushForNextRoute('crud.customer.save.success', 'success');
            $location.path('/collection');
        };

    } ])

    .controller('testpagingListCtrl',
        ['$scope', 'modalService', 'testPaging', 'testPagingProxy', 'baseDataProxy', 'siteProxy',
            function ($scope, modalService, testPaging, testPagingProxy, baseDataProxy, siteProxy) {

                //        angular.extend($scope, crudListExtensions('/collection'));

                //        $scope.onConfigClicked = function (customer, $event) {
                //            //alert('processing ' + customer.customerCode);


                //            // Don't let the click bubble up to the ng-click on the enclosing div, which will try to trigger
                //            // an edit of this item.
                //            $event.stopPropagation();
                //        };

                $scope.onUploadAging = function (size) {

                    var modalDefaults = { templateUrl: 'app/collection/uploader.tpl.html', controller: 'uploaderController' };

                    var modalOptions = {
                        headerText: 'AR Aging Upload'
                    };

                    modalService.showModal(modalDefaults, modalOptions).then(function (result) {

                    });
                };

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

                //$scope.testPagings = newSearchPaging[0].results;
                $scope.list = testPaging[0].results; //首次当前页数据
                var filstr = "";

                $scope.selectedItems = [];

                //                $scope.$watchCollection('selectedUsers', function () {
                //                    $scope.selectedUser = angular.copy($scope.selectedUsers[$scope.selectedUsers.length - 1]);
                //                });

                //alert(newSearchPaging[0].count);
                baseDataProxy.SysTypeDetail("006", function (custclass) {
                    $scope.custClass = custclass;
                });

                $scope.testPagings = {
                    data: 'list',
                    jqueryUITheme: true,
                    multiSelect: true,
                    showSelectionCheckbox: true,
                    showFooter: true,
                    enableColumnResize: true,
                    selectedItems: $scope.selectedItems,
                    sortable: true,
                    columnDefs: [
                    { field: 'siteCode', displayName: 'Legal Entity' },
                     { field: '', displayName: 'Customer Code',
                         cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">{{row.entity.customerNum}} &nbsp; <img ng-click="resetSearch();" ng-show="{{row.entity.cusExFlg == 0}}" src="../Content/images/259.png"></div>'
                     },
                    { field: 'customerName', displayName: 'Customer Name' },
                    { field: 'billGroupName', displayName: 'Factory Group Name' },
                    { field: 'customerClass', displayName: 'Customer Class',
                        cellTemplate:
                        '<select id="risk" name="risk" ng-model="row.entity.customerClass" ng-options="cu.detailValue as cu.detailName for cu in custClass" ><option value="">--All--</option></select>'
                    },
                    { field: 'riskScore', displayName: 'Risk Score' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance' },
                    { field: 'due90Amt', displayName: 'Over 90 days' },
                    { field: 'creditLimit', displayName: 'Credit Limit' },
                    { field: 'accountStatus', displayName: 'Status' },
                    { field: 'accountStatus', displayName: 'Account Status' },
                    { field: '', displayName: 'Operator',
                        cellTemplate: '<div class="ngCellText"  ng-class="col.colIndex()">{{row.entity.operator}} <img ng-click="resetSearch();"  src="../Content/images/331.png"></div>'
                    },
                    { field: '', displayName: 'Operation',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="Save(this)"> Save </a></div>'
                    }
                    //                    
                    ]


                };

                //保存单行数据
                $scope.Save = function () {
                    this.$parent.row.entity.$update(function () { alert("Save Success"); }, function () { alert("Save Error"); });
                }

                //删除被选中行数据
                $scope.Del = function () {
                    //                    setTimeout(function () {
                    //$scope.selectedUsers[$scope.selectedUsers.length - 1]//被选中的最后一行
                    if ($scope.selectedItems) {
                        angular.forEach($scope.selectedItems, function (rowItem) {
                            rowItem.$remove(); //***********************
                        });
                        //重新绑定数据
                        testPagingProxy.newSearchPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                            $scope.totalItems = list[0].count;
                            $scope.list = list[0].results;
                        })
                    }
                    //$scope.selectedLevels = [];
                    //                    }, 2000);
                }

                //单页容量下拉列表定义
                $scope.levelList = [
                            { "id": 20, "levelName": '20' },
                            { "id": 500, "levelName": '500' },
                            { "id": 1000, "levelName": '1000' },
                            { "id": 2000, "levelName": '2000' },
                            { "id": 5000, "levelName": '5000' }
                            ];

                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.totalItems = testPaging[0].count; //查询结果初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1;
                $scope.maxSize = 10;
                //单页容量变化
                $scope.pagesizechange = function (selectedLevelId) {
                    var index = $scope.currentPage;
                    testPagingProxy.fortestPaging(index, selectedLevelId, filstr, function (list) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.list = list;
                    });
                };

                //翻页
                $scope.pageChanged = function () {

                    var index = $scope.currentPage;
                    testPagingProxy.fortestPaging(index, $scope.itemsperpage, filstr, function (list) {
                        $scope.list = list;
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
                            filterStr += "and (contains(CustomerNum,'" + $scope.custName + "'))"
                        } else {
                            filterStr += "&$filter=(contains(CustomerNum,'" + $scope.custName + "'))";
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


        .controller('uploaderController', ['$scope', 'FileUploader', 'APPSETTING', '$modalInstance', 'baseDataProxy', function ($scope, FileUploader, APPSETTING, $modalInstance, baseDataProxy) {

            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/Collection'
                //
            });

            // FILTERS

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    if ((item.size / 1024) > (10 * 1024)) {
                        alert("File size exceeds 10MB !");
                    }
                    if (item.name.toString().toUpperCase().split(".")[1] != "TXT" && item.name.toString().toUpperCase().split(".")[1] != "XLSM") {
                        alert("File format is not correct !");
                    }
                    return this.queue.length < 10;
                }
            });

            // CALLBACKS
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                if (response.split("&&")[0] == "error:1") {
                    document.getElementById("lblMessage").innerHTML = response.split("&&")[1];
                    document.getElementById("lblMessage").style.color = "green";
                } else if (response.split("&&")[0] == "error:2") {
                    document.getElementById("lblMessage").innerHTML = response.split("&&")[1];
                    document.getElementById("lblMessage").style.color = "red";
                } else {
                    document.getElementById("lblMessage").innerHTML = response;
                    document.getElementById("lblMessage").style.color = "red";
                }
                console.info('onSuccessItem', fileItem, response, status, headers);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                document.getElementById("lblMessage").innerHTML = response;
                document.getElementById("lblMessage").style.color = "red";
                console.info('onErrorItem', fileItem, response, status, headers);
            };

            console.info('uploader', uploader);

            baseDataProxy.SysTypeDetail("007", function (levellist) {
                $scope.levelList = levellist;
            });

            $scope.selectedLevels = [];

            $scope.$watchCollection('selectedLevels', function () {
                $scope.selectedLevel = angular.copy($scope.selectedLevels[0]);
            });

            var idx = 0;
            $scope.updateLevel = function (selectedLevel) {
                if (typeof (selectedLevel) == "undefined") {
                    alert("Please select account level report !");
                }
                else {
                    uploader.queue[idx].url = APPSETTING['serverUrl'] + '/api/Collection' + '?levelFlag=' + selectedLevel;
                    idx++;
                }
            };

            $scope.clearInfo = function () {
                document.getElementById("updfile").value = "";
                document.getElementById("lblMessage").innerHTML = "";
            };

            $scope.close = function () {
                $modalInstance.close();
            };

            $scope.$watch('selectedLevel[0].levels', function (selectedLevelId) {
                if (selectedLevelId) {
                    angular.forEach($scope.selectedLevel[0].levels, function (level) {
                        if (selectedLevelId == level.id) {
                            $scope.selectedLevel = level;
                        }
                    });
                }
            });

        } ]);
