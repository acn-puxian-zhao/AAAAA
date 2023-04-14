angular.module('app.masterdata.period', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

            .when('/admin/period', {
                templateUrl: 'app/masterdata/period/period-list.tpl.html',
                controller: 'periodConfigCtrl',
                resolve: {
                    //首次加载第一页
                    //                currentDeal: ['periodProxy', function (periodProxy) {
                    //                    return periodProxy.searchInfo("Deal");
                    //                } 
                    //                ]

                }
            });
    }])

    //*****************************************header***************************s
    .controller('periodConfigCtrl', ['$scope', 'periodProxy', '$http', 'modalService',
        function ($scope, periodProxy, $http, modalService) {
            $scope.$parent.helloAngular = "OTC - Collection Period";

            periodProxy.searchInfo("Deal", function (cdeal) {
                $scope.dealName = cdeal;
            });


            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页
            var filstr = "";

            var arrowMinDate = new Date();

            periodProxy.peroidPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                $scope.totalItems = list[0].count;
                $scope.list = list[0].results;
                //angular.forEach($scope.list, function (peroid) {
                //    endDate = new Date(peroid.periodEnd);
                //    //if (peroid.soaFlg == '1' && endDate > arrowMinDate)
                //    //{
                //    //    arrowMinDate = endDate;
                //    //}
                //    if (peroid.soaFlg === '1') {
                //        if (endDate < arrowMinDate)
                //            arrowMinDate = endDate;
                //    }
                //});
                //$scope.periodStartDate = arrowMinDate.setDate(arrowMinDate.getDate() + 1);
                //$scope.periodEndDate = arrowMinDate.setDate(arrowMinDate.getDate() + 1);
                $scope.dateOptions = {
                    formatYear: 'yy',
                    maxDate: new Date(3000, 1, 1),
                    //minDate: arrowMinDate,
                    startingDay: 1
                };
            }, function (error) {
                alert(error);
            });

            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];

            //加载nggrid数据绑定
            $scope.periodList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: true,
                data: 'list',
                columnDefs: [
                    { field: 'sortId', displayName: '#', width: '60' },
                    {
                        field: 'periodBegin', displayName: 'Start Date', width: '120',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">{{row.entity.periodBegin | date:"yyyy-MM-dd HH:mm:ss"}}</div>'
                    },
                    {
                        field: 'periodEnd', displayName: 'Close Date', width: '120',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">{{row.entity.periodEnd | date:"yyyy-MM-dd HH:mm:ss"}}</div>'
                    },
                    { field: 'operator', displayName: 'Operator', width: '100' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //Date扩展方法格式化时间
            Date.prototype.Format = function (fmt) { //author: meizz 
                var o = {
                    "M+": this.getMonth() + 1, //月份 
                    "d+": this.getDate(), //日 
                    "h+": this.getHours(), //小时 
                    "m+": this.getMinutes(), //分 
                    "s+": this.getSeconds(), //秒 
                    "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
                    "S": this.getMilliseconds() //毫秒 
                };
                if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
                for (var k in o)
                    if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length === 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
                return fmt;
            }

            //新账期开启
            $scope.addPeriod = function () {
                if ($scope.periodStartDate === null || $scope.periodStartDate === "" || $scope.periodStartDate === undefined) {
                    alert("Please select new period's start date!");
                    return;
                }
                else if ($scope.periodEndDate === null || $scope.periodEndDate === "" || $scope.periodEndDate === undefined) {
                    alert("Please select new period's end date!");
                    return;
                }
                else if (new Date($scope.periodEndDate) < new Date($scope.periodStartDate)) {
                    alert("new period's end date cann't less than start date");
                    return;
                }
                else {
                    var perEndDate = [];
                    perEndDate.push(new Date($scope.periodEndDate).Format("yyyy-MM-dd"));
                    perEndDate.push(new Date($scope.periodStartDate).Format("yyyy-MM-dd"));
                    periodProxy.addNewperoidByEndTime(perEndDate
                        , function (res) {
                            //alert("Successed!");
                            alert(res);
                            periodProxy.peroidPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                                $scope.totalItems = list[0].count;
                                $scope.list = list[0].results;
                                //angular.forEach($scope.list, function (peroid) {
                                    //endDate = new Date(peroid.periodEnd);
                                    //if (peroid.soaFlg == '1' && endDate > arrowMinDate) {
                                    //    arrowMinDate = endDate;
                                    //}
                                    //if (peroid.soaFlg === '1') {
                                    //    if (endDate < arrowMinDate) {
                                    //        arrowMinDate = endDate;
                                    //    }
                                    //    else {
                                    //arrowMinDate = new Date();
                                    //    }
                                    //}
                                //});
                               // $scope.periodEndDate = arrowMinDate.setDate(arrowMinDate.getDate() + 1);
                                $scope.dateOptions = {
                                    formatYear: 'yy',
                                    maxDate: new Date(3000, 1, 1),
                                    //minDate: arrowMinDate,
                                    startingDay: 1
                                };
                            });
                        }
                        , function (error) { alert(error) });
                }
            };

            $scope.editPeriod = function () {
                var selected = $scope.gridApi.selection.getSelectedRows();
                if (selected && selected.length > 0) {
                    var row = $scope.gridApi.selection.getSelectedRows()[0];
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/period/period-info.tpl.html',
                        controller: 'periodInfoCtrl',
                        resolve: {
                            periodDetail: row
                        }
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result !== undefined) {
                            alert(result);
                            if (result.indexOf("Success") >= 0) {
                                periodProxy.peroidPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                                    $scope.totalItems = list[0].count;
                                    $scope.list = list[0].results;
                                    //angular.forEach($scope.list, function (peroid) {
                                    //    endDate = new Date(peroid.periodEnd);
                                    //    arrowMinDate = new Date();
                                    //});
                                    //$scope.periodEndDate = arrowMinDate.setDate(arrowMinDate.getDate() + 1);
                                    $scope.dateOptions = {
                                        formatYear: 'yy',
                                        maxDate: new Date(3000, 1, 1),
                                        //minDate: arrowMinDate,
                                        startingDay: 1
                                    };
                                });
                            }
                        }
                    });
                }
            }
            $scope.deletePeriod = function () {
                var selected = $scope.gridApi.selection.getSelectedRows();
                if (selected && selected.length > 0) {
                    if (confirm("are you sure to delete the period?") == true) {
                        var row = $scope.gridApi.selection.getSelectedRows()[0];
                        periodProxy.deletePeriod(row.id
                            , function (res) {
                                //alert("Successed!");
                                alert(res);
                                //console.log(res);
                                periodProxy.peroidPaging($scope.currentPage, $scope.itemsperpage, filstr, function (list) {
                                    $scope.totalItems = list[0].count;
                                    $scope.list = list[0].results;
                                    angular.forEach($scope.list, function (peroid) {
                                        endDate = new Date(peroid.periodEnd);
                                        arrowMinDate = new Date();
                                    });
                                    $scope.periodEndDate = arrowMinDate.setDate(arrowMinDate.getDate() + 1);
                                    $scope.dateOptions = {
                                        formatYear: 'yy',
                                        maxDate: new Date(3000, 1, 1),
                                        //minDate: arrowMinDate,
                                        startingDay: 1
                                    };
                                });
                            }
                            , function (error) { alert(error) });
                    }
                }
            };
            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                var index = $scope.currentPage;
                periodProxy.peroidPaging($scope.currentPage, selectedLevelId, filstr, function (list) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = list[0].count;
                    $scope.list = list[0].results;
                }, function (error) {
                    alert(error);
                });
            };

            //翻页
            $scope.pageChanged = function () {

                var index = $scope.currentPage;
                periodProxy.peroidPaging(index, $scope.itemsperpage, filstr, function (list) {
                    $scope.list = list[0].results;
                    $scope.totalItems = list[0].count;
                }, function (error) {
                    alert(error);
                });
            }

            //angularjs时间控件Start
            $scope.clear = function () {
                $scope.periodEndDate = null;
                $scope.periodStartDate = null;
            };

            $scope.inlineOptions = {
                customClass: getDayClass,
                minDate: new Date(),
                showWeeks: true
            };



            // Disable weekend selection
            function disabled(data) {
                var date = data.date,
                    mode = data.mode;
                return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
            };

            $scope.toggleMin = function () {
                //$scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
                //$scope.dateOptions.minDate = $scope.inlineOptions.minDate;
            };

            $scope.open1 = function () {
                $scope.popup1.opened = true;
            };
            $scope.open = function () {
                $scope.popup.opened = true;
            };

            $scope.setDate = function (year, month, day) {
                $scope.periodEndDate = new Date(year, month, day);
                $scope.periodStartDate = new Date(year, month, day);
            };

            $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
            $scope.format = $scope.formats[0];
            $scope.altInputFormats = ['M!/d!/yyyy'];

            $scope.popup1 = {
                opened: false
            };
            $scope.popup = {
                opened: false
            };
            var tomorrow = new Date();
            tomorrow.setDate(tomorrow.getDate() + 1);
            var afterTomorrow = new Date();
            afterTomorrow.setDate(tomorrow.getDate() + 1);
            $scope.events = [
                {
                    date: tomorrow,
                    status: 'full'
                },
                {
                    date: afterTomorrow,
                    status: 'partially'
                }
            ];

            function getDayClass(data) {
                var date = data.date,
                    mode = data.mode;
                if (mode === 'day') {
                    var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                    for (var i = 0; i < $scope.events.length; i++) {
                        var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                        if (dayToCheck === currentDay) {
                            return $scope.events[i].status;
                        }
                    }
                }

                return '';
            }
            //angularjs时间控件End

        }])

    .controller('periodInfoCtrl', ['$scope', 'periodProxy', '$uibModalInstance', 'periodDetail',
        function ($scope, periodProxy, $uibModalInstance, periodDetail) {
            $scope.id = periodDetail.id;
            var startdate = new Date(periodDetail.periodBegin);
            var enddate = new Date(periodDetail.periodEnd);
            $scope.periodStartDate = startdate.setDate(startdate.getDate());
            $scope.periodEndDate = enddate.setDate(enddate.getDate());
            $scope.dateOptions = {
                formatYear: 'yy',
                maxDate: new Date(3000, 1, 1),
                //minDate: arrowMinDate,
                startingDay: 1
            };

            $scope.close = function () {
                $uibModalInstance.close();
            };
            $scope.save = function () {
                if ($scope.periodStartDate === null || $scope.periodStartDate === "" || $scope.periodStartDate === undefined) {
                    alert("Please select new period's start date!");
                    return;
                }
                else if ($scope.periodEndDate === null || $scope.periodEndDate === "" || $scope.periodEndDate === undefined) {
                    alert("Please select new period's end date!");
                    return;
                }
                else if (new Date($scope.periodEndDate) < new Date($scope.periodStartDate)) {
                    alert("new period's end date cann't less than start date");
                    return;
                }
                else {
                    var perEndDate = [];
                    perEndDate.push($scope.id);
                    perEndDate.push(new Date($scope.periodEndDate).Format("yyyy-MM-dd"));
                    perEndDate.push(new Date($scope.periodStartDate).Format("yyyy-MM-dd"));
                    periodProxy.addNewperoidByEndTime(perEndDate
                        , function (res) {
                            $uibModalInstance.close(res);
                            //alert(res);
                        }, function (error) { alert(error) });
                    //$uibModalInstance.close();
                }
            };
            //angularjs时间控件Start
            $scope.clear = function () {
                $scope.periodEndDate = null;
                $scope.periodStartDate = null;
            };

            $scope.inlineOptions = {
                customClass: getDayClass,
                minDate: new Date(),
                showWeeks: true
            };
            $scope.open1 = function () {
                $scope.popup1.opened = true;
            };
            $scope.open = function () {
                $scope.popup.opened = true;
            };

            $scope.setDate = function (year, month, day) {
                $scope.periodEndDate = new Date(year, month, day);
                $scope.periodStartDate = new Date(year, month, day);
            };

            $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
            $scope.format = $scope.formats[0];
            $scope.altInputFormats = ['M!/d!/yyyy'];

            $scope.popup1 = {
                opened: false
            };
            $scope.popup = {
                opened: false
            };
            function getDayClass(data) {
                var date = data.date,
                    mode = data.mode;
                if (mode === 'day') {
                    var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                    for (var i = 0; i < $scope.events.length; i++) {
                        var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                        if (dayToCheck === currentDay) {
                            return $scope.events[i].status;
                        }
                    }
                }
                return '';
            }
        }]);