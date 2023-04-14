angular.module('app.masterdata.ar', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/ar', {
                templateUrl: 'app/masterdata/ar/ARAnalysis.tpl.html',
                controller: 'arAnalysisListCtrl',
                resolve: {
                    //assessmentTypeList: ['assessmentTypeProxy', function (assessmentTypeProxy) {
                    //    return assessmentTypeProxy.getAssessmentType('');
                    //}]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('arAnalysisListCtrl',
    ['$scope', '$interval', 'customerProxy', 'modalService', 'customerAssessmentLogProxy', 'customerAssessmentHistoryProxy', 'assessmentTypeProxy',
        'customerAssessmentProxy', 'collectionProxy',
        function ($scope, $interval, customerProxy, modalService, customerAssessmentLogProxy, customerAssessmentHistoryProxy, assessmentTypeProxy,
            customerAssessmentProxy, collectionProxy) {
            $scope.$parent.helloAngular = "OTC - AR Analysis";
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
                    if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
                return fmt;
            }


            $scope.reAnalyse = function () {
                if (confirm("are you sure to reAnalyse?") == true)
                    collectionProxy.reAnalyse('',function () {

                        //重新绑定日期下拉框
                        customerAssessmentLogProxy.getAllAssessmengDate(function (list) {
                            var resultList = "";
                            angular.forEach(list, function (ad) {
                                resultList += new Date(ad).Format("yyyy-MM-dd") + "|";
                            });
                            $scope.AssessmentLogDateList = resultList.split("|");
                            $scope.assessmentDate = $scope.AssessmentLogDateList[0];
                        }, function (res) {
                            alert(res);
                            });

                        //重新绑定日期AssessmentLog
                        customerAssessmentLogProxy.getAllAssessmengLogCount(function (count) {
                            $scope.totalItemsCAL = count;
                        },
                            function (res) {
                                alert(res);
                            });

                        customerAssessmentLogProxy.getCustomerAssessmentLog('', function (list) {
                            $scope.CustomerAssessmentLogList.data = list;
                            $scope.totalItemsCAL = list.length;
                            $interval(function () { $scope.gridApi.selection.selectRow($scope.CustomerAssessmentLogList.data[0]); }, 0, 1);

                            //查询CustomerAssessment
                            var condition = {};
                            condition.Index = '1';
                            condition.ItemCount = '20';
                            condition.LegalEntity = list[0].legalEntity;
                            condition.AssessmentDate = list[0].assessmentDate;
                            condition.CustomerNum = $scope.custCode;
                            condition.AssessmentType = $scope.assessmentType;
                            condition.SiteUseId = $scope.custSiteUesId;
                            condition.CustomerName = $scope.custName;
                            customerAssessmentProxy.customerAssessmentPaging(condition, function (list) {
                                $scope.CustomerAssessmentList.data = list;
                            }, function (res) {
                                alert(res);
                            });

                        }, function (list) { });

                        alert('Analisys Success');
                    }, function () {
                        alert('Analisys Faild');
                    });
            }


            customerAssessmentLogProxy.getAllAssessmengDate(function (list) {
                var resultList = "";
                
                angular.forEach(list, function (ad) {
                    resultList += new Date(ad).Format("yyyy-MM-dd") + "|";
                });
                $scope.AssessmentLogDateList = resultList.split("|");
                $scope.AssessmentLogDateList.splice($scope.AssessmentLogDateList.length - 1);
                $scope.assessmentDate = $scope.AssessmentLogDateList[0]; 
                //根据Assessment Date下拉框的日期查询CustomerAssessmentLog
                customerAssessmentLogProxy.getCustomerAssessmentLog($scope.assessmentDate, function (list) {
                    $scope.CustomerAssessmentLogList.data = list;
                    $scope.totalItemsCAL = list.length;
                    $interval(function () { $scope.gridApi.selection.selectRow($scope.CustomerAssessmentLogList.data[0]); }, 0, 1);
                    //查询CustomerAssessment
                    var condition = {};
                    condition.Index = '1';
                    condition.ItemCount = '20';
                    condition.LegalEntity = list[0].legalEntity;
                    condition.AssessmentDate = list[0].assessmentDate;
                    condition.CustomerNum = $scope.custCode;
                    condition.AssessmentType = $scope.assessmentType;
                    condition.SiteUseId = $scope.custSiteUesId;
                    condition.CustomerName = $scope.custName;
                    customerAssessmentProxy.customerAssessmentPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    }, function (res) {
                        alert(res);
                    });

                }, function (list) { });

            }, function (res) {
                alert(res);
            });

            //customerAssessmentLogProxy.getAllAssessmengLogCount(function (count) {
            //    $scope.totalItemsCAL = count;
            //},
            //    function (res) {
            //        alert(res);
            //    });

            

            //customerAssessmentProxy.getCustomerAssessmentCount(function (count) {
            //    console.log(count);
            //    $scope.totalItemsCA = count;
            //}, function (res) {
            //    alert(res);
            //});

            assessmentTypeProxy.getAssessmentType('', function (list) {
                $scope.assessmentTypeList = list;
            }, function (list) { });

            $scope.selectAssessmentLogDate = function () {
                //如果是最新log时间save按钮启用
                if ($scope.assessmentDate == $scope.AssessmentLogDateList[0]) {
                    $scope.allowSave = false;
                }
                else {
                    $scope.allowSave = true;
                }
                //根据AssessmentDate查询对应CustomerAssessment表或CustomerAssessment_History表
                customerAssessmentLogProxy.getCustomerAssessmentLog(new Date($scope.assessmentDate).Format("yyyy-MM-dd"), function (list) {
                    $scope.CustomerAssessmentLogList.data = list;
                    $scope.totalItemsCAL = list.length;
                    $interval(function () { $scope.gridApi.selection.selectRow($scope.CustomerAssessmentLogList.data[0]); }, 0, 1);
                }, function (list) { });
            }

            $scope.searchYY = function () {
                $("#searchItem").toggle();
            }

            var now = new Date();
            //$scope.startTime=now.format("yyyy-MM-dd");
            $scope.CustomerAssessmentLogList = {
                multiSelect: false,
                enableFullRowSelection: true,
                noUnselect: true,
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '120' },
                    { field: 'assessmentDate', displayName: 'Assessment Date', width: '170' }

                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                    $scope.gridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                        $scope.SearchCustomerAssessment();
                    });
                }
            };

            $scope.saveCA = function () {
                if ($scope.CustomerAssessmentList.data.length == 0) {
                    alert("No data can be saved");
                }
                else {
                    customerAssessmentProxy.updateCustomerAssessment($scope.CustomerAssessmentList.data,
                        function (res) { alert(res); }, function (res) { alert(res); });
                }

            }


            $scope.CustomerAssessmentList = {
                multiSelect: false,
                enableFullRowSelection: true,
                noUnselect: true,
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '120', enableCellEdit: false },
                    { field: 'customeR_NUM', displayName: 'Customer NO.', width: '120', enableCellEdit: false },
                    { field: 'customeR_NAME', displayName: 'Customer Name', width: '200', enableCellEdit: false },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '90', enableCellEdit: false },
                    { field: 'isams', displayName: 'Is AMS', width: '60', enableCellEdit: false },
                    { field: 'assessmentScore', displayName: 'Score', width: '60', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'rank', displayName: 'Ranking', width: '60', enableCellEdit: false, cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    {
                        field: 'atName', displayName: 'Customer Class', width: '150',
                        editableCellTemplate: 'ui-grid/dropdownEditor',
                        editDropdownOptionsFunction: function (rowEntity, colDef) { return dropdownItem },
                        editDropdownIdLabel: 'id', editDropdownValueLabel: 'name'
                    },
                    { field: 'crediT_TREM', displayName: 'PaymentTerm', width: '120', enableCellEdit: false },
                    { field: 'crediT_LIMIT', displayName: 'Credit LIMIT', width: '120', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' }

                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                    gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef, newValue, oldValue) {
                        //$scope.msg.lastCellEdited = 'edited row id:' + rowEntity.id + ' Column:' + colDef.name + ' newValue:' + newValue + ' oldValue:' + oldValue;
                        var items = $scope.assessmentTypeList;
                        angular.forEach(items, function (g) {
                            if (g.id === newValue) {
                                rowEntity.assessmentType = newValue;
                                rowEntity.atName = g.name;
                            }
                        });
                        $scope.$apply();
                    });
                }
            };

            $scope.SearchCustomerAssessment = function () {
                //alert($scope.assessmentTypeListSecond);
                dropdownItem = $scope.assessmentTypeList;
                $scope.CustomerAssessmentList.data = null;
                var condition = {};
                condition.Index = $scope.currentPageCAL;
                condition.ItemCount = '20';
                condition.LegalEntity = $scope.gridApi.grid.selection.lastSelectedRow.entity.legalEntity;
                condition.AssessmentDate = $scope.gridApi.grid.selection.lastSelectedRow.entity.assessmentDate;
                condition.CustomerNum = $scope.custCode;
                condition.AssessmentType = $scope.assessmentType;
                condition.SiteUseId = $scope.custSiteUesId;
                condition.CustomerName = $scope.custName;
                if ($scope.AssessmentLogDateList && ($scope.assessmentDate == $scope.AssessmentLogDateList[0])) {
                    customerAssessmentProxy.customerAssessmentPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
                else {
                    customerAssessmentHistoryProxy.customerAssessmentHistoryPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
            };

            //if (custAssessmentPaging) {
            //    //$scope.CustomerAssessmentLogList.data = arlist;
            //    //$scope.CustomerAssessmentList.data = arlistR;
            //    $scope.totalItems = custPaging[0].count; //查询结果初始化
            //    $interval(function () { $scope.gridApi.selection.selectRow($scope.customerList.data[0]); }, 0, 1);
            //}

            $scope.selectedLevelCAL = 20;  //下拉单页容量初始化
            $scope.selectedLevelCA = 20;  //下拉单页容量初始化
            $scope.itemsperpageCAL = 20;
            $scope.itemsperpageCA = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页     
            var filstr = "&$filter=(CreateTime ge " + now.format("yyyy-MM-dd") + ")";
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];
            
            //CustomerAssessmentLogList单页容量变化
            $scope.CALpagesizechange = function (selectedLevelId) {
                var index = $scope.currentPageCAL;
                $scope.itemsperpageCAL = selectedLevelId;
                customerAssessmentLogProxy.getCustomerAssessmentLog('', function (list) {
                    $scope.CustomerAssessmentLogList.data = list; 
                    var result = '';
                    angular.forEach(list, function (cal) {
                        if (result.indexOf(cal.assessmentDate.substring(0, 10)) < 0) {
                            result += cal.assessmentDate.substring(0, 10) + "|";
                        }
                    });
                    var resultList = result.split('|');
                    $scope.AssessmentLogDateList = resultList;
                }, function (list) { });
            };

            //CustomerAssessmentList单页容量变化
            $scope.CApagesizechange = function (selectedLevelId) {
                var index = $scope.currentPageCA;
                $scope.itemsperpageCA = selectedLevelId;
                var condition = {};
                condition.Index = '1';
                condition.ItemCount = selectedLevelId;
                condition.LegalEntity = $scope.gridApi.grid.selection.lastSelectedRow.entity.legalEntity;
                condition.AssessmentDate = $scope.gridApi.grid.selection.lastSelectedRow.entity.assessmentDate;
                condition.CustomerNum = $scope.custCode;
                condition.AssessmentType = $scope.assessmentType;
                condition.SiteUseId = $scope.custSiteUesId;
                condition.CustomerName = $scope.custName;

                if ($scope.AssessmentLogDateList && ($scope.assessmentDate == $scope.AssessmentLogDateList[0])) {
                    customerAssessmentProxy.customerAssessmentPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
                else {
                    customerAssessmentHistoryProxy.customerAssessmentHistoryPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
            };

            //CustomerAssessmentLogList翻页
            $scope.CALpageChanged = function () {
                var index = $scope.currentPageCAL;
                customerAssessmentLogProxy.getCustomerAssessmentLog('', function (list) {
                    $scope.CustomerAssessmentLogList.data = list;
                    $interval(function () { $scope.gridApi.selection.selectRow($scope.CustomerAssessmentLogList.data[0]); }, 0, 1);
                }, function (list) { });
            };

            //CustomerAssessmentList翻页
            $scope.CApageChanged = function () {
                var index = $scope.currentPageCA;
                var condition = {};
                condition.Index = index;
                condition.ItemCount = $scope.itemsperpageCA;
                condition.LegalEntity = $scope.gridApi.grid.selection.lastSelectedRow.entity.legalEntity;
                condition.AssessmentDate = $scope.gridApi.grid.selection.lastSelectedRow.entity.assessmentDate;
                condition.CustomerNum = $scope.custCode;
                condition.AssessmentType = $scope.assessmentType;
                condition.SiteUseId = $scope.custSiteUesId;
                condition.CustomerName = $scope.custName;

               if ($scope.AssessmentLogDateList && ($scope.assessmentDate == $scope.AssessmentLogDateList[0])) {
                    customerAssessmentProxy.customerAssessmentPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
                else {
                    customerAssessmentHistoryProxy.customerAssessmentHistoryPaging(condition, function (json) {
                        if (json != null) {
                            $scope.CustomerAssessmentList.data = json.list;
                            $scope.totalItemsCA = json.totalItems;
                        }
                    })
                }
            };

            $scope.resetSearch = function () {
                $scope.custCode = "";
                $scope.custName = "";
                $scope.assessmentType = 0;
                $scope.custSiteUesId = "";
            }
            $scope.export = function () {
                dropdownItem = $scope.assessmentTypeList;
                var condition = {};
                condition.Index = $scope.currentPageCAL;
                condition.ItemCount = '20';
                condition.LegalEntity = $scope.gridApi.grid.selection.lastSelectedRow.entity.legalEntity;
                condition.AssessmentDate = $scope.gridApi.grid.selection.lastSelectedRow.entity.assessmentDate;
                condition.CustomerNum = $scope.custCode;
                condition.AssessmentType = $scope.assessmentType;
                condition.SiteUseId = $scope.custSiteUesId;
                condition.CustomerName = $scope.custName;
                if ($scope.assessmentDate == $scope.AssessmentLogDateList[0]) {
                    customerAssessmentProxy.exportCustomerAssessment(condition, function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                        function (res) { alert(res); })
                }
                else {
                    customerAssessmentHistoryProxy.exportCustomerAssessment(condition, function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },
                        function (res) { alert(res); })
                }
                //var search = {};
                //search.legalEntity = $scope.legalEntity;
                //search.custCode = $scope.custCode;
                //search.custName = $scope.custName;
                //search.invoicecode = $scope.invoicecode;
                //search.status = $scope.status;
                //search.reason = $scope.reason;
                //search.siteUseId = $scope.siteUseId;
                //search.eb = $scope.eb;
                //search.department = $scope.department;

                //disputeReportProxy.downloadReport(angular.toJson(search), function (path) {
                //    window.location = path;
                //    alert("Export Successful!");
                //},
                //    function (res) { alert(res); });
            };
        }]);




