angular.module('app.statistics', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/statistics/statistics', {
                templateUrl: 'app/statistics/statistics.tpl.html',

                controller: 'statisticsCtrl',
                resolve: {
                    regionlist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("044");
                    }],
                    typelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("046");
                    }],
                    collectorGraphlist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("045");
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('statisticsCtrl',
    ['$scope', '$interval', 'openARProxy', 'customerContactCoverageProxy', 'cashCollectedProxy', 'disputReasonProxy', 'regionlist', 'typelist','collectorGraphlist','statisticsCollectProxy','collectorStatisticsHisProxy',
        function ($scope, $interval, openARProxy, customerContactCoverageProxy, cashCollectedProxy, disputReasonProxy, regionlist, typelist, collectorGraphlist,statisticsCollectProxy, collectorStatisticsHisProxy) {
            $scope.now = new Date();
            $scope.openARSum = 0;
            $scope.OverdueSum = 0;
            $scope.OverduePrecent = 0;
            $scope.disputeSum = 0;
            $scope.PTPSum = 0;
            $scope.PTPPrecent = 0;

            function formatDate(now) {
                var year = now.getFullYear();
                var month = now.getMonth() + 1;
                var date = now.getDate();
                var hour = now.getHours();
                var minute = now.getMinutes();
                var second = now.getSeconds();
                return year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
            } 

            statisticsCollectProxy.GetStatisticsCollectSum(function (res) {
                if (res !== null)
                {
                    $scope.now = formatDate(new Date(res.now));
                    if (res.openAR) {
                        $scope.openARSum = res.openAR.toFixed(2);
                    }
                    if (res.overDure) {
                        $scope.OverdueSum = res.overDure.toFixed(2);
                    }
                    if (res.openAR && res.overDure) {
                        $scope.OverduePrecent = (res.overDure / res.openAR * 100).toFixed(2);
                    }
                    if (res.dispute) {
                        $scope.disputeSum = res.dispute.toFixed(2);
                    }
                    if (res.ptpAR) {
                        $scope.PTPSum = res.ptpAR.toFixed(2);
                    }
                    $scope.PTPPrecent = (res.ptpAR / res.openAR * 100).toFixed(2);                }
            }, function (res) { alert(res); })

            $scope.regionlist = new Array();
            var regionAll = {};
            regionAll.detailValue = 'all';
            regionAll.detailName = 'All';
            $scope.regionlist.push(regionAll);

            $scope.regionlist = $scope.regionlist.concat(regionlist);
            var regionOther = {};
            regionOther.detailValue = 'other';
            regionOther.detailName = 'Other';
            $scope.regionlist.push(regionOther);

            var currentYear = new Date().getFullYear();
            var currentMonth = new Date().getMonth() + 1;
            //alert($scope.openAR);
            $scope.openAR = currentYear + '-' + (currentMonth); 
            $scope.ccc = currentYear + '-' + (currentMonth);
            $scope.cashColl = currentYear + '-' + (currentMonth);
            $scope.startDate = currentYear + '-' + (currentMonth);
            $scope.endDate = currentYear + '-' + (currentMonth);
            $scope.region = 'all';
            $scope.region2 = 'all';

            $scope.fmoney = function (s) {
                n = 0;
                s = parseFloat((s + "").replace(/[^\d\.-]/g, "")).toFixed(n) + "";
                var l = s.split(".")[0].split("").reverse();
                t = "";
                for (i = 0; i < l.length; i++) {
                    t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? "," : "");
                }
                return t.split("").reverse().join("");
            } 

            openARProxy.GetOpenAR($scope.region,function (res) {

                // 基于准备好的dom，初始化echarts实例
                var cusAging = echarts.init(document.getElementById('CusAging'));

                if (res.length == 0) {
                    // 指定图表的配置项和数据
                    var caOption = {
                        color: ['#3398DB'],
                        title: {
                            text: 'Overdue By Aging Bucket'
                        },
                        tooltip: {},
                        legend: {
                            data: ['$', '$', '$', '$', '$', '$', '$', '$','$']
                        },
                        //legend: {
                        //    data: ['销量']
                        //},
                        xAxis: {
                            type: 'category',
                            data: ["A Current", "B1-30", "C31-60", "D61-90", "E91-120", "F121-150", "G151-180", "H181-360", "I > 360"],
                            axisLabel: {
                                interval: 0,
                                rotate: 55
                            }
                        },
                        yAxis: {
                            axisLabel: {
                                interval: 0,
                                rotate: 55,
                                formatter: '$ {value}'
                            }
                        },
                        series: [{
                            name: 'Aging Bucket',
                            type: 'bar',
                            itemStyle: {
                                normal: {
                                    label: {
                                        show: true, position: 'inside',
                                        formatter: function (a) {
                                            return $scope.fmoney(a['value']);
                                        } },
                                    color: function (parmars) {
                                        var colorList = ["#213395", "#0db328", "#269e95", "#df1525", "#faeb2e",
                                            "#f06270", "#c9e72e", "#3cb34b", "#2e92ce"];
                                        return colorList[parmars.dataIndex];
                                    }
                                }
                            },
                            data: [0, 0, 0, 0, 0, 0, 0, 0, 0 ]
                        }]
                    };

                } else {
                    // 指定图表的配置项和数据
                    var caOption = {
                        color: ['#3398DB'],
                        title: {
                            text: 'Overdue By Aging Bucket'
                        },
                        tooltip: {},
                        legend: {
                            data: ['$', '$', '$', '$', '$', '$', '$', '$', '$']
                        },
                        xAxis: {
                            type: 'category',
                            data: ["A Current", "B1-30", "C31-60", "D61-90", "E91-120", "F121-150", "G151-180", "H181-360", "I > 360"],
                            axisLabel: {
                                interval: 0,
                                rotate: 55
                            }
                        },
                        yAxis: {
                            axisLabel: {
                                interval: 0,
                                rotate: 55,
                                formatter: '$ {value}'
                            }
                        },
                        series: [{
                            name: 'Aging Bucket',
                            type: 'bar',
                            itemStyle: {
                                normal:
                                {
                                    label: {
                                        show: true, position: 'inside',
                                        formatter: function (a) {
                                            return $scope.fmoney(a['value']);
                                        }
                                    },
                                    color: function (parmars) {
                                        var colorList = ["#213395", "#0db328", "#269e95", "#df1525", "#faeb2e",
                                            "#f06270", "#c9e72e", "#3cb34b","#2e92ce"];
                                        return colorList[parmars.dataIndex];
                                    }
                                }
                            },
                            data: [res[0].aCurrent == null || res[0].aCurrent == 0 ? 0 : res[0].aCurrent.toFixed(2),
                                res[0].b30 == null || res[0].b30 == 0 ? 0 : res[0].b30.toFixed(2),
                                res[0].c60 == null || res[0].c60 == 0 ? 0 : res[0].c60.toFixed(2),
                                res[0].d90 == null || res[0].d90 == 0 ? 0 : res[0].d90.toFixed(2),
                                res[0].e120 == null || res[0].e120 == 0 ? 0 : res[0].e120.toFixed(2),
                                res[0].f150 == null || res[0].f150 == 0 ? 0 : res[0].f150.toFixed(2),
                                res[0].g180 == null || res[0].g180 == 0 ? 0 : res[0].g180.toFixed(2),
                                res[0].h360 == null || res[0].h360 == 0 ? 0 : res[0].h360.toFixed(2),
                                res[0].i360 == null || res[0].i360 == 0 ? 0 : res[0].i360.toFixed(2)]
                        }]
                    };
                }

                

                // 使用刚指定的配置项和数据显示图表。
                cusAging.setOption(caOption);

            }, function (res) {
                //console.log(res);
                });

            function GetCustomerContactCount(y,m)
            {
                // 基于准备好的dom，初始化echarts实例
                var customerContactCoverage = echarts.init(document.getElementById('CustomerContactCoverage'));

                // 指定图表的配置项和数据
                var cccOption = {
                    color: ['#3398DB'],
                    title: {
                        text: 'Customer Contact Coverage(MTD)'
                    },
                    tooltip: {},
                    //legend: {
                    //    data: ['销量']
                    //},
                    xAxis: {
                        type: 'category',
                        data: [],
                        axisLabel: {
                            interval: 0,
                            rotate: 55
                        }
                    },
                    yAxis: [{
                        axisLabel: {
                            interval: 0,
                            rotate: 55
                        }
                    },
                    {
                        type: 'value',
                        scale: true,
                        name: '',
                        max: 100,
                        min: 0,
                        boundaryGap: [0.2, 0.2],
                        axisLabel: {
                            formatter: '{value} %'
                        }
                    }],
                    series: [{
                        name: 'Contact number',
                        type: 'bar',
                        itemStyle: {
                            normal: {
                                label: { show: true, position: 'inside' },
                                color: function (parmars) {
                                    var colorList = ["#0db328", "#df1525", "#faeb2e",
                                        "#f06270", "#3cb34b", "#2e92ce"];
                                    return colorList[parmars.dataIndex];
                                }
                            }
                        },
                        data: []
                    }
                    //    ,
                    //{
                    //    name: 'Contact scale(%)',
                    //    type: 'line',
                    //    yAxisIndex: 1,
                    //    itemStyle: {
                    //        normal: {
                    //            label: {
                    //                show: true, position: 'inside',
                    //                //formatter: function (a) {
                    //                //    return a['value'] + "%";
                    //                //}
                    //            }
                    //        }
                    //    },
                    //    color: 'red',
                    //    data: []
                    //    }
                    ]
                };

                customerContactCoverageProxy.GetCustomerContactCount(y, m, function (res) {
                    customerContactCoverageProxy.GetCustomerCountPercent(y, m, function (result) {

                        var regionList = new Array();
                        var percentList = new Array();
                        var amtList = new Array();

                        angular.forEach(res, function (r) {
                            regionList.push(r.region);
                            amtList.push(r.amt);
                        });

                        angular.forEach(result, function (r) {
                            percentList.push(r.percent.toFixed(0));
                        });

                        cccOption.xAxis.data = regionList;//x轴赋值数据
                        cccOption.series[0].data = amtList;//y轴赋值数据
                        //cccOption.series[1].data = percentList;//y轴赋值数据
                        // 使用刚指定的配置项和数据显示图表。
                        customerContactCoverage.setOption(cccOption);
                    }, function (result) {
                        //console.log(result);
                    });
                }, function (res) {
                    //console.log(res);
                });
            }

            GetCustomerContactCount(currentYear, currentMonth);

            $("#ccc").on('changeDate', function (ev) {
                GetCustomerContactCount(ev.date.getFullYear(), ev.date.getMonth() + 1);
            });

            function GetCashCollected(y,m) {
                cashCollectedProxy.GetCashCollected(y, m, function (res) {

                    // 基于准备好的dom，初始化echarts实例
                    var CashCollected = echarts.init(document.getElementById('CashCollected'));

                    // 指定图表的配置项和数据
                    var cashCOption = {
                        color: ['#3398DB'],
                        title: {
                            text: 'Cash Collected(MTD)',
                            textColor: 'green'
                        },
                        opts: {
                            width: 50,
                            height: 50,
                            silent: true
                        },
                        tooltip: {},
                        //legend: {
                        //    data: ['销量']
                        //},
                        xAxis: {
                            type: 'category',
                            data: [],
                            axisLabel: {
                                interval: 0,
                                rotate: 55
                            }
                        },
                        yAxis: {
                            axisLabel: {
                                interval: 0,
                                rotate: 55,
                                formatter: '$ {value}'
                            },
                        },
                        series: [{
                            name: 'Cash Collected',
                            type: 'bar',
                            itemStyle: {
                                normal: {
                                    label: {
                                        show: true, position: 'inside',
                                        formatter: function (a) {
                                            return $scope.fmoney(a['value']);
                                        } },
                                    color: function (parmars) {
                                        var colorList = ["#0db328", "#df1525", "#faeb2e",
                                            "#f06270", "#3cb34b", "#2e92ce"];
                                        return colorList[parmars.dataIndex];
                                    }
                                }
                            },
                            data: []
                        }]
                    };

                    var regionList = new Array();
                    var blanceList = new Array();

                    angular.forEach(res, function (r) {
                        regionList.push(r.region);
                        blanceList.push(r.blance);
                    });

                    cashCOption.xAxis.data = regionList;//x轴赋值数据
                    cashCOption.series[0].data = blanceList;//y轴赋值数据

                    // 使用刚指定的配置项和数据显示图表。
                    CashCollected.setOption(cashCOption);

                }, function (res) {
                    //console.log(res);
                });
            }

            GetCashCollected(currentYear, currentMonth);

            $("#cashColl").on('changeDate', function (ev) {
                GetCashCollected(ev.date.getFullYear(), ev.date.getMonth() + 1);
            });

            $scope.obab = function () {
                openARProxy.GetOpenAR($scope.region, function (res) {

                    // 基于准备好的dom，初始化echarts实例
                    var cusAging = echarts.init(document.getElementById('CusAging'));

                    if (res.length == 0) {
                        // 指定图表的配置项和数据
                        var caOption = {
                            color: ['#3398DB'],
                            title: {
                                text: 'Overdue By Aging Bucket'
                            },
                            tooltip: {},
                            //legend: {
                            //    data: ['销量']
                            //},
                            xAxis: {
                                type: 'category',
                                data: ["A Current", "B1-30", "C31-60", "D61-90", "E91-120", "F121-150", "G151-180", "H181-360", "I > 360"],
                                axisLabel: {
                                    interval: 0,
                                    rotate: 55
                                }
                            },
                            yAxis: {
                                axisLabel: {
                                    interval: 0,
                                    rotate: 55,
                                    formatter: '$ {value}'
                                }
                            },
                            series: [{
                                name: 'Aging Bucket',
                                type: 'bar',
                                itemStyle: {
                                    normal: {
                                        label: {
                                            show: true, position: 'inside',
                                            formatter: function (a) {
                                                return $scope.fmoney(a['value']);
                                            } },
                                        color: function (parmars) {
                                            var colorList = ["#213395", "#0db328", "#269e95", "#df1525", "#faeb2e",
                                                "#f06270", "#c9e72e", "#3cb34b", "#2e92ce"];
                                            return colorList[parmars.dataIndex];
                                        }
                                    }
                                },
                                data: [0, 0, 0, 0, 0, 0, 0, 0, 0]
                            }]
                        };

                    } else {
                        // 指定图表的配置项和数据
                        var caOption = {
                            color: ['#3398DB'],
                            title: {
                                text: 'Overdue By Aging Bucket'
                            },
                            tooltip: {},
                            //legend: {
                            //    data: ['销量']
                            //},
                            xAxis: {
                                type: 'category',
                                data: ["A Current", "B1-30", "C31-60", "D61-90", "E91-120", "F121-150", "G151-180", "H181-360", "I > 360"],
                                axisLabel: {
                                    interval: 0,
                                    rotate: 55
                                }
                            },
                            yAxis: {
                                axisLabel: {
                                    interval: 0,
                                    rotate: 55,
                                    formatter: '$ {value}'
                                }
                            },
                            series: [{
                                name: 'Aging Bucket',
                                type: 'bar',
                                itemStyle: {
                                    normal: {
                                        label: {
                                            show: true, position: 'inside',
                                            formatter: function (a) {
                                                return $scope.fmoney(a['value']);
                                            }
                                        },
                                        color: function (parmars) {
                                            var colorList = ["#213395", "#0db328", "#269e95", "#df1525", "#faeb2e",
                                                "#f06270", "#c9e72e", "#3cb34b", "#2e92ce"];
                                            return colorList[parmars.dataIndex];
                                        }
                                    }
                                },
                                data: [res[0].aCurrent == null || res[0].aCurrent == 0 ? 0 : res[0].aCurrent.toFixed(2),
                                res[0].b30 == null || res[0].b30 == 0 ? 0 : res[0].b30.toFixed(2),
                                res[0].c60 == null || res[0].c60 == 0 ? 0 : res[0].c60.toFixed(2),
                                res[0].d90 == null || res[0].d90 == 0 ? 0 : res[0].d90.toFixed(2),
                                res[0].e120 == null || res[0].e120 == 0 ? 0 : res[0].e120.toFixed(2),
                                res[0].f150 == null || res[0].f150 == 0 ? 0 : res[0].f150.toFixed(2),
                                res[0].g180 == null || res[0].g180 == 0 ? 0 : res[0].g180.toFixed(2),
                                res[0].h360 == null || res[0].h360 == 0 ? 0 : res[0].h360.toFixed(2),
                                res[0].i360 == null || res[0].i360 == 0 ? 0 : res[0].i360.toFixed(2)]
                            }]
                        };
                    }

                    // 使用刚指定的配置项和数据显示图表。
                    cusAging.setOption(caOption);

                }, function (res) {
                    //console.log(res);
                    });

            }

            function GetDisputReason(region)
            {
                disputReasonProxy.GetDisputReason(region, function (res) {

                    // 基于准备好的dom，初始化echarts实例
                    var disputReason = echarts.init(document.getElementById('OverDueReason'));

                    // 指定图表的配置项和数据
                    var drOption = {
                        color: ['#3398DB'],
                        title: {
                            text: 'OverDue Reason'
                        },
                        tooltip: {},
                        //legend: {
                        //    data: ['销量']
                        //},
                        xAxis: {
                            type: 'category',
                            data: [],
                            axisLabel: {
                                interval: 0,
                                rotate: 30
                            }
                        },
                        yAxis: {
                            axisLabel: {
                                interval: 0,
                                rotate: 55,
                                formatter: '$ {value}'
                            }
                        },
                        series: [{
                            name: 'OverDue Reason',
                            type: 'bar',
                            itemStyle: {
                                normal: {
                                    label: {
                                        show: true, position: 'inside',
                                        formatter: function (a) {
                                            return $scope.fmoney(a['value']);
                                        }
                                    },
                                    color: function (parmars) {
                                        var colorList = ["#0db328", "#df1525", "#faeb2e",
                                            "#f06270", "#3cb34b", "#2e92ce", "#0db328", "#df1525", "#faeb2e",
                                            "#f06270", "#3cb34b", "#2e92ce", "#0db328", "#df1525", "#faeb2e",
                                            "#f06270", "#3cb34b", "#2e92ce"];
                                        return colorList[parmars.dataIndex];
                                    }
                                }
                            },
                            data: []
                        }]
                    };

                    var dnList = new Array();
                    var blanceList = new Array();
                    angular.forEach(res, function (r) {
                        dnList.push(r.detaiL_NAME);
                        blanceList.push(r.blance);
                    })

                    drOption.xAxis.data = dnList;//x轴赋值数据
                    drOption.series[0].data = blanceList;//y轴赋值数据
                        


                    // 使用刚指定的配置项和数据显示图表。
                    disputReason.setOption(drOption);

                }, function (res) {
                    //console.log(res);
                });
            }

            GetDisputReason($scope.region2);

            $scope.drSelectRegion = function () {
                GetDisputReason($scope.region2);
            }

            $scope.customerList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: true,
                columnDefs: [
                    { field: 'customerNum', displayName: 'Customer Num', width: '120' },
                    { field: 'customerName', displayName: 'Customer Name', width: '350' },
                    { field: 'siteUseId', displayName: 'Site Use ID', width: '110' },
                    { field: 'collector', displayName: 'Collector', width: '150' },
                    { field: 'openAR', displayName: 'Open AR($)', width: '170', cellClass: 'right' },
                    { field: 'overDure', displayName: 'Over Dure($)', width: '120', cellClass: 'right' },
                    { field: 'dispute', displayName: 'Dispute($)', width: '100', cellClass: 'right' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                    
                }
            };

            $scope.collectorList = {
                multiSelect: false,
                enableFullRowSelection: false,
                noUnselect: true,
                columnDefs: [
                    { field: 'id', displayName: 'Id', width: '50' },
                    { field: 'collector', displayName: 'Collector', width: '100' },
                    { field: 'ptpaR_PER', displayName: 'PTP AR Percent(%)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'ptpbrokeN_PER', displayName: 'PTP Broken Percent(%)', width: '160', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'overdueaR_PER', displayName: 'OverDue Percent(%)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'notrefusereplY_PER', displayName: 'Dispute Percent(%)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'ar', displayName: 'AR($)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'ptpar', displayName: 'PTP AR($)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'ptpbrokenar', displayName: 'PTP Broken AR($)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'overduear', displayName: 'OverDue AR($)', width: '150', cellFilter: 'number:2', cellClass: 'right' },
                    { field: 'notrefusereply', displayName: 'Dispute AR($)', width: '150', cellFilter: 'number:2', cellClass: 'right' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi_collector = gridApi;
                }
            };

            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.selectedLevel_collector = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.itemsperpage_collector = 20;
            $scope.currentPage = 1; //当前页
            $scope.currentPage_collector = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页  
            $scope.maxSize_collector = 10; //分页显示的最大页 

            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];

            $scope.regionCollect = 'all';

            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                statisticsCollectProxy.GetStatisticsCollect($scope.regionCollect, $scope.currentPage, selectedLevelId, function (list) {
                    if (list !== null) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = list.count;
                        $scope.customerList.data = list.result;
                        $interval(function () { $scope.gridApi.selection.selectRow($scope.customerList.data[0]); }, 0, 1);
                    }
                })
            };

            //单页容量变化
            $scope.pagesizechange_collector = function (selectedLevelId) {
                statisticsCollectProxy.GetStatisticsCollector($scope.currentPage_collector, selectedLevelId, function (list) {
                    if (list !== null) {
                        $scope.itemsperpage_collector = selectedLevelId;
                        $scope.totalItems_collector = list.count;
                        $scope.collectorList.data = list.result;
                        $interval(function () { $scope.gridApi_collector.selection.selectRow($scope.collectorList.data[0]); }, 0, 1);
                    }
                })
            };
            //翻页
            $scope.pageChanged = function () {
                statisticsCollectProxy.GetStatisticsCollect($scope.regionCollect, $scope.currentPage, $scope.itemsperpage, function (list) {
                    if (list !== null) {
                        $scope.totalItems = list.count;
                        $scope.customerList.data = list.result;
                        $interval(function () { $scope.gridApi.selection.selectRow($scope.customerList.data[0]); }, 0, 1);
                    }
                })
            };
            //翻页
            $scope.pageChanged_collector = function () {
                statisticsCollectProxy.GetStatisticsCollector($scope.currentPage_collector, $scope.itemsperpage_collector, function (list) {
                    if (list !== null) {
                        $scope.totalItems_collector = list.count;
                        $scope.collectorList.data = list.result;
                        $interval(function () { $scope.gridApi_collector.selection.selectRow($scope.collectorList.data[0]); }, 0, 1);
                    }
                })
            };

            statisticsCollectProxy.GetStatisticsCollect($scope.regionCollect,$scope.currentPage, $scope.itemsperpage, function (list) {
                if (list !== null ) {
                    $scope.totalItems = list.count;
                    $scope.customerList.data = list.result;
                    $interval(function () { $scope.gridApi.selection.selectRow($scope.customerList.data[0]); }, 0, 1);
                }
            })

            statisticsCollectProxy.GetStatisticsCollector($scope.currentPage_collector, $scope.itemsperpage_collector, function (list) {
                if (list !== null) {
                    $scope.totalItems_collector = list.count;
                    $scope.collectorList.data = list.result;
                    $interval(function () { $scope.gridApi_collector.selection.selectRow($scope.collectorList.data[0]); }, 0, 1);
                }
            })

            $scope.CollectSelectRegion = function () {
                statisticsCollectProxy.GetStatisticsCollect($scope.regionCollect,$scope.currentPage, $scope.itemsperpage, function (list) {
                    if (list !== null) {
                        $scope.totalItems = list.count;
                        $scope.customerList.data = list.result;
                        $interval(function () { $scope.gridApi.selection.selectRow($scope.customerList.data[0]); }, 0, 1);
                    }
                })
            }

            $scope.exportReport = function () {
                statisticsCollectProxy.downloadReport($scope.regionCollect, function (path) {
                    window.location = path;
                    alert("Export Successful!");
                },
                    function (res) { alert(res); });
            };

            $scope.exportCollectorReport = function () {
                statisticsCollectProxy.downloadCollectorReport(function (path) {
                    window.location = path;
                    alert("Export Successful!");
                },
                    function (res) { alert(res); });
            };

            //费用类别
            $scope.typeList1 = { "detailValue": "", "detailName": 'ALL' };
            $scope.typelist = new Array();
            $scope.typelist.push($scope.typeList1);
            angular.forEach(typelist, function (r) {
                $scope.typelist.push(r);
            });
            $scope.typeSelected = $scope.typelist[1].detailValue;
            //Collector
            $scope.collectorGraphList1 = { "detailValue": "", "detailName": 'ALL' };
            $scope.collectorGraphList = new Array();
            $scope.collectorGraphList.push($scope.collectorGraphList1);
            angular.forEach(collectorGraphlist, function (r) {
                $scope.collectorGraphList.push(r);
            });
            $scope.collectorSelected = $scope.collectorGraphList[0].detailName;

            function GetCollectorStatisticsHis(start,end,type,collector) {
                // 基于准备好的dom，初始化echarts实例
                var CollectorStatisticsHis = echarts.init(document.getElementById('CollectorStatisticsHis'));

                CollectorStatisticsHis.clear();

                // 指定图表的配置项和数据
                var cccOption = {
                    width: '80%',
                    grid: {
                        x: '17%',
                        x2: '17%'
                    }, 
                    color: ['#3398DB'],
                    legend: {
                        data: [],
                        orient: 'vertical',
                        x: 'left',
                        y: '20%'
                    },
                    title: {
                        text: 'Collector Statistic'
                    },
                    tooltip: {},
                    xAxis: {
                        type: 'category',
                        data: [],
                        axisLabel: {
                            interval: 0,
                            rotate: 30
                        }
                    },
                    yAxis: [{
                        axisLabel: {
                            interval: 0,
                            rotate: 55
                        }
                    }
                    //    ,
                    //{
                    //    type: 'value',
                    //    scale: true,
                    //    name: '',
                    //    max: 100,
                    //    min: 0,
                    //    boundaryGap: [0.2, 0.2],
                    //    axisLabel: {
                    //        formatter: '{value} %'
                    //    }
                    //}
                    ],
                    series: []
                };

                collectorStatisticsHisProxy.GetCollectorStatisticsHis(start, end, type, collector,function (res) {

                    var legendList = new Array();
                    var xAxisList = new Array();
                    var seriesList = new Array();

                    //Title赋值
                    cccOption.title.text = res.title;

                    //Legend赋值
                    angular.forEach(res.legend, function (r) {
                        legendList.push(r.key);
                    });
                    cccOption.legend.data = legendList;

                    //x轴赋值
                    angular.forEach(res.xAxis, function (r) {
                        xAxisList.push(r.key.substring(0,10));
                    });
                    cccOption.xAxis.data = xAxisList;

                    //series赋值
                    angular.forEach(legendList, function (r1) {
                        var dataList = new Array();
                        angular.forEach(res.series, function (r) {
                            if (r1 == r.sname) {
                                dataList.push(r.sValue);
                            }
                        });
                        seriesList.push({
                            "name": r1, "type": "line", "smooth": true, "data": dataList
                        });
                    });
                    cccOption.series = seriesList;

                    // 使用刚指定的配置项和数据显示图表。
                    CollectorStatisticsHis.setOption(cccOption);

                }, function (res) {
                    //console.log(res);
                });
            }


            GetCollectorStatisticsHis($scope.startDate, $scope.endDate, $scope.typeSelected, $scope.collectorSelected);

            //选择费用类别
            $scope.typeSelect = function ()
            {
                if ($scope.typeSelected == 'ALL') {
                    $scope.collectorSelected = $scope.collectorGraphList[1].detailName;
                }
                else {
                    $scope.collectorSelected = $scope.collectorGraphList[0].detailName;
                }
                GetCollectorStatisticsHis($scope.startDate, $scope.endDate, $scope.typeSelected, $scope.collectorSelected);
            }
            //选择Collector
            $scope.CollectorSelect = function () {
                if ($scope.collectorSelected == 'ALL') {
                    $scope.typeSelected = $scope.typelist[1].detailName;
                }
                else {
                    $scope.typeSelected = $scope.typelist[0].detailName;
                }
                GetCollectorStatisticsHis($scope.startDate, $scope.endDate, $scope.typeSelected, $scope.collectorSelected);
            }
            //日期选择
            $scope.dateSelect = function () {
                GetCollectorStatisticsHis($scope.startDate, $scope.endDate, $scope.typeSelected, $scope.collectorSelected);
            }
            $("#sd").on('changeDate', function (ev) {
                $scope.dateSelect();
            })
            $("#ed").on('changeDate', function (ev) {
                $scope.dateSelect();
            })
        }]);




